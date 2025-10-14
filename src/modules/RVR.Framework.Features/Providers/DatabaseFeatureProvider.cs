namespace RVR.Framework.Features.Providers;

using RVR.Framework.Features.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

/// <summary>
/// Feature provider that stores feature flags in a database using Entity Framework Core.
/// Supports hot-reload through change tracking and cache invalidation.
/// </summary>
public class DatabaseFeatureProvider : IFeatureProvider, IDisposable
{
    private readonly IDbContextFactory<FeatureDbContext> _contextFactory;
    private readonly ILogger<DatabaseFeatureProvider> _logger;
    private readonly ConcurrentDictionary<string, FeatureInfo> _featuresCache;
    private readonly ReaderWriterLockSlim _cacheLock;
    private readonly TimeSpan _cacheExpiration;
    private DateTime _lastCacheRefresh;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseFeatureProvider"/> class.
    /// </summary>
    /// <param name="contextFactory">The database context factory.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="cacheExpiration">The cache expiration time span.</param>
    public DatabaseFeatureProvider(
        IDbContextFactory<FeatureDbContext> contextFactory,
        ILogger<DatabaseFeatureProvider> logger,
        TimeSpan? cacheExpiration = null)
    {
        _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _featuresCache = new ConcurrentDictionary<string, FeatureInfo>(StringComparer.OrdinalIgnoreCase);
        _cacheLock = new ReaderWriterLockSlim();
        _cacheExpiration = cacheExpiration ?? TimeSpan.FromSeconds(30);
        _lastCacheRefresh = DateTime.MinValue;
    }

    /// <inheritdoc />
    public FeatureProvider ProviderType => FeatureProvider.Database;

    /// <inheritdoc />
    public bool IsEnabled(string featureName)
    {
        if (string.IsNullOrWhiteSpace(featureName))
        {
            return false;
        }

        EnsureCacheRefreshed();

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

        await EnsureCacheRefreshedAsync(cancellationToken);

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

        EnsureCacheRefreshed();

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

        await EnsureCacheRefreshedAsync(cancellationToken);

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
            using var context = _contextFactory.CreateDbContext();
            var featureEntity = context.Features
                .FirstOrDefault(f => f.Name == featureName);

            if (featureEntity == null)
            {
                _logger.LogWarning("Feature \"{FeatureName}\" not found in database", featureName);
                return false;
            }

            featureEntity.Enabled = enabled;
            featureEntity.LastModified = DateTime.UtcNow;
            context.SaveChanges();

            // Update cache
            _cacheLock.EnterWriteLock();
            try
            {
                if (_featuresCache.TryGetValue(featureName, out var cachedFeature))
                {
                    cachedFeature.Enabled = enabled;
                    cachedFeature.LastModified = DateTime.UtcNow;
                }
            }
            finally
            {
                _cacheLock.ExitWriteLock();
            }

            _logger.LogInformation("Feature \"{FeatureName}\" set to {Enabled}", featureName, enabled);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting feature \"{FeatureName}\" state", featureName);
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
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
            var featureEntity = await context.Features
                .FirstOrDefaultAsync(f => f.Name == featureName, cancellationToken);

            if (featureEntity == null)
            {
                _logger.LogWarning("Feature \"{FeatureName}\" not found in database", featureName);
                return false;
            }

            featureEntity.Enabled = enabled;
            featureEntity.LastModified = DateTime.UtcNow;
            await context.SaveChangesAsync(cancellationToken);

            // Update cache
            _cacheLock.EnterWriteLock();
            try
            {
                if (_featuresCache.TryGetValue(featureName, out var cachedFeature))
                {
                    cachedFeature.Enabled = enabled;
                    cachedFeature.LastModified = DateTime.UtcNow;
                }
            }
            finally
            {
                _cacheLock.ExitWriteLock();
            }

