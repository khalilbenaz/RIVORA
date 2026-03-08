namespace KBA.Framework.Features.Core;

using KBA.Framework.Features.Providers;
using Microsoft.Extensions.Logging;

/// <summary>
/// Centralized feature manager that coordinates multiple feature providers.
/// Implements a priority-based system where providers are checked in order.
/// </summary>
public class FeatureManager : IFeatureManager
{
    private readonly IEnumerable<IFeatureProvider> _providers;
    private readonly ILogger<FeatureManager> _logger;
    private readonly FeatureManagerOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="FeatureManager"/> class.
    /// </summary>
    /// <param name="providers">The collection of feature providers.</param>
    /// <param name="options">The feature manager options.</param>
    /// <param name="logger">The logger instance.</param>
    public FeatureManager(
        IEnumerable<IFeatureProvider> providers,
        FeatureManagerOptions options,
        ILogger<FeatureManager> logger)
    {
        _providers = providers?.OrderBy(p => GetProviderPriority(p)) ?? Enumerable.Empty<IFeatureProvider>();
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public bool IsEnabled(string featureName)
    {
        if (string.IsNullOrWhiteSpace(featureName))
        {
            return false;
        }

        foreach (var provider in _providers)
        {
            try
            {
                var result = provider.IsEnabled(featureName);
                _logger.LogDebug("Provider {ProviderType} returned {Result} for feature {FeatureName}", 
                    provider.ProviderType, result, featureName);
                
                if (_options.UseFirstProvider || HasFeature(provider, featureName))
                {
                    return result;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking feature {FeatureName} with provider {ProviderType}", 
                    featureName, provider.ProviderType);
            }
        }

        return _options.DefaultEnabledState;
    }

    /// <inheritdoc />
    public async Task<bool> IsEnabledAsync(string featureName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(featureName))
        {
            return false;
        }

        foreach (var provider in _providers)
        {
            try
            {
                var result = await provider.IsEnabledAsync(featureName, cancellationToken);
                _logger.LogDebug("Provider {ProviderType} returned {Result} for feature {FeatureName}", 
                    provider.ProviderType, result, featureName);
                
                if (_options.UseFirstProvider || await HasFeatureAsync(provider, featureName, cancellationToken))
                {
                    return result;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking feature {FeatureName} with provider {ProviderType}", 
                    featureName, provider.ProviderType);
            }
        }

        return _options.DefaultEnabledState;
    }

    /// <inheritdoc />
    public FeatureInfo? GetFeature(string featureName)
    {
        if (string.IsNullOrWhiteSpace(featureName))
        {
            return null;
        }

        foreach (var provider in _providers)
        {
            try
            {
                var feature = provider.GetFeature(featureName);
                if (feature != null)
                {
                    _logger.LogDebug("Found feature {FeatureName} in provider {ProviderType}", 
                        featureName, provider.ProviderType);
                    return feature;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting feature {FeatureName} from provider {ProviderType}", 
                    featureName, provider.ProviderType);
            }
        }

        return null;
    }

    /// <inheritdoc />
    public async Task<FeatureInfo?> GetFeatureAsync(string featureName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(featureName))
        {
            return null;
        }

        foreach (var provider in _providers)
        {
            try
            {
                var feature = await provider.GetFeatureAsync(featureName, cancellationToken);
                if (feature != null)
                {
                    _logger.LogDebug("Found feature {FeatureName} in provider {ProviderType}", 
                        featureName, provider.ProviderType);
                    return feature;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting feature {FeatureName} from provider {ProviderType}", 
                    featureName, provider.ProviderType);
            }
        }

