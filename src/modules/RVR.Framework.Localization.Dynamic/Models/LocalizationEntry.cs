namespace RVR.Framework.Localization.Dynamic.Models;

/// <summary>
/// Represents a single localization entry with a key-value pair for a specific culture.
/// </summary>
public class LocalizationEntry
{
    /// <summary>
    /// Unique identifier.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// The localization key (e.g., "Buttons.Save", "Errors.NotFound").
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// The translated value for this key in the specified culture.
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// The culture/locale code (e.g., "en-US", "fr-FR", "ar-SA").
    /// </summary>
    public string Culture { get; set; } = string.Empty;

    /// <summary>
    /// The date and time this entry was last modified.
    /// </summary>
    public DateTime LastModified { get; set; } = DateTime.UtcNow;
}
