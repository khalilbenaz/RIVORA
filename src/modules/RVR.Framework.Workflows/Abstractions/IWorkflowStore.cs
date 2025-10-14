namespace RVR.Framework.Workflows.Abstractions;

using RVR.Framework.Workflows.Models;

/// <summary>
/// Persistence abstraction for workflow instances.
/// </summary>
public interface IWorkflowStore
{
    /// <summary>
    /// Saves a workflow instance (create or update).
    /// </summary>
    /// <typeparam name="TState">The state type.</typeparam>
    /// <param name="instance">The workflow instance to save.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SaveAsync<TState>(WorkflowInstance<TState> instance, CancellationToken cancellationToken = default)
        where TState : notnull;

    /// <summary>
    /// Retrieves a workflow instance by its identifier.
    /// </summary>
    /// <typeparam name="TState">The state type.</typeparam>
    /// <param name="instanceId">The instance identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The workflow instance, or <c>null</c> if not found.</returns>
    Task<WorkflowInstance<TState>?> GetAsync<TState>(Guid instanceId, CancellationToken cancellationToken = default)
        where TState : notnull;

    /// <summary>
    /// Gets the transition history for a workflow instance.
    /// </summary>
    /// <typeparam name="TState">The state type.</typeparam>
    /// <param name="instanceId">The instance identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The list of history entries.</returns>
    Task<IReadOnlyList<WorkflowHistoryEntry<TState>>> GetHistoryAsync<TState>(
        Guid instanceId,
        CancellationToken cancellationToken = default)
        where TState : notnull;
}