        return null;
    }

    /// <inheritdoc />
    public bool SetEnabled(string featureName, bool enabled)
    {
        if (string.IsNullOrWhiteSpace(featureName))
        {
            return false;
        }

        // Try to find a writable provider (not Config provider)
        foreach (var provider in _providers.Where(p => p.ProviderType != FeatureProvider.Config))
        {
            try
            {
                var feature = provider.GetFeature(featureName);
                if (feature != null)
                {
                    var result = provider.SetEnabled(featureName, enabled);
                    if (result)
                    {
                        _logger.LogInformation("Feature {FeatureName} set to {Enabled} via provider {ProviderType}", 
                            featureName, enabled, provider.ProviderType);
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting feature {FeatureName} state via provider {ProviderType}", 
                    featureName, provider.ProviderType);
            }
        }

        _logger.LogWarning("No writable provider found for feature {FeatureName}", featureName);
        return false;
    }

    /// <inheritdoc />
    public async Task<bool> SetEnabledAsync(string featureName, bool enabled, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(featureName))
        {
            return false;
        }

        // Try to find a writable provider (not Config provider)
        foreach (var provider in _providers.Where(p => p.ProviderType != FeatureProvider.Config))
        {
            try
            {
                var feature = await provider.GetFeatureAsync(featureName, cancellationToken);
                if (feature != null)
                {
                    var result = await provider.SetEnabledAsync(featureName, enabled, cancellationToken);
                    if (result)
                    {
                        _logger.LogInformation("Feature {FeatureName} set to {Enabled} via provider {ProviderType}", 
                            featureName, enabled, provider.ProviderType);
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting feature {FeatureName} state via provider {ProviderType}", 
                    featureName, provider.ProviderType);
            }
        }

        _logger.LogWarning("No writable provider found for feature {FeatureName}", featureName);
        return false;
    }

    /// <inheritdoc />
    public IEnumerable<FeatureInfo> GetAllFeatures()
    {
        var allFeatures = new Dictionary<string, FeatureInfo>(StringComparer.OrdinalIgnoreCase);

        foreach (var provider in _providers)
        {
            try
            {
                var features = provider.GetAllFeatures();
                foreach (var feature in features)
                {
                    if (!allFeatures.ContainsKey(feature.Name))
                    {
                        allFeatures[feature.Name] = feature;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all features from provider {ProviderType}", provider.ProviderType);
            }
        }

        return allFeatures.Values.ToList();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<FeatureInfo>> GetAllFeaturesAsync(CancellationToken cancellationToken = default)
    {
        var allFeatures = new Dictionary<string, FeatureInfo>(StringComparer.OrdinalIgnoreCase);

        foreach (var provider in _providers)
        {
            try
            {
                var features = await provider.GetAllFeaturesAsync(cancellationToken);
                foreach (var feature in features)
                {
                    if (!allFeatures.ContainsKey(feature.Name))
                    {
                        allFeatures[feature.Name] = feature;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all features from provider {ProviderType}", provider.ProviderType);
            }
        }

        return allFeatures.Values.ToList();
    }

    private static int GetProviderPriority(IFeatureProvider provider)
    {
        return provider.ProviderType switch
        {
            FeatureProvider.Database => 0,  // Highest priority
            FeatureProvider.Azure => 1,
            FeatureProvider.Config => 2,    // Lowest priority
            _ => 3
        };
    }

    private bool HasFeature(IFeatureProvider provider, string featureName)
    {
        return provider.GetFeature(featureName) != null;
    }

    private async Task<bool> HasFeatureAsync(IFeatureProvider provider, string featureName, CancellationToken cancellationToken)
    {
        return await provider.GetFeatureAsync(featureName, cancellationToken) != null;
    }
}

/// <summary>
/// Options for configuring the feature manager.
/// </summary>
public class FeatureManagerOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether to use the first provider that has the feature.
    /// If false, all providers are checked and the highest priority provider wins.
    /// Default is true.
    /// </summary>
    public bool UseFirstProvider { get; set; } = true;

    /// <summary>
    /// Gets or sets the default enabled state when no provider has the feature.
    /// Default is false.
    /// </summary>
    public bool DefaultEnabledState { get; set; } = false;
}
