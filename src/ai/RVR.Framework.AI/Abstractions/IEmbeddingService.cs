namespace RVR.Framework.AI.Abstractions;

/// <summary>
/// Abstraction for generating vector embeddings from text.
/// </summary>
public interface IEmbeddingService
{
    /// <summary>
    /// Generates an embedding vector for a single text input.
    /// </summary>
    Task<float[]> GetEmbeddingAsync(string text, CancellationToken ct = default);

    /// <summary>
    /// Generates embedding vectors for multiple text inputs.
    /// </summary>
    Task<IReadOnlyList<float[]>> GetEmbeddingsAsync(IEnumerable<string> texts, CancellationToken ct = default);
}
