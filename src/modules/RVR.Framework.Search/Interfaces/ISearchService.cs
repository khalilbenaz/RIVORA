namespace RVR.Framework.Search.Interfaces;

using RVR.Framework.Search.Models;

/// <summary>
/// Defines the contract for search operations.
/// </summary>
/// <typeparam name="T">The type of the documents being searched.</typeparam>
public interface ISearchService<T> where T : class
{
    /// <summary>
    /// Searches for documents matching the given query and filters.
    /// </summary>
    /// <param name="query">The search query string.</param>
    /// <param name="filters">Optional filters to apply.</param>
    /// <param name="page">The page number (1-based).</param>
    /// <param name="pageSize">The number of results per page.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The search results.</returns>
    Task<SearchResult<T>> SearchAsync(
        string query,
        IEnumerable<SearchFilter>? filters = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Indexes a document for searching.
    /// </summary>
    /// <param name="document">The document to index.</param>
    /// <param name="id">Optional document ID. If null, the search engine will generate one.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task IndexAsync(T document, string? id = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a document by its ID.
    /// </summary>
    /// <param name="id">The document ID.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Re-indexes all documents. This is typically used for rebuilding the search index.
    /// </summary>
    /// <param name="documents">The documents to re-index.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ReindexAsync(IEnumerable<T> documents, CancellationToken cancellationToken = default);
}
