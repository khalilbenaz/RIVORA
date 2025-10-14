using System.Collections.Concurrent;
using RVR.Framework.Localization.Dynamic.Models;

namespace RVR.Framework.Localization.Dynamic.Services;

/// <summary>
/// In-memory implementation of <see cref="ILocalizationStore"/>.
/// Useful for development, testing, or as a fallback when no database is configured.
/// </summary>
public class InMemoryLocalizationStore : ILocalizationStore
{
    // Key: "{culture}|{key}", Value: LocalizationEntry
    private readonly ConcurrentDictionary<string, LocalizationEntry> _entries = new(StringComparer.OrdinalIgnoreCase);

    /// <inheritdoc />
    public Task<string?> GetTranslationAsync(string culture, string key)
    {
        var compositeKey = BuildKey(culture, key);
        if (_entries.TryGetValue(compositeKey, out var entry))
        {
            return Task.FromResult<string?>(entry.Value);
        }
        return Task.FromResult<string?>(null);
    }

    /// <inheritdoc />
    public Task SetTranslationAsync(string culture, string key, string value)
    {
        var compositeKey = BuildKey(culture, key);
        _entries.AddOrUpdate(
            compositeKey,
            _ => new LocalizationEntry
            {
                Key = key,
                Value = value,
                Culture = culture,
                LastModified = DateTime.UtcNow
            },
            (_, existing) =>
            {
                existing.Value = value;
                existing.LastModified = DateTime.UtcNow;
                return existing;
            });
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<LocalizationEntry>> GetAllForCultureAsync(string culture)
    {
        var results = _entries.Values
            .Where(e => string.Equals(e.Culture, culture, StringComparison.OrdinalIgnoreCase))
            .OrderBy(e => e.Key)
            .ToList();
        return Task.FromResult<IReadOnlyList<LocalizationEntry>>(results.AsReadOnly());
    }

    private static string BuildKey(string culture, string key) => $"{culture}|{key}";
}
