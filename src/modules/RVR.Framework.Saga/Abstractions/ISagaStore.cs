namespace RVR.Framework.Saga.Abstractions;

using RVR.Framework.Saga.Models;

/// <summary>
/// Abstraction for persisting and retrieving saga state.
/// </summary>
public interface ISagaStore
{
    /// <summary>
    /// Saves or updates the state of a saga instance.
    /// </summary>
    /// <typeparam name="TData">The saga data type.</typeparam>
    /// <param name="state">The saga state to save.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SaveAsync<TData>(SagaState<TData> state, CancellationToken cancellationToken = default) where TData : class, new();

    /// <summary>
    /// Loads the state of a saga instance by its identifier.
    /// </summary>
    /// <typeparam name="TData">The saga data type.</typeparam>
    /// <param name="sagaId">The unique identifier of the saga instance.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The saga state, or null if not found.</returns>
    Task<SagaState<TData>?> LoadAsync<TData>(Guid sagaId, CancellationToken cancellationToken = default) where TData : class, new();

    /// <summary>
    /// Marks a saga as completed.
    /// </summary>
    /// <typeparam name="TData">The saga data type.</typeparam>
    /// <param name="sagaId">The unique identifier of the saga instance.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task MarkCompletedAsync<TData>(Guid sagaId, CancellationToken cancellationToken = default) where TData : class, new();

    /// <summary>
    /// Marks a saga as failed with an error message.
    /// </summary>
    /// <typeparam name="TData">The saga data type.</typeparam>
    /// <param name="sagaId">The unique identifier of the saga instance.</param>
    /// <param name="errorMessage">The error message describing the failure.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task MarkFailedAsync<TData>(Guid sagaId, string errorMessage, CancellationToken cancellationToken = default) where TData : class, new();
}
