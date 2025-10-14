using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace RVR.Framework.Core.Modules;

/// <summary>
/// Interface pour définir un module du framework RVR (2.4)
/// </summary>
public interface IRvrModule
{
    /// <summary>
    /// Nom du module
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Enregistre les services du module
    /// </summary>
    void ConfigureServices(IServiceCollection services, IConfiguration configuration);

    /// <summary>
    /// Configure le pipeline HTTP du module
    /// </summary>
    void Configure(IApplicationBuilder app);
}
