namespace RVR.Framework.Saga.Abstractions;

using RVR.Framework.Saga.Models;

/// <summary>
/// Defines a saga (process manager) that coordinates a long-running business process.
/// </summary>
/// <typeparam name="TData">The type of data the saga tracks during its lifecycle.</typeparam>
public interface ISaga<TData> where TData : class, new()
{
    /// <summary>
    /// Handles an event within this saga, potentially transitioning state.
    /// </summary>
    /// <typeparam name="TEvent">The type of saga event to handle.</typeparam>
    /// <param name="state">The current saga state.</param>
    /// <param name="event">The event to handle.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated saga state.</returns>
    Task<SagaState<TData>> HandleAsync<TEvent>(SagaState<TData> state, TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : ISagaEvent;

    /// <summary>
    /// Determines whether this saga can handle the given event type.
    /// </summary>
    /// <param name="eventType">The event type to check.</param>
    /// <returns>True if this saga handles the event type.</returns>
    bool CanHandle(Type eventType);

    /// <summary>
    /// Gets the name of this saga for identification and logging purposes.
    /// </summary>
    string SagaName { get; }
}
