using Spectre.Console;
using RVR.CLI.Analyzers;
using RVR.CLI.Analyzers.Models;
using RVR.CLI.Analyzers.Output;
using RVR.CLI.Services;

namespace RVR.CLI.Commands;

/// <summary>
/// Provides AI-powered code review with multiple analyzers, LLM integration,
/// and configurable output formats.
/// </summary>
public static class AiReviewCommand
{
    /// <summary>
    /// Executes the AI code review command.
    /// </summary>
    public static async Task<int> ExecuteAsync(
        string path,
        bool architecture,
        bool performance,
        bool security,
        bool ddd,
        bool all,
        string? provider,
        string? apiKey,
        string output,
        string? outputFile,
        bool ci)
    {
        if (!ci)
        {
            AnsiConsole.Write(new FigletText("AI Review").Color(Color.Cyan1));
            AnsiConsole.MarkupLine("[grey]AI-powered code review[/]" + Environment.NewLine);
        }

        // Resolve project path
        var projectPath = Path.GetFullPath(path);
        if (!Directory.Exists(projectPath) && !File.Exists(projectPath))
        {
            WriteError($"Path '{projectPath}' does not exist.", ci);
            return 1;
        }

        // Build the list of analyzers to run
        var analyzers = BuildAnalyzerList(architecture, performance, security, ddd, all);

        // Optionally create an LLM client for enhanced suggestions
        ILlmClient? llmClient = null;
        LlmRefactoringAnalyzer? llmAnalyzer = null;

        if (!string.IsNullOrWhiteSpace(provider))
        {
            try
            {
                llmClient = LlmClientFactory.Create(provider, apiKey);

                if (!llmClient.IsAvailable)
                {
                    WriteError($"LLM provider '{provider}' is not available. Check your API key.", ci);
                    return 1;
                }

                llmAnalyzer = new LlmRefactoringAnalyzer(llmClient);
                analyzers.Add(llmAnalyzer);

                if (!ci)
                    AnsiConsole.MarkupLine($"[bold cyan]LLM Provider:[/] {llmClient.ProviderName}");
            }
            catch (ArgumentException ex)
            {
                WriteError(ex.Message, ci);
                return 1;
            }
        }

        if (!ci)
        {
            AnsiConsole.MarkupLine($"[bold cyan]Path:[/] {projectPath}");
            AnsiConsole.MarkupLine($"[bold cyan]Analyzers:[/] {string.Join(", ", analyzers.Select(a => a.Name))}");
            AnsiConsole.MarkupLine($"[bold cyan]Output:[/] {output}");
            AnsiConsole.WriteLine();
        }

        // Run all analyzers in parallel
        var results = new List<AnalysisResult>();
        var cts = new CancellationTokenSource();

        if (!ci)
        {
            await AnsiConsole.Progress()
                .AutoClear(false)
                .Columns(
                    new TaskDescriptionColumn(),
                    new ProgressBarColumn(),
                    new PercentageColumn(),
                    new SpinnerColumn())
                .StartAsync(async ctx =>
                {
                    var tasks = analyzers.Select(async analyzer =>
                    {
                        var task = ctx.AddTask($"[cyan]{Markup.Escape(analyzer.Name)}[/]");
                        task.IsIndeterminate = true;

                        var result = await analyzer.AnalyzeAsync(projectPath, cts.Token);
                        results.Add(result);

                        task.IsIndeterminate = false;
                        task.Value = 100;
                    });

                    await Task.WhenAll(tasks);
                });
        }
        else
        {
            // CI mode: run without interactive UI
            var tasks = analyzers.Select(a => a.AnalyzeAsync(projectPath, cts.Token));
            results.AddRange(await Task.WhenAll(tasks));
        }

        // If LLM provider is configured, enhance the most critical findings
        if (llmAnalyzer is not null && llmClient is not null)
        {
            var staticFindings = results
                .Where(r => r.AnalyzerName != llmAnalyzer.Name)
                .SelectMany(r => r.Findings)
                .ToList();

            if (staticFindings.Count > 0)
            {
                if (!ci)
                    AnsiConsole.MarkupLine("[grey]Enhancing findings with AI suggestions...[/]");

                var enhanced = await llmAnalyzer.EnhanceFindings(staticFindings, cts.Token);

                // Replace static findings with enhanced ones
                foreach (var result in results.Where(r => r.AnalyzerName != llmAnalyzer.Name))
                {
                    var enhancedForAnalyzer = enhanced
                        .Where(e => result.Findings.Any(f =>
                            f.RuleId == e.RuleId && f.FilePath == e.FilePath && f.LineNumber == e.LineNumber))
                        .ToList();

                    if (enhancedForAnalyzer.Count > 0)
                    {
                        // Rebuild result with enhanced findings
                        var remaining = result.Findings
                            .Where(f => !enhancedForAnalyzer.Any(e =>
                                e.RuleId == f.RuleId && e.FilePath == f.FilePath && e.LineNumber == f.LineNumber))
                            .ToList();

                        remaining.AddRange(enhancedForAnalyzer);

                        // Replace in-place via index
                        var idx = results.IndexOf(result);
                        results[idx] = new AnalysisResult
                        {
                            AnalyzerName = result.AnalyzerName,
                            Findings = remaining,
                            Duration = result.Duration,
                            FilesScanned = result.FilesScanned
                        };
                    }
                }
            }
        }

        // Format output
        IOutputFormatter formatter = output.ToLowerInvariant() switch
        {
            "json" => new JsonFormatter(),
            "sarif" => new SarifFormatter(),
            _ => new ConsoleFormatter()
        };

        var formatted = formatter.Format(results);

        // Write to file if requested
        if (!string.IsNullOrWhiteSpace(outputFile))
        {
            var filePath = Path.GetFullPath(outputFile);
            await File.WriteAllTextAsync(filePath, formatted, cts.Token);

            if (!ci)
                AnsiConsole.MarkupLine($"[bold green]Output written to:[/] {filePath}");
        }
        else if (output.ToLowerInvariant() != "console" && !string.IsNullOrWhiteSpace(formatted))
        {
            // Non-console format without file output: write to stdout
            Console.WriteLine(formatted);
        }

        // CI mode: set exit code based on severity
        if (ci)
        {
            var maxSeverity = results
                .SelectMany(r => r.Findings)
                .Select(f => f.Severity)
                .DefaultIfEmpty(FindingSeverity.Info)
                .Max();

            return maxSeverity >= FindingSeverity.Error ? 1 : 0;
        }

        return 0;
    }

