namespace RVR.Framework.Search.Models;

/// <summary>
/// Represents the result of a search operation.
/// </summary>
/// <typeparam name="T">The type of the search result items.</typeparam>
public class SearchResult<T>
{
    /// <summary>
    /// Gets or sets the search result items.
    /// </summary>
    public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();

    /// <summary>
    /// Gets or sets the total number of matching documents.
    /// </summary>
    public long TotalCount { get; set; }

    /// <summary>
    /// Gets or sets the facets/aggregations from the search.
    /// </summary>
    public IDictionary<string, IReadOnlyDictionary<string, long>> Facets { get; set; }
        = new Dictionary<string, IReadOnlyDictionary<string, long>>();

    /// <summary>
    /// Gets or sets the time taken in milliseconds.
    /// </summary>
    public long Took { get; set; }
}
