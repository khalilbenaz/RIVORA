using System;

namespace RVR.Framework.FeatureManagement.Domain;

/// <summary>
/// A definition of a Feature available in the system.
/// </summary>
public class FeatureDefinition
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string DefaultValue { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Type of the feature value (e.g., "Boolean", "Int32", "String")
    /// </summary>
    public string ValueType { get; set; } = "Boolean";
}