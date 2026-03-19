namespace RVR.Framework.AI.Agents;

/// <summary>
/// Core abstraction for an AI agent that can process requests and produce responses.
/// </summary>
public interface IAgent
{
    /// <summary>
    /// Gets the unique name of this agent.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets a human-readable description of what this agent does.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Executes the agent with the given request.
    /// </summary>
    /// <param name="request">The agent request containing input and context.</param>
    /// <param name="ct">A cancellation token to cancel the operation.</param>
    /// <returns>The agent response containing output, status, and execution steps.</returns>
    Task<AgentResponse> ExecuteAsync(AgentRequest request, CancellationToken ct = default);
}

/// <summary>
/// Represents a request to an agent, containing the input prompt and optional context.
/// </summary>
/// <param name="Input">The primary input text for the agent.</param>
/// <param name="Context">Optional context carrying memory and history across agent invocations.</param>
/// <param name="Parameters">Optional additional parameters for the agent.</param>
public sealed record AgentRequest(
    string Input,
    AgentContext? Context = null,
    Dictionary<string, object>? Parameters = null);

/// <summary>
/// Represents the response from an agent execution.
/// </summary>
/// <param name="Output">The generated output text.</param>
/// <param name="Success">Indicates whether the agent executed successfully.</param>
/// <param name="Context">The updated context after execution.</param>
/// <param name="Steps">The list of execution steps taken during processing.</param>
public sealed record AgentResponse(
    string Output,
    bool Success,
    AgentContext Context,
    List<AgentStep> Steps);

/// <summary>
/// Represents a single execution step within an agent pipeline.
/// </summary>
/// <param name="AgentName">The name of the agent that performed this step.</param>
/// <param name="Input">The input provided to this step.</param>
/// <param name="Output">The output produced by this step.</param>
/// <param name="Duration">The wall-clock duration of this step.</param>
/// <param name="Success">Indicates whether this step completed successfully.</param>
public sealed record AgentStep(
    string AgentName,
    string Input,
    string Output,
    TimeSpan Duration,
    bool Success);

/// <summary>
/// Carries contextual state across agent invocations, including persistent memory and conversation history.
/// </summary>
/// <param name="Memory">A key-value store for persistent data shared between agents.</param>
/// <param name="History">An ordered list of previous interactions or observations.</param>
public sealed record AgentContext(
    Dictionary<string, object> Memory,
    List<string> History);
