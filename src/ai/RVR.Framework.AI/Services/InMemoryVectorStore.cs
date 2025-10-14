using System.Collections.Concurrent;
using System.Numerics;
using RVR.Framework.AI.Abstractions;

namespace RVR.Framework.AI.Services;

/// <summary>
/// An in-memory implementation of <see cref="IVectorStore"/> that uses cosine similarity for search.
/// Suitable for development and testing; not recommended for production workloads with large datasets.
/// </summary>
public class InMemoryVectorStore : IVectorStore
{
    private readonly ConcurrentDictionary<string, VectorEntry> _store = new();

    /// <inheritdoc />
    public Task UpsertAsync(string id, float[] embedding, Dictionary<string, string> metadata, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentNullException.ThrowIfNull(embedding);
        ArgumentNullException.ThrowIfNull(metadata);

        _store[id] = new VectorEntry
        {
            Id = id,
            Embedding = embedding,
            Metadata = new Dictionary<string, string>(metadata)
        };

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<VectorSearchResult>> SearchAsync(float[] queryEmbedding, int topK = 5, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(queryEmbedding);

        var results = _store.Values
            .Select(entry => new VectorSearchResult
            {
                Id = entry.Id,
                Score = CosineSimilarity(queryEmbedding, entry.Embedding),
                Metadata = new Dictionary<string, string>(entry.Metadata)
            })
            .OrderByDescending(r => r.Score)
            .Take(topK)
            .ToList();

        return Task.FromResult<IReadOnlyList<VectorSearchResult>>(results);
    }

    /// <inheritdoc />
    public Task DeleteAsync(string id, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        _store.TryRemove(id, out _);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Computes the cosine similarity between two vectors.
    /// Uses SIMD-accelerated operations via <see cref="Vector{T}"/> when possible.
    /// </summary>
    private static float CosineSimilarity(float[] a, float[] b)
    {
        if (a.Length != b.Length)
            throw new ArgumentException("Vectors must have the same dimensionality.");

        if (a.Length == 0)
            return 0f;

        var dotProduct = 0f;
        var normA = 0f;
        var normB = 0f;

        var simdLength = Vector<float>.Count;
        var i = 0;

        // SIMD-accelerated path
        if (Vector.IsHardwareAccelerated && a.Length >= simdLength)
        {
            var vDot = Vector<float>.Zero;
            var vNormA = Vector<float>.Zero;
            var vNormB = Vector<float>.Zero;

            for (; i <= a.Length - simdLength; i += simdLength)
            {
                var va = new Vector<float>(a, i);
                var vb = new Vector<float>(b, i);
                vDot += va * vb;
                vNormA += va * va;
                vNormB += vb * vb;
            }

            dotProduct = Vector.Sum(vDot);
            normA = Vector.Sum(vNormA);
            normB = Vector.Sum(vNormB);
        }

        // Handle remaining elements
        for (; i < a.Length; i++)
        {
            dotProduct += a[i] * b[i];
            normA += a[i] * a[i];
            normB += b[i] * b[i];
        }

        var denominator = MathF.Sqrt(normA) * MathF.Sqrt(normB);
        return denominator == 0f ? 0f : dotProduct / denominator;
    }

    private sealed class VectorEntry
    {
        public required string Id { get; init; }
        public required float[] Embedding { get; init; }
        public required Dictionary<string, string> Metadata { get; init; }
    }
}
