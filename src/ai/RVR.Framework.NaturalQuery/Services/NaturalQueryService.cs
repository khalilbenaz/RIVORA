namespace RVR.Framework.NaturalQuery.Services;

using RVR.Framework.NaturalQuery.Models;
using Microsoft.Extensions.Logging;

/// <summary>
/// Default implementation of <see cref="INaturalQueryService"/>
/// that uses a rule-based NL parser and expression builder.
/// </summary>
public sealed class NaturalQueryService : INaturalQueryService
{
    private readonly NaturalLanguageParser _parser;
    private readonly ExpressionBuilder _expressionBuilder;
    private readonly ILogger<NaturalQueryService> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="NaturalQueryService"/>.
    /// </summary>
    public NaturalQueryService(
        NaturalLanguageParser parser,
        ExpressionBuilder expressionBuilder,
        ILogger<NaturalQueryService> logger)
    {
        _parser = parser;
        _expressionBuilder = expressionBuilder;
        _logger = logger;
    }

    /// <inheritdoc />
    public QueryPlan Parse(string naturalLanguageQuery, Type entityType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(naturalLanguageQuery);
        ArgumentNullException.ThrowIfNull(entityType);

        _logger.LogInformation("Parsing natural language query for entity {EntityType}: '{Query}'",
            entityType.Name, naturalLanguageQuery);

        return _parser.Parse(naturalLanguageQuery, entityType);
    }

    /// <inheritdoc />
    public IQueryable<T> Apply<T>(IQueryable<T> source, QueryPlan plan) where T : class
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(plan);

        _logger.LogDebug("Applying QueryPlan with {FilterCount} filters, {SortCount} sorts to {EntityType}",
            plan.Filters.Count, plan.Sorts.Count, typeof(T).Name);

        return _expressionBuilder.Apply(source, plan);
    }

    /// <inheritdoc />
    public IQueryable<T> Query<T>(IQueryable<T> source, string naturalLanguageQuery) where T : class
    {
        var plan = Parse(naturalLanguageQuery, typeof(T));
        return Apply(source, plan);
    }
}
