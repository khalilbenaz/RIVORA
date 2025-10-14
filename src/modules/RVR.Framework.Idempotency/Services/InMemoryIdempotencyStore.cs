using Microsoft.Extensions.Caching.Memory;

namespace RVR.Framework.Idempotency.Services;

/// <summary>
/// In-memory implementation of <see cref="IIdempotencyStore"/> using <see cref="IMemoryCache"/>.
/// </summary>
public sealed class InMemoryIdempotencyStore : IIdempotencyStore
{
    private readonly IMemoryCache _cache;
    private static readonly TimeSpan DefaultExpiration = TimeSpan.FromHours(24);

    /// <summary>
    /// Initializes a new instance of the <see cref="InMemoryIdempotencyStore"/> class.
    /// </summary>
    public InMemoryIdempotencyStore(IMemoryCache cache)
    {
        _cache = cache;
    }

    /// <inheritdoc />
    public Task<IdempotencyEntry?> TryGetAsync(string key, CancellationToken ct = default)
    {
        _cache.TryGetValue($"idempotency:{key}", out IdempotencyEntry? entry);
        return Task.FromResult(entry);
    }

    /// <inheritdoc />
    public Task SetAsync(string key, IdempotencyEntry entry, CancellationToken ct = default)
    {
        _cache.Set($"idempotency:{key}", entry, DefaultExpiration);
        return Task.CompletedTask;
    }
}
