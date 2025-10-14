namespace RVR.Framework.Core;

/// <summary>
/// Abstract base class for aggregate roots.
/// Inherits from Entity and implements IAggregateRoot.
/// </summary>
/// <remarks>
/// Aggregate roots are the entry points for operations on an aggregate.
/// They maintain the consistency of the aggregate by encapsulating business logic.
/// </remarks>
public abstract class AggregateRoot : Entity, IAggregateRoot
{
    /// <summary>
    /// Gets the current version of the aggregate for optimistic concurrency.
    /// </summary>
    public int Version { get; protected set; } = 1;

    /// <summary>
    /// Marks the aggregate as deleted.
    /// </summary>
    public bool IsDeleted { get; protected set; }

    /// <summary>
    /// Applies a domain event to the aggregate.
    /// </summary>
    /// <param name="domainEvent">The domain event to apply.</param>
    protected void Apply(IDomainEvent domainEvent)
    {
        RaiseDomainEvent(domainEvent);
    }

    /// <summary>
    /// Marks the aggregate for soft deletion.
    /// </summary>
    public void MarkAsDeleted()
    {
        IsDeleted = true;
        SetModified(null);
    }

    /// <summary>
    /// Increments the version for optimistic concurrency control.
    /// </summary>
    public void IncrementVersion()
    {
        Version++;
    }
}
