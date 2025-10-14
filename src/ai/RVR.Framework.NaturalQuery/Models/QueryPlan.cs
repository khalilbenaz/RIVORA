namespace RVR.Framework.NaturalQuery.Models;

/// <summary>
/// Represents a structured query plan parsed from natural language.
/// </summary>
public class QueryPlan
{
    /// <summary>
    /// Filter conditions to apply (WHERE clause).
    /// </summary>
    public List<FilterCondition> Filters { get; set; } = [];

    /// <summary>
    /// Sort conditions to apply (ORDER BY clause).
    /// </summary>
    public List<SortCondition> Sorts { get; set; } = [];

    /// <summary>
    /// Number of records to skip (OFFSET).
    /// </summary>
    public int? Skip { get; set; }

    /// <summary>
    /// Number of records to take (LIMIT / TOP).
    /// </summary>
    public int? Take { get; set; }

    /// <summary>
    /// Properties to select (projection). Empty means select all.
    /// </summary>
    public List<string> SelectProperties { get; set; } = [];
}

/// <summary>
/// Represents a single filter condition.
/// </summary>
public class FilterCondition
{
    /// <summary>
    /// The property name on the entity to filter.
    /// </summary>
    public string PropertyName { get; set; } = string.Empty;

    /// <summary>
    /// The comparison operator.
    /// </summary>
    public FilterOperator Operator { get; set; } = FilterOperator.Equals;

    /// <summary>
    /// The value to compare against (as string, will be converted).
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// Logical operator combining this condition with the previous one.
    /// </summary>
    public LogicalOperator LogicalOp { get; set; } = LogicalOperator.And;
}

/// <summary>
/// Supported filter operators.
/// </summary>
public enum FilterOperator
{
    /// <summary>Exact equality.</summary>
    Equals,
    /// <summary>Not equal.</summary>
    NotEquals,
    /// <summary>String contains.</summary>
    Contains,
    /// <summary>String starts with.</summary>
    StartsWith,
    /// <summary>String ends with.</summary>
    EndsWith,
    /// <summary>Greater than.</summary>
    GreaterThan,
    /// <summary>Greater than or equal.</summary>
    GreaterThanOrEqual,
    /// <summary>Less than.</summary>
    LessThan,
    /// <summary>Less than or equal.</summary>
    LessThanOrEqual,
    /// <summary>Is null check.</summary>
    IsNull,
    /// <summary>Is not null check.</summary>
    IsNotNull,
    /// <summary>Value in a set.</summary>
    In
}

/// <summary>
/// Logical operators for combining filter conditions.
/// </summary>
public enum LogicalOperator
{
    /// <summary>Logical AND.</summary>
    And,
    /// <summary>Logical OR.</summary>
    Or
}

/// <summary>
/// Represents a sort condition.
/// </summary>
public class SortCondition
{
    /// <summary>
    /// The property name to sort by.
    /// </summary>
    public string PropertyName { get; set; } = string.Empty;

    /// <summary>
    /// Whether to sort descending (true) or ascending (false).
    /// </summary>
    public bool Descending { get; set; }
}
