namespace RVR.Framework.Caching.Services;

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using RVR.Framework.Caching.Interfaces;

/// <summary>
/// Implementation of <see cref="IRvrHybridCache"/> using .NET 9 HybridCache (L1 Memory + L2 Distributed).
/// </summary>
public class HybridCacheService : IRvrHybridCache
{
    private readonly HybridCache _hybridCache;
    private readonly ILogger<HybridCacheService>? _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="HybridCacheService"/> class.
    /// </summary>
    /// <param name="hybridCache">The underlying .NET HybridCache instance.</param>
    /// <param name="logger">The logger.</param>
    public HybridCacheService(
        HybridCache hybridCache,
        ILogger<HybridCacheService>? logger = null)
    {
        _hybridCache = hybridCache ?? throw new ArgumentNullException(nameof(hybridCache));
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<T> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan? expiration = null,
        IEnumerable<string>? tags = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var entryOptions = expiration.HasValue
            ? new HybridCacheEntryOptions
            {
                Expiration = expiration.Value,
                LocalCacheExpiration = expiration.Value
            }
            : new HybridCacheEntryOptions();

        var tagArray = tags?.ToArray();

        _logger?.LogDebug("HybridCache GetOrCreate: {Key}", key);

        return await _hybridCache.GetOrCreateAsync(
            key,
            async (ct) => await factory(ct),
            entryOptions,
            tagArray,
            cancellationToken);
    }

    /// <inheritdoc/>
    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        _logger?.LogDebug("HybridCache Remove: {Key}", key);

        await _hybridCache.RemoveAsync(key, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? expiration = null,
        IEnumerable<string>? tags = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var entryOptions = expiration.HasValue
            ? new HybridCacheEntryOptions
            {
                Expiration = expiration.Value,
                LocalCacheExpiration = expiration.Value
            }
            : new HybridCacheEntryOptions();

        var tagArray = tags?.ToArray();

        _logger?.LogDebug("HybridCache Set: {Key}", key);

        await _hybridCache.SetAsync(
            key,
            value,
            entryOptions,
            tagArray,
            cancellationToken);
    }
}
