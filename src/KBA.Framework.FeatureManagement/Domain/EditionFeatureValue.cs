using System;
using System.Collections.Generic;

namespace KBA.Framework.FeatureManagement.Domain;

/// <summary>
/// Represents the value of a feature for a specific Edition.
/// </summary>
public class EditionFeatureValue
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid EditionId { get; set; }
    public string FeatureName { get; set; } = string.Empty;
    
    /// <summary>
    /// The limit or boolean value for this feature in this edition.
    /// E.g., "true" (for access), "100" (for a limit like MaxUsers).
    /// </summary>
    public string Value { get; set; } = string.Empty;
}