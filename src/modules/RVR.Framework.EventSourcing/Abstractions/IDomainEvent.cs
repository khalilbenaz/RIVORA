namespace RVR.Framework.EventSourcing.Abstractions;

/// <summary>
/// Marker interface for domain events used in event sourcing.
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// Gets the unique identifier of this event occurrence.
    /// </summary>
    Guid EventId { get; }

    /// <summary>
    /// Gets the UTC timestamp when this event occurred.
    /// </summary>
    DateTime OccurredAt { get; }
}
