namespace RVR.Framework.Search.Models;

/// <summary>
/// Represents a search filter.
/// </summary>
public class SearchFilter
{
    /// <summary>
    /// Gets or sets the field name to filter on.
    /// </summary>
    public string Field { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the filter value.
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the filter operator.
    /// </summary>
    public SearchFilterOperator Operator { get; set; } = SearchFilterOperator.Equals;
}

/// <summary>
/// Search filter operators.
/// </summary>
public enum SearchFilterOperator
{
    /// <summary>Exact match.</summary>
    Equals,

    /// <summary>Contains the value.</summary>
    Contains,

    /// <summary>Greater than.</summary>
    GreaterThan,

    /// <summary>Less than.</summary>
    LessThan,

    /// <summary>Range filter.</summary>
    Range
}
