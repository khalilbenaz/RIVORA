namespace RVR.Framework.Privacy.Attributes;

/// <summary>
/// Marks a property as containing sensitive personal data requiring special protection
/// under GDPR Article 9. Sensitive data categories require explicit consent and
/// additional safeguards.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class SensitiveDataAttribute : Attribute
{
    /// <summary>
    /// Gets the category of sensitive data.
    /// </summary>
    public DataCategory Category { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SensitiveDataAttribute"/> class.
    /// </summary>
    /// <param name="category">The sensitive data category.</param>
    public SensitiveDataAttribute(DataCategory category)
    {
        Category = category;
    }
}

/// <summary>
/// Categories of sensitive personal data as defined by GDPR Article 9.
/// </summary>
public enum DataCategory
{
    /// <summary>Health-related data.</summary>
    Health,

    /// <summary>Financial data such as bank accounts, credit scores.</summary>
    Financial,

    /// <summary>Biometric data used for identification.</summary>
    Biometric,

    /// <summary>Data revealing racial or ethnic origin.</summary>
    RacialOrEthnicOrigin,

    /// <summary>Data revealing political opinions.</summary>
    PoliticalOpinions,

    /// <summary>Data revealing religious or philosophical beliefs.</summary>
    ReligiousBeliefs,

    /// <summary>Data concerning trade union membership.</summary>
    TradeUnionMembership,

    /// <summary>Genetic data.</summary>
    Genetic,

    /// <summary>Data concerning sex life or sexual orientation.</summary>
    SexualOrientation
}
