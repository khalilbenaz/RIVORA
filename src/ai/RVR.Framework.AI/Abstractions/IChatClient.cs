namespace RVR.Framework.AI.Abstractions;

/// <summary>
/// Abstraction for interacting with a chat-based language model.
/// </summary>
public interface IChatClient
{
    /// <summary>
    /// Sends a chat request and returns the complete response.
    /// </summary>
    Task<ChatResponse> ChatAsync(ChatRequest request, CancellationToken ct = default);

    /// <summary>
    /// Sends a chat request and returns a streaming response.
    /// </summary>
    Task<IAsyncEnumerable<string>> StreamChatAsync(ChatRequest request, CancellationToken ct = default);
}

/// <summary>
/// Represents a chat completion request.
/// </summary>
public class ChatRequest
{
    /// <summary>
    /// The model identifier to use for the request.
    /// </summary>
    public string Model { get; set; } = string.Empty;

    /// <summary>
    /// The list of messages in the conversation.
    /// </summary>
    public List<ChatMessage> Messages { get; set; } = [];

    /// <summary>
    /// Sampling temperature between 0 and 1.
    /// </summary>
    public float Temperature { get; set; } = 0.7f;

    /// <summary>
    /// Maximum number of tokens to generate.
    /// </summary>
    public int MaxTokens { get; set; } = 1024;

    /// <summary>
    /// Optional system prompt to prepend to the conversation.
    /// </summary>
    public string? SystemPrompt { get; set; }
}

/// <summary>
/// Represents a single message in a chat conversation.
/// </summary>
public class ChatMessage
{
    /// <summary>
    /// The role of the message author: system, user, or assistant.
    /// </summary>
    public string Role { get; set; } = "user";

    /// <summary>
    /// The text content of the message.
    /// </summary>
    public string Content { get; set; } = string.Empty;
}

/// <summary>
/// Represents the response from a chat completion request.
/// </summary>
public class ChatResponse
{
    /// <summary>
    /// The generated text content.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// The model that generated the response.
    /// </summary>
    public string Model { get; set; } = string.Empty;

    /// <summary>
    /// Number of tokens in the prompt.
    /// </summary>
    public int PromptTokens { get; set; }

    /// <summary>
    /// Number of tokens in the completion.
    /// </summary>
    public int CompletionTokens { get; set; }

    /// <summary>
    /// Total tokens used (prompt + completion).
    /// </summary>
    public int TotalTokens { get; set; }
}
