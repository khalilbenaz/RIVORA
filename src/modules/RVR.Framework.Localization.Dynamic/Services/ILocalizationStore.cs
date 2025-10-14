using RVR.Framework.Localization.Dynamic.Models;

namespace RVR.Framework.Localization.Dynamic.Services;

/// <summary>
/// Abstraction for storing and retrieving dynamic localization entries.
/// Implementations can use in-memory storage, databases, or distributed caches.
/// </summary>
public interface ILocalizationStore
{
    /// <summary>
    /// Gets the translated value for the given key and culture.
    /// </summary>
    /// <param name="culture">The culture code (e.g., "en-US").</param>
    /// <param name="key">The localization key.</param>
    /// <returns>The translated value, or null if not found.</returns>
    Task<string?> GetTranslationAsync(string culture, string key);

    /// <summary>
    /// Sets or updates a translation for the given key and culture.
    /// </summary>
    /// <param name="culture">The culture code.</param>
    /// <param name="key">The localization key.</param>
    /// <param name="value">The translated value.</param>
    Task SetTranslationAsync(string culture, string key, string value);

    /// <summary>
    /// Gets all localization entries for a specific culture.
    /// </summary>
    /// <param name="culture">The culture code.</param>
    /// <returns>All localization entries for that culture.</returns>
    Task<IReadOnlyList<LocalizationEntry>> GetAllForCultureAsync(string culture);
}
