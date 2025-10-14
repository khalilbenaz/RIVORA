namespace RVR.Framework.Saga.Abstractions;

/// <summary>
/// Marker interface for events that can be handled by sagas.
/// </summary>
public interface ISagaEvent
{
    /// <summary>
    /// Gets the correlation identifier used to route the event to the correct saga instance.
    /// </summary>
    Guid CorrelationId { get; }
}
