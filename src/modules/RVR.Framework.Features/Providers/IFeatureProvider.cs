namespace RVR.Framework.Features.Providers;

using RVR.Framework.Features.Core;

/// <summary>
/// Interface for feature flag providers.
/// Providers are responsible for storing and retrieving feature flag data from different sources.
/// </summary>
public interface IFeatureProvider
{
    /// <summary>
    /// Gets the type of this provider.
    /// </summary>
    FeatureProvider ProviderType { get; }

    /// <summary>
    /// Checks if a feature is enabled.
    /// </summary>
    /// <param name="featureName">The name of the feature to check.</param>
    /// <returns>True if the feature is enabled, false otherwise.</returns>
    bool IsEnabled(string featureName);

    /// <summary>
    /// Checks if a feature is enabled asynchronously.
    /// </summary>
    /// <param name="featureName">The name of the feature to check.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The result contains true if the feature is enabled, false otherwise.</returns>
    Task<bool> IsEnabledAsync(string featureName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets information about a feature.
    /// </summary>
    /// <param name="featureName">The name of the feature.</param>
    /// <returns>The feature information, or null if the feature does not exist.</returns>
    FeatureInfo? GetFeature(string featureName);

    /// <summary>
    /// Gets information about a feature asynchronously.
    /// </summary>
    /// <param name="featureName">The name of the feature.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The result contains the feature information, or null if the feature does not exist.</returns>
    Task<FeatureInfo?> GetFeatureAsync(string featureName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets the enabled state of a feature.
    /// </summary>
    /// <param name="featureName">The name of the feature.</param>
    /// <param name="enabled">True to enable the feature, false to disable it.</param>
    /// <returns>True if the feature was updated, false if the feature does not exist.</returns>
    bool SetEnabled(string featureName, bool enabled);

    /// <summary>
    /// Sets the enabled state of a feature asynchronously.
    /// </summary>
    /// <param name="featureName">The name of the feature.</param>
    /// <param name="enabled">True to enable the feature, false to disable it.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The result is true if the feature was updated, false if the feature does not exist.</returns>
    Task<bool> SetEnabledAsync(string featureName, bool enabled, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all feature flags.
    /// </summary>
    /// <returns>A collection of all feature information.</returns>
    IEnumerable<FeatureInfo> GetAllFeatures();

    /// <summary>
    /// Gets all feature flags asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The result contains a collection of all feature information.</returns>
    Task<IEnumerable<FeatureInfo>> GetAllFeaturesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Initializes the provider.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task InitializeAsync(CancellationToken cancellationToken = default);
}
