using KBA.Framework.Localization.Dynamic.Domain.Entities;
using System.Collections.Concurrent;

namespace KBA.Framework.Localization.Dynamic.Providers;

public class DatabaseLocalizationProvider
{
    // A simple in-memory cache. In production, use Redis/IDistributedCache
    private readonly ConcurrentDictionary<string, string> _cache = new();

    // Mock dependency
    // private readonly AppDbContext _dbContext;

    public string GetString(string culture, string key)
    {
        var cacheKey = $"{culture}_{key}";

        if (_cache.TryGetValue(cacheKey, out var value))
        {
            return value;
        }

        // Logic to fetch from DB:
        // var entity = _dbContext.LanguageTexts.FirstOrDefault(t => t.Culture == culture && t.Key == key);
        var entity = new LanguageText { Value = "Translated from DB (Mock)" }; // Mocked

        if (entity != null)
        {
            _cache.TryAdd(cacheKey, entity.Value);
            return entity.Value;
        }

        return key; // Fallback to key if not found
    }

    public void ClearCache()
    {
        _cache.Clear();
    }
}
