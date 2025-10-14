namespace RVR.Framework.Workflows.Models;

/// <summary>
/// Records a single state transition in a workflow instance's history.
/// </summary>
/// <typeparam name="TState">The type representing workflow states.</typeparam>
public class WorkflowHistoryEntry<TState> where TState : notnull
{
    /// <summary>
    /// Gets or sets the state before the transition.
    /// </summary>
    public TState FromState { get; set; } = default!;

    /// <summary>
    /// Gets or sets the state after the transition.
    /// </summary>
    public TState ToState { get; set; } = default!;

    /// <summary>
    /// Gets or sets the trigger that caused the transition.
    /// </summary>
    public string Trigger { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the timestamp when the transition occurred.
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
}
