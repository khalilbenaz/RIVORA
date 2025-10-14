using OpenAI.Chat;
using System.ClientModel;

namespace RVR.CLI.Services;

/// <summary>
/// LLM client backed by OpenAI (GPT-4o by default).
/// </summary>
public sealed class OpenAiLlmClient : ILlmClient
{
    private readonly string _apiKey;
    private readonly string _model;

    public OpenAiLlmClient(string apiKey, string model = "gpt-4o")
    {
        _apiKey = apiKey;
        _model = model;
    }

    public string ProviderName => "OpenAI";

    public bool IsAvailable => !string.IsNullOrWhiteSpace(_apiKey);

    public async Task<string> CompleteAsync(string systemPrompt, string userPrompt, CancellationToken ct = default)
    {
        var client = new ChatClient(_model, new ApiKeyCredential(_apiKey));

        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(systemPrompt),
            new UserChatMessage(userPrompt)
        };

        var completion = await client.CompleteChatAsync(messages, cancellationToken: ct);
        return completion.Value.Content[0].Text;
    }
}
