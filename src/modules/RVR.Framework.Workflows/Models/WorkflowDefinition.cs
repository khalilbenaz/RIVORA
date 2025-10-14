namespace RVR.Framework.Workflows.Models;

/// <summary>
/// Defines the structure of a workflow including its states and transitions.
/// </summary>
/// <typeparam name="TState">The type representing workflow states.</typeparam>
public class WorkflowDefinition<TState> where TState : notnull
{
    /// <summary>
    /// Gets or sets the unique name of this workflow definition.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets all valid states in this workflow.
    /// </summary>
    public List<TState> States { get; set; } = [];

    /// <summary>
    /// Gets or sets all valid transitions in this workflow.
    /// </summary>
    public List<WorkflowTransition<TState>> Transitions { get; set; } = [];

    /// <summary>
    /// Gets or sets the initial state of the workflow.
    /// </summary>
    public TState InitialState { get; set; } = default!;
}
