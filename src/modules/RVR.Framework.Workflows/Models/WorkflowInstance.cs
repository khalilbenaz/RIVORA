namespace RVR.Framework.Workflows.Models;

/// <summary>
/// Represents a running instance of a workflow, including its current state,
/// associated data, and transition history.
/// </summary>
/// <typeparam name="TState">The type representing workflow states.</typeparam>
public class WorkflowInstance<TState> where TState : notnull
{
    /// <summary>
    /// Gets or sets the unique identifier of this workflow instance.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the name of the workflow definition this instance belongs to.
    /// </summary>
    public string DefinitionName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the current state of the workflow instance.
    /// </summary>
    public TState CurrentState { get; set; } = default!;

    /// <summary>
    /// Gets or sets arbitrary data associated with this workflow instance.
    /// </summary>
    public Dictionary<string, object> Data { get; set; } = [];

    /// <summary>
    /// Gets or sets the history of state transitions for this workflow instance.
    /// </summary>
    public List<WorkflowHistoryEntry<TState>> History { get; set; } = [];

    /// <summary>
    /// Gets or sets the timestamp when this instance was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets or sets the timestamp of the last state change.
    /// </summary>
    public DateTimeOffset LastModifiedAt { get; set; } = DateTimeOffset.UtcNow;
}
