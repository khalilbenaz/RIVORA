using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using RVR.Framework.Idempotency.Middleware;
using RVR.Framework.Idempotency.Services;

namespace RVR.Framework.Idempotency.Extensions;

/// <summary>
/// Extension methods for registering idempotency services.
/// </summary>
public static class IdempotencyServiceCollectionExtensions
{
    /// <summary>
    /// Adds the Rivora idempotency services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddRvrIdempotency(this IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddSingleton<IIdempotencyStore, InMemoryIdempotencyStore>();
        return services;
    }

    /// <summary>
    /// Adds the idempotency middleware to the application pipeline.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The application builder for chaining.</returns>
    public static IApplicationBuilder UseIdempotency(this IApplicationBuilder app)
    {
        return app.UseMiddleware<IdempotencyMiddleware>();
    }
}
