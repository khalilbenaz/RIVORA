namespace KBA.Framework.Caching.Interfaces;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Defines the contract for response caching service with tag-based invalidation.
/// </summary>
public interface IResponseCacheService
{
    /// <summary>
    /// Gets a cached response by key.
    /// </summary>
    /// <typeparam name="T">The type of the cached response.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The cached response, or default if not found.</returns>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets a cached response with the specified key and options.
    /// </summary>
    /// <typeparam name="T">The type of the response.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="value">The response to cache.</param>
    /// <param name="durationSeconds">The cache duration in seconds.</param>
    /// <param name="tags">The tags for grouping cache entries.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SetAsync<T>(
        string key,
        T value,
        int durationSeconds = 300,
        IEnumerable<string>? tags = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a cached response by key.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes all cached responses with the specified tag.
    /// </summary>
    /// <param name="tag">The tag to invalidate.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task InvalidateByTagAsync(string tag, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes all cached responses with any of the specified tags.
    /// </summary>
    /// <param name="tags">The tags to invalidate.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task InvalidateByTagsAsync(IEnumerable<string> tags, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes all cached responses.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ClearAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a response is cached.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>True if the response is cached.</returns>
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets or creates a cached value using a factory function.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="factory">The factory function to create the value if not cached.</param>
    /// <param name="durationSeconds">The cache duration in seconds.</param>
    /// <param name="tags">The tags for grouping cache entries.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The cached or newly created value.</returns>
    Task<T> GetOrSetAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        int durationSeconds = 300,
        IEnumerable<string>? tags = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all keys associated with a tag.
    /// </summary>
    /// <param name="tag">The tag.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A collection of cache keys.</returns>
    Task<IEnumerable<string>> GetKeysByTagAsync(string tag, CancellationToken cancellationToken = default);

    /// <summary>
    /// Refreshes the expiration of a cached item.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <param name="durationSeconds">The new duration in seconds.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RefreshAsync(string key, int durationSeconds, CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents cached response metadata.
/// </summary>
public class CachedResponseMetadata
{
    /// <summary>
    /// Gets or sets the cache key.
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the tags associated with the cache entry.
    /// </summary>
    public IEnumerable<string> Tags { get; set; } = Enumerable.Empty<string>();

    /// <summary>
    /// Gets or sets the creation time of the cache entry.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the expiration time of the cache entry.
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Gets or sets the size of the cached data in bytes.
    /// </summary>
    public long SizeBytes { get; set; }

    /// <summary>
    /// Gets or sets the number of times this cache entry has been accessed.
    /// </summary>
    public int HitCount { get; set; }

    /// <summary>
    /// Gets or sets the last access time.
    /// </summary>
    public DateTime? LastAccessedAt { get; set; }
}

/// <summary>
/// Represents cache statistics.
/// </summary>
public class CacheStatistics
{
    /// <summary>
    /// Gets or sets the total number of cache entries.
    /// </summary>
    public int TotalEntries { get; set; }

    /// <summary>
    /// Gets or sets the total size of cached data in bytes.
    /// </summary>
    public long TotalSizeBytes { get; set; }

    /// <summary>
    /// Gets or sets the number of cache hits.
    /// </summary>
    public long Hits { get; set; }

    /// <summary>
    /// Gets or sets the number of cache misses.
    /// </summary>
    public long Misses { get; set; }

    /// <summary>
    /// Gets or sets the hit rate (0-1).
    /// </summary>
    public double HitRate => TotalRequests > 0 ? (double)Hits / TotalRequests : 0;

    /// <summary>
    /// Gets or sets the total number of requests.
    /// </summary>
    public long TotalRequests => Hits + Misses;

    /// <summary>
    /// Gets or sets the number of evictions.
    /// </summary>
    public long Evictions { get; set; }

    /// <summary>
    /// Gets or sets the number of tag invalidations.
    /// </summary>
    public long TagInvalidations { get; set; }
}
