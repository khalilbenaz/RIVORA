using System.Diagnostics;

namespace RVR.CLI.Analyzers;

using RVR.CLI.Analyzers.Models;
using RVR.CLI.Services;

/// <summary>
/// Analyzer that uses an LLM backend to provide AI-powered refactoring suggestions.
/// It takes the most critical findings from other analyzers, sends code snippets to the
/// configured LLM for contextual suggestions, and returns enhanced findings with
/// AI-generated refactoring code.
/// </summary>
public class LlmRefactoringAnalyzer : IAnalyzer
{
    public string Name => "AI-Powered Refactoring";

    private readonly ILlmClient _llmClient;

    private const string SystemPrompt =
        """
        You are an expert .NET / C# code reviewer. When given a code snippet and a description
        of a problem, respond ONLY with a brief explanation (1-2 sentences) followed by a
        corrected code snippet in a fenced code block. Do not add any other commentary.
        """;

    public LlmRefactoringAnalyzer(ILlmClient llmClient) => _llmClient = llmClient;

    /// <summary>
    /// Scans the most problematic source files in the project and asks the LLM for
    /// refactoring suggestions.
    /// </summary>
    public async Task<AnalysisResult> AnalyzeAsync(string projectPath, CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        var findings = new List<AnalysisFinding>();
        var filesScanned = 0;

        var csFiles = Directory.GetFiles(projectPath, "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains(Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar)
                     && !f.Contains(Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar))
            .Take(10) // Limit to 10 files to control cost/latency
            .ToList();

        foreach (var file in csFiles)
        {
            ct.ThrowIfCancellationRequested();
            filesScanned++;

            var code = await File.ReadAllTextAsync(file, ct);
            if (code.Length > 8000) code = code[..8000]; // Truncate large files

            try
            {
                var userPrompt = $"Review the following C# code and suggest the single most impactful refactoring.\n\n```csharp\n{code}\n```";
                var response = await _llmClient.CompleteAsync(SystemPrompt, userPrompt, ct);

                if (!string.IsNullOrWhiteSpace(response))
                {
                    findings.Add(new AnalysisFinding
                    {
                        RuleId = "LLM001",
                        Message = "AI Refactoring Suggestion",
                        Severity = FindingSeverity.Info,
                        FilePath = Path.GetRelativePath(projectPath, file),
                        LineNumber = 1,
                        CodeSnippet = code.Length > 200 ? code[..200] + "..." : code,
                        Suggestion = response
                    });
                }
            }
            catch (Exception ex)
            {
                findings.Add(new AnalysisFinding
                {
                    RuleId = "LLM999",
                    Message = "LLM Analysis Error",
                    Severity = FindingSeverity.Warning,
                    FilePath = Path.GetRelativePath(projectPath, file),
                    LineNumber = 0,
                    CodeSnippet = string.Empty,
                    Suggestion = $"Could not obtain LLM suggestion: {ex.Message}"
                });
            }
        }

        sw.Stop();
        return new AnalysisResult
        {
            AnalyzerName = Name,
            Findings = findings,
            Duration = sw.Elapsed,
            FilesScanned = filesScanned
        };
    }

    /// <summary>
    /// Takes existing findings from other analyzers and enhances them with LLM-generated
    /// fix suggestions including concrete refactored code.
    /// </summary>
    public async Task<List<AnalysisFinding>> EnhanceFindings(
        List<AnalysisFinding> findings, CancellationToken ct = default)
    {
        // Focus on the most critical findings to limit LLM calls
        var criticalFindings = findings
            .Where(f => f.Severity >= FindingSeverity.Warning)
            .OrderByDescending(f => f.Severity)
            .Take(5)
            .ToList();

        var enhanced = new List<AnalysisFinding>();

        foreach (var finding in criticalFindings)
        {
            ct.ThrowIfCancellationRequested();

            try
            {
                var userPrompt =
                    $"""
                    A static analyzer found the following issue:

                    Rule: {finding.RuleId} - {finding.Title}
                    File: {finding.FilePath} (line {finding.LineNumber})
                    Problem: {finding.Suggestion}
                    Code snippet:
                    ```csharp
                    {finding.CodeSnippet}
                    ```

                    Provide a concrete fix with corrected code.
                    """;

                var response = await _llmClient.CompleteAsync(SystemPrompt, userPrompt, ct);

                enhanced.Add(new AnalysisFinding
                {
                    RuleId = finding.RuleId,
                    Message = finding.Title,
                    Severity = finding.Severity,
                    FilePath = finding.FilePath,
                    LineNumber = finding.LineNumber,
                    CodeSnippet = finding.CodeSnippet,
                    Suggestion = $"{finding.Suggestion}\n\n--- AI Suggestion ---\n{response}"
                });
            }
            catch
            {
                // If the LLM call fails, keep the original finding unchanged
                enhanced.Add(finding);
            }
        }

        // Return enhanced findings merged with the non-critical ones that were not sent to the LLM
        var enhancedIds = new HashSet<(string, string, int)>(
            enhanced.Select(f => (f.RuleId, f.FilePath, f.LineNumber)));

        var unchanged = findings
            .Where(f => !enhancedIds.Contains((f.RuleId, f.FilePath, f.LineNumber)));

        return enhanced.Concat(unchanged).ToList();
    }
}
