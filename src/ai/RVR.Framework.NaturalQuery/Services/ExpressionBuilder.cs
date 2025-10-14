namespace RVR.Framework.NaturalQuery.Services;

using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using RVR.Framework.NaturalQuery.Models;
using Microsoft.Extensions.Logging;

/// <summary>
/// Converts <see cref="FilterCondition"/> and <see cref="SortCondition"/> instances
/// into LINQ expression trees and applies them to <see cref="IQueryable{T}"/>.
/// </summary>
public sealed class ExpressionBuilder
{
    private readonly ILogger<ExpressionBuilder> _logger;

    private static readonly MethodInfo StringContainsMethod =
        typeof(string).GetMethod(nameof(string.Contains), [typeof(string)])!;

    private static readonly MethodInfo StringStartsWithMethod =
        typeof(string).GetMethod(nameof(string.StartsWith), [typeof(string)])!;

    private static readonly MethodInfo StringEndsWithMethod =
        typeof(string).GetMethod(nameof(string.EndsWith), [typeof(string)])!;

    private static readonly MethodInfo StringToLowerMethod =
        typeof(string).GetMethod(nameof(string.ToLower), Type.EmptyTypes)!;

    /// <summary>
    /// Initializes a new instance of <see cref="ExpressionBuilder"/>.
    /// </summary>
    public ExpressionBuilder(ILogger<ExpressionBuilder> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Applies a <see cref="QueryPlan"/> to an <see cref="IQueryable{T}"/> source.
    /// </summary>
    public IQueryable<T> Apply<T>(IQueryable<T> source, QueryPlan plan) where T : class
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(plan);

        // 1. Apply filters
        source = ApplyFilters(source, plan.Filters);

        // 2. Apply sorts
        source = ApplySorts(source, plan.Sorts);

        // 3. Apply skip
        if (plan.Skip.HasValue && plan.Skip.Value > 0)
            source = source.Skip(plan.Skip.Value);

        // 4. Apply take
        if (plan.Take.HasValue && plan.Take.Value > 0)
            source = source.Take(plan.Take.Value);

        return source;
    }

    // ── Filters ──────────────────────────────────────────────────────

    private IQueryable<T> ApplyFilters<T>(IQueryable<T> source, List<FilterCondition> filters) where T : class
    {
        if (filters.Count == 0) return source;

        var parameter = Expression.Parameter(typeof(T), "x");
        Expression? combined = null;

        foreach (var filter in filters)
        {
            var filterExpr = BuildFilterExpression<T>(parameter, filter);
            if (filterExpr is null)
            {
                _logger.LogWarning("Could not build expression for filter on property '{Property}'", filter.PropertyName);
                continue;
            }

            if (combined is null)
            {
                combined = filterExpr;
            }
            else
            {
                combined = filter.LogicalOp == LogicalOperator.Or
                    ? Expression.OrElse(combined, filterExpr)
                    : Expression.AndAlso(combined, filterExpr);
            }
        }

        if (combined is null) return source;

        var lambda = Expression.Lambda<Func<T, bool>>(combined, parameter);
        return source.Where(lambda);
    }

    private Expression? BuildFilterExpression<T>(ParameterExpression parameter, FilterCondition filter)
    {
        var property = typeof(T).GetProperty(filter.PropertyName, BindingFlags.Public | BindingFlags.Instance);
        if (property is null)
        {
            _logger.LogWarning("Property '{PropertyName}' not found on type '{Type}'", filter.PropertyName, typeof(T).Name);
            return null;
        }

        var memberAccess = Expression.Property(parameter, property);
        var propertyType = property.PropertyType;
        var underlyingType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

        // Handle IsNull / IsNotNull
        if (filter.Operator == FilterOperator.IsNull)
        {
            if (propertyType.IsValueType && Nullable.GetUnderlyingType(propertyType) is null)
                return Expression.Constant(false); // Non-nullable value types are never null
            return Expression.Equal(memberAccess, Expression.Constant(null, propertyType));
        }

        if (filter.Operator == FilterOperator.IsNotNull)
        {
            if (propertyType.IsValueType && Nullable.GetUnderlyingType(propertyType) is null)
                return Expression.Constant(true); // Non-nullable value types are never null
            return Expression.NotEqual(memberAccess, Expression.Constant(null, propertyType));
        }

        // Convert the string value to the target type
        object? convertedValue;
        try
        {
            convertedValue = ConvertValue(filter.Value, underlyingType);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not convert value '{Value}' to type '{Type}'", filter.Value, underlyingType.Name);
            return null;
        }

        // For nullable types, wrap the constant
        var constant = Nullable.GetUnderlyingType(propertyType) is not null
            ? Expression.Constant(convertedValue, propertyType)
            : Expression.Constant(convertedValue, underlyingType);

        // For nullable value type access, get the .Value for comparisons
        Expression memberForComparison = memberAccess;
        if (Nullable.GetUnderlyingType(propertyType) is not null && filter.Operator != FilterOperator.Equals && filter.Operator != FilterOperator.NotEquals)
        {
            // Use the HasValue check + Value access for safe nullable comparison
            var hasValue = Expression.Property(memberAccess, "HasValue");
            var valueAccess = Expression.Property(memberAccess, "Value");
            var innerConstant = Expression.Constant(convertedValue, underlyingType);

            var innerExpr = BuildComparisonExpression(valueAccess, filter.Operator, innerConstant, underlyingType);
            if (innerExpr is null) return null;

            return Expression.AndAlso(hasValue, innerExpr);
        }

        return BuildComparisonExpression(memberForComparison, filter.Operator, constant, underlyingType);
    }

