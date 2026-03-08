namespace KBA.Framework.Security.Services;

using System;
using System.Threading;
using System.Threading.Tasks;
using KBA.Framework.Security.Interfaces;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

/// <summary>
/// Redis implementation of <see cref="IRateLimitStore"/>.
/// Suitable for distributed deployments with multiple instances.
/// </summary>
public class RedisRateLimitStore : IRateLimitStore
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisRateLimitStore> _logger;
    private readonly string _keyPrefix;

    /// <summary>
    /// Initializes a new instance of the <see cref="RedisRateLimitStore"/> class.
    /// </summary>
    /// <param name="redis">The Redis connection multiplexer.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="keyPrefix">Optional prefix for all keys.</param>
    public RedisRateLimitStore(
        IConnectionMultiplexer redis,
        ILogger<RedisRateLimitStore> logger,
        string keyPrefix = "ratelimit:")
    {
        _redis = redis ?? throw new ArgumentNullException(nameof(redis));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _keyPrefix = keyPrefix ?? "ratelimit:";
    }

    private IDatabase Database => _redis.GetDatabase();

    /// <inheritdoc/>
    public async Task<long> GetCountAsync(string key, CancellationToken cancellationToken = default)
    {
        var fullKey = $"{_keyPrefix}count:{key}";
        var value = await Database.StringGetAsync(fullKey);

        if (value.IsNullOrEmpty)
        {
            return 0;
        }

        if (!long.TryParse(value.ToString(), out var count))
        {
            return 0;
        }

        return count;
    }

    /// <inheritdoc/>
    public async Task<long> IncrementAsync(string key, int windowSeconds, CancellationToken cancellationToken = default)
    {
        var fullKey = $"{_keyPrefix}count:{key}";

        var newValue = await Database.StringIncrementAsync(fullKey, 1);

        // Set expiration only if this is the first increment
        if (newValue == 1)
        {
            await Database.KeyExpireAsync(fullKey, TimeSpan.FromSeconds(windowSeconds));
        }

        return newValue;
    }

    /// <inheritdoc/>
    public async Task SetAsync(string key, long value, int expirationSeconds, CancellationToken cancellationToken = default)
    {
        var fullKey = $"{_keyPrefix}count:{key}";
        await Database.StringSetAsync(fullKey, value.ToString(), TimeSpan.FromSeconds(expirationSeconds));
    }

    /// <inheritdoc/>
    public async Task<DateTime?> GetTimestampAsync(string key, CancellationToken cancellationToken = default)
    {
        var fullKey = $"{_keyPrefix}ts:{key}";
        var value = await Database.StringGetAsync(fullKey);

        if (value.IsNullOrEmpty)
        {
            return null;
        }

        if (!DateTime.TryParse(value.ToString(), out var timestamp))
        {
            return null;
        }

        return timestamp;
    }

    /// <inheritdoc/>
    public async Task SetTimestampAsync(string key, DateTime timestamp, int expirationSeconds, CancellationToken cancellationToken = default)
    {
        var fullKey = $"{_keyPrefix}ts:{key}";
        await Database.StringSetAsync(fullKey, timestamp.ToString("O"), TimeSpan.FromSeconds(expirationSeconds));
    }

    /// <inheritdoc/>
    public async Task<double> GetTokenCountAsync(string key, CancellationToken cancellationToken = default)
    {
        var tokensKey = $"{_keyPrefix}tokens:{key}";
        var refillKey = $"{_keyPrefix}refill:{key}";

        var tokensValue = await Database.StringGetAsync(tokensKey);
        var refillValue = await Database.StringGetAsync(refillKey);

        if (tokensValue.IsNullOrEmpty || refillValue.IsNullOrEmpty)
        {
            return 0;
        }

        if (!double.TryParse(tokensValue.ToString(), out var tokens) ||
            !DateTime.TryParse(refillValue.ToString(), out var lastRefill))
        {
            return 0;
        }

        // Get capacity and refill rate from hash
        var bucketData = await Database.HashGetAllAsync($"{_keyPrefix}bucket:{key}");
        var capacity = 100.0;
        var refillRate = 1.0;

        foreach (var entry in bucketData)
        {
            if (entry.Name == "capacity" && double.TryParse(entry.Value.ToString(), out var c))
            {
                capacity = c;
            }
            else if (entry.Name == "refillRate" && double.TryParse(entry.Value.ToString(), out var r))
            {
                refillRate = r;
            }
        }

        // Calculate tokens based on time elapsed
        var now = DateTime.UtcNow;
        var elapsed = (now - lastRefill).TotalSeconds;
        var refilledTokens = Math.Min(capacity, tokens + (elapsed * refillRate));

        return refilledTokens;
    }

    /// <inheritdoc/>
    public async Task SetTokenBucketAsync(string key, double tokenCount, DateTime lastRefill, int expirationSeconds, CancellationToken cancellationToken = default)
    {
        var tokensKey = $"{_keyPrefix}tokens:{key}";
        var refillKey = $"{_keyPrefix}refill:{key}";
        var bucketKey = $"{_keyPrefix}bucket:{key}";

        await Database.StringSetAsync(tokensKey, tokenCount.ToString(), TimeSpan.FromSeconds(expirationSeconds), flags: CommandFlags.DemandMaster);
        await Database.StringSetAsync(refillKey, lastRefill.ToString("O"), TimeSpan.FromSeconds(expirationSeconds), flags: CommandFlags.DemandMaster);
        await Database.HashSetAsync(bucketKey, new[]
        {
            new HashEntry("capacity", tokenCount.ToString()),
            new HashEntry("refillRate", "1.0")
        }, flags: CommandFlags.DemandMaster);
    }
}
