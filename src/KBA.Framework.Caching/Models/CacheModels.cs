namespace KBA.Framework.Caching.Models;

using System;
using System.Collections.Generic;

/// <summary>
/// Represents a cached item with metadata.
/// </summary>
/// <typeparam name="T">The type of the cached value.</typeparam>
public class CacheEntry<T>
{
    /// <summary>
    /// Gets or sets the cached value.
    /// </summary>
    public T? Value { get; set; }

    /// <summary>
    /// Gets or sets the absolute expiration time.
    /// </summary>
    public DateTime? AbsoluteExpiration { get; set; }

    /// <summary>
    /// Gets or sets the sliding expiration.
    /// </summary>
    public TimeSpan? SlidingExpiration { get; set; }

    /// <summary>
    /// Gets or sets the tags associated with this cache entry.
    /// </summary>
    public HashSet<string> Tags { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets or sets the creation time.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the last access time.
    /// </summary>
    public DateTime? LastAccessedAt { get; set; }

    /// <summary>
    /// Gets or sets the access count.
    /// </summary>
    public int AccessCount { get; set; }

    /// <summary>
    /// Gets or sets the size in bytes.
    /// </summary>
    public long SizeBytes { get; set; }

    /// <summary>
    /// Gets whether this entry is expired.
    /// </summary>
    public bool IsExpired
    {
        get
        {
            var now = DateTime.UtcNow;

            if (AbsoluteExpiration.HasValue && now >= AbsoluteExpiration.Value)
            {
                return true;
            }

            if (SlidingExpiration.HasValue && LastAccessedAt.HasValue)
            {
                return now >= LastAccessedAt.Value + SlidingExpiration.Value;
            }

            return false;
        }
    }
}

/// <summary>
/// Represents cache configuration options.
/// </summary>
public class CacheOptions
{
    /// <summary>
    /// Gets or sets the default cache duration in seconds.
    /// </summary>
    public int DefaultDurationSeconds { get; set; } = 300;

    /// <summary>
    /// Gets or sets the maximum number of entries for in-memory cache.
    /// </summary>
    public int? MaxEntries { get; set; }

    /// <summary>
    /// Gets or sets the maximum size of the cache in bytes.
    /// </summary>
    public long? MaxSizeBytes { get; set; }

    /// <summary>
    /// Gets or sets the key prefix for all cache entries.
    /// </summary>
    public string KeyPrefix { get; set; } = "kba:";

    /// <summary>
    /// Gets or sets whether to enable cache statistics tracking.
    /// </summary>
    public bool EnableStatistics { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to log cache operations.
    /// </summary>
    public bool EnableLogging { get; set; } = false;

    /// <summary>
    /// Gets or sets the compression threshold in bytes. Values larger than this will be compressed.
    /// </summary>
    public int CompressionThresholdBytes { get; set; } = 1024;

    /// <summary>
    /// Gets or sets whether to enable compression.
    /// </summary>
    public bool EnableCompression { get; set; } = true;
}

/// <summary>
/// Represents Redis-specific cache options.
/// </summary>
public class RedisCacheOptions
{
    /// <summary>
    /// Gets or sets the Redis connection string.
    /// </summary>
    public string ConnectionString { get; set; } = "localhost:6379";

    /// <summary>
    /// Gets or sets the Redis instance name.
    /// </summary>
    public string InstanceName { get; set; } = "kba:";

    /// <summary>
    /// Gets or sets the database number.
    /// </summary>
    public int Database { get; set; } = 0;

    /// <summary>
    /// Gets or sets whether to use SSL.
    /// </summary>
    public bool Ssl { get; set; } = false;

    /// <summary>
    /// Gets or sets the password.
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// Gets or sets the connection timeout in milliseconds.
    /// </summary>
    public int ConnectTimeout { get; set; } = 5000;

    /// <summary>
    /// Gets or sets the sync timeout in milliseconds.
    /// </summary>
    public int SyncTimeout { get; set; } = 5000;

    /// <summary>
    /// Gets or sets the async timeout in milliseconds.
    /// </summary>
    public int AsyncTimeout { get; set; } = 5000;

    /// <summary>
    /// Gets or sets whether to allow admin operations.
    /// </summary>
    public bool AllowAdmin { get; set; } = false;

    /// <summary>
    /// Gets or sets the abort on connect failure setting.
    /// </summary>
    public bool AbortOnConnectFail { get; set; } = false;

    /// <summary>
    /// Gets or sets whether to keep the connection alive.
    /// </summary>
    public bool KeepAlive { get; set; } = true;

    /// <summary>
    /// Gets or sets the tie breaker for multiplexer selection.
    /// </summary>
    public string? TieBreaker { get; set; }

    /// <summary>
    /// Gets or sets the write buffer size.
    /// </summary>
    public int WriteBuffer { get; set; } = 4096;
}

/// <summary>
/// Represents memory cache-specific options.
/// </summary>
public class MemoryCacheOptions
{
    /// <summary>
    /// Gets or sets the maximum number of entries.
    /// </summary>
    public long? SizeLimit { get; set; }

    /// <summary>
    /// Gets or sets the expiration scan frequency.
    /// </summary>
    public TimeSpan? ExpirationScanFrequency { get; set; }

    /// <summary>
    /// Gets or sets the compaction percentage.
    /// </summary>
    public double CompactionPercentage { get; set; } = 0.5;
}