    private static List<IAnalyzer> BuildAnalyzerList(
        bool architecture, bool performance, bool security, bool ddd, bool all)
    {
        var analyzers = new List<IAnalyzer>();

        // When --all is true (the default) or no specific flag is set, run everything
        bool runAll = all || (!architecture && !performance && !security && !ddd);

        if (runAll || architecture)
            analyzers.Add(new ArchitectureAnalyzer());

        if (runAll || performance)
            analyzers.Add(new PerformanceAnalyzer());

        if (runAll || security)
            analyzers.Add(new SecurityAnalyzer());

        if (runAll || ddd)
            analyzers.Add(new DddAnalyzer());

        return analyzers;
    }

    private static void WriteError(string message, bool ci)
    {
        if (ci)
            Console.Error.WriteLine($"ERROR: {message}");
        else
            AnsiConsole.MarkupLine($"[bold red]Error:[/] {Markup.Escape(message)}");
    }

    // -----------------------------------------------------------------------
    // Built-in static analyzers
    // -----------------------------------------------------------------------

    /// <summary>Checks Clean Architecture layer conformance.</summary>
    private sealed class ArchitectureAnalyzer : IAnalyzer
    {
        public string Name => "Clean Architecture";

        public async Task<AnalysisResult> AnalyzeAsync(string projectPath, CancellationToken ct = default)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var findings = new List<AnalysisFinding>();
            var files = GetCsFiles(projectPath);

