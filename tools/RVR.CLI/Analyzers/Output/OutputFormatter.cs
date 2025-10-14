using System.Text.Json;
using System.Text.Json.Serialization;
using Spectre.Console;
using RVR.CLI.Analyzers.Models;

namespace RVR.CLI.Analyzers.Output;

/// <summary>
/// Formats a list of <see cref="AnalysisResult"/> into a string representation.
/// </summary>
public interface IOutputFormatter
{
    /// <summary>Formats all analysis results into a single output string.</summary>
    string Format(List<AnalysisResult> results);

    /// <summary>The file extension appropriate for this format (e.g. ".json").</summary>
    string FileExtension { get; }
}

// ---------------------------------------------------------------------------
// Console Formatter (Spectre.Console rich output)
// ---------------------------------------------------------------------------

/// <summary>
/// Renders analysis results as rich console output using Spectre.Console.
/// This formatter writes directly to the console and returns an empty string.
/// </summary>
public class ConsoleFormatter : IOutputFormatter
{
    public string FileExtension => ".txt";

    public string Format(List<AnalysisResult> results)
    {
        // Summary table with severity counts per analyzer
        var summaryTable = new Table()
            .Title("[bold underline]Analysis Summary[/]")
            .Border(TableBorder.Rounded)
            .AddColumn("Analyzer")
            .AddColumn("[blue]Info[/]")
            .AddColumn("[yellow]Warning[/]")
            .AddColumn("[red]Error[/]")
            .AddColumn("[bold red]Critical[/]")
            .AddColumn("Files")
            .AddColumn("Duration");

        foreach (var result in results)
        {
            var info = result.Findings.Count(f => f.Severity == FindingSeverity.Info);
            var warn = result.Findings.Count(f => f.Severity == FindingSeverity.Warning);
            var err = result.Findings.Count(f => f.Severity == FindingSeverity.Error);
            var crit = result.Findings.Count(f => f.Severity == FindingSeverity.Critical);

            summaryTable.AddRow(
                Markup.Escape(result.AnalyzerName),
                info > 0 ? $"[blue]{info}[/]" : "[dim]0[/]",
                warn > 0 ? $"[yellow]{warn}[/]" : "[dim]0[/]",
                err > 0 ? $"[red]{err}[/]" : "[dim]0[/]",
                crit > 0 ? $"[bold red]{crit}[/]" : "[dim]0[/]",
                result.FilesScanned.ToString(),
                $"{result.Duration.TotalSeconds:F1}s");
        }

        AnsiConsole.Write(summaryTable);
        AnsiConsole.WriteLine();

        // Detailed panels for each finding, ordered by severity descending
        var allFindings = results.SelectMany(r => r.Findings)
            .OrderByDescending(f => f.Severity)
            .ToList();

        if (allFindings.Count == 0)
        {
            AnsiConsole.MarkupLine("[bold green]No issues found. Code looks great![/]");
            return string.Empty;
        }

        foreach (var finding in allFindings)
        {
            var severityColor = finding.Severity switch
            {
                FindingSeverity.Critical => "bold red",
                FindingSeverity.Error => "red",
                FindingSeverity.Warning => "yellow",
                _ => "blue"
            };

            var snippet = string.IsNullOrWhiteSpace(finding.CodeSnippet)
                ? string.Empty
                : $"\n[grey]{Markup.Escape(finding.CodeSnippet.Length > 300 ? finding.CodeSnippet[..300] + "..." : finding.CodeSnippet)}[/]";

            var body =
                $"[bold]Rule:[/]  {Markup.Escape(finding.RuleId)}\n" +
                $"[bold]File:[/]  {Markup.Escape(finding.FilePath)}:{finding.LineNumber}\n" +
                $"[bold]Issue:[/] {Markup.Escape(finding.Title)}\n" +
                $"[bold green]Fix:[/]   {Markup.Escape(finding.Suggestion)}" +
                snippet;

            var panel = new Panel(body)
            {
                Header = new PanelHeader($"[{severityColor}]{finding.Severity}[/]"),
                Border = BoxBorder.Rounded
            };

            AnsiConsole.Write(panel);
            AnsiConsole.WriteLine();
        }

        return string.Empty; // Output was written directly to console
    }
}

// ---------------------------------------------------------------------------
// JSON Formatter
// ---------------------------------------------------------------------------

/// <summary>
/// Serializes analysis results to a JSON array.
/// </summary>
public class JsonFormatter : IOutputFormatter
{
    public string FileExtension => ".json";

    public string Format(List<AnalysisResult> results)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
        };

        return JsonSerializer.Serialize(results, options);
    }
}

// ---------------------------------------------------------------------------
// SARIF 2.1.0 Formatter
// ---------------------------------------------------------------------------

/// <summary>
/// Produces a valid SARIF 2.1.0 JSON document consumable by GitHub Actions,
/// Azure DevOps, and other CI systems.
/// </summary>
public class SarifFormatter : IOutputFormatter
{
    public string FileExtension => ".sarif";

    public string Format(List<AnalysisResult> results)
    {
        var runs = new List<object>();

        foreach (var result in results)
        {
            var rules = result.Findings
                .Select(f => f.RuleId)
                .Distinct()
                .Select(id =>
                {
                    var sample = result.Findings.First(f => f.RuleId == id);
                    return new Dictionary<string, object>
                    {
                        ["id"] = id,
                        ["name"] = sample.Title,
                        ["shortDescription"] = new Dictionary<string, string>
                        {
                            ["text"] = sample.Title
                        },
                        ["defaultConfiguration"] = new Dictionary<string, string>
                        {
                            ["level"] = MapSeverityToSarif(sample.Severity)
                        }
                    };
                })
                .ToList();

            var sarifResults = result.Findings.Select(f => new Dictionary<string, object>
            {
                ["ruleId"] = f.RuleId,
                ["level"] = MapSeverityToSarif(f.Severity),
                ["message"] = new Dictionary<string, string>
                {
                    ["text"] = $"{f.Title}: {f.Suggestion}"
                },
                ["locations"] = new[]
                {
                    new Dictionary<string, object>
                    {
                        ["physicalLocation"] = new Dictionary<string, object>
                        {
                            ["artifactLocation"] = new Dictionary<string, string>
                            {
                                ["uri"] = f.FilePath.Replace('\\', '/')
                            },
                            ["region"] = new Dictionary<string, int>
                            {
                                ["startLine"] = Math.Max(1, f.LineNumber)
                            }
                        }
                    }
                }
            }).ToList();

            runs.Add(new Dictionary<string, object>
            {
                ["tool"] = new Dictionary<string, object>
                {
                    ["driver"] = new Dictionary<string, object>
                    {
                        ["name"] = result.AnalyzerName,
                        ["version"] = "2.1.0",
                        ["informationUri"] = "https://github.com/khalilbenaz/RIVORA",
                        ["rules"] = rules
                    }
                },
                ["results"] = sarifResults
            });
        }

        var sarif = new Dictionary<string, object>
        {
            ["$schema"] = "https://raw.githubusercontent.com/oasis-tcs/sarif-spec/main/sarif-2.1/schema/sarif-schema-2.1.0.json",
            ["version"] = "2.1.0",
            ["runs"] = runs
        };

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        return JsonSerializer.Serialize(sarif, options);
    }

    private static string MapSeverityToSarif(FindingSeverity severity) => severity switch
    {
        FindingSeverity.Critical => "error",
        FindingSeverity.Error => "error",
        FindingSeverity.Warning => "warning",
        FindingSeverity.Info => "note",
        _ => "none"
    };
}
