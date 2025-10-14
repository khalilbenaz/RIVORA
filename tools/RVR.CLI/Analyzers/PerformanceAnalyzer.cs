namespace RVR.CLI.Analyzers;

using RVR.CLI.Analyzers.Models;
using System.Diagnostics;
using System.Text.RegularExpressions;

/// <summary>
/// Detects common performance anti-patterns in C# / EF Core projects.
/// Rules: PERF001 through PERF010.
/// </summary>
public sealed partial class PerformanceAnalyzer : IAnalyzer
{
    private const string Category = "Performance";

    public string Name => "Performance Anti-Patterns";

    public async Task<AnalysisResult> AnalyzeAsync(string projectPath, CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        var findings = new List<AnalysisFinding>();

        var csFiles = Directory.GetFiles(projectPath, "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}")
                     && !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}")
                     && !f.Contains($"{Path.DirectorySeparatorChar}node_modules{Path.DirectorySeparatorChar}"));

        int fileCount = 0;
        foreach (var file in csFiles)
        {
            ct.ThrowIfCancellationRequested();
            var lines = await File.ReadAllLinesAsync(file, ct);
            var relativePath = Path.GetRelativePath(projectPath, file);
            fileCount++;

            CheckForNPlusOne(lines, relativePath, findings);
            CheckForSyncOverAsync(lines, relativePath, findings);
            CheckForUnboundedCollections(lines, relativePath, findings);
            CheckForMissingAsNoTracking(lines, relativePath, findings);
            CheckForStringConcatInLoop(lines, relativePath, findings);
            CheckForMultipleEnumeration(lines, relativePath, findings);
            CheckForMissingConfigureAwait(lines, relativePath, findings);
            CheckForSyncIO(lines, relativePath, findings);
            CheckForLoadEntireEntityForUpdate(lines, relativePath, findings);
            CheckForMissingPagination(lines, relativePath, findings);
        }

        sw.Stop();
        return new AnalysisResult
        {
            AnalyzerName = Name,
            Findings = findings,
            Duration = sw.Elapsed,
            FilesScanned = fileCount
        };
    }

    // ---------------------------------------------------------------
    // PERF001 – N+1 query pattern
    // ---------------------------------------------------------------
    private static void CheckForNPlusOne(string[] lines, string file, List<AnalysisFinding> findings)
    {
        bool insideForeach = false;
        int foreachLine = -1;
        int braceDepth = 0;

        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];

            if (ForeachRegex().IsMatch(line))
            {
                insideForeach = true;
                foreachLine = i;
                braceDepth = 0;
            }

            if (insideForeach)
            {
                braceDepth += line.Count(c => c == '{');
                braceDepth -= line.Count(c => c == '}');

                if (braceDepth < 0)
                {
                    insideForeach = false;
                    continue;
                }

                if (i > foreachLine && AwaitDbCallRegex().IsMatch(line))
                {
                    findings.Add(new AnalysisFinding
                    {
                        RuleId = "PERF001",
                        Category = Category,
                        Message = "N+1 query pattern detected",
                        Severity = FindingSeverity.Error,
                        FilePath = file,
                        LineNumber = i + 1,
                        CodeSnippet = line.Trim(),
                        Suggestion = "Load all required data before the loop using a single query with " +
                                     ".Where(x => ids.Contains(x.Id)) or .Include(). " +
                                     "This avoids issuing one database round-trip per iteration."
                    });
                }

                if (braceDepth == 0 && i > foreachLine)
                    insideForeach = false;
            }
        }
    }

    // ---------------------------------------------------------------
    // PERF002 – Synchronous blocking over async (.Result / .Wait())
    // ---------------------------------------------------------------
    private static void CheckForSyncOverAsync(string[] lines, string file, List<AnalysisFinding> findings)
    {
        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            if (SyncOverAsyncRegex().IsMatch(line))
            {
                findings.Add(new AnalysisFinding
                {
                    RuleId = "PERF002",
                    Category = Category,
                    Message = "Synchronous blocking over async",
                    Severity = FindingSeverity.Error,
                    FilePath = file,
                    LineNumber = i + 1,
                    CodeSnippet = line.Trim(),
                    Suggestion = "Replace .Result, .Wait(), or .GetAwaiter().GetResult() with 'await'. " +
                                 "Synchronous blocking can cause thread-pool starvation and deadlocks."
                });
            }
        }
    }

    // ---------------------------------------------------------------
    // PERF003 – Unbounded collection loading
    // ---------------------------------------------------------------
    private static void CheckForUnboundedCollections(string[] lines, string file, List<AnalysisFinding> findings)
    {
        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            if (!ToCollectionRegex().IsMatch(line))
                continue;

            // Look back up to 5 lines for a Take() or Where() clause
            bool hasBound = false;
            int lookback = Math.Max(0, i - 5);
            for (int j = lookback; j <= i; j++)
            {
                if (BoundingClauseRegex().IsMatch(lines[j]))
                {
                    hasBound = true;
                    break;
                }
            }

            if (!hasBound)
            {
                findings.Add(new AnalysisFinding
                {
                    RuleId = "PERF003",
                    Category = Category,
                    Message = "Unbounded collection loading",
                    Severity = FindingSeverity.Warning,
                    FilePath = file,
                    LineNumber = i + 1,
                    CodeSnippet = line.Trim(),
                    Suggestion = "Add a .Where() filter and/or .Take(limit) before .ToList()/.ToArray() " +
                                 "to avoid loading an entire table into memory."
                });
            }
        }
    }

    // ---------------------------------------------------------------
    // PERF004 – Missing AsNoTracking for read-only queries
    // ---------------------------------------------------------------
    private static void CheckForMissingAsNoTracking(string[] lines, string file, List<AnalysisFinding> findings)
    {
        var fullText = string.Join('\n', lines);
        if (!fullText.Contains("DbContext") && !fullText.Contains("DbSet") && !fullText.Contains("_context"))
            return;

        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            if (!DbQueryRegex().IsMatch(line))
                continue;

            bool hasTracking = false;
            bool hasMutation = false;
            int lookahead = Math.Min(lines.Length - 1, i + 5);
            for (int j = i; j <= lookahead; j++)
            {
                if (lines[j].Contains("AsNoTracking"))
                    hasTracking = true;
                if (MutationRegex().IsMatch(lines[j]))
                    hasMutation = true;
            }

            if (!hasTracking && !hasMutation)
            {
                findings.Add(new AnalysisFinding
                {
                    RuleId = "PERF004",
                    Category = Category,
                    Message = "Missing AsNoTracking for read-only query",
                    Severity = FindingSeverity.Warning,
                    FilePath = file,
                    LineNumber = i + 1,
                    CodeSnippet = line.Trim(),
                    Suggestion = "Add .AsNoTracking() to read-only queries to improve performance by " +
                                 "skipping change-tracker overhead."
                });
            }
        }
    }

    // ---------------------------------------------------------------
    // PERF005 – String concatenation in loops
    // ---------------------------------------------------------------
    private static void CheckForStringConcatInLoop(string[] lines, string file, List<AnalysisFinding> findings)
    {
        bool insideLoop = false;
        int loopStart = -1;
        int braceDepth = 0;

        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];

            if (LoopStartRegex().IsMatch(line))
            {
                insideLoop = true;
                loopStart = i;
                braceDepth = 0;
            }

            if (insideLoop)
            {
                braceDepth += line.Count(c => c == '{');
                braceDepth -= line.Count(c => c == '}');

                if (i > loopStart && StringConcatRegex().IsMatch(line))
                {
                    findings.Add(new AnalysisFinding
                    {
                        RuleId = "PERF005",
                        Category = Category,
                        Message = "String concatenation in loop",
                        Severity = FindingSeverity.Warning,
                        FilePath = file,
                        LineNumber = i + 1,
                        CodeSnippet = line.Trim(),
                        Suggestion = "Use StringBuilder instead of string concatenation (+=) inside loops. " +
                                     "Each concatenation allocates a new string object."
                    });
                }

                if (braceDepth <= 0 && i > loopStart)
                    insideLoop = false;
            }
        }
    }

    // ---------------------------------------------------------------
    // PERF006 – LINQ multiple enumeration
    // ---------------------------------------------------------------
    private static void CheckForMultipleEnumeration(string[] lines, string file, List<AnalysisFinding> findings)
    {
        var enumerableVars = new Dictionary<string, int>();

        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];

            var declMatch = EnumerableDeclRegex().Match(line);
            if (declMatch.Success)
            {
                var varName = declMatch.Groups[1].Value;
                enumerableVars[varName] = i;
                continue;
            }

            foreach (var kvp in enumerableVars)
            {
                if (i == kvp.Value) continue;

                if (Regex.IsMatch(line, $@"\b{Regex.Escape(kvp.Key)}\s*\."))
                {
                    findings.Add(new AnalysisFinding
                    {
                        RuleId = "PERF006",
                        Category = Category,
                        Message = $"Possible multiple enumeration of IEnumerable '{kvp.Key}'",
                        Severity = FindingSeverity.Warning,
                        FilePath = file,
                        LineNumber = i + 1,
                        CodeSnippet = line.Trim(),
                        Suggestion = $"The IEnumerable variable '{kvp.Key}' may be enumerated more than once. " +
                                     "Materialize it with .ToList() or .ToArray() before reuse."
                    });
                    enumerableVars.Remove(kvp.Key);
                    break;
                }
            }
        }
    }

    // ---------------------------------------------------------------
    // PERF007 – Missing ConfigureAwait(false) in library code
    // ---------------------------------------------------------------
    private static void CheckForMissingConfigureAwait(string[] lines, string file, List<AnalysisFinding> findings)
    {
        if (file.Contains("Controller") || file.Contains("Endpoint") || file.Contains("Pages")
            || file.Contains("Program.cs") || file.Contains("Startup.cs"))
            return;

        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            if (!AwaitCallRegex().IsMatch(line))
                continue;

            if (!line.Contains("ConfigureAwait"))
            {
                findings.Add(new AnalysisFinding
                {
                    RuleId = "PERF007",
                    Category = Category,
                    Message = "Missing ConfigureAwait(false) in library code",
                    Severity = FindingSeverity.Info,
                    FilePath = file,
                    LineNumber = i + 1,
                    CodeSnippet = line.Trim(),
                    Suggestion = "Add .ConfigureAwait(false) to await calls in library/service code that does not " +
                                 "need to resume on the original synchronization context."
                });
            }
        }
    }

    // ---------------------------------------------------------------
    // PERF008 – Synchronous I/O
    // ---------------------------------------------------------------
    private static void CheckForSyncIO(string[] lines, string file, List<AnalysisFinding> findings)
    {
        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            if (SyncIORegex().IsMatch(line))
            {
                findings.Add(new AnalysisFinding
                {
                    RuleId = "PERF008",
                    Category = Category,
                    Message = "Synchronous I/O detected",
                    Severity = FindingSeverity.Warning,
                    FilePath = file,
                    LineNumber = i + 1,
                    CodeSnippet = line.Trim(),
                    Suggestion = "Use the async equivalent (e.g., File.ReadAllTextAsync, File.WriteAllTextAsync, " +
                                 "StreamReader.ReadToEndAsync) to avoid blocking the thread during I/O."
                });
            }
        }
    }

    // ---------------------------------------------------------------
    // PERF009 – Loading entire entity for update
    // ---------------------------------------------------------------
    private static void CheckForLoadEntireEntityForUpdate(string[] lines, string file, List<AnalysisFinding> findings)
    {
        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            if (!FindEntityRegex().IsMatch(line))
                continue;

            int lookahead = Math.Min(lines.Length - 1, i + 10);
            bool hasSaveChanges = false;
            int propertySetCount = 0;

            for (int j = i + 1; j <= lookahead; j++)
            {
                if (lines[j].Contains("SaveChanges"))
                    hasSaveChanges = true;
                if (PropertySetRegex().IsMatch(lines[j]))
                    propertySetCount++;
            }

            if (hasSaveChanges && propertySetCount is > 0 and <= 2)
            {
                findings.Add(new AnalysisFinding
                {
                    RuleId = "PERF009",
                    Category = Category,
                    Message = "Loading entire entity for a small update",
                    Severity = FindingSeverity.Info,
                    FilePath = file,
                    LineNumber = i + 1,
                    CodeSnippet = line.Trim(),
                    Suggestion = "Consider using ExecuteUpdateAsync() (EF Core 7+) to update only the required " +
                                 "columns without loading the full entity into memory."
                });
            }
        }
    }

    // ---------------------------------------------------------------
    // PERF010 – Missing pagination on API endpoints
    // ---------------------------------------------------------------
    private static void CheckForMissingPagination(string[] lines, string file, List<AnalysisFinding> findings)
    {
        if (!file.Contains("Controller") && !file.Contains("Endpoint"))
            return;

        var fullText = string.Join('\n', lines);
        bool hasPagination = fullText.Contains("page", StringComparison.OrdinalIgnoreCase)
                          || fullText.Contains("PageSize", StringComparison.OrdinalIgnoreCase)
                          || fullText.Contains("Skip(")
                          || fullText.Contains("Take(");

        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            if (!ReturnCollectionRegex().IsMatch(line))
                continue;

            if (!hasPagination)
            {
                findings.Add(new AnalysisFinding
                {
                    RuleId = "PERF010",
                    Category = Category,
                    Message = "Missing pagination on collection endpoint",
                    Severity = FindingSeverity.Warning,
                    FilePath = file,
                    LineNumber = i + 1,
                    CodeSnippet = line.Trim(),
                    Suggestion = "Add pagination (skip/take or page/pageSize) to endpoints that return " +
                                 "collections to avoid unbounded result sets."
                });
            }
        }
    }

    // ---------------------------------------------------------------
    // Compiled Regex patterns (source-generated)
    // ---------------------------------------------------------------

    [GeneratedRegex(@"\bforeach\s*\(")]
    private static partial Regex ForeachRegex();

    [GeneratedRegex(@"\bawait\b.*\b(Get|Find|First|Single|Any|Count|Where|Select|Query|Load|Fetch|Read|Execute)\w*Async\b")]
    private static partial Regex AwaitDbCallRegex();

    [GeneratedRegex(@"\.(Result|Wait\(\)|GetAwaiter\(\)\s*\.GetResult\(\))")]
    private static partial Regex SyncOverAsyncRegex();

    [GeneratedRegex(@"\.(ToList|ToArray|ToListAsync|ToArrayAsync)\s*\(")]
    private static partial Regex ToCollectionRegex();

    [GeneratedRegex(@"\.(Take|Where|Skip)\s*\(")]
    private static partial Regex BoundingClauseRegex();

    [GeneratedRegex(@"_context\.\w+\s*\.\s*(Where|FirstOrDefault|SingleOrDefault|Find|Any|Count|Select|OrderBy)")]
    private static partial Regex DbQueryRegex();

    [GeneratedRegex(@"\.(Update|Remove|Add|Attach|SaveChanges)")]
    private static partial Regex MutationRegex();

    [GeneratedRegex(@"\b(for|foreach|while|do)\b\s*[\({]")]
    private static partial Regex LoopStartRegex();

    [GeneratedRegex(@"\+\s*=\s*(""|@""|[$]""|string\.)")]
    private static partial Regex StringConcatRegex();

    [GeneratedRegex(@"IEnumerable<[^>]+>\s+(\w+)\s*=")]
    private static partial Regex EnumerableDeclRegex();

    [GeneratedRegex(@"\bawait\b")]
    private static partial Regex AwaitCallRegex();

    [GeneratedRegex(@"File\.(ReadAllText|ReadAllLines|ReadAllBytes|WriteAllText|WriteAllLines|WriteAllBytes|ReadLines|AppendAllText|Copy|Move)\s*\(")]
    private static partial Regex SyncIORegex();

    [GeneratedRegex(@"\.(FindAsync|FirstOrDefaultAsync|SingleOrDefaultAsync|GetByIdAsync)\s*\(")]
    private static partial Regex FindEntityRegex();

    [GeneratedRegex(@"\.\w+\s*=\s*[^=]")]
    private static partial Regex PropertySetRegex();

    [GeneratedRegex(@"return\s+.*\b(List|Collection|IEnumerable|IList|ICollection)\b")]
    private static partial Regex ReturnCollectionRegex();
}
