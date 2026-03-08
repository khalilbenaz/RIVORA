using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using KBA.AI.RAG.App.Domain.Entities;

namespace KBA.AI.RAG.App.AI;

public interface IAiProvider
{
    Task<AiResponse> CompleteAsync(AiRequest request, CancellationToken ct = default);
    Task<EmbeddingResponse> EmbedAsync(string text, CancellationToken ct = default);
    decimal CalculateCost(int inputTokens, int outputTokens);
}

public class AiRequest
{
    public List<Message> Messages { get; set; } = new();
    public string Model { get; set; } = string.Empty;
    public double Temperature { get; set; } = 0.7;
    public int MaxTokens { get; set; } = 1024;
    public List<string>? StopSequences { get; set; }
}

public class Message
{
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}

public class AiResponse
{
    public string Content { get; set; } = string.Empty;
    public int InputTokens { get; set; }
    public int OutputTokens { get; set; }
    public int TotalTokens { get; set; }
    public string Model { get; set; } = string.Empty;
    public TimeSpan Latency { get; set; }
}

public class EmbeddingResponse
{
    public List<float> Embedding { get; set; } = new();
    public int Dimensions { get; set; }
}

public class OpenAiProvider : IAiProvider
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OpenAiProvider> _logger;
    private readonly string _apiKey;

    public OpenAiProvider(HttpClient httpClient, IOptions<OpenAiSettings> settings, ILogger<OpenAiProvider> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _apiKey = settings.Value.ApiKey;
        _httpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);
    }

    public async Task<AiResponse> CompleteAsync(AiRequest request, CancellationToken ct = default)
    {
        var startTime = DateTime.UtcNow;
        
        var payload = new
        {
            model = request.Model,
            messages = request.Messages,
            temperature = request.Temperature,
            max_tokens = request.MaxTokens
        };

        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("v1/chat/completions", content, ct);
        response.EnsureSuccessStatusCode();
        
        var responseJson = await response.Content.ReadAsStringAsync(ct);
        var result = JsonDocument.Parse(responseJson);
        
        var latency = DateTime.UtcNow - startTime;
        var usage = result.RootElement.GetProperty("usage");
        
        return new AiResponse
        {
            Content = result.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? "",
            InputTokens = usage.GetProperty("prompt_tokens").GetInt32(),
            OutputTokens = usage.GetProperty("completion_tokens").GetInt32(),
            TotalTokens = usage.GetProperty("total_tokens").GetInt32(),
            Model = request.Model,
            Latency = latency
        };
    }

    public async Task<EmbeddingResponse> EmbedAsync(string text, CancellationToken ct = default)
    {
        var payload = new { model = "text-embedding-ada-002", input = text };
        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("v1/embeddings", content, ct);
        response.EnsureSuccessStatusCode();
        
        var responseJson = await response.Content.ReadAsStringAsync(ct);
        var result = JsonDocument.Parse(responseJson);
        var embedding = result.RootElement.GetProperty("data")[0].GetProperty("embedding");
        
        return new EmbeddingResponse
        {
            Embedding = embedding.EnumerateArray().Select(e => e.GetSingle()).ToList(),
            Dimensions = 1536
        };
    }

    public decimal CalculateCost(int inputTokens, int outputTokens)
    {
        // GPT-3.5-turbo pricing: $0.0015/1K input, $0.002/1K output
        return (inputTokens * 0.0015m / 1000) + (outputTokens * 0.002m / 1000);
    }
}

public class OpenAiSettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string Endpoint { get; set; } = "https://api.openai.com";
}

public class AnthropicProvider : IAiProvider
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AnthropicProvider> _logger;
    private readonly string _apiKey;

    public AnthropicProvider(HttpClient httpClient, IOptions<AnthropicSettings> settings, ILogger<AnthropicProvider> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _apiKey = settings.Value.ApiKey;
        _httpClient.DefaultRequestHeaders.Add("x-api-key", _apiKey);
        _httpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
    }

    public async Task<AiResponse> CompleteAsync(AiRequest request, CancellationToken ct = default)
    {
        var startTime = DateTime.UtcNow;
        
        var payload = new
        {
            model = request.Model,
            max_tokens = request.MaxTokens,
            messages = request.Messages
        };

        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("v1/messages", content, ct);
        response.EnsureSuccessStatusCode();
        
        var responseJson = await response.Content.ReadAsStringAsync(ct);
        var result = JsonDocument.Parse(responseJson);
        
        var latency = DateTime.UtcNow - startTime;
        
        return new AiResponse
        {
            Content = result.RootElement.GetProperty("content")[0].GetProperty("text").GetString() ?? "",
            InputTokens = result.RootElement.GetProperty("usage").GetProperty("input_tokens").GetInt32(),
            OutputTokens = result.RootElement.GetProperty("usage").GetProperty("output_tokens").GetInt32(),
            TotalTokens = result.RootElement.GetProperty("usage").GetProperty("input_tokens").GetInt32() + 
                         result.RootElement.GetProperty("usage").GetProperty("output_tokens").GetInt32(),
            Model = request.Model,
            Latency = latency
        };
    }

    public Task<EmbeddingResponse> EmbedAsync(string text, CancellationToken ct = default)
    {
        // Anthropic doesn't have embeddings API, use OpenAI or other
        throw new NotImplementedException("Anthropic does not support embeddings");
    }

    public decimal CalculateCost(int inputTokens, int outputTokens)
    {
        // Claude pricing: $0.0008/1K input, $0.0024/1K output (Claude 3 Sonnet)
        return (inputTokens * 0.0008m / 1000) + (outputTokens * 0.0024m / 1000);
    }
}

public class AnthropicSettings
{
    public string ApiKey { get; set; } = string.Empty;
}

public class OllamaProvider : IAiProvider
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OllamaProvider> _logger;

    public OllamaProvider(HttpClient httpClient, IOptions<OllamaSettings> settings, ILogger<OllamaProvider> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<AiResponse> CompleteAsync(AiRequest request, CancellationToken ct = default)
    {
        var startTime = DateTime.UtcNow;
        
        var payload = new
        {
            model = request.Model,
            messages = request.Messages,
            stream = false
        };

        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("api/chat", content, ct);
        response.EnsureSuccessStatusCode();
        
        var responseJson = await response.Content.ReadAsStringAsync(ct);
        var result = JsonDocument.Parse(responseJson);
        
        var latency = DateTime.UtcNow - startTime;
        
        return new AiResponse
        {
            Content = result.RootElement.GetProperty("message").GetProperty("content").GetString() ?? "",
            InputTokens = 0, // Ollama doesn't return token counts by default
            OutputTokens = 0,
            TotalTokens = 0,
            Model = request.Model,
            Latency = latency
        };
    }

    public async Task<EmbeddingResponse> EmbedAsync(string text, CancellationToken ct = default)
    {
        var payload = new { model = "nomic-embed-text", prompt = text };
        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("api/embeddings", content, ct);
        response.EnsureSuccessStatusCode();
        
        var responseJson = await response.Content.ReadAsStringAsync(ct);
        var result = JsonDocument.Parse(responseJson);
        var embedding = result.RootElement.GetProperty("embedding");
        
        return new EmbeddingResponse
        {
            Embedding = embedding.EnumerateArray().Select(e => e.GetSingle()).ToList(),
            Dimensions = 768
        };
    }

    public decimal CalculateCost(int inputTokens, int outputTokens)
    {
        // Ollama is free (local)
        return 0;
    }
}

public class OllamaSettings
{
    public string Endpoint { get; set; } = "http://localhost:11434";
}
