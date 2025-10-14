using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace RVR.Framework.Plugins.Abstractions;

/// <summary>
/// Defines a plugin that can be loaded into the RIVORA Framework.
/// </summary>
public interface IPlugin
{
    /// <summary>
    /// The unique name of the plugin.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// The version of the plugin.
    /// </summary>
    string Version { get; }

    /// <summary>
    /// Registers services required by the plugin into the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    void Initialize(IServiceCollection services);

    /// <summary>
    /// Configures the application pipeline with middleware or endpoints provided by the plugin.
    /// </summary>
    /// <param name="app">The application builder.</param>
    void Configure(IApplicationBuilder app);
}
