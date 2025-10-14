namespace RVR.Framework.EventSourcing.Abstractions;

using RVR.Framework.EventSourcing.Models;

/// <summary>
/// Abstraction for persisting and retrieving event streams.
/// </summary>
public interface IEventStore
{
    /// <summary>
    /// Appends one or more events to an event stream.
    /// </summary>
    /// <param name="streamId">The identifier of the event stream (typically the aggregate ID).</param>
    /// <param name="events">The events to append.</param>
    /// <param name="expectedVersion">The expected current version for optimistic concurrency. Use -1 for new streams.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task AppendAsync(string streamId, IEnumerable<IDomainEvent> events, int expectedVersion, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads all events for a given stream.
    /// </summary>
    /// <param name="streamId">The identifier of the event stream.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The ordered list of event store entries.</returns>
    Task<IReadOnlyList<EventStoreEntry>> LoadAsync(string streamId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads events for a given stream starting from a specific version.
    /// </summary>
    /// <param name="streamId">The identifier of the event stream.</param>
    /// <param name="fromVersion">The version to start loading from (inclusive).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The ordered list of event store entries from the specified version.</returns>
    Task<IReadOnlyList<EventStoreEntry>> LoadAsync(string streamId, int fromVersion, CancellationToken cancellationToken = default);
}
