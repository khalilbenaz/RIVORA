namespace RVR.Framework.Workflows.Engine;

using Microsoft.Extensions.Logging;
using RVR.Framework.Workflows.Abstractions;
using RVR.Framework.Workflows.Models;

/// <summary>
/// Core workflow engine that manages workflow instances, applies transitions,
/// and persists state via <see cref="IWorkflowStore"/>.
/// </summary>
public class WorkflowEngine
{
    private readonly IWorkflowStore _store;
    private readonly ILogger<WorkflowEngine> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="WorkflowEngine"/> class.
    /// </summary>
    /// <param name="store">The workflow persistence store.</param>
    /// <param name="logger">The logger instance.</param>
    public WorkflowEngine(IWorkflowStore store, ILogger<WorkflowEngine> logger)
    {
        _store = store;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new workflow instance from a workflow definition.
    /// </summary>
    /// <typeparam name="TState">The state type.</typeparam>
    /// <param name="workflow">The workflow definition.</param>
    /// <param name="data">Optional initial data for the instance.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The newly created workflow instance.</returns>
    public async Task<WorkflowInstance<TState>> CreateInstanceAsync<TState>(
        IWorkflow<TState> workflow,
        Dictionary<string, object>? data = null,
        CancellationToken cancellationToken = default)
        where TState : notnull
    {
        var instance = new WorkflowInstance<TState>
        {
            DefinitionName = workflow.Name,
            CurrentState = workflow.InitialState,
            Data = data ?? []
        };

        _logger.LogInformation(
            "Created workflow instance {InstanceId} for definition '{DefinitionName}' with initial state {InitialState}.",
            instance.Id, workflow.Name, workflow.InitialState);

        await _store.SaveAsync(instance, cancellationToken);
        return instance;
    }

    /// <summary>
    /// Fires a trigger on a workflow instance, transitioning it to a new state
    /// if a valid transition exists and the guard (if any) allows it.
    /// </summary>
    /// <typeparam name="TState">The state type.</typeparam>
    /// <param name="workflow">The workflow definition.</param>
    /// <param name="instanceId">The instance identifier.</param>
    /// <param name="trigger">The trigger name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated workflow instance.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no instance is found, no matching transition exists, or the guard rejects the transition.
    /// </exception>
    public async Task<WorkflowInstance<TState>> TriggerAsync<TState>(
        IWorkflow<TState> workflow,
        Guid instanceId,
        string trigger,
        CancellationToken cancellationToken = default)
        where TState : notnull
    {
        var instance = await _store.GetAsync<TState>(instanceId, cancellationToken)
                       ?? throw new InvalidOperationException(
                           $"Workflow instance '{instanceId}' not found.");

        var transition = workflow.Transitions
            .FirstOrDefault(t =>
                EqualityComparer<TState>.Default.Equals(t.FromState, instance.CurrentState) &&
                string.Equals(t.Trigger, trigger, StringComparison.OrdinalIgnoreCase));

        if (transition is null)
        {
            throw new InvalidOperationException(
                $"No transition found from state '{instance.CurrentState}' with trigger '{trigger}' " +
                $"in workflow '{workflow.Name}'.");
        }

        if (transition.Guard is not null && !transition.Guard())
        {
            throw new InvalidOperationException(
                $"Guard rejected transition from '{transition.FromState}' to '{transition.ToState}' " +
                $"via trigger '{trigger}' in workflow '{workflow.Name}'.");
        }

        // Execute transition action if present
        if (transition.Action is not null)
        {
            await transition.Action();
        }

        var previousState = instance.CurrentState;
        instance.CurrentState = transition.ToState;
        instance.LastModifiedAt = DateTimeOffset.UtcNow;

        instance.History.Add(new WorkflowHistoryEntry<TState>
        {
            FromState = previousState,
            ToState = transition.ToState,
            Trigger = trigger,
            Timestamp = DateTimeOffset.UtcNow
        });

        _logger.LogInformation(
            "Workflow instance {InstanceId} transitioned from {FromState} to {ToState} via trigger '{Trigger}'.",
            instanceId, previousState, transition.ToState, trigger);

        await _store.SaveAsync(instance, cancellationToken);
        return instance;
    }

    /// <summary>
    /// Retrieves a workflow instance by its identifier.
    /// </summary>
    /// <typeparam name="TState">The state type.</typeparam>
    /// <param name="instanceId">The instance identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The workflow instance, or <c>null</c> if not found.</returns>
    public Task<WorkflowInstance<TState>?> GetInstanceAsync<TState>(
        Guid instanceId,
        CancellationToken cancellationToken = default)
        where TState : notnull
    {
        return _store.GetAsync<TState>(instanceId, cancellationToken);
    }

    /// <summary>
    /// Gets the transition history for a workflow instance.
    /// </summary>
    /// <typeparam name="TState">The state type.</typeparam>
    /// <param name="instanceId">The instance identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The list of history entries.</returns>
    public Task<IReadOnlyList<WorkflowHistoryEntry<TState>>> GetHistoryAsync<TState>(
        Guid instanceId,
        CancellationToken cancellationToken = default)
        where TState : notnull
    {
        return _store.GetHistoryAsync<TState>(instanceId, cancellationToken);
    }
}
