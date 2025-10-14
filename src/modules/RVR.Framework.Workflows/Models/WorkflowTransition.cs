namespace RVR.Framework.Workflows.Models;

/// <summary>
/// Represents a transition between two states in a workflow, triggered by a named event.
/// </summary>
/// <typeparam name="TState">The type representing workflow states.</typeparam>
public class WorkflowTransition<TState> where TState : notnull
{
    /// <summary>
    /// Gets or sets the state from which the transition originates.
    /// </summary>
    public TState FromState { get; set; } = default!;

    /// <summary>
    /// Gets or sets the state to which the transition leads.
    /// </summary>
    public TState ToState { get; set; } = default!;

    /// <summary>
    /// Gets or sets the trigger name that initiates this transition.
    /// </summary>
    public string Trigger { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets an optional guard function. The transition only proceeds
    /// when this function returns <c>true</c>. When <c>null</c>, the transition
    /// is always allowed.
    /// </summary>
    public Func<bool>? Guard { get; set; }

    /// <summary>
    /// Gets or sets an optional action to execute when the transition occurs.
    /// </summary>
    public Func<Task>? Action { get; set; }
}
