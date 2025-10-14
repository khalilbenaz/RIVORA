namespace RVR.Studio.DomainModeling;

using System.Text;

/// <summary>
/// Generates C# source code from a <see cref="DomainModel"/> using RVR.Framework conventions.
/// Produces entities, EF Core configurations, DTOs, and repository interfaces.
/// </summary>
public static class CodeGenerator
{
    /// <summary>
    /// Generates all code artifacts for a domain model and writes them to the specified output path.
    /// </summary>
    public static Dictionary<string, string> GenerateAll(DomainModel model)
    {
        var files = new Dictionary<string, string>();

        foreach (var context in model.BoundedContexts)
        {
            foreach (var entity in context.Entities)
            {
                var ns = SanitizeNamespace(context.Name);

                files[$"Domain/Entities/{ns}/{entity.Name}.cs"] = GenerateEntity(entity, ns);
                files[$"Infrastructure/Data/Configurations/{ns}/{entity.Name}Configuration.cs"] = GenerateEfCoreConfiguration(entity, ns);
                files[$"Application/DTOs/{ns}/{entity.Name}Dto.cs"] = GenerateDtos(entity, ns);
                files[$"Domain/Repositories/{ns}/I{entity.Name}Repository.cs"] = GenerateRepositoryInterface(entity, ns);
            }

            foreach (var vo in context.ValueObjects)
            {
                var ns = SanitizeNamespace(context.Name);
                files[$"Domain/ValueObjects/{ns}/{vo.Name}.cs"] = GenerateValueObject(vo, ns);
            }

            foreach (var evt in context.DomainEvents)
            {
                var ns = SanitizeNamespace(context.Name);
                files[$"Domain/Events/{ns}/{evt.Name}.cs"] = GenerateDomainEvent(evt, ns);
            }
        }

        return files;
    }

    /// <summary>
    /// Writes all generated files to disk under the given root output path.
    /// </summary>
    public static void WriteAllToDisk(Dictionary<string, string> files, string outputPath)
    {
        foreach (var (relativePath, content) in files)
        {
            var fullPath = Path.Combine(outputPath, relativePath);
            var directory = Path.GetDirectoryName(fullPath)!;
            Directory.CreateDirectory(directory);
            File.WriteAllText(fullPath, content);
        }
    }

    // ── Entity Generation ──────────────────────────────────────────────

    public static string GenerateEntity(EntityDefinition entity, string contextNamespace)
    {
        var sb = new StringBuilder();
        sb.AppendLine("using RVR.Framework.Core;");
        sb.AppendLine();
        sb.AppendLine($"namespace RVR.Framework.Domain.Entities.{contextNamespace};");
        sb.AppendLine();

        var baseClass = entity.IsAggregateRoot ? "AggregateRoot" : "Entity";
        sb.AppendLine($"public class {entity.Name} : {baseClass}");
        sb.AppendLine("{");

        // Properties
        foreach (var prop in entity.Properties)
        {
            var typeStr = FormatCSharpType(prop);
            var nullable = prop.IsRequired ? "" : "?";
            if (prop.Type == "string" && !prop.IsRequired) nullable = "?";
            sb.AppendLine($"    public {typeStr}{nullable} {prop.Name} {{ get; private set; }}{FormatDefaultValue(prop)}");
        }

        sb.AppendLine();

        // Protected parameterless constructor for EF Core
        sb.AppendLine($"    protected {entity.Name}() {{ }}");
        sb.AppendLine();

        // Constructor with required properties
        var requiredProps = entity.Properties.Where(p => p.IsRequired).ToList();
        if (requiredProps.Count > 0)
        {
            var ctorParams = string.Join(", ", requiredProps.Select(p => $"{FormatCSharpType(p)} {ToCamelCase(p.Name)}"));
            sb.AppendLine($"    public {entity.Name}({ctorParams})");
            sb.AppendLine("    {");
            foreach (var prop in requiredProps)
            {
                sb.AppendLine($"        {prop.Name} = {ToCamelCase(prop.Name)};");
            }
            sb.AppendLine("    }");
            sb.AppendLine();
        }

        // Update method
        if (requiredProps.Count > 0)
        {
            var updateParams = string.Join(", ", requiredProps.Select(p => $"{FormatCSharpType(p)} {ToCamelCase(p.Name)}"));
            sb.AppendLine($"    public void Update({updateParams})");
            sb.AppendLine("    {");
            foreach (var prop in requiredProps)
            {
                sb.AppendLine($"        {prop.Name} = {ToCamelCase(prop.Name)};");
            }
            sb.AppendLine("    }");
            sb.AppendLine();
        }

        // Domain methods
        foreach (var method in entity.Methods)
        {
            sb.AppendLine($"    public void {method}()");
            sb.AppendLine("    {");
            sb.AppendLine($"        // TODO: Implement {method}");
            sb.AppendLine("    }");
            sb.AppendLine();
        }

        // Domain event raising methods
        foreach (var eventName in entity.DomainEvents)
        {
            var methodName = eventName.StartsWith("On") ? eventName : $"Raise{eventName}";
            sb.AppendLine($"    public void {methodName}()");
            sb.AppendLine("    {");
            sb.AppendLine($"        RaiseDomainEvent(new {eventName}(Id));");
            sb.AppendLine("    }");
            sb.AppendLine();
        }

        sb.AppendLine("}");
        return sb.ToString();
    }

