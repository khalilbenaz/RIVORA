namespace RVR.Framework.Idempotency.Services;

/// <summary>
/// Stores idempotency responses keyed by the idempotency key.
/// </summary>
public interface IIdempotencyStore
{
    /// <summary>
    /// Tries to get a cached response for the given idempotency key.
    /// </summary>
    /// <param name="key">The idempotency key.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The cached response bytes and status code, or null if not found.</returns>
    Task<IdempotencyEntry?> TryGetAsync(string key, CancellationToken ct = default);

    /// <summary>
    /// Stores a response for the given idempotency key.
    /// </summary>
    /// <param name="key">The idempotency key.</param>
    /// <param name="entry">The response entry to cache.</param>
    /// <param name="ct">Cancellation token.</param>
    Task SetAsync(string key, IdempotencyEntry entry, CancellationToken ct = default);
}

/// <summary>
/// Represents a cached idempotent response.
/// </summary>
public sealed class IdempotencyEntry
{
    /// <summary>
    /// The HTTP status code of the cached response.
    /// </summary>
    public int StatusCode { get; init; }

    /// <summary>
    /// The response body bytes.
    /// </summary>
    public byte[] Body { get; init; } = [];

    /// <summary>
    /// The content type of the response.
    /// </summary>
    public string? ContentType { get; init; }
}
