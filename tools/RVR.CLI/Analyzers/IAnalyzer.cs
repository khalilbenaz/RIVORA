namespace RVR.CLI.Analyzers;

using RVR.CLI.Analyzers.Models;

/// <summary>
/// Common interface for all project analyzers.
/// </summary>
public interface IAnalyzer
{
    /// <summary>Human-readable name of this analyzer.</summary>
    string Name { get; }

    /// <summary>
    /// Scans the project at <paramref name="projectPath"/> and returns findings.
    /// </summary>
    Task<AnalysisResult> AnalyzeAsync(string projectPath, CancellationToken ct = default);
}
