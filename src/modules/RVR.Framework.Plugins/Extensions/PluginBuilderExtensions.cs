using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RVR.Framework.Plugins.Abstractions;
using RVR.Framework.Plugins.Discovery;
using RVR.Framework.Plugins.Installation;
using RVR.Framework.Plugins.Models;
using RVR.Framework.Plugins.Security;

namespace RVR.Framework.Plugins.Extensions;

/// <summary>
/// Extension methods for registering the enhanced RIVORA plugin system with auto-discovery,
/// NuGet integration, signature verification, and compatibility checking.
/// </summary>
public static class PluginBuilderExtensions
{
    /// <summary>
    /// Adds the full RIVORA plugin system to the service collection, binding configuration
    /// from the <c>Rivora:Plugins</c> and <c>Rivora:Plugins:Security</c> sections.
    /// Registers NuGet discovery, compatibility checking, signature verification, and the plugin installer.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="config">The application configuration root.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddRvrPluginSystem(
        this IServiceCollection services,
        IConfiguration config)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(config);

        // Bind options
        services.Configure<PluginOptions>(config.GetSection("Rivora:Plugins"));
        services.Configure<PluginSecurityOptions>(config.GetSection("Rivora:Plugins:Security"));

        // Register HTTP client for NuGet API calls
        services.AddHttpClient("NuGetPluginDiscovery");
        services.AddHttpClient("PluginInstaller");

        // Register plugin system services
        services.TryAddSingleton<NuGetPluginDiscovery>();
        services.TryAddSingleton<PluginCompatibilityChecker>();
        services.TryAddSingleton<PluginSignatureVerifier>();
        services.TryAddSingleton<PluginInstaller>();

        return services;
    }

    /// <summary>
    /// Scans all currently loaded assemblies for types implementing <see cref="IRvrPlugin"/>
    /// and registers them as singletons in the service collection. Each discovered plugin's
    /// <see cref="IPlugin.Initialize"/> method is invoked to allow it to register its own services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddRvrPluginAutoDiscovery(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        var pluginTypes = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic && !IsSystemAssembly(a))
            .SelectMany(SafeGetTypes)
            .Where(t => typeof(IRvrPlugin).IsAssignableFrom(t) && t is { IsAbstract: false, IsInterface: false })
            .Distinct()
            .ToList();

        foreach (var pluginType in pluginTypes)
        {
            if (Activator.CreateInstance(pluginType) is IRvrPlugin plugin)
            {
                services.AddSingleton<IRvrPlugin>(plugin);
                services.AddSingleton(plugin.GetType(), plugin);
                plugin.Initialize(services);
            }
        }

        return services;
    }

    private static bool IsSystemAssembly(Assembly assembly)
    {
        var name = assembly.GetName().Name;

        if (name is null)
        {
            return true;
        }

        return name.StartsWith("System", StringComparison.Ordinal)
            || name.StartsWith("Microsoft", StringComparison.Ordinal)
            || name.StartsWith("netstandard", StringComparison.Ordinal)
            || name.StartsWith("mscorlib", StringComparison.Ordinal);
    }

    private static IEnumerable<Type> SafeGetTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types.Where(t => t is not null)!;
        }
    }
}
