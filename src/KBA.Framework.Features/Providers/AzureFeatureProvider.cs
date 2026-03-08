namespace KBA.Framework.Features.Providers;

using Azure;
using Azure.Data.AppConfiguration;
using KBA.Framework.Features.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Text.Json;

/// <summary>
/// Feature provider that reads feature flags from Azure App Configuration.
/// Supports hot-reload through Azure App Configuration push notifications.
/// </summary>
public class AzureFeatureProvider : IFeatureProvider, IDisposable
{
    private readonly ConfigurationClient _configurationClient;
    private readonly ILogger<AzureFeatureProvider> _logger;
    private readonly AzureFeatureProviderOptions _options;
    private readonly ConcurrentDictionary<string, FeatureInfo> _featuresCache;
    private readonly ReaderWriterLockSlim _cacheLock;
    private bool _disposed;
    private bool _initialized;

    /// <summary>
    /// Initializes a new instance of the <see cref="AzureFeatureProvider"/> class.
    /// </summary>
    /// <param name="configurationClient">The Azure App Configuration client.</param>
    /// <param name="options">The Azure feature provider options.</param>
    /// <param name="logger">The logger instance.</param>
    public AzureFeatureProvider(
        ConfigurationClient configurationClient,
        IOptions<AzureFeatureProviderOptions> options,
        ILogger<AzureFeatureProvider> logger)
    {
        _configurationClient = configurationClient ?? throw new ArgumentNullException(nameof(configurationClient));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _featuresCache = new ConcurrentDictionary<string, FeatureInfo>(StringComparer.OrdinalIgnoreCase);
        _cacheLock = new ReaderWriterLockSlim();
    }

    /// <inheritdoc />
    public FeatureProvider ProviderType => FeatureProvider.Azure;

    /// <inheritdoc />
    public bool IsEnabled(string featureName)
    {
        if (string.IsNullOrWhiteSpace(featureName))
        {
            return false;
        }

        EnsureInitialized();

        _cacheLock.EnterReadLock();
        try
        {
            return _featuresCache.TryGetValue(featureName, out var feature) && feature.Enabled;
        }
        finally
        {
            _cacheLock.ExitReadLock();
        }
    }

    /// <inheritdoc />
    public async Task<bool> IsEnabledAsync(string featureName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(featureName))
        {
            return false;
        }

        await EnsureInitializedAsync(cancellationToken);

