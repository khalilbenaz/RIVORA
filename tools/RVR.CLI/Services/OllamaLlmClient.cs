using OpenAI.Chat;
using System.ClientModel;

namespace RVR.CLI.Services;

/// <summary>
/// LLM client backed by a local Ollama instance (OpenAI-compatible API).
/// </summary>
public sealed class OllamaLlmClient : ILlmClient
{
    private readonly string _endpoint;
    private readonly string _model;

    public OllamaLlmClient(string endpoint = "http://localhost:11434", string model = "llama3")
    {
        _endpoint = endpoint.TrimEnd('/');
        _model = model;
    }

    public string ProviderName => "Ollama";

    public bool IsAvailable => true; // Assume local Ollama is available; real check would ping the endpoint.

    public async Task<string> CompleteAsync(string systemPrompt, string userPrompt, CancellationToken ct = default)
    {
        var apiUrl = _endpoint.EndsWith("/v1") ? _endpoint : $"{_endpoint}/v1";

        var clientOptions = new OpenAI.OpenAIClientOptions { Endpoint = new Uri(apiUrl) };
        var client = new ChatClient(_model, new ApiKeyCredential("ollama"), clientOptions);

        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(systemPrompt),
            new UserChatMessage(userPrompt)
        };

        var completion = await client.CompleteChatAsync(messages, cancellationToken: ct);
        return completion.Value.Content[0].Text;
    }
}
