namespace RVR.Framework.Caching.Interfaces;

using System;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Defines the contract for hybrid caching (L1 Memory + L2 Distributed).
/// </summary>
public interface IRvrHybridCache
{
    /// <summary>
    /// Gets or creates a cached value using a factory function.
    /// </summary>
    /// <typeparam name="T">The type of the cached value.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="factory">The factory function to create the value if not cached.</param>
    /// <param name="expiration">Optional expiration time.</param>
    /// <param name="tags">Optional tags for cache invalidation.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The cached or newly created value.</returns>
    Task<T> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan? expiration = null,
        IEnumerable<string>? tags = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a cached value by key.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets a cached value with the specified key.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="value">The value to cache.</param>
    /// <param name="expiration">Optional expiration time.</param>
    /// <param name="tags">Optional tags for cache invalidation.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? expiration = null,
        IEnumerable<string>? tags = null,
        CancellationToken cancellationToken = default);
}
