using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RVR.AI.RAG.App.RAG;

namespace RVR.AI.RAG.App.VectorStore;

public interface IVectorStore
{
    Task<string> UpsertAsync(string collection, DocumentChunk chunk, List<float> embedding, CancellationToken ct = default);
    Task<List<SearchResult>> SearchAsync(string collection, List<float> query, int limit = 5, CancellationToken ct = default);
    Task DeleteAsync(string collection, string id, CancellationToken ct = default);
    Task CreateCollectionAsync(string collection, int dimensions = 1536, CancellationToken ct = default);
}

public class SearchResult
{
    public string Id { get; set; } = string.Empty;
    public double Score { get; set; }
    public string Content { get; set; } = string.Empty;
    public Dictionary<string, string> Metadata { get; set; } = new();
}

public class QdrantVectorStore : IVectorStore
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<QdrantVectorStore> _logger;
    private readonly string _apiKey;

    public QdrantVectorStore(HttpClient httpClient, IOptions<QdrantSettings> settings, ILogger<QdrantVectorStore> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _apiKey = settings.Value.ApiKey ?? string.Empty;
        if (!string.IsNullOrEmpty(_apiKey))
        {
            _httpClient.DefaultRequestHeaders.Add("api-key", _apiKey);
        }
    }

    public async Task<string> UpsertAsync(string collection, DocumentChunk chunk, List<float> embedding, CancellationToken ct = default)
    {
        var id = chunk.EmbeddingId.IsNullOrEmpty() ? Guid.NewGuid().ToString() : chunk.EmbeddingId;
        
        var payload = new
        {
            points = new[]
            {
                new
                {
                    id = id,
                    vector = embedding,
                    payload = new Dictionary<string, object>(chunk.Metadata)
                    {
                        { "content", chunk.Content },
                        { "chunk_index", chunk.ChunkIndex },
                        { "document_id", chunk.DocumentId },
                        { "document_title", chunk.DocumentTitle }
                    }
                }
            }
        };

        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync($"/collections/{collection}/points", content, ct);
        response.EnsureSuccessStatusCode();
        
        _logger.LogInformation($"Upserted chunk {id} to collection {collection}");
        return id;
    }

    public async Task<List<SearchResult>> SearchAsync(string collection, List<float> query, int limit = 5, CancellationToken ct = default)
    {
        var payload = new
        {
            vector = query,
            limit = limit,
            with_payload = true,
            score_threshold = 0.5
        };

        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync($"/collections/{collection}/points/search", content, ct);
        response.EnsureSuccessStatusCode();
        
        var responseJson = await response.Content.ReadAsStringAsync(ct);
        var result = JsonDocument.Parse(responseJson);
        var points = result.RootElement.GetProperty("result");
        
        var results = new List<SearchResult>();
        foreach (var point in points.EnumerateArray())
        {
            var payload = point.GetProperty("payload");
            var metadata = new Dictionary<string, string>();
            
            foreach (var prop in payload.EnumerateObject())
            {
                metadata[prop.Name] = prop.Value.ToString();
            }
            
            results.Add(new SearchResult
            {
                Id = point.GetProperty("id").ToString(),
                Score = point.GetProperty("score").GetDouble(),
                Content = metadata.GetValueOrDefault("content", ""),
                Metadata = metadata
            });
        }
        
        return results;
    }

    public async Task DeleteAsync(string collection, string id, CancellationToken ct = default)
    {
        var payload = new { points = new[] { id } };
        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync($"/collections/{collection}/points/delete", content, ct);
        response.EnsureSuccessStatusCode();
    }

    public async Task CreateCollectionAsync(string collection, int dimensions = 1536, CancellationToken ct = default)
    {
        var payload = new
        {
            vectors = new
            {
                size = dimensions,
                distance = "Cosine"
            }
        };

        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        var response = await _httpClient.PutAsync($"/collections/{collection}", content, ct);
        response.EnsureSuccessStatusCode();
        
        _logger.LogInformation($"Created collection {collection} with {dimensions} dimensions");
    }
}

public class QdrantSettings
{
    public string Endpoint { get; set; } = "http://localhost:6333";
    public string? ApiKey { get; set; }
}
