using RVR.Framework.FeatureManagement.Domain;
using Microsoft.Extensions.Options;

namespace RVR.Framework.FeatureManagement.Services;

/// <summary>
/// Provider en memoire pour les definitions de features.
/// Utilisable en dev ou pour les features statiques. En production,
/// remplacer par un provider base de donnees.
/// </summary>
public class InMemoryFeatureDefinitionProvider : IFeatureDefinitionProvider
{
    private readonly Dictionary<string, FeatureDefinition> _features = new(StringComparer.OrdinalIgnoreCase);

    public InMemoryFeatureDefinitionProvider(IOptions<FeatureManagementOptions>? options = null)
    {
        if (options?.Value?.Features != null)
        {
            foreach (var reg in options.Value.Features)
            {
                _features[reg.Name] = new FeatureDefinition
                {
                    Name = reg.Name,
                    DisplayName = reg.Name,
                    DefaultValue = reg.DefaultValue,
                    ValueType = reg.ValueType
                };
            }
        }
    }

    /// <summary>
    /// Enregistre une definition de feature.
    /// </summary>
    public void Register(FeatureDefinition definition)
    {
        _features[definition.Name] = definition;
    }

    public Task<FeatureDefinition?> GetAsync(string featureName)
    {
        _features.TryGetValue(featureName, out var definition);
        return Task.FromResult(definition);
    }

    public Task<IReadOnlyList<FeatureDefinition>> GetAllAsync()
    {
        IReadOnlyList<FeatureDefinition> result = _features.Values.ToList().AsReadOnly();
        return Task.FromResult(result);
    }
}
