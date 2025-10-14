namespace RVR.Framework.EventSourcing.Abstractions;

/// <summary>
/// Represents an event-sourced aggregate root that reconstructs state from domain events.
/// </summary>
public interface IAggregateRoot
{
    /// <summary>
    /// Gets the unique identifier of this aggregate.
    /// </summary>
    Guid Id { get; }

    /// <summary>
    /// Gets the current version of the aggregate (number of events applied).
    /// </summary>
    int Version { get; }

    /// <summary>
    /// Applies a domain event to this aggregate, updating its state.
    /// </summary>
    /// <param name="domainEvent">The domain event to apply.</param>
    void Apply(IDomainEvent domainEvent);

    /// <summary>
    /// Reconstructs the aggregate state from a history of domain events.
    /// </summary>
    /// <param name="history">The ordered sequence of past domain events.</param>
    void LoadFromHistory(IEnumerable<IDomainEvent> history);

    /// <summary>
    /// Returns all uncommitted events that have been applied since the last save.
    /// </summary>
    /// <returns>A read-only list of uncommitted domain events.</returns>
    IReadOnlyList<IDomainEvent> GetUncommittedEvents();

    /// <summary>
    /// Clears the list of uncommitted events (typically called after persisting).
    /// </summary>
    void ClearUncommittedEvents();
}
