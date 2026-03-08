namespace KBA.Framework.Security.Interfaces;

using System;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Defines the contract for rate limit storage operations.
/// </summary>
public interface IRateLimitStore
{
    /// <summary>
    /// Gets the current count for a rate limit key.
    /// </summary>
    /// <param name="key">The rate limit key.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The current count.</returns>
    Task<long> GetCountAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Increments the count for a rate limit key within a time window.
    /// </summary>
    /// <param name="key">The rate limit key.</param>
    /// <param name="windowSeconds">The time window in seconds.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The new count after incrementing.</returns>
    Task<long> IncrementAsync(string key, int windowSeconds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets a value with an expiration time.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="value">The value.</param>
    /// <param name="expirationSeconds">The expiration time in seconds.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SetAsync(string key, long value, int expirationSeconds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the timestamp for a sliding window.
    /// </summary>
    /// <param name="key">The rate limit key.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The timestamp, or null if not found.</returns>
    Task<DateTime?> GetTimestampAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets the timestamp for a sliding window.
    /// </summary>
    /// <param name="key">The rate limit key.</param>
    /// <param name="timestamp">The timestamp.</param>
    /// <param name="expirationSeconds">The expiration time in seconds.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SetTimestampAsync(string key, DateTime timestamp, int expirationSeconds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current token count for token bucket algorithm.
    /// </summary>
    /// <param name="key">The rate limit key.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The current token count.</returns>
    Task<double> GetTokenCountAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets the token count and last refill time for token bucket algorithm.
    /// </summary>
    /// <param name="key">The rate limit key.</param>
    /// <param name="tokenCount">The token count.</param>
    /// <param name="lastRefill">The last refill time.</param>
    /// <param name="expirationSeconds">The expiration time in seconds.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SetTokenBucketAsync(string key, double tokenCount, DateTime lastRefill, int expirationSeconds, CancellationToken cancellationToken = default);
}
