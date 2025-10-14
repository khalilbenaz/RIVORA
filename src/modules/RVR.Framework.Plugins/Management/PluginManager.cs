using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RVR.Framework.Plugins.Abstractions;
using RVR.Framework.Plugins.Models;

namespace RVR.Framework.Plugins.Management;

/// <summary>
/// Manages the lifecycle of plugins: loading, initialization, configuration, and unloading.
/// </summary>
public class PluginManager
{
    private readonly IPluginLoader _pluginLoader;
    private readonly ILogger<PluginManager> _logger;
    private readonly PluginOptions _options;
    private bool _initialized;

    /// <summary>
    /// Initializes a new instance of <see cref="PluginManager"/>.
    /// </summary>
    /// <param name="pluginLoader">The plugin loader.</param>
    /// <param name="options">The plugin options.</param>
    /// <param name="logger">The logger.</param>
    public PluginManager(
        IPluginLoader pluginLoader,
        IOptions<PluginOptions> options,
        ILogger<PluginManager> logger)
    {
        _pluginLoader = pluginLoader ?? throw new ArgumentNullException(nameof(pluginLoader));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// Gets all loaded plugins.
    /// </summary>
    public IReadOnlyList<IPlugin> LoadedPlugins => _pluginLoader.GetLoadedPlugins();

    /// <summary>
    /// Loads plugins from the configured plugin directory.
    /// </summary>
    /// <returns>The loaded plugins.</returns>
    public IReadOnlyList<IPlugin> LoadPlugins()
    {
        _logger.LogInformation("Loading plugins from '{Directory}'.", _options.PluginDirectory);
        return _pluginLoader.LoadPlugins(_options.PluginDirectory);
    }

    /// <summary>
    /// Initializes all loaded plugins by registering their services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    public void InitializePlugins(IServiceCollection services)
    {
        if (_initialized)
        {
            _logger.LogWarning("Plugins have already been initialized. Skipping.");
            return;
        }

        foreach (var plugin in _pluginLoader.GetLoadedPlugins())
        {
            try
            {
                _logger.LogInformation("Initializing plugin '{Name}' v{Version}.", plugin.Name, plugin.Version);
                plugin.Initialize(services);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize plugin '{Name}'.", plugin.Name);
            }
        }

        _initialized = true;
    }

    /// <summary>
    /// Configures all loaded plugins by applying middleware and endpoints.
    /// </summary>
    /// <param name="app">The application builder.</param>
    public void ConfigurePlugins(IApplicationBuilder app)
    {
        foreach (var plugin in _pluginLoader.GetLoadedPlugins())
        {
            try
            {
                _logger.LogInformation("Configuring plugin '{Name}' v{Version}.", plugin.Name, plugin.Version);
                plugin.Configure(app);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to configure plugin '{Name}'.", plugin.Name);
            }
        }
    }

    /// <summary>
    /// Unloads all plugins by clearing the loaded plugin list.
    /// </summary>
    public void UnloadPlugins()
    {
        _logger.LogInformation("Unloading {Count} plugins.", _pluginLoader.GetLoadedPlugins().Count);
        _initialized = false;
    }
}
