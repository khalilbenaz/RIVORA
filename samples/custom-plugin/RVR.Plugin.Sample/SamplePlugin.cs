using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using RVR.Framework.Plugins.Abstractions;

namespace RVR.Plugin.Sample;

/// <summary>
/// A minimal sample plugin that demonstrates how to implement <see cref="IRvrPlugin"/>
/// for the RIVORA Framework. Registers a health-check endpoint at <c>/plugins/sample/ping</c>.
/// </summary>
public sealed class SamplePlugin : IRvrPlugin
{
    /// <inheritdoc />
    public string Name => "RVR.Plugin.Sample";

    /// <inheritdoc />
    public string Version => "1.0.0";

    /// <inheritdoc />
    public string MinimumRivoraVersion => "3.1.0";

    /// <inheritdoc />
    public IEnumerable<string> Dependencies => [];

    /// <inheritdoc />
    public PluginMetadata Metadata => new(
        Author: "RIVORA Team",
        Description: "A sample plugin demonstrating the RIVORA plugin system.",
        ProjectUrl: "https://github.com/rivora/framework",
        LicenseExpression: "MIT",
        Tags: ["sample", "demo", "getting-started"]);

    /// <inheritdoc />
    public void Initialize(IServiceCollection services)
    {
        // Register any services your plugin needs.
        // For example, a custom service:
        services.AddSingleton<SamplePluginService>();
    }

    /// <inheritdoc />
    public void Configure(IApplicationBuilder app)
    {
        // Add middleware or endpoints provided by this plugin.
        app.UseEndpoints(endpoints =>
        {
            if (endpoints is IEndpointRouteBuilder routeBuilder)
            {
                routeBuilder.MapGet("/plugins/sample/ping", () =>
                    Results.Ok(new { plugin = Name, version = Version, status = "healthy" }));
            }
        });
    }
}

/// <summary>
/// A trivial service registered by the sample plugin to demonstrate DI integration.
/// </summary>
public sealed class SamplePluginService
{
    /// <summary>
    /// Returns a greeting from the sample plugin.
    /// </summary>
    /// <param name="name">The name to greet.</param>
    /// <returns>A greeting string.</returns>
    public string Greet(string name) => $"Hello, {name}! Greetings from RVR.Plugin.Sample.";
}
