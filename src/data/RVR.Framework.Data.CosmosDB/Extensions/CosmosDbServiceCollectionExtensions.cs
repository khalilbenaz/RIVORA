using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RVR.Framework.Data.CosmosDB.Models;

namespace RVR.Framework.Data.CosmosDB.Extensions;

/// <summary>
/// Extension methods for registering Cosmos DB services.
/// </summary>
public static class CosmosDbServiceCollectionExtensions
{
    /// <summary>
    /// Adds the Rivora Cosmos DB services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Configuration action for <see cref="CosmosDbOptions"/>.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddRvrCosmosDb(
        this IServiceCollection services,
        Action<CosmosDbOptions> configure)
    {
        services.Configure(configure);

        services.AddSingleton<CosmosClient>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<CosmosDbOptions>>().Value;
            return new CosmosClient(options.ConnectionString);
        });

        return services;
    }
}
