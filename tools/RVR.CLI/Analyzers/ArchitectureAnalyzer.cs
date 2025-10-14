using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using RVR.CLI.Analyzers.Models;

namespace RVR.CLI.Analyzers;

/// <summary>
/// Validates Clean Architecture layer dependency rules.
/// Detects violations where lower layers reference higher layers.
/// </summary>
public class ArchitectureAnalyzer : IAnalyzer
{
    /// <inheritdoc />
    public string Name => "Architecture Conformance";

    /// <summary>
    /// Maps layer name segments to their ordinal position in Clean Architecture.
    /// Lower numbers represent inner layers; higher numbers represent outer layers.
    /// Inner layers must never reference outer layers.
    /// </summary>
    private static readonly Dictionary<string, int> LayerOrder = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Domain"] = 0,
        ["Core"] = 0,
        ["Application"] = 1,
        ["Infrastructure"] = 2,
        ["Persistence"] = 2,
        ["Api"] = 3,
        ["Presentation"] = 3,
        ["Web"] = 3,
        ["Blazor"] = 3,
    };

    /// <summary>
    /// Forbidden namespace patterns for each layer, keyed by layer name.
    /// A domain project must not use namespaces from Application, Infrastructure, or Api layers.
    /// </summary>
    private static readonly Dictionary<string, string[]> ForbiddenNamespaces = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Domain"] = ["Infrastructure", "Persistence", "Api", "Presentation", "Application", "Web", "Blazor"],
        ["Core"] = ["Infrastructure", "Persistence", "Api", "Presentation", "Application", "Web", "Blazor"],
        ["Application"] = ["Infrastructure", "Persistence", "Api", "Presentation", "Web", "Blazor"],
        ["Infrastructure"] = ["Api", "Presentation", "Web", "Blazor"],
        ["Persistence"] = ["Api", "Presentation", "Web", "Blazor"],
    };

    private static readonly Regex UsingDirectiveRegex = new(
        @"^\s*using\s+(?!static\s)(?<ns>[A-Za-z_][\w.]*)\s*;",
        RegexOptions.Compiled | RegexOptions.Multiline);

    private static readonly Regex NewConcreteRegex = new(
        @"new\s+(?<type>[A-Z]\w*(?:Repository|Service|Context|DbContext|Handler|Adapter|Client|Provider))\s*\(",
        RegexOptions.Compiled);

    private static readonly Regex ConstructorParamRegex = new(
        @"(?:public|internal|protected)\s+\w+\s*\((?<params>[^)]+)\)",
        RegexOptions.Compiled);

    /// <inheritdoc />
    public async Task<AnalysisResult> AnalyzeAsync(string projectPath, CancellationToken ct = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var findings = new List<AnalysisFinding>();
        var filesScanned = 0;

        var csprojFiles = Directory.GetFiles(projectPath, "*.csproj", SearchOption.AllDirectories)
            .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}")
                     && !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}")
                     && !f.Contains($"{Path.DirectorySeparatorChar}.auto-claude{Path.DirectorySeparatorChar}"))
            .ToList();

        // Step 1: Build project dependency graph from .csproj files and check layer violations
        var projectLayerMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var csproj in csprojFiles)
        {
            ct.ThrowIfCancellationRequested();
            var projectName = Path.GetFileNameWithoutExtension(csproj);
            var layer = DetectLayer(projectName);
            if (layer is not null)
            {
                projectLayerMap[projectName] = layer;
            }
        }

        foreach (var csproj in csprojFiles)
        {
            ct.ThrowIfCancellationRequested();
            filesScanned++;

            var projectName = Path.GetFileNameWithoutExtension(csproj);
            var sourceLayer = DetectLayer(projectName);
            if (sourceLayer is null) continue;

            var referencedProjects = GetProjectReferences(csproj);
            foreach (var refProject in referencedProjects)
            {
                var refName = Path.GetFileNameWithoutExtension(refProject);
                var targetLayer = DetectLayer(refName);
                if (targetLayer is null) continue;

                if (!LayerOrder.TryGetValue(sourceLayer, out var sourceOrd) ||
                    !LayerOrder.TryGetValue(targetLayer, out var targetOrd))
                    continue;

                if (sourceOrd < targetOrd)
                {
                    var ruleId = GetProjectRefRuleId(sourceLayer);
                    findings.Add(new AnalysisFinding
                    {
                        FilePath = Path.GetRelativePath(projectPath, csproj),
                        LineNumber = FindProjectReferenceLine(csproj, refProject),
                        Severity = FindingSeverity.Error,
                        Category = "Architecture",
                        RuleId = ruleId,
                        Message = $"{sourceLayer} layer project '{projectName}' references {targetLayer} layer project '{refName}'. " +
                                  $"Inner layers must not depend on outer layers.",
                        Suggestion = $"Remove the ProjectReference to '{refName}'. Use dependency inversion (interfaces in {sourceLayer}, implementations in {targetLayer}) instead.",
                        CodeSnippet = $"<ProjectReference Include=\"...{refName}.csproj\" />"
                    });
                }
            }
        }

        // Step 2: Scan .cs files for using directive violations and concrete class usage
        var csFiles = Directory.GetFiles(projectPath, "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}")
                     && !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}")
                     && !f.Contains($"{Path.DirectorySeparatorChar}.auto-claude{Path.DirectorySeparatorChar}")
                     && !f.EndsWith(".g.cs")
                     && !f.EndsWith(".AssemblyInfo.cs"))
            .ToList();

        foreach (var csFile in csFiles)
        {
            ct.ThrowIfCancellationRequested();
            filesScanned++;

            var fileLayer = DetectLayerFromPath(csFile);
            if (fileLayer is null) continue;

            var lines = await File.ReadAllLinesAsync(csFile, ct);
            var relativePath = Path.GetRelativePath(projectPath, csFile);

            // Check using directives
            if (ForbiddenNamespaces.TryGetValue(fileLayer, out var forbidden))
            {
                for (var i = 0; i < lines.Length; i++)
                {
                    var line = lines[i];
                    var match = UsingDirectiveRegex.Match(line);
                    if (!match.Success) continue;

                    var ns = match.Groups["ns"].Value;
                    foreach (var forbiddenLayer in forbidden)
                    {
                        // Check if the namespace contains a forbidden layer segment
                        if (NamespaceContainsLayer(ns, forbiddenLayer))
                        {
                            var ruleId = GetUsingRuleId(fileLayer);
                            findings.Add(new AnalysisFinding
                            {
                                FilePath = relativePath,
                                LineNumber = i + 1,
                                Severity = FindingSeverity.Error,
                                Category = "Architecture",
                                RuleId = ruleId,
                                Message = $"{fileLayer} layer file uses namespace '{ns}' which belongs to the {forbiddenLayer} layer. " +
                                          $"This violates Clean Architecture dependency rules.",
                                Suggestion = $"Remove the using directive for '{ns}'. Define abstractions in the {fileLayer} layer and implement them in {forbiddenLayer}.",
                                CodeSnippet = line.Trim()
                            });
                            break;
                        }
                    }
                }
            }

            // ARCH004: Check for cross-layer direct instantiation
            if (fileLayer is "Domain" or "Core" or "Application")
            {
                for (var i = 0; i < lines.Length; i++)
                {
                    var line = lines[i];
                    var newMatch = NewConcreteRegex.Match(line);
                    if (newMatch.Success)
                    {
                        var typeName = newMatch.Groups["type"].Value;
                        findings.Add(new AnalysisFinding
                        {
                            FilePath = relativePath,
                            LineNumber = i + 1,
                            Severity = FindingSeverity.Warning,
                            Category = "Architecture",
                            RuleId = "ARCH004",
                            Message = $"Direct instantiation of infrastructure-style type '{typeName}' in {fileLayer} layer. " +
                                      $"This couples the {fileLayer} layer to concrete implementations.",
                            Suggestion = $"Use dependency injection instead. Inject an interface (e.g., I{typeName}) through the constructor.",
                            CodeSnippet = line.Trim()
                        });
                    }
                }
            }

            // ARCH005: Check for concrete class injection in constructors (missing dependency inversion)
            if (fileLayer is "Application")
            {
                var fullContent = string.Join(Environment.NewLine, lines);
                var ctorMatches = ConstructorParamRegex.Matches(fullContent);
                foreach (Match ctorMatch in ctorMatches)
                {
                    var paramsText = ctorMatch.Groups["params"].Value;
                    var parameters = paramsText.Split(',', StringSplitOptions.TrimEntries);
                    foreach (var param in parameters)
                    {
                        var parts = param.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length < 2) continue;

                        var typePart = parts[0];
                        // Skip if it's an interface, generic, primitive, or string
                        if (typePart.StartsWith("I") && typePart.Length > 1 && char.IsUpper(typePart[1]))
                            continue;
                        if (typePart.Contains('<') || typePart.Contains('?'))
                            continue;
                        if (IsCommonNonServiceType(typePart))
                            continue;

                        // Check if this looks like a concrete service/repository class
                        if (IsLikelyConcreteServiceType(typePart))
                        {
                            var lineNumber = FindLineInContent(lines, param.Trim());
                            findings.Add(new AnalysisFinding
                            {
                                FilePath = relativePath,
                                LineNumber = lineNumber,
                                Severity = FindingSeverity.Warning,
                                Category = "Architecture",
                                RuleId = "ARCH005",
                                Message = $"Constructor parameter uses concrete type '{typePart}' instead of an interface. " +
                                          $"This violates the Dependency Inversion Principle.",
                                Suggestion = $"Define an interface 'I{typePart}' and inject that instead. Register the mapping in the DI container.",
                                CodeSnippet = param.Trim()
                            });
                        }
                    }
                }
            }
        }

        stopwatch.Stop();

        return new AnalysisResult
        {
            AnalyzerName = Name,
            Findings = findings,
            Duration = stopwatch.Elapsed,
            FilesScanned = filesScanned,
        };
    }

    /// <summary>
    /// Detects the Clean Architecture layer from a project name.
    /// Looks for known layer name segments in the project name.
    /// </summary>
    private static string? DetectLayer(string projectName)
    {
        // Check from most specific to least specific
        foreach (var kvp in LayerOrder.OrderByDescending(k => k.Key.Length))
        {
            if (projectName.Contains(kvp.Key, StringComparison.OrdinalIgnoreCase))
            {
                return kvp.Key;
            }
        }
        return null;
    }

    /// <summary>
    /// Detects the layer from a file's directory path.
    /// </summary>
    private static string? DetectLayerFromPath(string filePath)
    {
        var normalizedPath = filePath.Replace('\\', '/');
        foreach (var kvp in LayerOrder.OrderByDescending(k => k.Key.Length))
        {
            // Check for layer name as a directory segment or project name segment
            if (normalizedPath.Contains($".{kvp.Key}/", StringComparison.OrdinalIgnoreCase) ||
                normalizedPath.Contains($".{kvp.Key}\\", StringComparison.OrdinalIgnoreCase) ||
                normalizedPath.Contains($"/{kvp.Key}/", StringComparison.OrdinalIgnoreCase) ||
                normalizedPath.Contains($"\\{kvp.Key}\\", StringComparison.OrdinalIgnoreCase) ||
                normalizedPath.Contains($".{kvp.Key}.csproj", StringComparison.OrdinalIgnoreCase))
            {
                return kvp.Key;
            }
        }
        return null;
    }

    /// <summary>
    /// Checks if a namespace string contains a layer segment (as a dotted component).
    /// </summary>
    private static bool NamespaceContainsLayer(string ns, string layerName)
    {
        var segments = ns.Split('.');
        return segments.Any(s => s.Equals(layerName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Gets all ProjectReference paths from a .csproj file.
    /// </summary>
    private static List<string> GetProjectReferences(string csprojPath)
    {
        var references = new List<string>();
        try
        {
            var doc = XDocument.Load(csprojPath);
            var projectDir = Path.GetDirectoryName(csprojPath) ?? ".";

            foreach (var projectRef in doc.Descendants("ProjectReference"))
            {
                var includeAttr = projectRef.Attribute("Include");
                if (includeAttr is not null)
                {
                    var refPath = includeAttr.Value.Replace('\\', Path.DirectorySeparatorChar);
                    var fullPath = Path.GetFullPath(Path.Combine(projectDir, refPath));
                    references.Add(fullPath);
                }
            }
        }
        catch
        {
            // Skip unparseable project files
        }
        return references;
    }

    /// <summary>
    /// Finds the line number of a ProjectReference in a .csproj file.
    /// </summary>
    private static int FindProjectReferenceLine(string csprojPath, string referencePath)
    {
        try
        {
            var refName = Path.GetFileNameWithoutExtension(referencePath);
            var lines = File.ReadAllLines(csprojPath);
            for (var i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains("ProjectReference", StringComparison.OrdinalIgnoreCase) &&
                    lines[i].Contains(refName, StringComparison.OrdinalIgnoreCase))
                {
                    return i + 1;
                }
            }
        }
        catch
        {
            // Ignore
        }
        return 1;
    }

    /// <summary>
    /// Returns the appropriate ARCH rule ID for ProjectReference violations based on source layer.
    /// </summary>
    private static string GetProjectRefRuleId(string sourceLayer) => sourceLayer switch
    {
        "Domain" or "Core" => "ARCH001",
        "Application" => "ARCH002",
        "Infrastructure" or "Persistence" => "ARCH003",
        _ => "ARCH001"
    };

    /// <summary>
    /// Returns the appropriate ARCH rule ID for using directive violations based on source layer.
    /// </summary>
    private static string GetUsingRuleId(string sourceLayer) => sourceLayer switch
    {
        "Domain" or "Core" => "ARCH001",
        "Application" => "ARCH002",
        "Infrastructure" or "Persistence" => "ARCH003",
        _ => "ARCH001"
    };

    /// <summary>
    /// Finds the one-based line number where a string appears in lines of code.
    /// </summary>
    private static int FindLineInContent(string[] lines, string searchText)
    {
        for (var i = 0; i < lines.Length; i++)
        {
            if (lines[i].Contains(searchText, StringComparison.Ordinal))
            {
                return i + 1;
            }
        }
        return 1;
    }

    /// <summary>
    /// Returns true for common types that are not services and should not be flagged.
    /// </summary>
    private static bool IsCommonNonServiceType(string typeName) =>
        typeName is "string" or "int" or "long" or "bool" or "decimal" or "double" or "float"
            or "Guid" or "DateTime" or "DateTimeOffset" or "TimeSpan" or "CancellationToken"
            or "byte" or "char" or "object" or "Uri" or "Type";

    /// <summary>
    /// Returns true if the type name looks like a concrete service, repository, or handler class.
    /// </summary>
    private static bool IsLikelyConcreteServiceType(string typeName) =>
        typeName.EndsWith("Repository", StringComparison.Ordinal) ||
        typeName.EndsWith("Service", StringComparison.Ordinal) ||
        typeName.EndsWith("Handler", StringComparison.Ordinal) ||
        typeName.EndsWith("Provider", StringComparison.Ordinal) ||
        typeName.EndsWith("Client", StringComparison.Ordinal) ||
        typeName.EndsWith("Adapter", StringComparison.Ordinal) ||
        typeName.EndsWith("Context", StringComparison.Ordinal) ||
        typeName.EndsWith("DbContext", StringComparison.Ordinal);
}