            foreach (var file in files)
            {
                ct.ThrowIfCancellationRequested();
                var content = await File.ReadAllTextAsync(file, ct);
                var relativePath = Path.GetRelativePath(projectPath, file);

                // Domain layer should not reference Infrastructure/Persistence
                if (relativePath.Contains("Domain", StringComparison.OrdinalIgnoreCase))
                {
                    if (content.Contains("using Microsoft.EntityFrameworkCore") ||
                        content.Contains("using System.Data"))
                    {
                        findings.Add(new AnalysisFinding
                        {
                            RuleId = "ARCH001",
                            Message = "Domain references Infrastructure",
                            Severity = FindingSeverity.Error,
                            FilePath = relativePath,
                            LineNumber = FindLine(content, "using Microsoft.EntityFrameworkCore", "using System.Data"),
                            CodeSnippet = ExtractLine(content, "using Microsoft.EntityFrameworkCore", "using System.Data"),
                            Suggestion = "Domain layer must not depend on infrastructure. Use repository interfaces instead."
                        });
                    }
                }

                // Application layer should not reference Infrastructure directly
                if (relativePath.Contains("Application", StringComparison.OrdinalIgnoreCase)
                    && !relativePath.Contains("Infrastructure", StringComparison.OrdinalIgnoreCase))
                {
                    if (content.Contains("using Microsoft.EntityFrameworkCore"))
                    {
                        findings.Add(new AnalysisFinding
                        {
                            RuleId = "ARCH002",
                            Message = "Application references EF Core directly",
                            Severity = FindingSeverity.Warning,
                            FilePath = relativePath,
                            LineNumber = FindLine(content, "using Microsoft.EntityFrameworkCore"),
                            CodeSnippet = ExtractLine(content, "using Microsoft.EntityFrameworkCore"),
                            Suggestion = "Application layer should use repository abstractions, not EF Core directly."
                        });
                    }
                }

                // Presentation should not contain business logic patterns
                if (relativePath.Contains("Controllers", StringComparison.OrdinalIgnoreCase) ||
                    relativePath.Contains("Api", StringComparison.OrdinalIgnoreCase))
                {
                    if (content.Contains("DbContext") || content.Contains("IDbConnection"))
                    {
                        findings.Add(new AnalysisFinding
                        {
                            RuleId = "ARCH003",
                            Message = "Controller contains data access",
                            Severity = FindingSeverity.Error,
                            FilePath = relativePath,
                            LineNumber = FindLine(content, "DbContext", "IDbConnection"),
                            CodeSnippet = ExtractLine(content, "DbContext", "IDbConnection"),
                            Suggestion = "Controllers should delegate to application services / MediatR handlers."
                        });
                    }
                }
            }

