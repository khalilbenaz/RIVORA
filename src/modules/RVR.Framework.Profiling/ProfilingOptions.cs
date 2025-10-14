namespace RVR.Framework.Profiling;

/// <summary>
/// Configuration options for the RVR performance profiling module.
/// </summary>
public class ProfilingOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether profiling is enabled.
    /// Defaults to <c>true</c> only in the Development environment.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the base path for MiniProfiler UI resources.
    /// </summary>
    public string RouteBasePath { get; set; } = "/mini-profiler-resources";

    /// <summary>
    /// Gets or sets the threshold in milliseconds for flagging slow SQL queries.
    /// Queries exceeding this threshold are highlighted in the profiler UI.
    /// A value of <c>0</c> disables the threshold.
    /// </summary>
    public int SqlThresholdMs { get; set; }
}
