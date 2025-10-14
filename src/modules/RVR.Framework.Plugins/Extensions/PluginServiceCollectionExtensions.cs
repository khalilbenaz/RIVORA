using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RVR.Framework.Plugins.Abstractions;
using RVR.Framework.Plugins.Loaders;
using RVR.Framework.Plugins.Management;
using RVR.Framework.Plugins.Models;

namespace RVR.Framework.Plugins.Extensions;

/// <summary>
/// Extension methods for configuring RIVORA Framework Plugin system.
/// </summary>
public static class PluginServiceCollectionExtensions
{
    /// <summary>
    /// Adds RIVORA Framework Plugin services to the service collection.
    /// Optionally auto-loads and initializes plugins from the configured directory.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Action to configure plugin options.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddRvrPlugins(
        this IServiceCollection services,
        Action<PluginOptions>? configureOptions = null)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        services.AddOptions<PluginOptions>()
            .Configure(configureOptions ?? (_ => { }));

        services.AddSingleton<IPluginLoader, AssemblyPluginLoader>();
        services.AddSingleton<PluginManager>();

        // Build a temporary provider to read options and optionally auto-load plugins
        var tempProvider = services.BuildServiceProvider();
        var options = tempProvider.GetRequiredService<IOptions<PluginOptions>>().Value;

        if (options.AutoLoadOnStartup)
        {
            var manager = tempProvider.GetRequiredService<PluginManager>();
            manager.LoadPlugins();
            manager.InitializePlugins(services);
        }

        return services;
    }

    /// <summary>
    /// Configures the application pipeline with all loaded plugins.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The application builder.</returns>
    public static IApplicationBuilder UseRvrPlugins(this IApplicationBuilder app)
    {
        if (app == null)
        {
            throw new ArgumentNullException(nameof(app));
        }

        var manager = app.ApplicationServices.GetRequiredService<PluginManager>();
        manager.ConfigurePlugins(app);

        return app;
    }
}
