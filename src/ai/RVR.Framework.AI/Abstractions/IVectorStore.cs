namespace RVR.Framework.AI.Abstractions;

/// <summary>
/// Abstraction for storing and searching vector embeddings.
/// </summary>
public interface IVectorStore
{
    /// <summary>
    /// Inserts or updates a vector entry with its metadata.
    /// </summary>
    Task UpsertAsync(string id, float[] embedding, Dictionary<string, string> metadata, CancellationToken ct = default);

    /// <summary>
    /// Searches for the most similar vectors to the query embedding.
    /// </summary>
    Task<IReadOnlyList<VectorSearchResult>> SearchAsync(float[] queryEmbedding, int topK = 5, CancellationToken ct = default);

    /// <summary>
    /// Deletes a vector entry by its identifier.
    /// </summary>
    Task DeleteAsync(string id, CancellationToken ct = default);
}

/// <summary>
/// Represents a single result from a vector similarity search.
/// </summary>
public class VectorSearchResult
{
    /// <summary>
    /// The identifier of the matched vector entry.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// The similarity score (higher is more similar).
    /// </summary>
    public float Score { get; set; }

    /// <summary>
    /// The metadata associated with the matched entry.
    /// </summary>
    public Dictionary<string, string> Metadata { get; set; } = [];
}