    // ── EF Core Configuration Generation ───────────────────────────────

    public static string GenerateEfCoreConfiguration(EntityDefinition entity, string contextNamespace)
    {
        var sb = new StringBuilder();
        sb.AppendLine("using Microsoft.EntityFrameworkCore;");
        sb.AppendLine("using Microsoft.EntityFrameworkCore.Metadata.Builders;");
        sb.AppendLine($"using RVR.Framework.Domain.Entities.{contextNamespace};");
        sb.AppendLine();
        sb.AppendLine($"namespace RVR.Framework.Infrastructure.Data.Configurations.{contextNamespace};");
        sb.AppendLine();
        sb.AppendLine($"public class {entity.Name}Configuration : IEntityTypeConfiguration<{entity.Name}>");
        sb.AppendLine("{");
        sb.AppendLine($"    public void Configure(EntityTypeBuilder<{entity.Name}> builder)");
        sb.AppendLine("    {");
        sb.AppendLine($"        builder.ToTable(\"{Pluralize(entity.Name)}\");");
        sb.AppendLine($"        builder.HasKey(x => x.Id);");
        sb.AppendLine();

        foreach (var prop in entity.Properties)
        {
            sb.Append($"        builder.Property(x => x.{prop.Name})");

            if (prop.IsRequired && prop.Type == "string")
                sb.Append(".IsRequired()");

            if (prop.Type == "string")
                sb.Append(".HasMaxLength(256)");

            if (prop.Type == "decimal")
                sb.Append(".HasColumnType(\"decimal(18,2)\")");

            sb.AppendLine(";");
        }

        sb.AppendLine("    }");
        sb.AppendLine("}");
        return sb.ToString();
    }

    // ── DTO Generation ─────────────────────────────────────────────────

