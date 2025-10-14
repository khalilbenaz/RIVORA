namespace RVR.Framework.Core.Pagination;

/// <summary>
/// Represents a page of results using cursor-based (keyset) pagination.
/// </summary>
/// <typeparam name="T">The type of items in the result set.</typeparam>
public class PagedResult<T>
{
    /// <summary>
    /// The items in the current page.
    /// </summary>
    public IReadOnlyList<T> Items { get; set; } = [];

    /// <summary>
    /// The cursor to use to fetch the next page, or null if there are no more pages.
    /// </summary>
    public string? NextCursor { get; set; }

    /// <summary>
    /// The cursor to use to fetch the previous page, or null if this is the first page.
    /// </summary>
    public string? PreviousCursor { get; set; }

    /// <summary>
    /// Whether there are more items beyond this page.
    /// </summary>
    public bool HasMore { get; set; }

    /// <summary>
    /// The total count of items matching the query (without pagination).
    /// </summary>
    public int TotalCount { get; set; }
}

/// <summary>
/// Represents a request for cursor-based pagination.
/// </summary>
public class CursorPageRequest
{
    /// <summary>
    /// The cursor pointing to the position to start from.
    /// Null means start from the beginning.
    /// </summary>
    public string? Cursor { get; set; }

    /// <summary>
    /// The number of items per page. Defaults to 20.
    /// </summary>
    public int PageSize { get; set; } = 20;

    /// <summary>
    /// The property name to sort by. Defaults to "Id".
    /// </summary>
    public string SortBy { get; set; } = "Id";

    /// <summary>
    /// Whether to sort in descending order.
    /// </summary>
    public bool Descending { get; set; }
}
