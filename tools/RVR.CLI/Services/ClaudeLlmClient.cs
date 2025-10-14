using Anthropic.SDK;
using Anthropic.SDK.Constants;
using Anthropic.SDK.Messaging;

namespace RVR.CLI.Services;

/// <summary>
/// LLM client backed by the Anthropic Claude API.
/// </summary>
public sealed class ClaudeLlmClient : ILlmClient
{
    private readonly string _apiKey;
    private readonly string _model;

    public ClaudeLlmClient(string apiKey, string model = AnthropicModels.Claude35Sonnet)
    {
        _apiKey = apiKey;
        _model = model;
    }

    public string ProviderName => "Claude";

    public bool IsAvailable => !string.IsNullOrWhiteSpace(_apiKey);

    public async Task<string> CompleteAsync(string systemPrompt, string userPrompt, CancellationToken ct = default)
    {
        var client = new AnthropicClient(_apiKey);

        var messages = new List<Message>
        {
            new Message
            {
                Role = RoleType.User,
                Content = new List<ContentBase> { new TextContent { Text = userPrompt } }
            }
        };

        var parameters = new MessageParameters
        {
            Messages = messages,
            MaxTokens = 4096,
            Model = _model,
            System = new List<SystemMessage> { new SystemMessage(systemPrompt) }
        };

        var response = await client.Messages.GetClaudeMessageAsync(parameters, ct);
        var textContent = response.Message.Content[0] as TextContent;
        return textContent?.Text ?? string.Empty;
    }
}
