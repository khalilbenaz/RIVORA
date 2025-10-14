using Dapr.Client;
using Microsoft.Extensions.DependencyInjection;
using RVR.Framework.Dapr.Models;
using RVR.Framework.Dapr.Services;

namespace RVR.Framework.Dapr.Extensions;

/// <summary>
/// Extension methods for configuring RIVORA Framework Dapr integration.
/// </summary>
public static class DaprServiceCollectionExtensions
{
    /// <summary>
    /// Adds RIVORA Framework Dapr services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Action to configure Dapr options.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddRvrDapr(
        this IServiceCollection services,
        Action<DaprOptions>? configureOptions = null)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        var options = new DaprOptions();
        configureOptions?.Invoke(options);

        services.AddOptions<DaprOptions>()
            .Configure(configureOptions ?? (_ => { }));

        services.AddSingleton(_ =>
        {
            var builder = new DaprClientBuilder();
            builder.UseHttpEndpoint(options.HttpEndpoint);
            builder.UseGrpcEndpoint(options.GrpcEndpoint);
            return builder.Build();
        });

        services.AddSingleton<IDaprService, DaprServiceWrapper>();

        return services;
    }
}
