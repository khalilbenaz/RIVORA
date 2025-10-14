namespace RVR.Studio.DomainModeling;

using System.Text;

/// <summary>
/// Generates Mermaid class diagrams from a <see cref="DomainModel"/>.
/// </summary>
public static class MermaidGenerator
{
    /// <summary>
    /// Generates a full Mermaid class diagram string from the given domain model.
    /// </summary>
    public static string Generate(DomainModel model)
    {
        var sb = new StringBuilder();
        sb.AppendLine("classDiagram");

        foreach (var context in model.BoundedContexts)
        {
            sb.AppendLine($"    %% Bounded Context: {context.Name}");
            if (!string.IsNullOrWhiteSpace(context.Description))
                sb.AppendLine($"    %% {context.Description}");
            sb.AppendLine();

            // Entities
            foreach (var entity in context.Entities)
            {
                GenerateEntityClass(sb, entity);
            }

            // Value Objects
            foreach (var vo in context.ValueObjects)
            {
                GenerateValueObjectClass(sb, vo);
            }

            // Domain Events
            foreach (var evt in context.DomainEvents)
            {
                GenerateDomainEventClass(sb, evt);
            }

            // Relationships
            foreach (var rel in context.Relationships)
            {
                GenerateRelationship(sb, rel);
            }

            // Link domain events to their trigger entities
            foreach (var evt in context.DomainEvents)
            {
                if (!string.IsNullOrWhiteSpace(evt.TriggerEntity))
                {
                    sb.AppendLine($"    {evt.TriggerEntity} ..> {evt.Name} : raises");
                }
            }
        }

        return sb.ToString();
    }

    private static void GenerateEntityClass(StringBuilder sb, EntityDefinition entity)
    {
        sb.AppendLine($"    class {entity.Name} {{");

        if (entity.IsAggregateRoot)
            sb.AppendLine($"        <<AggregateRoot>>");

        foreach (var prop in entity.Properties)
        {
            var typeStr = FormatType(prop);
            var requiredMark = prop.IsRequired ? "" : "?";
            sb.AppendLine($"        +{typeStr}{requiredMark} {prop.Name}");
        }

        foreach (var method in entity.Methods)
        {
            sb.AppendLine($"        +{method}()");
        }

        sb.AppendLine("    }");
        sb.AppendLine();
    }

    private static void GenerateValueObjectClass(StringBuilder sb, ValueObjectDefinition vo)
    {
        sb.AppendLine($"    class {vo.Name} {{");
        sb.AppendLine($"        <<ValueObject>>");

        foreach (var prop in vo.Properties)
        {
            var typeStr = FormatType(prop);
            sb.AppendLine($"        +{typeStr} {prop.Name}");
        }

        sb.AppendLine("    }");
        sb.AppendLine();
    }

    private static void GenerateDomainEventClass(StringBuilder sb, DomainEventDefinition evt)
    {
        sb.AppendLine($"    class {evt.Name} {{");
        sb.AppendLine($"        <<DomainEvent>>");

        foreach (var prop in evt.Properties)
        {
            var typeStr = FormatType(prop);
            sb.AppendLine($"        +{typeStr} {prop.Name}");
        }

        sb.AppendLine("    }");
        sb.AppendLine();
    }

    private static void GenerateRelationship(StringBuilder sb, RelationshipDefinition rel)
    {
        var arrow = rel.Type switch
        {
            RelationshipType.OneToOne => " \"1\" --> \"1\" ",
            RelationshipType.OneToMany => " \"1\" --> \"*\" ",
            RelationshipType.ManyToMany => " \"*\" --> \"*\" ",
            _ => " --> "
        };

        var label = !string.IsNullOrWhiteSpace(rel.NavigationProperty)
            ? $" : {rel.NavigationProperty}"
            : "";

        sb.AppendLine($"    {rel.FromEntity}{arrow}{rel.ToEntity}{label}");
    }

    private static string FormatType(PropertyDefinition prop)
    {
        return prop.IsCollection ? $"List~{prop.Type}~" : prop.Type;
    }
}
