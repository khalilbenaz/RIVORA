namespace RVR.Framework.Search.Services;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RVR.Framework.Search.Interfaces;
using RVR.Framework.Search.Models;

/// <summary>
/// In-memory implementation of <see cref="ISearchService{T}"/> for development and testing.
/// </summary>
/// <typeparam name="T">The type of the documents being searched.</typeparam>
public class InMemorySearchService<T> : ISearchService<T> where T : class
{
    private readonly ConcurrentDictionary<string, T> _documents = new();
    private readonly ILogger<InMemorySearchService<T>>? _logger;
    private int _autoIdCounter;

    /// <summary>
    /// Initializes a new instance of the <see cref="InMemorySearchService{T}"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    public InMemorySearchService(ILogger<InMemorySearchService<T>>? logger = null)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public Task<SearchResult<T>> SearchAsync(
        string query,
        IEnumerable<SearchFilter>? filters = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();

        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;

        var allDocs = _documents.Values.ToList();
        IEnumerable<T> results = allDocs;

        // Simple text search: serialize to JSON and check for query substring
        if (!string.IsNullOrWhiteSpace(query))
        {
            var lowerQuery = query.ToLowerInvariant();
            results = results.Where(doc =>
            {
                var json = JsonSerializer.Serialize(doc).ToLowerInvariant();
                return json.Contains(lowerQuery);
            });
        }

        // Apply filters
        if (filters != null)
        {
            foreach (var filter in filters)
            {
                var f = filter;
                results = results.Where(doc =>
                {
                    var json = JsonSerializer.Serialize(doc);
                    using var jsonDoc = JsonDocument.Parse(json);
                    if (jsonDoc.RootElement.TryGetProperty(f.Field, out var prop))
                    {
                        var propValue = prop.ToString();
                        return f.Operator switch
                        {
                            SearchFilterOperator.Equals => string.Equals(propValue, f.Value, StringComparison.OrdinalIgnoreCase),
                            SearchFilterOperator.Contains => propValue.Contains(f.Value, StringComparison.OrdinalIgnoreCase),
                            _ => true
                        };
                    }
                    return false;
                });
            }
        }

        var resultList = results.ToList();
        var total = resultList.Count;
        var paged = resultList.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        sw.Stop();

        var searchResult = new SearchResult<T>
        {
            Items = paged,
            TotalCount = total,
            Took = sw.ElapsedMilliseconds
        };

        _logger?.LogDebug("In-memory search completed: {TotalCount} results in {Took}ms", total, sw.ElapsedMilliseconds);

        return Task.FromResult(searchResult);
    }

    /// <inheritdoc/>
    public Task IndexAsync(T document, string? id = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(document);

        var docId = id ?? Interlocked.Increment(ref _autoIdCounter).ToString();
        _documents[docId] = document;

        _logger?.LogDebug("In-memory index: document {Id} indexed", docId);

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        _documents.TryRemove(id, out _);

        _logger?.LogDebug("In-memory delete: document {Id} removed", id);

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task ReindexAsync(IEnumerable<T> documents, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(documents);

        _documents.Clear();
        _autoIdCounter = 0;

        foreach (var doc in documents)
        {
            var docId = Interlocked.Increment(ref _autoIdCounter).ToString();
            _documents[docId] = doc;
        }

        _logger?.LogInformation("In-memory reindex: {Count} documents indexed", _documents.Count);

        return Task.CompletedTask;
    }
}
