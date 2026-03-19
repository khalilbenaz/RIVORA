using System.Collections.Concurrent;

namespace RVR.Framework.Webhooks.Incoming;

/// <summary>
/// Thread-safe in-memory store for incoming webhook configurations and logs.
/// Keeps a maximum of 1000 log entries, evicting the oldest when the limit is exceeded.
/// </summary>
public class InMemoryIncomingWebhookStore
{
    private const int MaxLogs = 1000;

    private readonly ConcurrentDictionary<string, IncomingWebhookConfig> _configs = new();
    private readonly ConcurrentQueue<IncomingWebhookLog> _logQueue = new();
    private readonly ConcurrentDictionary<string, IncomingWebhookLog> _logIndex = new();
    private readonly object _evictionLock = new();

    /// <summary>
    /// Adds or updates a webhook configuration.
    /// </summary>
    public void AddConfig(IncomingWebhookConfig config)
    {
        _configs[config.Id] = config;
    }

    /// <summary>
    /// Returns all webhook configurations.
    /// </summary>
    public IReadOnlyList<IncomingWebhookConfig> GetConfigs()
    {
        return _configs.Values
            .OrderByDescending(c => c.CreatedAt)
            .ToList();
    }

    /// <summary>
    /// Returns a webhook configuration by its identifier, or null if not found.
    /// </summary>
    public IncomingWebhookConfig? GetConfigById(string id)
    {
        return _configs.TryGetValue(id, out var config) ? config : null;
    }

    /// <summary>
    /// Returns the first active configuration matching the given source name, or null if not found.
    /// </summary>
    public IncomingWebhookConfig? GetConfigBySource(string source)
    {
        return _configs.Values
            .FirstOrDefault(c => c.IsActive &&
                c.Source.Equals(source, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Removes a webhook configuration by its identifier.
    /// </summary>
    public bool RemoveConfig(string id)
    {
        return _configs.TryRemove(id, out _);
    }

    /// <summary>
    /// Adds a log entry. If the store exceeds the maximum number of logs, the oldest entry is evicted.
    /// </summary>
    public void AddLog(IncomingWebhookLog log)
    {
        _logIndex[log.Id] = log;
        _logQueue.Enqueue(log);

        EvictOldLogs();
    }

    /// <summary>
    /// Returns logs for a specific config, ordered by most recent first.
    /// </summary>
    public IReadOnlyList<IncomingWebhookLog> GetLogs(string configId, int limit = 50)
    {
        return _logIndex.Values
            .Where(l => l.ConfigId == configId)
            .OrderByDescending(l => l.ReceivedAt)
            .Take(limit)
            .ToList();
    }

    /// <summary>
    /// Returns a log entry by its identifier, or null if not found.
    /// </summary>
    public IncomingWebhookLog? GetLogById(string id)
    {
        return _logIndex.TryGetValue(id, out var log) ? log : null;
    }

    /// <summary>
    /// Returns all logs ordered by most recent first, up to the specified limit.
    /// </summary>
    public IReadOnlyList<IncomingWebhookLog> GetAllLogs(int limit = 50)
    {
        return _logIndex.Values
            .OrderByDescending(l => l.ReceivedAt)
            .Take(limit)
            .ToList();
    }

    private void EvictOldLogs()
    {
        if (_logIndex.Count <= MaxLogs)
            return;

        lock (_evictionLock)
        {
            while (_logIndex.Count > MaxLogs && _logQueue.TryDequeue(out var oldest))
            {
                _logIndex.TryRemove(oldest.Id, out _);
            }
        }
    }
}
