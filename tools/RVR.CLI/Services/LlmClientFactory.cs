namespace RVR.CLI.Services;

/// <summary>
/// Factory that creates the appropriate <see cref="ILlmClient"/> for a given provider name.
/// </summary>
public static class LlmClientFactory
{
    /// <summary>
    /// Creates an LLM client based on the provider string.
    /// </summary>
    /// <param name="provider">Provider name: openai, claude, or ollama.</param>
    /// <param name="apiKey">Optional API key override (falls back to env vars).</param>
    /// <param name="endpoint">Optional endpoint override (mainly for Ollama).</param>
    /// <returns>A configured <see cref="ILlmClient"/> instance.</returns>
    public static ILlmClient Create(string provider, string? apiKey = null, string? endpoint = null)
    {
        return provider.ToLowerInvariant() switch
        {
            "openai" => new OpenAiLlmClient(
                apiKey ?? Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? ""),
            "claude" => new ClaudeLlmClient(
                apiKey ?? Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY") ?? ""),
            "ollama" => new OllamaLlmClient(
                endpoint ?? Environment.GetEnvironmentVariable("OLLAMA_API_URL") ?? "http://localhost:11434"),
            _ => throw new ArgumentException($"Unknown LLM provider: '{provider}'. Supported: openai, claude, ollama.")
        };
    }
}