    private Expression? BuildComparisonExpression(Expression member, FilterOperator op, Expression constant, Type underlyingType)
    {
        // String operations
        if (underlyingType == typeof(string))
        {
            // Make string comparisons case-insensitive
            var memberLower = Expression.Call(member, StringToLowerMethod);
            var constantLower = Expression.Call(constant, StringToLowerMethod);

            return op switch
            {
                FilterOperator.Equals => Expression.Equal(memberLower, constantLower),
                FilterOperator.NotEquals => Expression.NotEqual(memberLower, constantLower),
                FilterOperator.Contains => Expression.Call(memberLower, StringContainsMethod, constantLower),
                FilterOperator.StartsWith => Expression.Call(memberLower, StringStartsWithMethod, constantLower),
                FilterOperator.EndsWith => Expression.Call(memberLower, StringEndsWithMethod, constantLower),
                _ => Expression.Equal(member, constant)
            };
        }

        // Numeric / DateTime comparisons
        return op switch
        {
            FilterOperator.Equals => Expression.Equal(member, constant),
            FilterOperator.NotEquals => Expression.NotEqual(member, constant),
            FilterOperator.GreaterThan => Expression.GreaterThan(member, constant),
            FilterOperator.GreaterThanOrEqual => Expression.GreaterThanOrEqual(member, constant),
            FilterOperator.LessThan => Expression.LessThan(member, constant),
            FilterOperator.LessThanOrEqual => Expression.LessThanOrEqual(member, constant),
            FilterOperator.Contains when underlyingType == typeof(string) => Expression.Call(member, StringContainsMethod, constant),
            _ => Expression.Equal(member, constant)
        };
    }

    // ── Sorts ────────────────────────────────────────────────────────

    private static IQueryable<T> ApplySorts<T>(IQueryable<T> source, List<SortCondition> sorts) where T : class
    {
        if (sorts.Count == 0) return source;

        IOrderedQueryable<T>? ordered = null;

        for (var i = 0; i < sorts.Count; i++)
        {
            var sort = sorts[i];
            var property = typeof(T).GetProperty(sort.PropertyName, BindingFlags.Public | BindingFlags.Instance);
            if (property is null) continue;

            var parameter = Expression.Parameter(typeof(T), "x");
            var memberAccess = Expression.Property(parameter, property);
            var lambda = Expression.Lambda(memberAccess, parameter);

            if (i == 0)
            {
                var method = sort.Descending
                    ? nameof(Queryable.OrderByDescending)
                    : nameof(Queryable.OrderBy);

                ordered = (IOrderedQueryable<T>)source.Provider.CreateQuery<T>(
                    Expression.Call(
                        typeof(Queryable),
                        method,
                        [typeof(T), property.PropertyType],
                        source.Expression,
                        Expression.Quote(lambda)));
            }
            else if (ordered is not null)
            {
                var method = sort.Descending
                    ? nameof(Queryable.ThenByDescending)
                    : nameof(Queryable.ThenBy);

                ordered = (IOrderedQueryable<T>)ordered.Provider.CreateQuery<T>(
                    Expression.Call(
                        typeof(Queryable),
                        method,
                        [typeof(T), property.PropertyType],
                        ordered.Expression,
                        Expression.Quote(lambda)));
            }
        }

        return ordered ?? source;
    }

    // ── Value conversion ─────────────────────────────────────────────

    /// <summary>
    /// Converts a string value to the target type.
    /// </summary>
    internal static object? ConvertValue(string value, Type targetType)
    {
        if (targetType == typeof(string))
            return value;

        if (targetType == typeof(bool))
        {
            return value.ToLowerInvariant() switch
            {
                "true" or "1" or "yes" or "oui" or "vrai" => true,
                "false" or "0" or "no" or "non" or "faux" => false,
                _ => bool.Parse(value)
            };
        }

        if (targetType == typeof(int))
            return int.Parse(value, CultureInfo.InvariantCulture);

        if (targetType == typeof(long))
            return long.Parse(value, CultureInfo.InvariantCulture);

        if (targetType == typeof(decimal))
            return decimal.Parse(value, CultureInfo.InvariantCulture);

        if (targetType == typeof(double))
            return double.Parse(value, CultureInfo.InvariantCulture);

        if (targetType == typeof(float))
            return float.Parse(value, CultureInfo.InvariantCulture);

        if (targetType == typeof(DateTime))
            return DateTime.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);

        if (targetType == typeof(DateTimeOffset))
            return DateTimeOffset.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);

        if (targetType == typeof(DateOnly))
            return DateOnly.Parse(value, CultureInfo.InvariantCulture);

        if (targetType == typeof(Guid))
            return Guid.Parse(value);

        if (targetType.IsEnum)
            return Enum.Parse(targetType, value, ignoreCase: true);

        return Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
    }
}
