namespace RVR.Framework.Features.Extensions;

using Azure.Data.AppConfiguration;
using RVR.Framework.Features.Core;
using RVR.Framework.Features.Providers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

/// <summary>
/// Extension methods for registering feature flag services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds feature flag services with configuration-based provider.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddFeatureFlags(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        return services.AddFeatureFlags(options =>
        {
            configuration.Bind("FeatureFlags", options);
            
            // Set config file path for hot-reload
            if (configuration is IConfigurationRoot configRoot)
            {
                var providers = configRoot.Providers.ToList();
                foreach (var provider in providers)
                {
                    if (provider.TryGet("FeatureFlags:ConfigFilePath", out var path))
                    {
                        options.ConfigFilePath = path;
                        break;
                    }
                }
            }
        });
    }

    /// <summary>
    /// Adds feature flag services with custom configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Action to configure the options.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddFeatureFlags(
        this IServiceCollection services,
        Action<FeatureFlagsOptions> configureOptions)
    {
        services.Configure(configureOptions);
        
        services.AddLogging();
        services.AddSingleton<IFeatureProvider, ConfigFeatureProvider>();
        services.AddSingleton<IFeatureManager, FeatureManager>();
        services.Configure<FeatureManagerOptions>(options =>
        {
            options.UseFirstProvider = true;
            options.DefaultEnabledState = false;
        });

        return services;
    }

    /// <summary>
    /// Adds feature flag services with database provider.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureDbContext">Action to configure the database context options.</param>
    /// <param name="cacheExpiration">The cache expiration time span.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddFeatureFlagsWithDatabase(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder> configureDbContext,
        TimeSpan? cacheExpiration = null)
    {
        services.AddDbContextFactory<FeatureDbContext>(configureDbContext);
        services.AddLogging();
        services.AddSingleton<IFeatureProvider>(sp =>
        {
            var contextFactory = sp.GetRequiredService<IDbContextFactory<FeatureDbContext>>();
            var logger = sp.GetRequiredService<ILogger<DatabaseFeatureProvider>>();
            return new DatabaseFeatureProvider(contextFactory, logger, cacheExpiration);
        });
        services.AddSingleton<IFeatureManager, FeatureManager>();
        services.Configure<FeatureManagerOptions>(options =>
        {
            options.UseFirstProvider = true;
            options.DefaultEnabledState = false;
        });

        return services;
    }

    /// <summary>
    /// Adds feature flag services with Azure App Configuration provider.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="connectionString">The Azure App Configuration connection string.</param>
    /// <param name="configureOptions">Action to configure the Azure options.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddFeatureFlagsWithAzure(
        this IServiceCollection services,
        string connectionString,
        Action<AzureFeatureProviderOptions>? configureOptions = null)
    {
        services.Configure<AzureFeatureProviderOptions>(options =>
        {
            configureOptions?.Invoke(options);
        });

        services.AddSingleton<ConfigurationClient>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<ConfigurationClient>>();
            return new ConfigurationClient(connectionString);
        });

        services.AddLogging();
        services.AddSingleton<IFeatureProvider, AzureFeatureProvider>();
        services.AddSingleton<IFeatureManager, FeatureManager>();
        services.Configure<FeatureManagerOptions>(options =>
        {
            options.UseFirstProvider = true;
            options.DefaultEnabledState = false;
        });

        return services;
    }

    /// <summary>
    /// Adds feature flag services with multiple providers.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <param name="configureProviders">Action to configure which providers to use.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddFeatureFlagsWithProviders(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<FeatureProvidersBuilder> configureProviders)
    {
        var builder = new FeatureProvidersBuilder(services, configuration);
        configureProviders(builder);

        services.AddSingleton<IFeatureManager, FeatureManager>();
        services.Configure<FeatureManagerOptions>(options =>
        {
            options.UseFirstProvider = true;
            options.DefaultEnabledState = false;
        });

        return services;
    }

    /// <summary>
    /// Adds the feature flags dashboard Razor Pages.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddFeatureFlagsDashboard(this IServiceCollection services)
    {
        services.AddRazorPages();
        return services;
    }
}

/// <summary>
/// Builder for configuring feature providers.
/// </summary>
public class FeatureProvidersBuilder
{
    private readonly IServiceCollection _services;
    private readonly IConfiguration _configuration;
    private readonly List<Func<IServiceProvider, IFeatureProvider>> _providerFactories = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="FeatureProvidersBuilder"/> class.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    public FeatureProvidersBuilder(IServiceCollection services, IConfiguration configuration)
    {
        _services = services;
        _configuration = configuration;
    }

    /// <summary>
    /// Adds the configuration-based provider.
    /// </summary>
    /// <returns>The builder.</returns>
    public FeatureProvidersBuilder WithConfigProvider()
    {
        _services.Configure<FeatureFlagsOptions>(_configuration.GetSection("FeatureFlags"));
        _services.AddSingleton<IFeatureProvider, ConfigFeatureProvider>();
        return this;
    }

    /// <summary>
    /// Adds the database provider.
    /// </summary>
    /// <param name="configureDbContext">Action to configure the database context options.</param>
    /// <param name="cacheExpiration">The cache expiration time span.</param>
    /// <returns>The builder.</returns>
    public FeatureProvidersBuilder WithDatabaseProvider(
        Action<DbContextOptionsBuilder> configureDbContext,
        TimeSpan? cacheExpiration = null)
    {
        _services.AddDbContextFactory<FeatureDbContext>(configureDbContext);
        _services.AddSingleton<IFeatureProvider>(sp =>
        {
            var contextFactory = sp.GetRequiredService<IDbContextFactory<FeatureDbContext>>();
            var logger = sp.GetRequiredService<ILogger<DatabaseFeatureProvider>>();
            return new DatabaseFeatureProvider(contextFactory, logger, cacheExpiration);
        });
        return this;
    }

    /// <summary>
    /// Adds the Azure App Configuration provider.
    /// </summary>
    /// <param name="connectionString">The Azure App Configuration connection string.</param>
    /// <param name="configureOptions">Action to configure the Azure options.</param>
    /// <returns>The builder.</returns>
    public FeatureProvidersBuilder WithAzureProvider(
        string connectionString,
        Action<AzureFeatureProviderOptions>? configureOptions = null)
    {
        _services.Configure<AzureFeatureProviderOptions>(options =>
        {
            configureOptions?.Invoke(options);
        });

        _services.AddSingleton<ConfigurationClient>(sp =>
        {
            return new ConfigurationClient(connectionString);
        });

        _services.AddSingleton<IFeatureProvider, AzureFeatureProvider>();
        return this;
    }
}
