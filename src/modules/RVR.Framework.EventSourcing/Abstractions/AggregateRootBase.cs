namespace RVR.Framework.EventSourcing.Abstractions;

/// <summary>
/// Base class for event-sourced aggregate roots providing common plumbing.
/// Derived classes should implement When(IDomainEvent) to handle specific event types.
/// </summary>
public abstract class AggregateRootBase : IAggregateRoot
{
    private readonly List<IDomainEvent> _uncommittedEvents = [];

    /// <inheritdoc />
    public Guid Id { get; protected set; }

    /// <inheritdoc />
    public int Version { get; private set; }

    /// <inheritdoc />
    public void Apply(IDomainEvent domainEvent)
    {
        When(domainEvent);
        Version++;
        _uncommittedEvents.Add(domainEvent);
    }

    /// <inheritdoc />
    public void LoadFromHistory(IEnumerable<IDomainEvent> history)
    {
        foreach (var domainEvent in history)
        {
            When(domainEvent);
            Version++;
        }
    }

    /// <inheritdoc />
    public IReadOnlyList<IDomainEvent> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();

    /// <inheritdoc />
    public void ClearUncommittedEvents() => _uncommittedEvents.Clear();

    /// <summary>
    /// Handles a specific domain event by mutating the aggregate state.
    /// Derived classes must implement this to dispatch events to appropriate handlers.
    /// </summary>
    /// <param name="domainEvent">The domain event to handle.</param>
    protected abstract void When(IDomainEvent domainEvent);
}