    public static string GenerateDtos(EntityDefinition entity, string contextNamespace)
    {
        var sb = new StringBuilder();
        sb.AppendLine("using System.ComponentModel.DataAnnotations;");
        sb.AppendLine();
        sb.AppendLine($"namespace RVR.Framework.Application.DTOs.{contextNamespace};");
        sb.AppendLine();

        // Response DTO
        sb.AppendLine($"/// <summary>");
        sb.AppendLine($"/// Read DTO for {entity.Name}.");
        sb.AppendLine($"/// </summary>");
        sb.AppendLine($"public class {entity.Name}Dto");
        sb.AppendLine("{");
        sb.AppendLine("    public Guid Id { get; set; }");
        foreach (var prop in entity.Properties)
        {
            var typeStr = FormatCSharpType(prop);
            var nullable = prop.IsRequired ? "" : "?";
            if (prop.Type == "string" && !prop.IsRequired) nullable = "?";
            sb.AppendLine($"    public {typeStr}{nullable} {prop.Name} {{ get; set; }}");
        }
        sb.AppendLine("    public DateTime CreatedAt { get; set; }");
        sb.AppendLine("    public DateTime? ModifiedAt { get; set; }");
        sb.AppendLine("}");
        sb.AppendLine();

        // Create DTO
        sb.AppendLine($"/// <summary>");
        sb.AppendLine($"/// DTO for creating a new {entity.Name}.");
        sb.AppendLine($"/// </summary>");
        sb.AppendLine($"public class Create{entity.Name}Dto");
        sb.AppendLine("{");
        foreach (var prop in entity.Properties.Where(p => p.IsRequired))
        {
            sb.AppendLine("    [Required]");
            if (prop.Type == "string")
                sb.AppendLine("    [MaxLength(256)]");
            sb.AppendLine($"    public {FormatCSharpType(prop)} {prop.Name} {{ get; set; }} = default!;");
        }
        foreach (var prop in entity.Properties.Where(p => !p.IsRequired))
        {
            sb.AppendLine($"    public {FormatCSharpType(prop)}? {prop.Name} {{ get; set; }}");
        }
        sb.AppendLine("}");
        sb.AppendLine();

        // Update DTO
        sb.AppendLine($"/// <summary>");
        sb.AppendLine($"/// DTO for updating an existing {entity.Name}.");
        sb.AppendLine($"/// </summary>");
        sb.AppendLine($"public class Update{entity.Name}Dto");
        sb.AppendLine("{");
        foreach (var prop in entity.Properties.Where(p => p.IsRequired))
        {
            sb.AppendLine("    [Required]");
            if (prop.Type == "string")
                sb.AppendLine("    [MaxLength(256)]");
            sb.AppendLine($"    public {FormatCSharpType(prop)} {prop.Name} {{ get; set; }} = default!;");
        }
        foreach (var prop in entity.Properties.Where(p => !p.IsRequired))
        {
            sb.AppendLine($"    public {FormatCSharpType(prop)}? {prop.Name} {{ get; set; }}");
        }
        sb.AppendLine("}");

        return sb.ToString();
    }

    // ── Repository Interface Generation ────────────────────────────────

