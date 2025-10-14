using System.Diagnostics;
using System.Text.RegularExpressions;
using RVR.CLI.Analyzers.Models;

namespace RVR.CLI.Analyzers;

/// <summary>
/// Detects Domain-Driven Design anti-patterns such as anemic models,
/// encapsulation violations, leaked domain logic, and missing DDD building blocks.
/// </summary>
public class DddAnalyzer : IAnalyzer
{
    /// <inheritdoc />
    public string Name => "DDD Anti-Pattern Detection";

    // ── Regex patterns ───────────────────────────────────────────────────

    /// <summary>Matches class declarations that inherit from a base type.</summary>
    private static readonly Regex ClassDeclarationRegex = new(
        @"^\s*(?:public|internal|protected)?\s*(?:sealed|abstract)?\s*(?:partial\s+)?class\s+(?<name>\w+)\s*(?:<[^>]+>)?\s*:\s*(?<bases>[^\{]+)",
        RegexOptions.Compiled);

    /// <summary>Matches a simple auto-property (get; set; or get; init;).</summary>
    private static readonly Regex AutoPropertyRegex = new(
        @"^\s*public\s+[\w<>\[\]?,\s]+\s+\w+\s*\{\s*get;\s*(?:set|init);\s*\}",
        RegexOptions.Compiled);

    /// <summary>Matches a property with a private or protected setter.</summary>
    private static readonly Regex PrivateSetterRegex = new(
        @"\{\s*get;\s*(?:private|protected)\s+set;\s*\}",
        RegexOptions.Compiled);

    /// <summary>Matches a property with a plain public setter (no access modifier on set).</summary>
    private static readonly Regex PublicSetterPropertyRegex = new(
        @"^\s*public\s+[\w<>\[\]?,\s]+\s+(?<propName>\w+)\s*\{\s*get;\s*set;\s*\}",
        RegexOptions.Compiled);

    /// <summary>Matches a public method declaration (not property, not constructor).</summary>
    private static readonly Regex PublicMethodRegex = new(
        @"^\s*public\s+(?!class\s|interface\s|enum\s|record\s|struct\s|static\s+class)[\w<>\[\]?,\s]+\s+\w+\s*\(",
        RegexOptions.Compiled);

    /// <summary>Matches EF Core / data annotation attributes that indicate infrastructure concerns.</summary>
    private static readonly Regex InfraAttributeRegex = new(
        @"^\s*\[\s*(?:Table|Column|Key|ForeignKey|Index|Required|MaxLength|StringLength|DatabaseGenerated|NotMapped|InverseProperty|Owned)\b",
        RegexOptions.Compiled);

    /// <summary>Matches DbContext usage in code.</summary>
    private static readonly Regex DbContextUsageRegex = new(
        @"\bDbContext\b|\.SaveChanges|\.Set<|\.Database\b|\.ChangeTracker\b",
        RegexOptions.Compiled);

    /// <summary>Matches a public List/Collection property (not IReadOnlyList / IReadOnlyCollection).</summary>
    private static readonly Regex PublicMutableCollectionRegex = new(
        @"^\s*public\s+(?:List|IList|ICollection|Collection|HashSet|ISet|ObservableCollection)<(?<element>[^>]+)>\s+(?<propName>\w+)\s*\{",
        RegexOptions.Compiled);

    /// <summary>Matches a public parameterless constructor.</summary>
    private static readonly Regex PublicParameterlessCtorRegex = new(
        @"^\s*public\s+(?<name>\w+)\s*\(\s*\)\s*(?:\{|:)",
        RegexOptions.Compiled);

