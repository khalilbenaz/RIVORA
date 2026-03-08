namespace KBA.Framework.Caching.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KBA.Framework.Caching.Interfaces;
using KBA.Framework.Caching.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

/// <summary>
/// Redis implementation of IResponseCacheService with tag-based invalidation.
/// </summary>
public class RedisResponseCacheService : IResponseCacheService
{
    private readonly IConnectionMultiplexer _connection;
    private readonly IDatabase _database;
    private readonly CacheOptions _options;
    private readonly RedisCacheOptions _redisOptions;
    private readonly ILogger<RedisResponseCacheService>? _logger;

    private const string TagSetPrefix = "kba:tags:";
    private const string AllTagsKey = "kba:alltags";

    /// <summary>
    /// Initializes a new instance of the <see cref="RedisResponseCacheService"/> class.
    /// </summary>
    /// <param name="connection">The Redis connection multiplexer.</param>
    /// <param name="options">The cache options.</param>
    /// <param name="redisOptions">The Redis-specific options.</param>
    /// <param name="logger">The logger.</param>
    public RedisResponseCacheService(
        IConnectionMultiplexer connection,
        IOptions<CacheOptions> options,
        IOptions<RedisCacheOptions> redisOptions,
        ILogger<RedisResponseCacheService>? logger = null)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        _options = options?.Value ?? new CacheOptions();
        _redisOptions = redisOptions?.Value ?? new RedisCacheOptions();
        _logger = logger;
        _database = connection.GetDatabase(_redisOptions.Database);
    }

    /// <inheritdoc/>
    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return default;
        }

        var fullKey = $"{_options.KeyPrefix}{key}";

        try
        {
            var value = await _database.StringGetAsync(fullKey);

            if (value.IsNullOrEmpty)
            {
                _logger?.LogDebug("Cache miss: {Key}", key);
                return default;
            }

            var result = System.Text.Json.JsonSerializer.Deserialize<T>(value!);
            _logger?.LogDebug("Cache hit: {Key}", key);
            return result;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error getting cache key: {Key}", key);
            return default;
        }
    }

    /// <inheritdoc/>
    public async Task SetAsync<T>(
        string key,
        T value,
        int durationSeconds = 300,
        IEnumerable<string>? tags = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return;
        }

        var fullKey = $"{_options.KeyPrefix}{key}";
        var tagList = tags?.ToList() ?? new List<string>();

        try
        {
            var serializedValue = System.Text.Json.JsonSerializer.Serialize(value);
            var expiration = TimeSpan.FromSeconds(durationSeconds);

            await _database.StringSetAsync(fullKey, serializedValue, expiration);

            // Index by tags
            if (tagList.Count > 0)
            {
                await IndexByTagsAsync(fullKey, tagList, expiration);
            }

            _logger?.LogDebug("Cache set: {Key}, Duration: {Duration}s, Tags: {Tags}",
                key, durationSeconds, string.Join(", ", tagList));
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error setting cache key: {Key}", key);
        }
    }

    /// <inheritdoc/>
    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return;
        }

        var fullKey = $"{_options.KeyPrefix}{key}";

        try
        {
            // Get tags before removing
            var tags = await GetTagsForKeyAsync(fullKey);

            await _database.KeyDeleteAsync(fullKey);

            // Remove from tag indexes
            foreach (var tag in tags)
            {
                await _database.SetRemoveAsync($"{TagSetPrefix}{tag}", fullKey);
            }

            _logger?.LogDebug("Cache remove: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error removing cache key: {Key}", key);
        }
    }

    /// <inheritdoc/>
    public async Task InvalidateByTagAsync(string tag, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tag))
        {
            return;
        }

        try
        {
            var tagKey = $"{TagSetPrefix}{tag}";
            var members = await _database.SetMembersAsync(tagKey);

            if (members.Length > 0)
            {
                await _database.KeyDeleteAsync(members);
                await _database.KeyDeleteAsync(tagKey);
            }

            _logger?.LogInformation("Cache invalidated by tag: {Tag}, Keys removed: {Count}", tag, members.Length);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error invalidating cache by tag: {Tag}", tag);
        }
    }

    /// <inheritdoc/>
    public async Task InvalidateByTagsAsync(IEnumerable<string> tags, CancellationToken cancellationToken = default)
    {
        if (tags == null)
        {
            return;
        }

        foreach (var tag in tags)
        {
            await InvalidateByTagAsync(tag, cancellationToken);
        }
    }

    /// <inheritdoc/>
    public async Task ClearAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var endpoints = _connection.GetEndPoints();
            foreach (var endpoint in endpoints)
            {
                var server = _connection.GetServer(endpoint);
                var keys = server.Keys(_database.Database, $"{_options.KeyPrefix}*");

                foreach (var key in keys)
                {
                    await _database.KeyDeleteAsync(key);
                }

                // Also delete tag indexes
                var tagKeys = server.Keys(_database.Database, $"{TagSetPrefix}*");
                foreach (var tagKey in tagKeys)
                {
                    await _database.KeyDeleteAsync(tagKey);
                }
            }

            _logger?.LogInformation("Cache cleared");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error clearing cache");
        }
    }

    /// <inheritdoc/>
    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return false;
        }

        var fullKey = $"{_options.KeyPrefix}{key}";

        try
        {
            return await _database.KeyExistsAsync(fullKey);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error checking cache key existence: {Key}", key);
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<T> GetOrSetAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        int durationSeconds = 300,
        IEnumerable<string>? tags = null,
        CancellationToken cancellationToken = default)
    {
        var cached = await GetAsync<T>(key, cancellationToken);

        if (cached != null)
        {
            return cached;
        }

        var value = await factory(cancellationToken);
        await SetAsync(key, value, durationSeconds, tags, cancellationToken);

        return value;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<string>> GetKeysByTagAsync(string tag, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tag))
        {
            return Enumerable.Empty<string>();
        }

        try
        {
            var tagKey = $"{TagSetPrefix}{tag}";
            var members = await _database.SetMembersAsync(tagKey);
            return members.Select(m => m.ToString()!).Where(k => k != null);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error getting keys by tag: {Tag}", tag);
            return Enumerable.Empty<string>();
        }
    }

    /// <inheritdoc/>
    public async Task RefreshAsync(string key, int durationSeconds, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return;
        }

        var fullKey = $"{_options.KeyPrefix}{key}";

        try
        {
            await _database.KeyExpireAsync(fullKey, TimeSpan.FromSeconds(durationSeconds));
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error refreshing cache key: {Key}", key);
        }
    }

    private async Task IndexByTagsAsync(string key, IEnumerable<string> tags, TimeSpan expiration)
    {
        foreach (var tag in tags)
        {
            var tagKey = $"{TagSetPrefix}{tag}";
            await _database.SetAddAsync(tagKey, key);
            await _database.KeyExpireAsync(tagKey, expiration);
        }
    }

    private async Task<IEnumerable<string>> GetTagsForKeyAsync(string key)
    {
        var tags = new List<string>();

        try
        {
            var endpoints = _connection.GetEndPoints();
            foreach (var endpoint in endpoints)
            {
                var server = _connection.GetServer(endpoint);
                var allTagKeys = server.Keys(_database.Database, $"{TagSetPrefix}*`");

                foreach (var tagKey in allTagKeys)
                {
                    if (await _database.SetMemberIsMemberAsync(tagKey, key))
                    {
                        var tagName = tagKey.ToString()!.Replace(TagSetPrefix, "");
                        tags.Add(tagName);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error getting tags for key: {Key}", key);
        }

        return tags;
    }
}
