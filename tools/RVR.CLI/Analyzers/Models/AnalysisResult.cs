namespace RVR.CLI.Analyzers.Models;

/// <summary>
/// Represents the result of running an analyzer against a project.
/// </summary>
public sealed class AnalysisResult
{
    /// <summary>The name of the analyzer that produced this result.</summary>
    public string AnalyzerName { get; set; } = string.Empty;

    /// <summary>All findings discovered during analysis.</summary>
    public List<AnalysisFinding> Findings { get; set; } = [];

    /// <summary>How long the analysis took.</summary>
    public TimeSpan Duration { get; set; }

    /// <summary>Number of files that were scanned.</summary>
    public int FilesScanned { get; set; }

    /// <summary>True when no findings were reported.</summary>
    public bool IsClean => Findings.Count == 0;
}

/// <summary>
/// A single finding produced by an analyzer.
/// </summary>
public sealed class AnalysisFinding
{
    /// <summary>Relative file path where the issue was found.</summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>One-based line number of the finding.</summary>
    public int LineNumber { get; set; }

    /// <summary>Severity of the finding.</summary>
    public FindingSeverity Severity { get; set; }

    /// <summary>Category grouping for the finding (e.g. Architecture, DDD).</summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>Rule identifier (e.g. ARCH001, DDD003).</summary>
    public string RuleId { get; set; } = string.Empty;

    /// <summary>Short title for the finding.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Human-readable description of the problem.</summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>Explanation of the problem and how to fix it.</summary>
    public string Suggestion { get; set; } = string.Empty;

    /// <summary>The source code snippet that triggered the finding.</summary>
    public string? CodeSnippet { get; set; }
}

/// <summary>
/// Severity levels for analysis findings.
/// </summary>
public enum FindingSeverity
{
    /// <summary>Informational finding.</summary>
    Info,
    /// <summary>Warning-level finding.</summary>
    Warning,
    /// <summary>Error-level finding.</summary>
    Error,
    /// <summary>Critical finding that must be addressed.</summary>
    Critical
}
