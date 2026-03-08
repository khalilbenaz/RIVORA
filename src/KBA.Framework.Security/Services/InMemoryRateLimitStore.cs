namespace KBA.Framework.Security.Services;

using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using KBA.Framework.Security.Interfaces;

/// <summary>
/// In-memory implementation of <see cref="IRateLimitStore"/>.
/// Uses thread-safe concurrent collections for storage.
/// Suitable for single-instance deployments.
/// </summary>
public class InMemoryRateLimitStore : IRateLimitStore
{
    private readonly ConcurrentDictionary<string, RateLimitEntry> _entries = new();
    private readonly ConcurrentDictionary<string, TimestampEntry> _timestamps = new();
    private readonly ConcurrentDictionary<string, TokenBucketEntry> _tokenBuckets = new();

    /// <inheritdoc/>
    public Task<long> GetCountAsync(string key, CancellationToken cancellationToken = default)
    {
        if (_entries.TryGetValue(key, out var entry))
        {
            if (entry.ExpiresAt > DateTime.UtcNow)
            {
                return Task.FromResult(entry.Count);
            }
        }

        return Task.FromResult(0L);
    }

    /// <inheritdoc/>
    public Task<long> IncrementAsync(string key, int windowSeconds, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var expiresAt = now.AddSeconds(windowSeconds);

        var newCount = _entries.AddOrUpdate(
            key,
            _ => new RateLimitEntry { Count = 1, ExpiresAt = expiresAt },
            (_, entry) =>
            {
                if (entry.ExpiresAt <= now)
                {
                    // Window expired, reset
                    entry.Count = 1;
                    entry.ExpiresAt = expiresAt;
                }
                else
                {
                    entry.Count++;
                }

                return entry;
            });

        return Task.FromResult(newCount.Count);
    }

    /// <inheritdoc/>
    public Task SetAsync(string key, long value, int expirationSeconds, CancellationToken cancellationToken = default)
    {
        _entries[key] = new RateLimitEntry
        {
            Count = value,
            ExpiresAt = DateTime.UtcNow.AddSeconds(expirationSeconds)
        };

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task<DateTime?> GetTimestampAsync(string key, CancellationToken cancellationToken = default)
    {
        if (_timestamps.TryGetValue(key, out var entry))
        {
            if (entry.ExpiresAt > DateTime.UtcNow)
            {
                return Task.FromResult<DateTime?>(entry.Timestamp);
            }
        }

        return Task.FromResult<DateTime?>(null);
    }

    /// <inheritdoc/>
    public Task SetTimestampAsync(string key, DateTime timestamp, int expirationSeconds, CancellationToken cancellationToken = default)
    {
        _timestamps[key] = new TimestampEntry
        {
            Timestamp = timestamp,
            ExpiresAt = DateTime.UtcNow.AddSeconds(expirationSeconds)
        };

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task<double> GetTokenCountAsync(string key, CancellationToken cancellationToken = default)
    {
        if (_tokenBuckets.TryGetValue(key, out var entry))
        {
            if (entry.ExpiresAt > DateTime.UtcNow)
            {
                // Calculate tokens based on time elapsed since last refill
                var now = DateTime.UtcNow;
                var elapsed = (now - entry.LastRefill).TotalSeconds;
                var refilledTokens = Math.Min(entry.Capacity, entry.Tokens + (elapsed * entry.RefillRate));

                return Task.FromResult(refilledTokens);
            }
        }

        return Task.FromResult(0.0);
    }

    /// <inheritdoc/>
    public Task SetTokenBucketAsync(string key, double tokenCount, DateTime lastRefill, int expirationSeconds, CancellationToken cancellationToken = default)
    {
        _tokenBuckets[key] = new TokenBucketEntry
        {
            Tokens = tokenCount,
            LastRefill = lastRefill,
            Capacity = tokenCount, // Store the capacity
            RefillRate = 1.0, // Default refill rate
            ExpiresAt = DateTime.UtcNow.AddSeconds(expirationSeconds)
        };

        return Task.CompletedTask;
    }

    private class RateLimitEntry
    {
        public long Count { get; set; }
        public DateTime ExpiresAt { get; set; }
    }

    private class TimestampEntry
    {
        public DateTime Timestamp { get; set; }
        public DateTime ExpiresAt { get; set; }
    }

    private class TokenBucketEntry
    {
        public double Tokens { get; set; }
        public DateTime LastRefill { get; set; }
        public double Capacity { get; set; }
        public double RefillRate { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
