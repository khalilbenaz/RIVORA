namespace KBA.Framework.Features.Providers;

using KBA.Framework.Features.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

/// <summary>
/// Feature provider that reads feature flags from configuration sources.
/// Supports hot-reload via IOptionsMonitor and FileSystemWatcher for file-based configurations.
/// </summary>
public class ConfigFeatureProvider : IFeatureProvider, IDisposable
{
    private readonly IOptionsMonitor<FeatureFlagsOptions> _optionsMonitor;
    private readonly ILogger<ConfigFeatureProvider> _logger;
    private readonly ConcurrentDictionary<string, FeatureInfo> _featuresCache;
    private readonly ReaderWriterLockSlim _cacheLock;
    private FileSystemWatcher? _fileWatcher;
    private bool _disposed;
    private string? _watchedFilePath;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigFeatureProvider"/> class.
    /// </summary>
    /// <param name="optionsMonitor">The options monitor for feature flags configuration.</param>
    /// <param name="logger">The logger instance.</param>
    public ConfigFeatureProvider(
        IOptionsMonitor<FeatureFlagsOptions> optionsMonitor,
        ILogger<ConfigFeatureProvider> logger)
    {
        _optionsMonitor = optionsMonitor ?? throw new ArgumentNullException(nameof(optionsMonitor));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _featuresCache = new ConcurrentDictionary<string, FeatureInfo>(StringComparer.OrdinalIgnoreCase);
        _cacheLock = new ReaderWriterLockSlim();
        
        // Subscribe to configuration changes
        _optionsMonitor.OnChange(OnConfigurationChanged);
        
        // Initialize cache
        LoadFeaturesFromConfiguration();
        
        // Setup file watcher for hot-reload
        SetupFileWatcher();
    }

    /// <inheritdoc />
    public FeatureProvider ProviderType => FeatureProvider.Config;

    /// <inheritdoc />
    public bool IsEnabled(string featureName)
    {
        if (string.IsNullOrWhiteSpace(featureName))
        {
            return false;
        }

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
        await Task.Yield(); // No async operation needed for config provider
        cancellationToken.ThrowIfCancellationRequested();
        return IsEnabled(featureName);
    }

    /// <inheritdoc />
    public FeatureInfo? GetFeature(string featureName)
    {
        if (string.IsNullOrWhiteSpace(featureName))
        {
            return null;
        }

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
        await Task.Yield();
        cancellationToken.ThrowIfCancellationRequested();
        return GetFeature(featureName);
    }

    /// <inheritdoc />
    public bool SetEnabled(string featureName, bool enabled)
    {
        // Config provider is read-only for runtime changes
        // Use Database or Azure provider for runtime modifications
        _logger.LogWarning("Attempted to set feature \"{FeatureName}\" to {Enabled} state, but Config provider is read-only", 
            featureName, enabled);
        return false;
    }

    /// <inheritdoc />
    public async Task<bool> SetEnabledAsync(string featureName, bool enabled, CancellationToken cancellationToken = default)
    {
        await Task.Yield();
        cancellationToken.ThrowIfCancellationRequested();
        return SetEnabled(featureName, enabled);
    }

    /// <inheritdoc />
    public IEnumerable<FeatureInfo> GetAllFeatures()
    {
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
        await Task.Yield();
        cancellationToken.ThrowIfCancellationRequested();
        return GetAllFeatures();
    }

    /// <inheritdoc />
    public Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        // Already initialized in constructor
        return Task.CompletedTask;
    }

    private void LoadFeaturesFromConfiguration()
    {
        var options = _optionsMonitor.CurrentValue;
        
        _cacheLock.EnterWriteLock();
        try
        {
            _featuresCache.Clear();
            
            foreach (var featureConfig in options.Features)
            {
                var feature = new FeatureInfo(featureConfig.Name)
                {
                    Enabled = featureConfig.Enabled,
                    Description = featureConfig.Description ?? string.Empty,
                    Provider = FeatureProvider.Config,
                    LastModified = DateTime.UtcNow
                };

                if (featureConfig.Metadata != null)
                {
                    foreach (var kvp in featureConfig.Metadata)
                    {
                        feature.SetMetadata(kvp.Key, kvp.Value);
                    }
                }

                _featuresCache[featureConfig.Name] = feature;
            }

            _logger.LogInformation("Loaded {Count} features from configuration", _featuresCache.Count);
        }
        finally
        {
            _cacheLock.ExitWriteLock();
        }
    }

    private void OnConfigurationChanged(FeatureFlagsOptions newOptions)
    {
        _logger.LogDebug("Configuration change detected, reloading features");
        LoadFeaturesFromConfiguration();
    }

    private void SetupFileWatcher()
    {
        var options = _optionsMonitor.CurrentValue;
        
        if (!options.EnableHotReload || string.IsNullOrEmpty(options.ConfigFilePath))
        {
            return;
        }

        _watchedFilePath = Path.GetFullPath(options.ConfigFilePath);
        var directory = Path.GetDirectoryName(_watchedFilePath);
        var fileName = Path.GetFileName(_watchedFilePath);

        if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory))
        {
            _logger.LogWarning("Cannot setup file watcher: directory \"{Directory}\" does not exist", directory);
            return;
        }

        try
        {
            _fileWatcher = new FileSystemWatcher
            {
                Path = directory,
                Filter = fileName,
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.CreationTime,
                EnableRaisingEvents = true
            };

            _fileWatcher.Changed += OnFileChanged;
            _fileWatcher.Created += OnFileChanged;
            
            _logger.LogInformation("File watcher setup for hot-reload on \"{FilePath}\"", _watchedFilePath);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to setup file watcher for hot-reload");
        }
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        // Debounce file changes to avoid multiple reloads
        Thread.Sleep(100);
        
        try
        {
            _logger.LogDebug("File change detected: {ChangeType} - {Path}", e.ChangeType, e.FullPath);
            LoadFeaturesFromConfiguration();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error reloading features from file change");
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
            _fileWatcher?.Dispose();
            _cacheLock.Dispose();
        }

        _disposed = true;
    }
}

/// <summary>
/// Options for configuring feature flags.
/// </summary>
public class FeatureFlagsOptions
{
    /// <summary>
    /// Gets or sets the list of feature configurations.
    /// </summary>
    public List<FeatureConfig> Features { get; set; } = new();

    /// <summary>
    /// Gets or sets a value indicating whether hot-reload is enabled.
    /// </summary>
    public bool EnableHotReload { get; set; } = true;

    /// <summary>
    /// Gets or sets the path to the configuration file for file watching.
    /// </summary>
    public string? ConfigFilePath { get; set; }
}

/// <summary>
/// Configuration for a single feature flag.
/// </summary>
public class FeatureConfig
{
    /// <summary>
    /// Gets or sets the name of the feature.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the feature is enabled.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Gets or sets the description of the feature.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets additional metadata for the feature.
    /// </summary>
    public Dictionary<string, string>? Metadata { get; set; }
}
