namespace RVR.Framework.NaturalQuery.Extensions;

using RVR.Framework.NaturalQuery.Services;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for registering NaturalQuery services in the DI container.
/// </summary>
public static class NaturalQueryServiceCollectionExtensions
{
    /// <summary>
    /// Adds the RVR Natural Query services to the service collection.
    /// This enables converting natural language queries into LINQ expressions.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Optional configuration action for <see cref="NaturalQueryOptions"/>.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddRvrNaturalQuery(
        this IServiceCollection services,
        Action<NaturalQueryOptions>? configure = null)
    {
        services.Configure(configure ?? (_ => { }));
        services.AddScoped<NaturalLanguageParser>();
        services.AddScoped<ExpressionBuilder>();
        services.AddScoped<INaturalQueryService, NaturalQueryService>();
        return services;
    }
}