            _logger.LogInformation("Feature \"{FeatureName}\" set to {Enabled}", featureName, enabled);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting feature \"{FeatureName}\" state", featureName);
            return false;
        }
    }

    /// <inheritdoc />
    public IEnumerable<FeatureInfo> GetAllFeatures()
    {
        EnsureCacheRefreshed();

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
        await EnsureCacheRefreshedAsync(cancellationToken);

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
        await RefreshCacheAsync(cancellationToken);
    }

    private void EnsureCacheRefreshed()
    {
        if (DateTime.UtcNow - _lastCacheRefresh > _cacheExpiration)
        {
            RefreshCache();
        }
    }

    private async Task EnsureCacheRefreshedAsync(CancellationToken cancellationToken)
    {
        if (DateTime.UtcNow - _lastCacheRefresh > _cacheExpiration)
        {
            await RefreshCacheAsync(cancellationToken);
        }
    }

    private void RefreshCache()
    {
        try
        {
            using var context = _contextFactory.CreateDbContext();
            var features = context.Features.ToList();

            _cacheLock.EnterWriteLock();
            try
            {
                _featuresCache.Clear();
                foreach (var featureEntity in features)
                {
                    var feature = new FeatureInfo(featureEntity.Name)
                    {
                        Enabled = featureEntity.Enabled,
                        Description = featureEntity.Description ?? string.Empty,
                        Provider = FeatureProvider.Database,
                        LastModified = featureEntity.LastModified,
                        LastModifiedBy = featureEntity.LastModifiedBy
                    };

                    if (!string.IsNullOrEmpty(featureEntity.Metadata))
                    {
                        var metadata = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(featureEntity.Metadata);
                        if (metadata != null)
                        {
                            foreach (var kvp in metadata)
                            {
                                feature.SetMetadata(kvp.Key, kvp.Value);
                            }
                        }
                    }

                    _featuresCache[featureEntity.Name] = feature;
                }

                _lastCacheRefresh = DateTime.UtcNow;
                _logger.LogDebug("Cache refreshed with {Count} features", _featuresCache.Count);
            }
            finally
            {
                _cacheLock.ExitWriteLock();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing feature cache");
        }
    }

    private async Task RefreshCacheAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
            var features = await context.Features.ToListAsync(cancellationToken);

            _cacheLock.EnterWriteLock();
            try
            {
                _featuresCache.Clear();
                foreach (var featureEntity in features)
                {
                    var feature = new FeatureInfo(featureEntity.Name)
                    {
                        Enabled = featureEntity.Enabled,
                        Description = featureEntity.Description ?? string.Empty,
                        Provider = FeatureProvider.Database,
                        LastModified = featureEntity.LastModified,
                        LastModifiedBy = featureEntity.LastModifiedBy
                    };

                    if (!string.IsNullOrEmpty(featureEntity.Metadata))
                    {
                        var metadata = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(featureEntity.Metadata);
                        if (metadata != null)
                        {
                            foreach (var kvp in metadata)
                            {
                                feature.SetMetadata(kvp.Key, kvp.Value);
                            }
                        }
                    }

                    _featuresCache[featureEntity.Name] = feature;
                }

                _lastCacheRefresh = DateTime.UtcNow;
                _logger.LogDebug("Cache refreshed with {Count} features", _featuresCache.Count);
            }
            finally
            {
                _cacheLock.ExitWriteLock();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing feature cache");
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
/// Entity Framework Core database context for feature flags.
/// </summary>
public class FeatureDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FeatureDbContext"/> class.
    /// </summary>
    /// <param name="options">The database context options.</param>
    public FeatureDbContext(DbContextOptions<FeatureDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Gets or sets the DbSet of feature flags.
    /// </summary>
    public DbSet<FeatureEntity> Features => Set<FeatureEntity>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<FeatureEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name).IsUnique();

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(256);

            entity.Property(e => e.Description)
                .HasMaxLength(1024);

            entity.Property(e => e.Metadata)
                .HasMaxLength(4096);

            entity.ToTable("FeatureFlags");
        });
    }
}

/// <summary>
/// Entity representing a feature flag in the database.
/// </summary>
public class FeatureEntity
{
    /// <summary>
    /// Gets or sets the unique identifier.
    /// </summary>
    public int Id { get; set; }

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
    /// Gets or sets the JSON metadata for the feature.
    /// </summary>
    public string? Metadata { get; set; }

    /// <summary>
    /// Gets or sets the user who last modified the feature.
    /// </summary>
    public string? LastModifiedBy { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the feature was last modified.
    /// </summary>
    public DateTime LastModified { get; set; } = DateTime.UtcNow;
}