    /// <summary>Known entity / aggregate root base types that identify domain entities.</summary>
    private static readonly HashSet<string> EntityBaseTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "Entity",
        "AggregateRoot",
        "BaseEntity",
        "AuditableEntity",
        "DomainEntity",
        "Aggregate",
    };

    /// <summary>Known value object base types.</summary>
    private static readonly HashSet<string> ValueObjectBaseTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "ValueObject",
        "BaseValueObject",
    };

    /// <summary>Known aggregate root base types (subset of entity base types).</summary>
    private static readonly HashSet<string> AggregateRootBaseTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "AggregateRoot",
        "Aggregate",
    };

    /// <summary>Business logic indicators found in non-domain layers.</summary>
    private static readonly Regex BusinessLogicIndicatorRegex = new(
        @"(?:\.IsValid\b|\.Validate\b|\.CanBe\w+|\.Calculate\w+|\.Apply\w+Rule|if\s*\([^)]*\.Status\s*[!=]=|\.ChangeStatus\(|\.Transition\w+|\.Approve\(|\.Reject\()",
        RegexOptions.Compiled);

    /// <inheritdoc />
    public async Task<AnalysisResult> AnalyzeAsync(string projectPath, CancellationToken ct = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var findings = new List<AnalysisFinding>();
        var filesScanned = 0;

        // Collect all .cs files, excluding generated/build artifacts
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

            var lines = await File.ReadAllLinesAsync(csFile, ct);
            var relativePath = Path.GetRelativePath(projectPath, csFile);
            var isDomainFile = IsDomainFile(csFile);
            var isApplicationFile = IsApplicationOrServiceFile(csFile);

            // Parse class declarations to find entities, aggregates, value objects
            var classInfos = ParseClassInfos(lines);

            foreach (var classInfo in classInfos)
            {
                var isEntity = classInfo.BaseTypes.Any(b => EntityBaseTypes.Contains(b));
                var isAggregateRoot = classInfo.BaseTypes.Any(b => AggregateRootBaseTypes.Contains(b));
                var isValueObject = classInfo.BaseTypes.Any(b => ValueObjectBaseTypes.Contains(b));

                // Also detect entities by folder convention
                if (!isEntity && isDomainFile &&
                    (csFile.Contains($"{Path.DirectorySeparatorChar}Entities{Path.DirectorySeparatorChar}") ||
                     csFile.Contains($"{Path.DirectorySeparatorChar}Aggregates{Path.DirectorySeparatorChar}")))
                {
                    isEntity = true;
                }

                if (isEntity || (isDomainFile && classInfo.BaseTypes.Count > 0))
                {
                    // DDD001: Anemic domain model
                    CheckAnemicModel(findings, relativePath, classInfo, lines);

                    // DDD003: Entity without private setters
                    CheckPublicSetters(findings, relativePath, classInfo, lines);

                    // DDD006: Public mutable collection properties
                    CheckMutableCollections(findings, relativePath, classInfo, lines);

                    // DDD007: Public parameterless constructor
                    CheckPublicParameterlessCtor(findings, relativePath, classInfo, lines);
                }

                if (isDomainFile)
                {
                    // DDD004: Infrastructure concerns in domain layer
                    CheckInfrastructureConcerns(findings, relativePath, classInfo, lines);
                }

                if (isAggregateRoot)
                {
                    // DDD005: Missing domain events
                    CheckMissingDomainEvents(findings, relativePath, classInfo, lines);
                }

                if (isValueObject)
                {
                    // DDD008: Value object not implementing Equals/GetHashCode
                    CheckValueObjectEquality(findings, relativePath, classInfo, lines);
                }
            }

            // DDD002: Domain logic leaked to Application/Infrastructure
            if (isApplicationFile)
            {
                CheckLeakedDomainLogic(findings, relativePath, lines);
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

    // ── Detection methods ────────────────────────────────────────────────

    /// <summary>
    /// DDD001: Detects anemic domain models -- entities with many properties but few or no methods.
    /// </summary>
    private static void CheckAnemicModel(
        List<AnalysisFinding> findings, string filePath, ClassInfo classInfo, string[] lines)
    {
        if (classInfo.PropertyCount == 0) return;

        // An entity is considered anemic if it has properties but no meaningful public methods
        // (excluding property accessors and overrides like ToString/Equals)
        if (classInfo.PropertyCount >= 2 && classInfo.MethodCount == 0)
        {
            findings.Add(new AnalysisFinding
            {
                FilePath = filePath,
                LineNumber = classInfo.DeclarationLine,
                Severity = FindingSeverity.Warning,
                Category = "DDD",
                RuleId = "DDD001",
                Message = $"Entity '{classInfo.Name}' has {classInfo.PropertyCount} properties but no domain methods. " +
                          $"This is an anemic domain model anti-pattern.",
                Suggestion = "Add domain behavior methods that encapsulate business rules and state transitions. " +
                             "Move logic out of application services and into the entity itself.",
                CodeSnippet = lines[classInfo.DeclarationLine - 1].Trim()
            });
        }
    }

    /// <summary>
    /// DDD002: Detects business logic that has leaked into application or infrastructure layers.
    /// </summary>
    private static void CheckLeakedDomainLogic(
        List<AnalysisFinding> findings, string filePath, string[] lines)
    {
        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            if (BusinessLogicIndicatorRegex.IsMatch(line))
            {
                findings.Add(new AnalysisFinding
                {
                    FilePath = filePath,
                    LineNumber = i + 1,
                    Severity = FindingSeverity.Warning,
                    Category = "DDD",
                    RuleId = "DDD002",
                    Message = "Possible domain logic in application/infrastructure layer. " +
                              "Business rules like validation, status transitions, and calculations should live in domain entities.",
                    Suggestion = "Move this business logic into the relevant domain entity or domain service. " +
                                 "Application services should only orchestrate, not contain business rules.",
                    CodeSnippet = line.Trim()
                });
            }
        }
    }

    /// <summary>
    /// DDD003: Detects entity properties that have public setters instead of private/protected setters.
    /// </summary>
    private static void CheckPublicSetters(
        List<AnalysisFinding> findings, string filePath, ClassInfo classInfo, string[] lines)
    {
        for (var i = classInfo.DeclarationLine - 1; i < Math.Min(classInfo.ClassEndLine, lines.Length); i++)
        {
            var line = lines[i];
            if (PublicSetterPropertyRegex.IsMatch(line) && !PrivateSetterRegex.IsMatch(line))
            {
                var match = PublicSetterPropertyRegex.Match(line);
                var propName = match.Groups["propName"].Value;

                // Skip Id properties as they commonly need public setters for ORM
                if (propName.Equals("Id", StringComparison.OrdinalIgnoreCase)) continue;

                findings.Add(new AnalysisFinding
                {
                    FilePath = filePath,
                    LineNumber = i + 1,
                    Severity = FindingSeverity.Warning,
                    Category = "DDD",
                    RuleId = "DDD003",
                    Message = $"Property '{propName}' on entity '{classInfo.Name}' has a public setter. " +
                              $"Entity state should be modified only through domain methods.",
                    Suggestion = $"Change to 'private set;' or 'init;' and provide a domain method to modify this property. " +
                                 $"For example: public void Update{propName}({GetSimpleTypeName(line)} value) {{ {propName} = value; }}",
                    CodeSnippet = line.Trim()
                });
            }
        }
    }

    /// <summary>
    /// DDD004: Detects infrastructure concerns (EF attributes, DbContext usage) in domain entities.
    /// </summary>
    private static void CheckInfrastructureConcerns(
        List<AnalysisFinding> findings, string filePath, ClassInfo classInfo, string[] lines)
    {
        for (var i = classInfo.DeclarationLine - 1; i < Math.Min(classInfo.ClassEndLine, lines.Length); i++)
        {
            var line = lines[i];

            // Check for EF/data annotation attributes
            if (InfraAttributeRegex.IsMatch(line))
            {
                findings.Add(new AnalysisFinding
                {
                    FilePath = filePath,
                    LineNumber = i + 1,
                    Severity = FindingSeverity.Error,
                    Category = "DDD",
                    RuleId = "DDD004",
                    Message = $"Domain entity '{classInfo.Name}' contains infrastructure attribute. " +
                              $"Data mapping attributes belong in the Infrastructure layer.",
                    Suggestion = "Remove the data annotation attribute and use EF Core Fluent API configuration " +
                                 "in the Infrastructure layer instead (e.g., in an IEntityTypeConfiguration<T> class).",
                    CodeSnippet = line.Trim()
                });
            }

            // Check for DbContext usage
            if (DbContextUsageRegex.IsMatch(line))
            {
                findings.Add(new AnalysisFinding
                {
                    FilePath = filePath,
                    LineNumber = i + 1,
                    Severity = FindingSeverity.Critical,
                    Category = "DDD",
                    RuleId = "DDD004",
                    Message = $"Domain entity '{classInfo.Name}' references DbContext or persistence operations. " +
                              $"Domain entities must be persistence-ignorant.",
                    Suggestion = "Remove all DbContext references from the domain layer. " +
                                 "Use repository interfaces defined in the domain and implemented in the infrastructure layer.",
                    CodeSnippet = line.Trim()
                });
            }
        }
    }

    /// <summary>
    /// DDD005: Detects aggregate roots that do not raise or store domain events.
    /// </summary>
    private static void CheckMissingDomainEvents(
        List<AnalysisFinding> findings, string filePath, ClassInfo classInfo, string[] lines)
    {
        var hasDomainEvents = false;
        for (var i = classInfo.DeclarationLine - 1; i < Math.Min(classInfo.ClassEndLine, lines.Length); i++)
        {
            var line = lines[i];
            if (line.Contains("DomainEvent", StringComparison.OrdinalIgnoreCase) ||
                line.Contains("DomainEvents", StringComparison.OrdinalIgnoreCase) ||
                line.Contains("AddDomainEvent", StringComparison.OrdinalIgnoreCase) ||
                line.Contains("RaiseDomainEvent", StringComparison.OrdinalIgnoreCase) ||
                line.Contains("IDomainEvent", StringComparison.OrdinalIgnoreCase) ||
                line.Contains("_domainEvents", StringComparison.OrdinalIgnoreCase))
            {
                hasDomainEvents = true;
                break;
            }
        }

        if (!hasDomainEvents)
        {
            findings.Add(new AnalysisFinding
            {
                FilePath = filePath,
                LineNumber = classInfo.DeclarationLine,
                Severity = FindingSeverity.Info,
                Category = "DDD",
                RuleId = "DDD005",
                Message = $"Aggregate root '{classInfo.Name}' does not appear to use domain events. " +
                          $"Domain events are important for decoupling aggregates and triggering side effects.",
                Suggestion = "Consider adding domain events for significant state changes. " +
                             "Add a DomainEvents collection and raise events in state-changing methods.",
                CodeSnippet = lines[classInfo.DeclarationLine - 1].Trim()
            });
        }
    }

    /// <summary>
    /// DDD006: Detects public mutable collection properties that should use IReadOnlyCollection or IReadOnlyList.
    /// </summary>
    private static void CheckMutableCollections(
        List<AnalysisFinding> findings, string filePath, ClassInfo classInfo, string[] lines)
    {
        for (var i = classInfo.DeclarationLine - 1; i < Math.Min(classInfo.ClassEndLine, lines.Length); i++)
        {
            var line = lines[i];
            var match = PublicMutableCollectionRegex.Match(line);
            if (match.Success)
            {
                var propName = match.Groups["propName"].Value;
                var elementType = match.Groups["element"].Value;

                findings.Add(new AnalysisFinding
                {
                    FilePath = filePath,
                    LineNumber = i + 1,
                    Severity = FindingSeverity.Warning,
                    Category = "DDD",
                    RuleId = "DDD006",
                    Message = $"Property '{propName}' on entity '{classInfo.Name}' exposes a mutable collection. " +
                              $"External code can modify the collection without going through domain methods.",
                    Suggestion = $"Use a private backing field (e.g., private readonly List<{elementType}> _{char.ToLower(propName[0])}{propName[1..]} = new()) " +
                                 $"and expose as IReadOnlyCollection<{elementType}> or IReadOnlyList<{elementType}>. " +
                                 $"Provide Add/Remove methods that enforce invariants.",
                    CodeSnippet = line.Trim()
                });
            }
        }
    }

    /// <summary>
    /// DDD007: Detects entities with a public parameterless constructor.
    /// Entities should enforce invariants through rich constructors or factory methods.
    /// </summary>
    private static void CheckPublicParameterlessCtor(
        List<AnalysisFinding> findings, string filePath, ClassInfo classInfo, string[] lines)
    {
        for (var i = classInfo.DeclarationLine - 1; i < Math.Min(classInfo.ClassEndLine, lines.Length); i++)
        {
            var line = lines[i];
            var match = PublicParameterlessCtorRegex.Match(line);
            if (match.Success && match.Groups["name"].Value == classInfo.Name)
            {
                findings.Add(new AnalysisFinding
                {
                    FilePath = filePath,
                    LineNumber = i + 1,
                    Severity = FindingSeverity.Warning,
                    Category = "DDD",
                    RuleId = "DDD007",
                    Message = $"Entity '{classInfo.Name}' has a public parameterless constructor. " +
                              $"Entities should enforce invariants at construction time.",
                    Suggestion = "Make the parameterless constructor private or protected (for ORM use) and provide " +
                                 "a constructor with required parameters, or use a static factory method like " +
                                 $"public static {classInfo.Name} Create(...) that validates invariants.",
                    CodeSnippet = line.Trim()
                });
            }
        }
    }

    /// <summary>
    /// DDD008: Detects value objects that do not implement Equals or GetHashCode.
    /// </summary>
    private static void CheckValueObjectEquality(
        List<AnalysisFinding> findings, string filePath, ClassInfo classInfo, string[] lines)
    {
        var hasEquals = false;
        var hasGetHashCode = false;

        for (var i = classInfo.DeclarationLine - 1; i < Math.Min(classInfo.ClassEndLine, lines.Length); i++)
        {
            var line = lines[i];
            if (line.Contains("Equals(", StringComparison.Ordinal) ||
                line.Contains("operator ==(", StringComparison.Ordinal) ||
                line.Contains("IEquatable<", StringComparison.Ordinal))
            {
                hasEquals = true;
            }
            if (line.Contains("GetHashCode(", StringComparison.Ordinal))
            {
                hasGetHashCode = true;
            }
        }

        // If the base class likely provides Equals/GetHashCode, skip the check.
        // Many ValueObject base classes handle this. Only flag if neither is found.
        if (!hasEquals && !hasGetHashCode)
        {
            // Check if the base class name suggests it already handles equality
            var baseHandlesEquality = classInfo.BaseTypes.Any(b =>
                b.Contains("ValueObject", StringComparison.OrdinalIgnoreCase) &&
                b != classInfo.Name);

            if (!baseHandlesEquality)
            {
                findings.Add(new AnalysisFinding
                {
                    FilePath = filePath,
                    LineNumber = classInfo.DeclarationLine,
                    Severity = FindingSeverity.Warning,
                    Category = "DDD",
                    RuleId = "DDD008",
                    Message = $"Value object '{classInfo.Name}' does not implement Equals or GetHashCode. " +
                              $"Value objects are compared by value, not by reference.",
                    Suggestion = "Override Equals(object) and GetHashCode(), or implement IEquatable<T>. " +
                                 "Consider also overriding == and != operators. " +
                                 "Alternatively, inherit from a ValueObject base class that provides equality semantics.",
                    CodeSnippet = lines[classInfo.DeclarationLine - 1].Trim()
                });
            }
        }
    }

    // ── Helper methods ───────────────────────────────────────────────────

    /// <summary>
    /// Parses class declarations, counting properties and methods within each class body.
    /// </summary>
    private static List<ClassInfo> ParseClassInfos(string[] lines)
    {
        var results = new List<ClassInfo>();
        var braceDepth = 0;
        ClassInfo? current = null;
        var classStartBraceDepth = 0;

        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i];

            // Track brace depth
            foreach (var ch in line)
            {
                if (ch == '{') braceDepth++;
                else if (ch == '}')
                {
                    braceDepth--;
                    if (current is not null && braceDepth < classStartBraceDepth)
                    {
                        current.ClassEndLine = i + 1;
                        results.Add(current);
                        current = null;
                    }
                }
            }

            // Try to match a class declaration
            var classMatch = ClassDeclarationRegex.Match(line);
            if (classMatch.Success)
            {
                var basesText = classMatch.Groups["bases"].Value;
                var baseTypes = basesText.Split(',', StringSplitOptions.TrimEntries)
                    .Select(b => b.Split('<')[0].Trim()) // remove generic params
                    .Where(b => !string.IsNullOrWhiteSpace(b))
                    .ToList();

                current = new ClassInfo
                {
                    Name = classMatch.Groups["name"].Value,
                    DeclarationLine = i + 1,
                    BaseTypes = baseTypes,
                };
                classStartBraceDepth = braceDepth;

                // Account for opening brace on the same line
                if (line.Contains('{'))
                {
                    classStartBraceDepth = braceDepth;
                }
                continue;
            }

            if (current is null) continue;

            // Only count members at the immediate class level (one brace deeper than class start)
            if (braceDepth != classStartBraceDepth + 1) continue;

            // Count auto-properties
            if (AutoPropertyRegex.IsMatch(line))
            {
                current.PropertyCount++;
            }
            // Count public methods (not property-like, not constructors)
            else if (PublicMethodRegex.IsMatch(line) && !line.Contains($" {current.Name}("))
            {
                // Exclude overrides of object methods
                if (!line.Contains(" ToString(") && !line.Contains(" Equals(") &&
                    !line.Contains(" GetHashCode("))
                {
                    current.MethodCount++;
                }
            }
        }

        // If we ended without closing the last class
        if (current is not null)
        {
            current.ClassEndLine = lines.Length;
            results.Add(current);
        }

        return results;
    }

    /// <summary>
    /// Determines if a file belongs to the Domain layer based on its path.
    /// </summary>
    private static bool IsDomainFile(string filePath)
    {
        var normalized = filePath.Replace('\\', '/');
        return normalized.Contains(".Domain/", StringComparison.OrdinalIgnoreCase) ||
               normalized.Contains(".Domain\\", StringComparison.OrdinalIgnoreCase) ||
               normalized.Contains("/Domain/", StringComparison.OrdinalIgnoreCase) ||
               normalized.Contains("\\Domain\\", StringComparison.OrdinalIgnoreCase) ||
               normalized.Contains(".Core/", StringComparison.OrdinalIgnoreCase) ||
               normalized.Contains(".Core\\", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Determines if a file belongs to the Application or Infrastructure layer.
    /// </summary>
    private static bool IsApplicationOrServiceFile(string filePath)
    {
        var normalized = filePath.Replace('\\', '/');
        return normalized.Contains(".Application/", StringComparison.OrdinalIgnoreCase) ||
               normalized.Contains(".Application\\", StringComparison.OrdinalIgnoreCase) ||
               normalized.Contains(".Infrastructure/", StringComparison.OrdinalIgnoreCase) ||
               normalized.Contains(".Infrastructure\\", StringComparison.OrdinalIgnoreCase) ||
               normalized.Contains("/Services/", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Extracts a simple type name from a property declaration line.
    /// </summary>
    private static string GetSimpleTypeName(string propertyLine)
    {
        var parts = propertyLine.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        // public Type PropertyName { get; set; }
        return parts.Length >= 2 ? parts[1] : "object";
    }

    /// <summary>
    /// Holds parsed information about a class declaration.
    /// </summary>
    private sealed class ClassInfo
    {
        public string Name { get; set; } = string.Empty;
        public int DeclarationLine { get; set; }
        public int ClassEndLine { get; set; }
        public List<string> BaseTypes { get; set; } = [];
        public int PropertyCount { get; set; }
        public int MethodCount { get; set; }
    }
}
