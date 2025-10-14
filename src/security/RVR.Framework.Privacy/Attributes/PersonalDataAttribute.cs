namespace RVR.Framework.Privacy.Attributes;

/// <summary>
/// Marks a property as containing personally identifiable information (PII).
/// Properties decorated with this attribute will be targeted during data anonymization
/// and included in data subject access requests.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class PersonalDataAttribute : Attribute
{
    /// <summary>
    /// Gets or sets an optional description of what kind of personal data this property holds.
    /// </summary>
    public string? Description { get; set; }
}