        _cacheLock.EnterReadLock();
        try
        {
            return _featuresCache.TryGetValue(featureName, out var feature) && feature.Enabled;
        }
        finally
        {
            _cacheLock.ExitReadLock();
        }
    }

    /// <inheritdoc />
    public FeatureInfo? GetFeature(string featureName)
    {
        if (string.IsNullOrWhiteSpace(featureName))
        {
            return null;
        }

        EnsureInitialized();

        _cacheLock.EnterReadLock();
        try
        {
            return _featuresCache.TryGetValue(featureName, out var feature) 
                ? feature.Clone() 
                : null;
        }
        finally
        {
            _cacheLock.ExitReadLock();
        }
    }

    /// <inheritdoc />
    public async Task<FeatureInfo?> GetFeatureAsync(string featureName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(featureName))
        {
            return null;
        }

        await EnsureInitializedAsync(cancellationToken);

        _cacheLock.EnterReadLock();
        try
        {
            return _featuresCache.TryGetValue(featureName, out var feature) 
                ? feature.Clone() 
                : null;
        }
        finally
        {
            _cacheLock.ExitReadLock();
        }
    }

    /// <inheritdoc />
    public bool SetEnabled(string featureName, bool enabled)
    {
        if (string.IsNullOrWhiteSpace(featureName))
        {
            return false;
        }

        try
        {
            var key = $"{_options.KeyPrefix}{featureName}";
            var feature = GetFeature(featureName);
            
            if (feature == null)
            {
                _logger.LogWarning("Feature \"{FeatureName}\" not found in Azure App Configuration", featureName);
                return false;
            }

            feature.Enabled = enabled;
            feature.LastModified = DateTime.UtcNow;

            var content = JsonSerializer.Serialize(new
            {
                feature.Name,
                feature.Enabled,
                feature.Description,
                feature.Metadata
            });

            var setting = new ConfigurationSetting(key, content);
            _configurationClient.SetConfigurationSetting(setting);

            // Update cache
            _cacheLock.EnterWriteLock();
            try
            {
                _featuresCache[featureName] = feature;
            }
            finally
            {
                _cacheLock.ExitWriteLock();
            }

            _logger.LogInformation("Feature \"{FeatureName}\" set to {Enabled} in Azure", featureName, enabled);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting feature \"{FeatureName}\" state in Azure", featureName);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<bool> SetEnabledAsync(string featureName, bool enabled, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(featureName))
        {
            return false;
        }

        try
        {
            var key = $"{_options.KeyPrefix}{featureName}";
            var feature = await GetFeatureAsync(featureName, cancellationToken);
            
            if (feature == null)
            {
                _logger.LogWarning("Feature \"{FeatureName}\" not found in Azure App Configuration", featureName);
                return false;
            }

            feature.Enabled = enabled;
            feature.LastModified = DateTime.UtcNow;

            var content = JsonSerializer.Serialize(new
            {
                feature.Name,
                feature.Enabled,
                feature.Description,
                feature.Metadata
            });

            var setting = new ConfigurationSetting(key, content);
            await _configurationClient.SetConfigurationSettingAsync(setting, cancellationToken);

            // Update cache
            _cacheLock.EnterWriteLock();
            try
            {
                _featuresCache[featureName] = feature;
            }
            finally
            {
                _cacheLock.ExitWriteLock();
            }

            _logger.LogInformation("Feature \"{FeatureName}\" set to {Enabled} in Azure", featureName, enabled);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting feature \"{FeatureName}\" state in Azure", featureName);
            return false;
        }
    }

    /// <inheritdoc />
    public IEnumerable<FeatureInfo> GetAllFeatures()
    {
        EnsureInitialized();

        _cacheLock.EnterReadLock();
        try
        {
            return _featuresCache.Values.Select(f => f.Clone()).ToList();
        }
        finally
        {
            _cacheLock.ExitReadLock();
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<FeatureInfo>> GetAllFeaturesAsync(CancellationToken cancellationToken = default)
    {
        await EnsureInitializedAsync(cancellationToken);

        _cacheLock.EnterReadLock();
        try
        {
            return _featuresCache.Values.Select(f => f.Clone()).ToList();
        }
        finally
        {
            _cacheLock.ExitReadLock();
        }
    }

    /// <inheritdoc />
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await LoadFeaturesFromAzureAsync(cancellationToken);
        _initialized = true;
    }

    private void EnsureInitialized()
    {
        if (!_initialized)
        {
            LoadFeaturesFromAzure();
            _initialized = true;
        }
    }

    private async Task EnsureInitializedAsync(CancellationToken cancellationToken)
    {
        if (!_initialized)
        {
            await LoadFeaturesFromAzureAsync(cancellationToken);
            _initialized = true;
        }
    }

    private void LoadFeaturesFromAzure()
    {
        try
        {
            var selector = new SettingSelector
            {
                KeyFilter = $"{_options.KeyPrefix}*",
                Fields = SettingFields.Key | SettingFields.Value | SettingFields.Label | SettingFields.ContentType
            };

            var settings = _configurationClient.GetConfigurationSettings(selector);

            _cacheLock.EnterWriteLock();
            try
            {
                _featuresCache.Clear();

                foreach (var setting in settings)
                {
                    var featureName = setting.Key.Substring(_options.KeyPrefix.Length);
                    
                    try
                    {
                        using var doc = JsonDocument.Parse(setting.Value);
                        var root = doc.RootElement;

                        var feature = new FeatureInfo(featureName)
                        {
                            Enabled = root.TryGetProperty("enabled", out var enabledProp) && enabledProp.GetBoolean(),
                            Description = root.TryGetProperty("description", out var descProp) ? descProp.GetString() ?? string.Empty : string.Empty,
                            Provider = FeatureProvider.Azure,
                            LastModified = DateTime.UtcNow
                        };

                        if (root.TryGetProperty("metadata", out var metadataProp))
                        {
                            var metadata = JsonSerializer.Deserialize<Dictionary<string, string>>(metadataProp.GetRawText());
                            if (metadata != null)
                            {
                                foreach (var kvp in metadata)
                                {
                                    feature.SetMetadata(kvp.Key, kvp.Value);
                                }
                            }
                        }

                        _featuresCache[featureName] = feature;
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogWarning(ex, "Failed to parse feature \"{FeatureName}\" from Azure", featureName);
                    }
                }

                _logger.LogInformation("Loaded {Count} features from Azure App Configuration", _featuresCache.Count);
            }
            finally
            {
                _cacheLock.ExitWriteLock();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading features from Azure App Configuration");
        }
    }

    private async Task LoadFeaturesFromAzureAsync(CancellationToken cancellationToken)
    {
        try
        {
            var selector = new SettingSelector
            {
                KeyFilter = $"{_options.KeyPrefix}*",
                Fields = SettingFields.Key | SettingFields.Value | SettingFields.Label | SettingFields.ContentType
            };

            var settings = _configurationClient.GetConfigurationSettingsAsync(selector, cancellationToken);

            _cacheLock.EnterWriteLock();
            try
            {
                _featuresCache.Clear();

                await foreach (var setting in settings)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    
                    var featureName = setting.Key.Substring(_options.KeyPrefix.Length);
                    
                    try
                    {
                        using var doc = JsonDocument.Parse(setting.Value);
                        var root = doc.RootElement;

                        var feature = new FeatureInfo(featureName)
                        {
                            Enabled = root.TryGetProperty("enabled", out var enabledProp) && enabledProp.GetBoolean(),
                            Description = root.TryGetProperty("description", out var descProp) ? descProp.GetString() ?? string.Empty : string.Empty,
                            Provider = FeatureProvider.Azure,
                            LastModified = DateTime.UtcNow
                        };

                        if (root.TryGetProperty("metadata", out var metadataProp))
                        {
                            var metadata = JsonSerializer.Deserialize<Dictionary<string, string>>(metadataProp.GetRawText());
                            if (metadata != null)
                            {
                                foreach (var kvp in metadata)
                                {
                                    feature.SetMetadata(kvp.Key, kvp.Value);
                                }
                            }
                        }

                        _featuresCache[featureName] = feature;
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogWarning(ex, "Failed to parse feature \"{FeatureName}\" from Azure", featureName);
                    }
                }

                _logger.LogInformation("Loaded {Count} features from Azure App Configuration", _featuresCache.Count);
            }
            finally
            {
                _cacheLock.ExitWriteLock();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading features from Azure App Configuration");
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes of the provider resources.
    /// </summary>
    /// <param name="disposing">True if disposing managed resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            _cacheLock.Dispose();
        }

        _disposed = true;
    }
}

/// <summary>
/// Options for configuring the Azure feature provider.
/// </summary>
public class AzureFeatureProviderOptions
{
    /// <summary>
    /// Gets or sets the key prefix for feature flags in Azure App Configuration.
    /// Default is "featureflags/".
    /// </summary>
    public string KeyPrefix { get; set; } = "featureflags/";

    /// <summary>
    /// Gets or sets the label filter for feature flags.
    /// </summary>
    public string? Label { get; set; }

    /// <summary>
    /// Gets or sets the cache expiration time in seconds.
    /// Default is 30 seconds.
    /// </summary>
    public int CacheExpirationSeconds { get; set; } = 30;
}