            sw.Stop();
            return new AnalysisResult
            {
                AnalyzerName = Name,
                Findings = findings,
                Duration = sw.Elapsed,
                FilesScanned = files.Count
            };
        }
    }

    /// <summary>Detects common performance anti-patterns.</summary>
    private sealed class PerformanceAnalyzer : IAnalyzer
    {
        public string Name => "Performance";

        public async Task<AnalysisResult> AnalyzeAsync(string projectPath, CancellationToken ct = default)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var findings = new List<AnalysisFinding>();
            var files = GetCsFiles(projectPath);

            foreach (var file in files)
            {
                ct.ThrowIfCancellationRequested();
                var content = await File.ReadAllTextAsync(file, ct);
                var relativePath = Path.GetRelativePath(projectPath, file);

                if (content.Contains("async void"))
                {
                    findings.Add(new AnalysisFinding
                    {
                        RuleId = "PERF001",
                        Message = "Async void method detected",
                        Severity = FindingSeverity.Error,
                        FilePath = relativePath,
                        LineNumber = FindLine(content, "async void"),
                        CodeSnippet = ExtractLine(content, "async void"),
                        Suggestion = "Use async Task instead of async void to allow proper exception handling and awaiting."
                    });
                }

                if (content.Contains(".Result") || content.Contains(".Wait()"))
                {
                    var term = content.Contains(".Result") ? ".Result" : ".Wait()";
                    findings.Add(new AnalysisFinding
                    {
                        RuleId = "PERF002",
                        Message = "Sync-over-async blocking call",
                        Severity = FindingSeverity.Error,
                        FilePath = relativePath,
                        LineNumber = FindLine(content, term),
                        CodeSnippet = ExtractLine(content, term),
                        Suggestion = "Avoid .Result/.Wait() which can cause deadlocks. Use await instead."
                    });
                }

                if (content.Contains("ToList()") && content.Contains("Where("))
                {
                    findings.Add(new AnalysisFinding
                    {
                        RuleId = "PERF003",
                        Message = "Possible unnecessary materialization",
                        Severity = FindingSeverity.Warning,
                        FilePath = relativePath,
                        LineNumber = FindLine(content, "ToList()"),
                        CodeSnippet = ExtractLine(content, "ToList()"),
                        Suggestion = "Consider whether ToList() is needed or if deferred execution (IEnumerable/IQueryable) would suffice."
                    });
                }

                if (content.Contains("string +") || content.Contains("\" + \""))
                {
                    findings.Add(new AnalysisFinding
                    {
                        RuleId = "PERF004",
                        Message = "String concatenation in potential loop",
                        Severity = FindingSeverity.Info,
                        FilePath = relativePath,
                        LineNumber = FindLine(content, "\" + \""),
                        CodeSnippet = ExtractLine(content, "\" + \""),
                        Suggestion = "Consider using StringBuilder or string interpolation for repeated concatenation."
                    });
                }
            }

            sw.Stop();
            return new AnalysisResult
            {
                AnalyzerName = Name,
                Findings = findings,
                Duration = sw.Elapsed,
                FilesScanned = files.Count
            };
        }
    }

    /// <summary>Scans for common security vulnerabilities.</summary>
    private sealed class SecurityAnalyzer : IAnalyzer
    {
        public string Name => "Security";

        public async Task<AnalysisResult> AnalyzeAsync(string projectPath, CancellationToken ct = default)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var findings = new List<AnalysisFinding>();
            var files = GetCsFiles(projectPath);

            foreach (var file in files)
            {
                ct.ThrowIfCancellationRequested();
                var content = await File.ReadAllTextAsync(file, ct);
                var relativePath = Path.GetRelativePath(projectPath, file);

                // SQL injection risk
                if (content.Contains("FromSqlRaw") || content.Contains("ExecuteSqlRaw"))
                {
                    var term = content.Contains("FromSqlRaw") ? "FromSqlRaw" : "ExecuteSqlRaw";
                    findings.Add(new AnalysisFinding
                    {
                        RuleId = "SEC001",
                        Message = "Potential SQL injection via raw SQL",
                        Severity = FindingSeverity.Critical,
                        FilePath = relativePath,
                        LineNumber = FindLine(content, term),
                        CodeSnippet = ExtractLine(content, term),
                        Suggestion = "Use parameterized queries (FromSqlInterpolated) or stored procedures to prevent SQL injection."
                    });
                }

                // Hardcoded secrets
                var lines = content.Split('\n');
                for (int i = 0; i < lines.Length; i++)
                {
                    var line = lines[i];
                    if (line.Contains('=') &&
                        (line.Contains("password", StringComparison.OrdinalIgnoreCase) ||
                         line.Contains("secret", StringComparison.OrdinalIgnoreCase) ||
                         line.Contains("connectionstring", StringComparison.OrdinalIgnoreCase)) &&
                        (line.Contains('"') || line.Contains('\'')))
                    {
                        findings.Add(new AnalysisFinding
                        {
                            RuleId = "SEC002",
                            Message = "Potential hardcoded credential",
                            Severity = FindingSeverity.Critical,
                            FilePath = relativePath,
                            LineNumber = i + 1,
                            CodeSnippet = line.Trim(),
                            Suggestion = "Use environment variables, Azure Key Vault, or user secrets for sensitive data."
                        });
                    }
                }

                // Weak cryptography
                if (content.Contains("MD5.Create") || content.Contains("SHA1.Create"))
                {
                    var term = content.Contains("MD5.Create") ? "MD5.Create" : "SHA1.Create";
                    findings.Add(new AnalysisFinding
                    {
                        RuleId = "SEC003",
                        Message = "Weak cryptographic algorithm",
                        Severity = FindingSeverity.Error,
                        FilePath = relativePath,
                        LineNumber = FindLine(content, term),
                        CodeSnippet = ExtractLine(content, term),
                        Suggestion = "Use SHA256 or SHA512 for hashing. Use BCrypt/Argon2 for password hashing."
                    });
                }

                // Missing [Authorize]
                if (content.Contains("[HttpPost]") || content.Contains("[HttpPut]") || content.Contains("[HttpDelete]"))
                {
                    if (!content.Contains("[Authorize]") && !content.Contains("[AllowAnonymous]"))
                    {
                        findings.Add(new AnalysisFinding
                        {
                            RuleId = "SEC004",
                            Message = "HTTP mutation endpoint without authorization",
                            Severity = FindingSeverity.Warning,
                            FilePath = relativePath,
                            LineNumber = FindLine(content, "[HttpPost]", "[HttpPut]", "[HttpDelete]"),
                            CodeSnippet = ExtractLine(content, "[HttpPost]", "[HttpPut]", "[HttpDelete]"),
                            Suggestion = "Add [Authorize] attribute to mutation endpoints or explicitly mark with [AllowAnonymous]."
                        });
                    }
                }
            }

            sw.Stop();
            return new AnalysisResult
            {
                AnalyzerName = Name,
                Findings = findings,
                Duration = sw.Elapsed,
                FilesScanned = files.Count
            };
        }
    }

    /// <summary>Checks for DDD anti-patterns.</summary>
    private sealed class DddAnalyzer : IAnalyzer
    {
        public string Name => "DDD Patterns";

        public async Task<AnalysisResult> AnalyzeAsync(string projectPath, CancellationToken ct = default)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var findings = new List<AnalysisFinding>();
            var files = GetCsFiles(projectPath);

            foreach (var file in files)
            {
                ct.ThrowIfCancellationRequested();
                var content = await File.ReadAllTextAsync(file, ct);
                var relativePath = Path.GetRelativePath(projectPath, file);

                // Anemic domain models: entities with only public setters and no methods
                if (relativePath.Contains("Domain", StringComparison.OrdinalIgnoreCase)
                    && content.Contains("public class")
                    && content.Contains("{ get; set; }")
                    && !content.Contains("public void ")
                    && !content.Contains("public async "))
                {
                    findings.Add(new AnalysisFinding
                    {
                        RuleId = "DDD001",
                        Message = "Anemic domain model",
                        Severity = FindingSeverity.Warning,
                        FilePath = relativePath,
                        LineNumber = FindLine(content, "public class"),
                        CodeSnippet = ExtractLine(content, "public class"),
                        Suggestion = "Domain entities should encapsulate behavior. Add domain methods and make setters private."
                    });
                }

                // Domain entities exposing public setters
                if (relativePath.Contains("Domain", StringComparison.OrdinalIgnoreCase)
                    && content.Contains("{ get; set; }"))
                {
                    findings.Add(new AnalysisFinding
                    {
                        RuleId = "DDD002",
                        Message = "Public setter on domain entity",
                        Severity = FindingSeverity.Info,
                        FilePath = relativePath,
                        LineNumber = FindLine(content, "{ get; set; }"),
                        CodeSnippet = ExtractLine(content, "{ get; set; }"),
                        Suggestion = "Consider using private/init setters and factory methods to enforce invariants."
                    });
                }

                // Domain events not being raised
                if (relativePath.Contains("Domain", StringComparison.OrdinalIgnoreCase)
                    && content.Contains("DomainEvent")
                    && !content.Contains("AddDomainEvent") && !content.Contains("RaiseDomainEvent"))
                {
                    findings.Add(new AnalysisFinding
                    {
                        RuleId = "DDD003",
                        Message = "Domain event type without raise calls",
                        Severity = FindingSeverity.Warning,
                        FilePath = relativePath,
                        LineNumber = FindLine(content, "DomainEvent"),
                        CodeSnippet = ExtractLine(content, "DomainEvent"),
                        Suggestion = "Domain events should be raised via AddDomainEvent() when state changes occur."
                    });
                }

                // Value objects implemented as classes with mutable state
                if (relativePath.Contains("ValueObject", StringComparison.OrdinalIgnoreCase)
                    && content.Contains("{ get; set; }"))
                {
                    findings.Add(new AnalysisFinding
                    {
                        RuleId = "DDD004",
                        Message = "Mutable value object",
                        Severity = FindingSeverity.Error,
                        FilePath = relativePath,
                        LineNumber = FindLine(content, "{ get; set; }"),
                        CodeSnippet = ExtractLine(content, "{ get; set; }"),
                        Suggestion = "Value objects must be immutable. Use records or init-only properties."
                    });
                }
            }

            sw.Stop();
            return new AnalysisResult
            {
                AnalyzerName = Name,
                Findings = findings,
                Duration = sw.Elapsed,
                FilesScanned = files.Count
            };
        }
    }

    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    private static List<string> GetCsFiles(string projectPath)
    {
        if (File.Exists(projectPath))
            return [projectPath];

        var excludeDirs = new[] { "bin", "obj", "node_modules", ".git", "wwwroot" };

        return Directory.GetFiles(projectPath, "*.cs", SearchOption.AllDirectories)
            .Where(f => !excludeDirs.Any(d =>
                f.Contains(Path.DirectorySeparatorChar + d + Path.DirectorySeparatorChar)))
            .ToList();
    }

    private static int FindLine(string content, params string[] terms)
    {
        var lines = content.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            if (terms.Any(t => lines[i].Contains(t)))
                return i + 1;
        }
        return 1;
    }

    private static string ExtractLine(string content, params string[] terms)
    {
        var lines = content.Split('\n');
        foreach (var line in lines)
        {
            if (terms.Any(t => line.Contains(t)))
                return line.Trim();
        }
        return string.Empty;
    }
}
