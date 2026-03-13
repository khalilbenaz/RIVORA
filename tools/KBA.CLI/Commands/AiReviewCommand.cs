using Spectre.Console;
using System.Text;

namespace KBA.CLI.Commands;

/// <summary>
/// Provides AI-powered code review functionality.
/// </summary>
public static class AiReviewCommand
{
    private static string? _apiKey;
    private static string _provider = "openai";
    private static string _model = "gpt-4o";

    /// <summary>
    /// Executes the AI code review command.
    /// </summary>
    /// <param name="path">Path to the file or directory to review.</param>
    /// <param name="provider">The LLM provider (openai, claude).</param>
    /// <param name="model">The model to use.</param>
    /// <param name="apiKey">The API key for the provider.</param>
    /// <param name="focus">Specific focus areas (security, performance, style, all).</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task ExecuteAsync(
        string path,
        string provider,
        string model,
        string? apiKey,
        string focus = "all")
    {
        AnsiConsole.Write(new FigletText("AI Review").Color(Color.Cyan1));
        AnsiConsole.MarkupLine("[grey]AI-powered code review[/]" + Environment.NewLine);

        _provider = provider.ToLower();
        _model = string.IsNullOrWhiteSpace(model) ? GetDefaultModel(_provider) : model;
        _apiKey = apiKey ?? Environment.GetEnvironmentVariable(GetApiKeyEnvVar(_provider));

        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            AnsiConsole.MarkupLine("[bold red]Error:[/] API key not provided.");
            AnsiConsole.MarkupLine($"Set {GetApiKeyEnvVar(_provider)} environment variable or use --api-key option.");
            return;
        }

        // Validate path
        if (!Directory.Exists(path) && !File.Exists(path))
        {
            AnsiConsole.MarkupLine($"[bold red]Error:[/] Path '{path}' does not exist.");
            return;
        }

        var filesToReview = GetFilesToReview(path);

        AnsiConsole.MarkupLine($"[bold cyan]Provider:[/] {_provider}");
        AnsiConsole.MarkupLine($"[bold cyan]Model:[/] {_model}");
        AnsiConsole.MarkupLine($"[bold cyan]Focus:[/] {focus}");
        AnsiConsole.MarkupLine($"[bold cyan]Files to review:[/] {filesToReview.Count}");
        AnsiConsole.WriteLine();

        var allIssues = new List<ReviewIssue>();

        foreach (var file in filesToReview)
        {
            await AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .StartAsync($"Reviewing {Path.GetFileName(file)}...", async ctx =>
                {
                    var issues = await ReviewFileAsync(file, focus);
                    allIssues.AddRange(issues);
                });
        }

        // Display results
        DisplayReviewResults(allIssues);

        await Task.CompletedTask;
    }

    private static string GetDefaultModel(string provider) => provider switch
    {
        "claude" => "claude-sonnet-4-5-20250929",
        "openai" => "gpt-4o",
        _ => "gpt-4o"
    };

    private static string GetApiKeyEnvVar(string provider) => provider switch
    {
        "claude" => "ANTHROPIC_API_KEY",
        "openai" => "OPENAI_API_KEY",
        _ => "OPENAI_API_KEY"
    };

    private static List<string> GetFilesToReview(string path)
    {
        var files = new List<string>();
        var extensions = new[] { "*.cs", "*.cshtml", "*.razor", "*.js", "*.ts", "*.json", "*.yml", "*.yaml" };

        if (File.Exists(path))
        {
            files.Add(path);
        }
        else
        {
            foreach (var ext in extensions)
            {
                files.AddRange(Directory.GetFiles(path, ext, SearchOption.AllDirectories));
            }
        }

        // Exclude common non-source directories
        var excludeDirs = new[] { "bin", "obj", "node_modules", ".git", "wwwroot" };
        return files.Where(f => !excludeDirs.Any(d => f.Contains(Path.DirectorySeparatorChar + d + Path.DirectorySeparatorChar))).ToList();
    }

    private static async Task<List<ReviewIssue>> ReviewFileAsync(string filePath, string focus)
    {
        // Simulated review - in production, integrate with actual API
        await Task.Delay(500);

        var content = await File.ReadAllTextAsync(filePath);
        var issues = new List<ReviewIssue>();

        // Static analysis rules (simulated AI review)
        if (content.Contains("Console.WriteLine") && focus is "all" or "style")
        {
            issues.Add(new ReviewIssue
            {
                File = filePath,
                Line = FindLineNumber(content, "Console.WriteLine"),
                Severity = "Warning",
                Category = "Style",
                Message = "Avoid Console.WriteLine in production code. Use ILogger instead.",
                Suggestion = "Replace with _logger.LogInformation() or appropriate logging level."
            });
        }

        if (content.Contains("async void") && focus is "all" or "performance")
        {
            issues.Add(new ReviewIssue
            {
                File = filePath,
                Line = FindLineNumber(content, "async void"),
                Severity = "Error",
                Category = "Performance",
                Message = "Avoid async void methods. Use async Task instead.",
                Suggestion = "Change method signature to return async Task."
            });
        }

        if (content.Contains(".Result") || content.Contains(".Wait()") && focus is "all" or "performance")
        {
            issues.Add(new ReviewIssue
            {
                File = filePath,
                Line = FindLineNumber(content, ".Result"),
                Severity = "Error",
                Category = "Performance",
                Message = "Avoid .Result or .Wait() which can cause deadlocks.",
                Suggestion = "Use await instead of blocking calls."
            });
        }

        if (content.Contains("catch (Exception)") && focus is "all" or "security")
        {
            issues.Add(new ReviewIssue
            {
                File = filePath,
                Line = FindLineNumber(content, "catch (Exception)"),
                Severity = "Warning",
                Category = "Error Handling",
                Message = "Catching generic Exception may hide important errors.",
                Suggestion = "Catch specific exception types or rethrow after logging."
            });
        }

        if (content.Contains("TODO") || content.Contains("FIXME") && focus is "all" or "style")
        {
            issues.Add(new ReviewIssue
            {
                File = filePath,
                Line = FindLineNumber(content, "TODO"),
                Severity = "Info",
                Category = "Code Quality",
                Message = "Found TODO comment indicating incomplete implementation.",
                Suggestion = "Address the TODO item or create a tracking issue."
            });
        }

        if (content.Contains("password") || content.Contains("secret") || content.Contains("key") && focus is "all" or "security")
        {
            var lines = content.Split('\n');
            foreach (var line in lines.Where(l => l.Contains("=") && (l.Contains("password", StringComparison.OrdinalIgnoreCase) || l.Contains("secret", StringComparison.OrdinalIgnoreCase) || l.Contains("key", StringComparison.OrdinalIgnoreCase))))
            {
                if (line.Contains("\"") || line.Contains("'"))
                {
                    issues.Add(new ReviewIssue
                    {
                        File = filePath,
                        Line = FindLineNumber(content, line.Trim()),
                        Severity = "Critical",
                        Category = "Security",
                        Message = "Potential hardcoded credential detected.",
                        Suggestion = "Use environment variables or secure secret management."
                    });
                }
            }
        }

        if (content.Contains("public class") && !content.Contains("/// <summary>") && focus is "all" or "style")
        {
            issues.Add(new ReviewIssue
            {
                File = filePath,
                Line = FindLineNumber(content, "public class"),
                Severity = "Info",
                Category = "Documentation",
                Message = "Public class lacks XML documentation.",
                Suggestion = "Add /// <summary> documentation for public APIs."
            });
        }

        return issues;
    }

    private static int FindLineNumber(string content, string search)
    {
        var lines = content.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i].Contains(search))
                return i + 1;
        }
        return 0;
    }

    private static void DisplayReviewResults(List<ReviewIssue> issues)
    {
        AnsiConsole.WriteLine();

        if (issues.Count == 0)
        {
            AnsiConsole.MarkupLine("[bold green]✓ No issues found! Code looks great.[/]");
            return;
        }

        var summary = new Table();
        summary.AddColumn("Severity");
        summary.AddColumn("Count");

        var critical = issues.Count(i => i.Severity == "Critical");
        var error = issues.Count(i => i.Severity == "Error");
        var warning = issues.Count(i => i.Severity == "Warning");
        var info = issues.Count(i => i.Severity == "Info");

        if (critical > 0) summary.AddRow("[bold red]Critical[/]", $"[bold red]{critical}[/]");
        if (error > 0) summary.AddRow("[bold red]Error[/]", $"[red]{error}[/]");
        if (warning > 0) summary.AddRow("[bold yellow]Warning[/]", $"[yellow]{warning}[/]");
        if (info > 0) summary.AddRow("[bold blue]Info[/]", $"[blue]{info}[/]");

        AnsiConsole.Write(summary);
        AnsiConsole.WriteLine();

        // Detailed issues
        AnsiConsole.MarkupLine("[bold]Detailed Findings:[/]" + Environment.NewLine);

        foreach (var issue in issues.OrderByDescending(i => GetSeverityOrder(i.Severity)))
        {
            var severityColor = issue.Severity switch
            {
                "Critical" => "red",
                "Error" => "red",
                "Warning" => "yellow",
                "Info" => "blue",
                _ => "white"
            };

            var panel = new Panel($@"[bold]File:[/] {issue.File}
[bold]Line:[/] {issue.Line}
[bold]Category:[/] {issue.Category}
[bold]Message:[/] {issue.Message}
[bold green]Suggestion:[/] {issue.Suggestion}");

            panel.Header = new PanelHeader($"[{severityColor}]{issue.Severity}[/{severityColor}]");
            panel.Border = BoxBorder.Rounded;
            AnsiConsole.Write(panel);
            AnsiConsole.WriteLine();
        }
    }

    private static int GetSeverityOrder(string severity) => severity switch
    {
        "Critical" => 0,
        "Error" => 1,
        "Warning" => 2,
        "Info" => 3,
        _ => 4
    };

    /// <summary>
    /// Represents a code review issue.
    /// </summary>
    public class ReviewIssue
    {
        public string File { get; set; } = string.Empty;
        public int Line { get; set; }
        public string Severity { get; set; } = "Info";
        public string Category { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Suggestion { get; set; } = string.Empty;
    }
}
