namespace RVR.CLI.Services;

/// <summary>
/// Unified abstraction for LLM provider clients.
/// </summary>
public interface ILlmClient
{
    /// <summary>Name of the LLM provider (e.g. "OpenAI", "Claude", "Ollama").</summary>
    string ProviderName { get; }

    /// <summary>
    /// Sends a completion request to the configured LLM backend.
    /// </summary>
    /// <param name="systemPrompt">The system-level instruction.</param>
    /// <param name="userPrompt">The user-level prompt with context.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The model completion text.</returns>
    Task<string> CompleteAsync(string systemPrompt, string userPrompt, CancellationToken ct = default);

    /// <summary>Whether the provider is reachable / correctly configured.</summary>
    bool IsAvailable { get; }
}
