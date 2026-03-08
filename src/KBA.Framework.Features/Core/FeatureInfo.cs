namespace KBA.Framework.Features.Core;

using System;

/// <summary>
/// Represents information about a feature flag.
/// Contains the feature name, enabled state, description, and metadata.
/// </summary>
public class FeatureInfo
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FeatureInfo"/> class.
    /// </summary>
    public FeatureInfo()
    {
        Name = string.Empty;
        Description = string.Empty;
        Metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        LastModified = DateTime.UtcNow;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FeatureInfo"/> class with the specified name.
    /// </summary>
    /// <param name="name">The name of the feature.</param>
    public FeatureInfo(string name) : this()
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }

    /// <summary>
    /// Gets or sets the name of the feature.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the feature is enabled.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Gets or sets the description of the feature.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Gets or sets the provider that manages this feature.
    /// </summary>
    public FeatureProvider Provider { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the feature was last modified.
    /// </summary>
    public DateTime LastModified { get; set; }

    /// <summary>
    /// Gets or sets the user who last modified the feature.
    /// </summary>
    public string? LastModifiedBy { get; set; }

    /// <summary>
    /// Gets a dictionary of additional metadata for the feature.
    /// </summary>
    public IDictionary<string, string> Metadata { get; }

    /// <summary>
    /// Adds or updates a metadata value.
    /// </summary>
    /// <param name="key">The metadata key.</param>
    /// <param name="value">The metadata value.</param>
    public void SetMetadata(string key, string value)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentNullException(nameof(key));
        }

        Metadata[key] = value ?? string.Empty;
    }

    /// <summary>
    /// Gets a metadata value by key.
    /// </summary>
    /// <param name="key">The metadata key.</param>
    /// <param name="defaultValue">The default value if the key is not found.</param>
    /// <returns>The metadata value, or the default value if not found.</returns>
    public string GetMetadata(string key, string defaultValue = "")
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return defaultValue;
        }

        return Metadata.TryGetValue(key, out var value) ? value : defaultValue;
    }

    /// <summary>
    /// Creates a copy of this feature information.
    /// </summary>
    /// <returns>A new instance of <see cref="FeatureInfo"/> with the same values.</returns>
    public FeatureInfo Clone()
    {
        return new FeatureInfo(Name)
        {
            Enabled = Enabled,
            Description = Description,
            Provider = Provider,
            LastModified = LastModified,
            LastModifiedBy = LastModifiedBy,
            Metadata = new Dictionary<string, string>(Metadata, StringComparer.OrdinalIgnoreCase)
        };
    }
}
