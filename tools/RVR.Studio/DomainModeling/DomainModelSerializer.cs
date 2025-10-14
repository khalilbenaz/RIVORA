namespace RVR.Studio.DomainModeling;

using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Serializes and deserializes <see cref="DomainModel"/> instances to/from JSON,
/// and exports to Mermaid, PlantUML, and C# source files.
/// </summary>
public static class DomainModelSerializer
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    // ── JSON Persistence ───────────────────────────────────────────────

    /// <summary>
    /// Saves a domain model to a JSON file.
    /// </summary>
    public static async Task SaveToJsonAsync(DomainModel model, string filePath)
    {
        var json = JsonSerializer.Serialize(model, JsonOptions);
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);
        await File.WriteAllTextAsync(filePath, json);
    }

    /// <summary>
    /// Saves a domain model to a JSON file (synchronous).
    /// </summary>
    public static void SaveToJson(DomainModel model, string filePath)
    {
        var json = JsonSerializer.Serialize(model, JsonOptions);
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);
        File.WriteAllText(filePath, json);
    }

    /// <summary>
    /// Loads a domain model from a JSON file.
    /// </summary>
    public static async Task<DomainModel> LoadFromJsonAsync(string filePath)
    {
        var json = await File.ReadAllTextAsync(filePath);
        return JsonSerializer.Deserialize<DomainModel>(json, JsonOptions)
            ?? throw new InvalidOperationException($"Failed to deserialize domain model from '{filePath}'.");
    }

    /// <summary>
    /// Loads a domain model from a JSON file (synchronous).
    /// </summary>
    public static DomainModel LoadFromJson(string filePath)
    {
        var json = File.ReadAllText(filePath);
        return JsonSerializer.Deserialize<DomainModel>(json, JsonOptions)
            ?? throw new InvalidOperationException($"Failed to deserialize domain model from '{filePath}'.");
    }

    /// <summary>
    /// Serializes a domain model to a JSON string.
    /// </summary>
    public static string ToJson(DomainModel model)
    {
        return JsonSerializer.Serialize(model, JsonOptions);
    }

    /// <summary>
    /// Deserializes a domain model from a JSON string.
    /// </summary>
    public static DomainModel FromJson(string json)
    {
        return JsonSerializer.Deserialize<DomainModel>(json, JsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize domain model from JSON string.");
    }

    // ── Mermaid Export ─────────────────────────────────────────────────

    /// <summary>
    /// Exports the domain model as a Mermaid class diagram.
    /// </summary>
    public static string ExportToMermaid(DomainModel model)
    {
        return MermaidGenerator.Generate(model);
    }

    // ── PlantUML Export ────────────────────────────────────────────────

    /// <summary>
    /// Exports the domain model as a PlantUML class diagram.
    /// </summary>
    public static string ExportToPlantUml(DomainModel model)
    {
        var sb = new StringBuilder();
        sb.AppendLine("@startuml");
        sb.AppendLine("skinparam classAttributeIconSize 0");
        sb.AppendLine();

        foreach (var context in model.BoundedContexts)
        {
            sb.AppendLine($"package \"{context.Name}\" {{");
            sb.AppendLine();

            // Entities
            foreach (var entity in context.Entities)
            {
                var stereotype = entity.IsAggregateRoot ? " <<AggregateRoot>>" : "";
                sb.AppendLine($"    class {entity.Name}{stereotype} {{");

                foreach (var prop in entity.Properties)
                {
                    var typeStr = prop.IsCollection ? $"List<{prop.Type}>" : prop.Type;
                    var required = prop.IsRequired ? " {required}" : "";
                    sb.AppendLine($"        +{typeStr} {prop.Name}{required}");
                }

                foreach (var method in entity.Methods)
                {
                    sb.AppendLine($"        +{method}()");
                }

                sb.AppendLine("    }");
                sb.AppendLine();
            }

            // Value Objects
            foreach (var vo in context.ValueObjects)
            {
                sb.AppendLine($"    class {vo.Name} <<ValueObject>> {{");
                foreach (var prop in vo.Properties)
                {
                    var typeStr = prop.IsCollection ? $"List<{prop.Type}>" : prop.Type;
                    sb.AppendLine($"        +{typeStr} {prop.Name}");
                }
                sb.AppendLine("    }");
                sb.AppendLine();
            }

            // Domain Events
            foreach (var evt in context.DomainEvents)
            {
                sb.AppendLine($"    class {evt.Name} <<DomainEvent>> {{");
                foreach (var prop in evt.Properties)
                {
                    var typeStr = prop.IsCollection ? $"List<{prop.Type}>" : prop.Type;
                    sb.AppendLine($"        +{typeStr} {prop.Name}");
                }
                sb.AppendLine("    }");
                sb.AppendLine();

                // Link event to trigger entity
                if (!string.IsNullOrWhiteSpace(evt.TriggerEntity))
                {
                    sb.AppendLine($"    {evt.TriggerEntity} ..> {evt.Name} : raises");
                }
            }

            // Relationships
            foreach (var rel in context.Relationships)
            {
                var arrow = rel.Type switch
                {
                    RelationshipType.OneToOne => "\"1\" --> \"1\"",
                    RelationshipType.OneToMany => "\"1\" --> \"*\"",
                    RelationshipType.ManyToMany => "\"*\" --> \"*\"",
                    _ => "-->"
                };

                var label = !string.IsNullOrWhiteSpace(rel.NavigationProperty)
                    ? $" : {rel.NavigationProperty}"
                    : "";

                sb.AppendLine($"    {rel.FromEntity} {arrow} {rel.ToEntity}{label}");
            }

            sb.AppendLine("}");
            sb.AppendLine();
        }

        sb.AppendLine("@enduml");
        return sb.ToString();
    }

    // ── C# Export ──────────────────────────────────────────────────────

    /// <summary>
    /// Exports the domain model as C# entity files written to the specified output path.
    /// Returns a dictionary of relative file paths to their generated content.
    /// </summary>
    public static Dictionary<string, string> ExportToCSharp(DomainModel model, string? outputPath = null)
    {
        var files = CodeGenerator.GenerateAll(model);

        if (!string.IsNullOrWhiteSpace(outputPath))
        {
            CodeGenerator.WriteAllToDisk(files, outputPath);
        }

        return files;
    }
}
