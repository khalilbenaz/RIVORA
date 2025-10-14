namespace RVR.Framework.Caching.Services;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RVR.Framework.Caching.Interfaces;
using RVR.Framework.Caching.Models;
using RVR.Framework.Core.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

/// <summary>
/// In-memory implementation of IResponseCacheService.
/// </summary>
public class MemoryResponseCacheService : IResponseCacheService
{
    private readonly ConcurrentDictionary<string, object> _cache = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, HashSet<string>> _tagIndex = new(StringComparer.OrdinalIgnoreCase);
    private readonly CacheOptions _options;
    private readonly ILogger<MemoryResponseCacheService>? _logger;
    private CacheStatistics _statistics;

    /// <summary>
    /// Initializes a new instance of the <see cref="MemoryResponseCacheService"/> class.
    /// </summary>
    /// <param name="options">The cache options.</param>
    /// <param name="logger">The logger.</param>
    public MemoryResponseCacheService(
        IOptions<CacheOptions> options,
        ILogger<MemoryResponseCacheService>? logger = null)
    {
        _options = options?.Value ?? new CacheOptions();
        _logger = logger;
        _statistics = new CacheStatistics();
    }

    /// <inheritdoc/>
    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return Task.FromResult<T?>(default);
        }

        var fullKey = $"{_options.KeyPrefix}{key}";

        if (_cache.TryGetValue(fullKey, out var entryObj) && entryObj is CacheEntry<T> entry)
        {
            if (entry.IsExpired)
            {
                _cache.TryRemove(fullKey, out _);
                RemoveFromTagIndex(fullKey, entry.Tags);
                _statistics.Misses++;
                _logger?.LogDebug("Cache miss (expired): {Key}", LogSanitizer.Sanitize(key));
                return Task.FromResult<T?>(default);
            }

            entry.LastAccessedAt = DateTime.UtcNow;
            entry.AccessCount++;
            _statistics.Hits++;
            _logger?.LogDebug("Cache hit: {Key}", LogSanitizer.Sanitize(key));
            return Task.FromResult(entry.Value);
        }

        _statistics.Misses++;
        _logger?.LogDebug("Cache miss: {Key}", LogSanitizer.Sanitize(key));
        return Task.FromResult<T?>(default);
    }

    /// <inheritdoc/>
    public Task SetAsync<T>(
        string key,
        T value,
        int durationSeconds = 300,
        IEnumerable<string>? tags = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return Task.CompletedTask;
        }

        var fullKey = $"{_options.KeyPrefix}{key}";
        var tagList = tags?.ToList() ?? new List<string>();

        var entry = new CacheEntry<T>
        {
            Value = value,
            AbsoluteExpiration = DateTime.UtcNow.AddSeconds(durationSeconds),
            Tags = new HashSet<string>(tagList, StringComparer.OrdinalIgnoreCase),
            CreatedAt = DateTime.UtcNow,
            SizeBytes = EstimateSize(value)
        };

        _cache[fullKey] = entry;
        AddToTagIndex(fullKey, tagList);

        _logger?.LogDebug("Cache set: {Key}, Duration: {Duration}s, Tags: {Tags}",
            LogSanitizer.Sanitize(key), durationSeconds, LogSanitizer.Sanitize(string.Join(", ", tagList)));

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return Task.CompletedTask;
        }

        var fullKey = $"{_options.KeyPrefix}{key}";

        if (_cache.TryRemove(fullKey, out var entryObj) && entryObj is ICacheEntryWithTags entryWithTags)
        {
            RemoveFromTagIndex(fullKey, entryWithTags.GetTags());
            _logger?.LogDebug("Cache remove: {Key}", key);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public async Task InvalidateByTagAsync(string tag, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tag))
        {
            return;
        }

        var keys = await GetKeysByTagAsync(tag, cancellationToken);
        var keysList = keys.ToList();

        foreach (var key in keysList)
        {
            if (_cache.TryRemove(key, out var entryObj) && entryObj is ICacheEntryWithTags entryWithTags)
            {
                entryWithTags.GetTags().Remove(tag);
            }
        }

        _tagIndex.TryRemove(tag, out _);
        _statistics.TagInvalidations++;

        _logger?.LogInformation("Cache invalidated by tag: {Tag}, Keys removed: {Count}", tag, keysList.Count);
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
    public Task ClearAsync(CancellationToken cancellationToken = default)
    {
        _cache.Clear();
        _tagIndex.Clear();
        _statistics = new CacheStatistics { TagInvalidations = _statistics.TagInvalidations };

        _logger?.LogInformation("Cache cleared");
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return Task.FromResult(false);
        }

        var fullKey = $"{_options.KeyPrefix}{key}";
        var exists = _cache.ContainsKey(fullKey);

        return Task.FromResult(exists);
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
    public Task<IEnumerable<string>> GetKeysByTagAsync(string tag, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tag))
        {
            return Task.FromResult(Enumerable.Empty<string>());
        }

        if (_tagIndex.TryGetValue(tag, out var keys))
        {
            return Task.FromResult<IEnumerable<string>>(keys.ToList());
        }

        return Task.FromResult(Enumerable.Empty<string>());
    }

    /// <inheritdoc/>
    public Task RefreshAsync(string key, int durationSeconds, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return Task.CompletedTask;
        }

        var fullKey = $"{_options.KeyPrefix}{key}";

        if (_cache.TryGetValue(fullKey, out var entryObj))
        {
            if (entryObj is CacheEntry<object> objectEntry)
            {
                objectEntry.AbsoluteExpiration = DateTime.UtcNow.AddSeconds(durationSeconds);
            }
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Gets the cache statistics.
    /// </summary>
    /// <returns>The cache statistics.</returns>
    public CacheStatistics GetStatistics()
    {
        _statistics.TotalEntries = _cache.Count;
        _statistics.TotalSizeBytes = _cache.Values.OfType<ICacheEntryWithSize>().Sum(e => e.GetSize());
        return _statistics;
    }

    private void AddToTagIndex(string key, IEnumerable<string> tags)
    {
        foreach (var tag in tags)
        {
            _tagIndex.AddOrUpdate(
                tag,
                new HashSet<string> { key },
                (_, existing) =>
                {
                    existing.Add(key);
                    return existing;
                });
        }
    }

    private void RemoveFromTagIndex(string key, IEnumerable<string> tags)
    {
        foreach (var tag in tags)
        {
            if (_tagIndex.TryGetValue(tag, out var keys))
            {
                keys.Remove(key);
                if (keys.Count == 0)
                {
                    _tagIndex.TryRemove(tag, out _);
                }
            }
        }
    }

    private long EstimateSize<T>(T value)
    {
        // Rough size estimation
        if (value == null) return 0;

        var serialized = System.Text.Json.JsonSerializer.Serialize(value);
        return System.Text.Encoding.UTF8.GetByteCount(serialized);
    }
}

/// <summary>
/// Internal interface for accessing cache entry tags.
/// </summary>
internal interface ICacheEntryWithTags
{
    HashSet<string> GetTags();
}

/// <summary>
/// Internal interface for accessing cache entry size.
/// </summary>
internal interface ICacheEntryWithSize
{
    long GetSize();
}
