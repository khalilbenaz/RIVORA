using RVR.Framework.FeatureManagement.Domain;

namespace RVR.Framework.FeatureManagement.Services;

/// <summary>
/// Fournit les definitions de features disponibles dans le systeme.
/// </summary>
public interface IFeatureDefinitionProvider
{
    /// <summary>
    /// Recupere la definition d'une feature par son nom.
    /// </summary>
    Task<FeatureDefinition?> GetAsync(string featureName);

    /// <summary>
    /// Recupere toutes les definitions de features.
    /// </summary>
    Task<IReadOnlyList<FeatureDefinition>> GetAllAsync();
}
