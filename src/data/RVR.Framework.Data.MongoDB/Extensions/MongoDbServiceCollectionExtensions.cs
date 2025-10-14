using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using RVR.Framework.Data.MongoDB.Models;

namespace RVR.Framework.Data.MongoDB.Extensions;

/// <summary>
/// Extension methods for registering MongoDB services.
/// </summary>
public static class MongoDbServiceCollectionExtensions
{
    /// <summary>
    /// Adds the Rivora MongoDB services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Configuration action for <see cref="MongoDbOptions"/>.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddRvrMongoDB(
        this IServiceCollection services,
        Action<MongoDbOptions> configure)
    {
        services.Configure(configure);

        services.AddSingleton<IMongoClient>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<MongoDbOptions>>().Value;
            return new MongoClient(options.ConnectionString);
        });

        services.AddSingleton<IMongoDatabase>(sp =>
        {
            var client = sp.GetRequiredService<IMongoClient>();
            var options = sp.GetRequiredService<IOptions<MongoDbOptions>>().Value;
            return client.GetDatabase(options.DatabaseName);
        });

        return services;
    }
}
