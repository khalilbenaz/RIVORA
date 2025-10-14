namespace RVR.Framework.EventSourcing.Stores;

using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using RVR.Framework.EventSourcing.Abstractions;
using RVR.Framework.EventSourcing.Models;

/// <summary>
/// In-memory implementation of <see cref="IEventStore"/> using ConcurrentDictionary.
/// Suitable for testing and development scenarios.
/// </summary>
public class InMemoryEventStore : IEventStore
{
    private readonly ConcurrentDictionary<string, List<EventStoreEntry>> _streams = new();
    private readonly ILogger<InMemoryEventStore> _logger;
    private readonly object _lock = new();

    /// <summary>
    /// Initializes a new instance of <see cref="InMemoryEventStore"/>.
    /// </summary>
    /// <param name="logger">Logger instance.</param>
    public InMemoryEventStore(ILogger<InMemoryEventStore> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public Task AppendAsync(string streamId, IEnumerable<IDomainEvent> events, int expectedVersion, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var stream = _streams.GetOrAdd(streamId, _ => []);
            var currentVersion = stream.Count;

            if (expectedVersion >= 0 && currentVersion != expectedVersion)
            {
                throw new InvalidOperationException(
                    $"Concurrency conflict on stream '{streamId}'. Expected version {expectedVersion}, but current version is {currentVersion}.");
            }

            foreach (var domainEvent in events)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var entry = new EventStoreEntry
                {
                    StreamId = streamId,
                    EventType = domainEvent.GetType().AssemblyQualifiedName ?? domainEvent.GetType().FullName ?? domainEvent.GetType().Name,
                    Data = JsonSerializer.Serialize(domainEvent, domainEvent.GetType()),
                    Version = currentVersion + 1,
                    Timestamp = DateTime.UtcNow
                };

                stream.Add(entry);
                currentVersion++;
            }

            _logger.LogDebug("Appended {Count} event(s) to stream '{StreamId}'. New version: {Version}",
                stream.Count - (currentVersion - stream.Count), streamId, currentVersion);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<EventStoreEntry>> LoadAsync(string streamId, CancellationToken cancellationToken = default)
    {
        if (_streams.TryGetValue(streamId, out var stream))
        {
            return Task.FromResult<IReadOnlyList<EventStoreEntry>>(stream.AsReadOnly());
        }

        return Task.FromResult<IReadOnlyList<EventStoreEntry>>(Array.Empty<EventStoreEntry>());
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<EventStoreEntry>> LoadAsync(string streamId, int fromVersion, CancellationToken cancellationToken = default)
    {
        if (_streams.TryGetValue(streamId, out var stream))
        {
            var filtered = stream.Where(e => e.Version >= fromVersion).ToList();
            return Task.FromResult<IReadOnlyList<EventStoreEntry>>(filtered.AsReadOnly());
        }

        return Task.FromResult<IReadOnlyList<EventStoreEntry>>(Array.Empty<EventStoreEntry>());
    }
}
