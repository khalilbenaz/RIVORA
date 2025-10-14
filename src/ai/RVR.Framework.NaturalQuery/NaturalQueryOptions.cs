namespace RVR.Framework.NaturalQuery;

/// <summary>
/// Configuration options for the natural language query system.
/// </summary>
public class NaturalQueryOptions
{
    /// <summary>
    /// Maximum value allowed for Take (LIMIT). Defaults to 1000.
    /// Any parsed Take value above this will be clamped.
    /// </summary>
    public int MaxTake { get; set; } = 1000;

    /// <summary>
    /// Maximum value allowed for Skip (OFFSET). Defaults to 10000.
    /// Any parsed Skip value above this will be clamped.
    /// </summary>
    public int MaxSkip { get; set; } = 10_000;

    /// <summary>
    /// Optional whitelist of allowed property names. When set (non-empty),
    /// only properties in this list can be used in filters and sorts.
    /// Queries targeting non-whitelisted properties will have those conditions rejected.
    /// </summary>
    public HashSet<string>? AllowedProperties { get; set; }
}
