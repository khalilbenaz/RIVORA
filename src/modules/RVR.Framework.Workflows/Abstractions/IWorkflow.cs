namespace RVR.Framework.Workflows.Abstractions;

using RVR.Framework.Workflows.Models;

/// <summary>
/// Defines a workflow (state machine) for a given state type.
/// </summary>
/// <typeparam name="TState">The type representing workflow states (typically an enum).</typeparam>
public interface IWorkflow<TState> where TState : notnull
{
    /// <summary>
    /// Gets the unique name of this workflow definition.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets all valid states in this workflow.
    /// </summary>
    IReadOnlyList<TState> States { get; }

    /// <summary>
    /// Gets all valid transitions in this workflow.
    /// </summary>
    IReadOnlyList<WorkflowTransition<TState>> Transitions { get; }

    /// <summary>
    /// Gets the initial state of the workflow.
    /// </summary>
    TState InitialState { get; }
}
