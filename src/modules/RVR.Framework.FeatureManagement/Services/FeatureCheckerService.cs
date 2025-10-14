using Microsoft.Extensions.Logging;

namespace RVR.Framework.FeatureManagement.Services;

/// <summary>
/// Service de verification des features. Verifie si une feature est activee
/// et recupere sa valeur pour le contexte courant (tenant/edition).
/// </summary>
public class FeatureCheckerService : IFeatureCheckerService
{
    private readonly IFeatureDefinitionProvider _provider;
    private readonly ILogger<FeatureCheckerService> _logger;

    public FeatureCheckerService(
        IFeatureDefinitionProvider provider,
        ILogger<FeatureCheckerService> logger)
    {
        _provider = provider;
        _logger = logger;
    }

    public async Task<bool> IsEnabledAsync(string featureName)
    {
        var value = await GetValueAsync(featureName);
        return bool.TryParse(value, out var result) && result;
    }

    public async Task<string> GetValueAsync(string featureName)
    {
        var definition = await _provider.GetAsync(featureName);
        if (definition == null)
        {
            _logger.LogWarning("Feature '{FeatureName}' non trouvee, retourne valeur par defaut vide", featureName);
            return string.Empty;
        }

        // Pour l'instant, retourner la valeur par defaut.
        // En production, interroger EditionFeatureValue pour le tenant courant.
        return definition.DefaultValue;
    }

    public async Task<int> GetValueAsIntAsync(string featureName)
    {
        var value = await GetValueAsync(featureName);
        if (int.TryParse(value, out var result))
        {
            return result;
        }

        _logger.LogWarning("Feature '{FeatureName}' valeur '{Value}' n'est pas un entier valide", featureName, value);
        return 0;
    }
}
