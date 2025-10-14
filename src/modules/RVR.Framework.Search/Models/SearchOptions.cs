namespace RVR.Framework.Search.Models;

/// <summary>
/// Configuration options for the search service.
/// </summary>
public class SearchOptions
{
    /// <summary>
    /// Gets or sets the default index name.
    /// </summary>
    public string Index { get; set; } = "default";

    /// <summary>
    /// Gets or sets whether to enable highlighting in search results.
    /// </summary>
    public bool Highlight { get; set; } = false;

    /// <summary>
    /// Gets or sets the list of aggregation field names.
    /// </summary>
    public IList<string> Aggregations { get; set; } = new List<string>();

    /// <summary>
    /// Gets or sets the Elasticsearch node URI.
    /// </summary>
    public string NodeUri { get; set; } = "http://localhost:9200";

    /// <summary>
    /// Gets or sets the default page size.
    /// </summary>
    public int DefaultPageSize { get; set; } = 20;

    /// <summary>
    /// Gets or sets the maximum page size.
    /// </summary>
    public int MaxPageSize { get; set; } = 100;
}
