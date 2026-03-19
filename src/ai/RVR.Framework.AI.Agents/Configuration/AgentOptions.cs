namespace RVR.Framework.AI.Agents.Configuration;

/// <summary>
/// Configuration options for the AI Agents module.
/// </summary>
public sealed class AgentOptions
{
    /// <summary>
    /// The configuration section name for agent settings.
    /// </summary>
    public const string SectionName = "AI:Agents";

    /// <summary>
    /// Gets or sets the maximum number of iterations for iterative agent strategies (e.g., ReAct).
    /// Defaults to 10.
    /// </summary>
    public int MaxIterations { get; set; } = 10;

    /// <summary>
    /// Gets or sets the overall timeout in seconds for agent pipeline execution.
    /// Defaults to 120 seconds.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 120;

    /// <summary>
    /// Gets or sets the default LLM model identifier used by agents.
    /// Defaults to "gpt-4".
    /// </summary>
    public string DefaultModel { get; set; } = "gpt-4";

    /// <summary>
    /// Gets or sets whether tracing is enabled for agent executions.
    /// When enabled, detailed step information is logged. Defaults to <c>true</c>.
    /// </summary>
    public bool EnableTracing { get; set; } = true;
}
