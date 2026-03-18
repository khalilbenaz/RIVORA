namespace RVR.Framework.AI.Agents;

/// <summary>
/// Abstraction for a tool that an AI agent can invoke during function calling.
/// </summary>
public interface ITool
{
    /// <summary>
    /// Gets the unique name of this tool.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets a human-readable description of the tool for the LLM.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Gets the schema describing the tool's parameters.
    /// </summary>
    ToolSchema Schema { get; }

    /// <summary>
    /// Executes the tool with the supplied parameters.
    /// </summary>
    /// <param name="parameters">The parameter values keyed by parameter name.</param>
    /// <param name="ct">A cancellation token to cancel the operation.</param>
    /// <returns>The result of the tool execution.</returns>
    Task<ToolResult> ExecuteAsync(Dictionary<string, object> parameters, CancellationToken ct = default);
}

/// <summary>
/// Describes the schema of a tool, including its parameters.
/// </summary>
/// <param name="Name">The name of the tool.</param>
/// <param name="Description">A description of what the tool does.</param>
/// <param name="Parameters">The list of parameters the tool accepts.</param>
public sealed record ToolSchema(
    string Name,
    string Description,
    List<ToolParameter> Parameters);

/// <summary>
/// Describes a single parameter of a tool.
/// </summary>
/// <param name="Name">The parameter name.</param>
/// <param name="Type">The parameter type (e.g., "string", "integer", "boolean").</param>
/// <param name="Description">A description of what the parameter is for.</param>
/// <param name="Required">Whether the parameter is required. Defaults to <c>true</c>.</param>
public sealed record ToolParameter(
    string Name,
    string Type,
    string Description,
    bool Required = true);

/// <summary>
/// Represents the result of a tool execution.
/// </summary>
/// <param name="Success">Indicates whether the tool executed successfully.</param>
/// <param name="Data">The data returned by the tool, if any.</param>
/// <param name="Error">An error message if the tool failed.</param>
public sealed record ToolResult(
    bool Success,
    object? Data = null,
    string? Error = null);
