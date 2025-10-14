namespace RVR.Framework.Search.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RVR.Framework.Search.Interfaces;
using RVR.Framework.Search.Models;

/// <summary>
/// Elasticsearch implementation of <see cref="ISearchService{T}"/>.
/// </summary>
/// <typeparam name="T">The type of the documents being searched.</typeparam>
public class ElasticsearchSearchService<T> : ISearchService<T> where T : class
{
    private readonly ElasticsearchClient _client;
    private readonly SearchOptions _options;
    private readonly ILogger<ElasticsearchSearchService<T>>? _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ElasticsearchSearchService{T}"/> class.
    /// </summary>
    /// <param name="client">The Elasticsearch client.</param>
    /// <param name="options">The search options.</param>
    /// <param name="logger">The logger.</param>
    public ElasticsearchSearchService(
        ElasticsearchClient client,
        IOptions<SearchOptions> options,
        ILogger<ElasticsearchSearchService<T>>? logger = null)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _options = options?.Value ?? new SearchOptions();
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<SearchResult<T>> SearchAsync(
        string query,
        IEnumerable<SearchFilter>? filters = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = _options.DefaultPageSize;
        if (pageSize > _options.MaxPageSize) pageSize = _options.MaxPageSize;

        var from = (page - 1) * pageSize;

        try
        {
            var response = await _client.SearchAsync<T>(s =>
            {
                s.Index(_options.Index)
                 .From(from)
                 .Size(pageSize);

                // Build query
                if (!string.IsNullOrWhiteSpace(query))
                {
                    var filterList = filters?.ToList();

                    if (filterList != null && filterList.Count > 0)
                    {
                        s.Query(q => q
                            .Bool(b => b
                                .Must(m => m.QueryString(qs => qs.Query(query)))
                                .Filter(BuildFilters(filterList).ToArray())
                            )
                        );
                    }
                    else
                    {
                        s.Query(q => q
                            .QueryString(qs => qs.Query(query))
                        );
                    }
                }
                else if (filters != null)
                {
                    var filterList2 = filters.ToList();
                    if (filterList2.Count > 0)
                    {
                        s.Query(q => q
                            .Bool(b => b
                                .Filter(BuildFilters(filterList2).ToArray())
                            )
                        );
                    }
                    else
                    {
                        s.Query(q => q.MatchAll(new MatchAllQuery()));
                    }
                }
                else
                {
                    s.Query(q => q.MatchAll(new MatchAllQuery()));
                }
            }, cancellationToken);

            if (!response.IsValidResponse)
            {
                _logger?.LogWarning("Elasticsearch search failed: {DebugInfo}", response.DebugInformation);
                return new SearchResult<T>();
            }

            return new SearchResult<T>
            {
                Items = response.Documents.ToList(),
                TotalCount = response.Total,
                Took = response.Took
            };
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error searching Elasticsearch index: {Index}", _options.Index);
            return new SearchResult<T>();
        }
    }

    /// <inheritdoc/>
    public async Task IndexAsync(T document, string? id = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(document);

        try
        {
            IndexResponse response;

            if (!string.IsNullOrWhiteSpace(id))
            {
                response = await _client.IndexAsync(document, idx => idx
                    .Index(_options.Index)
                    .Id(id), cancellationToken);
            }
            else
            {
                response = await _client.IndexAsync(document, idx => idx
                    .Index(_options.Index), cancellationToken);
            }

            if (!response.IsValidResponse)
            {
                _logger?.LogWarning("Elasticsearch index failed: {DebugInfo}", response.DebugInformation);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error indexing document in Elasticsearch index: {Index}", _options.Index);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        try
        {
            var response = await _client.DeleteAsync<T>(id, idx => idx
                .Index(_options.Index), cancellationToken);

            if (!response.IsValidResponse)
            {
                _logger?.LogWarning("Elasticsearch delete failed for id {Id}: {DebugInfo}", id, response.DebugInformation);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error deleting document {Id} from Elasticsearch index: {Index}", id, _options.Index);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task ReindexAsync(IEnumerable<T> documents, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(documents);

        try
        {
            // Delete existing index
            var deleteResponse = await _client.Indices.DeleteAsync(_options.Index, cancellationToken);
            if (!deleteResponse.IsValidResponse)
            {
                _logger?.LogDebug("Index {Index} did not exist or could not be deleted", _options.Index);
            }

            // Bulk index all documents
            var documentList = documents.ToList();
            var bulkResponse = await _client.BulkAsync(b =>
            {
                b.Index(_options.Index);
                b.IndexMany(documentList);
            }, cancellationToken);

            if (!bulkResponse.IsValidResponse)
            {
                _logger?.LogWarning("Elasticsearch bulk reindex failed: {DebugInfo}", bulkResponse.DebugInformation);
            }
            else
            {
                _logger?.LogInformation("Successfully reindexed {Count} documents in index {Index}",
                    bulkResponse.Items.Count, _options.Index);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error reindexing Elasticsearch index: {Index}", _options.Index);
            throw;
        }
    }

    private static IEnumerable<Action<QueryDescriptor<T>>> BuildFilters(List<SearchFilter> filters)
    {
        foreach (var filter in filters)
        {
            var f = filter; // capture for closure
            var fieldName = new Field(f.Field);
            switch (f.Operator)
            {
                case SearchFilterOperator.Equals:
                    yield return q => q.Term(t => t.Field(fieldName).Value(f.Value));
                    break;
                case SearchFilterOperator.Contains:
                    yield return q => q.Match(m => m.Field(fieldName).Query(f.Value));
                    break;
                case SearchFilterOperator.GreaterThan:
                    yield return q => q.Range(r => r.NumberRange(nr => nr.Field(fieldName).Gt(double.Parse(f.Value))));
                    break;
                case SearchFilterOperator.LessThan:
                    yield return q => q.Range(r => r.NumberRange(nr => nr.Field(fieldName).Lt(double.Parse(f.Value))));
                    break;
                default:
                    break;
            }
        }
    }
}
