namespace RVR.Studio.DomainModeling;

using System.Text.Json;
using System.Text.Json.Serialization;

public class DomainModel
{
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = "1.0.0";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<BoundedContext> BoundedContexts { get; set; } = [];
}

public class BoundedContext
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<EntityDefinition> Entities { get; set; } = [];
    public List<ValueObjectDefinition> ValueObjects { get; set; } = [];
    public List<DomainEventDefinition> DomainEvents { get; set; } = [];
    public List<RelationshipDefinition> Relationships { get; set; } = [];
}

public class EntityDefinition
{
    public string Name { get; set; } = string.Empty;
    public bool IsAggregateRoot { get; set; }
    public List<PropertyDefinition> Properties { get; set; } = [];
    public List<string> Methods { get; set; } = [];
    public List<string> DomainEvents { get; set; } = [];
}

public class ValueObjectDefinition
{
    public string Name { get; set; } = string.Empty;
    public List<PropertyDefinition> Properties { get; set; } = [];
}

public class PropertyDefinition
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = "string";
    public bool IsRequired { get; set; } = true;
    public bool IsCollection { get; set; }
    public string? DefaultValue { get; set; }
}

public class DomainEventDefinition
{
    public string Name { get; set; } = string.Empty;
    public string TriggerEntity { get; set; } = string.Empty;
    public string Trigger { get; set; } = string.Empty;
    public List<PropertyDefinition> Properties { get; set; } = [];
}

public class RelationshipDefinition
{
    public string FromEntity { get; set; } = string.Empty;
    public string ToEntity { get; set; } = string.Empty;
    public RelationshipType Type { get; set; }
    public string? NavigationProperty { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RelationshipType { OneToOne, OneToMany, ManyToMany }