    public static string GenerateRepositoryInterface(EntityDefinition entity, string contextNamespace)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"using RVR.Framework.Domain.Entities.{contextNamespace};");
        sb.AppendLine();
        sb.AppendLine($"namespace RVR.Framework.Domain.Repositories.{contextNamespace};");
        sb.AppendLine();
        sb.AppendLine($"/// <summary>");
        sb.AppendLine($"/// Repository interface for <see cref=\"{entity.Name}\"/>.");
        sb.AppendLine($"/// </summary>");
        sb.AppendLine($"public interface I{entity.Name}Repository");
        sb.AppendLine("{");
        sb.AppendLine($"    Task<{entity.Name}?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);");
        sb.AppendLine($"    Task<IReadOnlyList<{entity.Name}>> GetAllAsync(CancellationToken cancellationToken = default);");
        sb.AppendLine($"    Task AddAsync({entity.Name} entity, CancellationToken cancellationToken = default);");
        sb.AppendLine($"    Task UpdateAsync({entity.Name} entity, CancellationToken cancellationToken = default);");
        sb.AppendLine($"    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);");
        sb.AppendLine("}");
        return sb.ToString();
    }

    // ── Value Object Generation ────────────────────────────────────────

    public static string GenerateValueObject(ValueObjectDefinition vo, string contextNamespace)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"namespace RVR.Framework.Domain.ValueObjects.{contextNamespace};");
        sb.AppendLine();
        sb.AppendLine($"/// <summary>");
        sb.AppendLine($"/// Value object: {vo.Name}.");
        sb.AppendLine($"/// </summary>");

        // Generate as a record for value semantics
        var props = vo.Properties.Select(p => $"{FormatCSharpType(p)} {p.Name}");
        sb.AppendLine($"public record {vo.Name}({string.Join(", ", props)});");

        return sb.ToString();
    }

    // ── Domain Event Generation ────────────────────────────────────────

    public static string GenerateDomainEvent(DomainEventDefinition evt, string contextNamespace)
    {
        var sb = new StringBuilder();
        sb.AppendLine("using RVR.Framework.Core;");
        sb.AppendLine();
        sb.AppendLine($"namespace RVR.Framework.Domain.Events.{contextNamespace};");
        sb.AppendLine();
        sb.AppendLine($"/// <summary>");
        sb.AppendLine($"/// Domain event: {evt.Name}.");
        if (!string.IsNullOrWhiteSpace(evt.TriggerEntity))
            sb.AppendLine($"/// Triggered by: {evt.TriggerEntity} - {evt.Trigger}");
        sb.AppendLine($"/// </summary>");
        sb.AppendLine($"public class {evt.Name} : IDomainEvent");
        sb.AppendLine("{");
        sb.AppendLine($"    public DateTime OccurredOn {{ get; }} = DateTime.UtcNow;");
        sb.AppendLine();

        foreach (var prop in evt.Properties)
        {
            var typeStr = FormatCSharpType(prop);
            sb.AppendLine($"    public {typeStr} {prop.Name} {{ get; init; }}");
        }

        // Constructor
        var allProps = evt.Properties.ToList();
        if (allProps.Count > 0)
        {
            sb.AppendLine();
            var ctorParams = string.Join(", ", allProps.Select(p => $"{FormatCSharpType(p)} {ToCamelCase(p.Name)}"));
            sb.AppendLine($"    public {evt.Name}({ctorParams})");
            sb.AppendLine("    {");
            foreach (var prop in allProps)
            {
                sb.AppendLine($"        {prop.Name} = {ToCamelCase(prop.Name)};");
            }
            sb.AppendLine("    }");
        }

        sb.AppendLine("}");
        return sb.ToString();
    }

    // ── Helpers ─────────────────────────────────────────────────────────

    private static string FormatCSharpType(PropertyDefinition prop)
    {
        if (prop.IsCollection)
            return $"List<{prop.Type}>";
        return prop.Type;
    }

    private static string FormatDefaultValue(PropertyDefinition prop)
    {
        if (prop.DefaultValue is null) return "";
        return prop.Type switch
        {
            "string" => $" = \"{prop.DefaultValue}\";",
            "bool" => $" = {prop.DefaultValue.ToLower()};",
            _ => $" = {prop.DefaultValue};"
        };
    }

    private static string ToCamelCase(string name)
    {
        if (string.IsNullOrEmpty(name)) return name;
        return char.ToLowerInvariant(name[0]) + name[1..];
    }

    private static string SanitizeNamespace(string name)
    {
        return name.Replace(" ", "").Replace("-", "").Replace(".", "");
    }

    private static string Pluralize(string name)
    {
        if (name.EndsWith("y", StringComparison.Ordinal) && !name.EndsWith("ay", StringComparison.Ordinal) && !name.EndsWith("ey", StringComparison.Ordinal) && !name.EndsWith("oy", StringComparison.Ordinal))
            return name[..^1] + "ies";
        if (name.EndsWith("s", StringComparison.Ordinal) || name.EndsWith("x", StringComparison.Ordinal) || name.EndsWith("ch", StringComparison.Ordinal) || name.EndsWith("sh", StringComparison.Ordinal))
            return name + "es";
        return name + "s";
    }
}
