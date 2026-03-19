using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace RVR.Framework.Webhooks.Incoming;

/// <summary>
/// Extension methods for registering incoming webhook services and mapping endpoints.
/// </summary>
public static class IncomingWebhookExtensions
{
    /// <summary>
    /// Registers the incoming webhook store as a singleton in the DI container.
    /// </summary>
    public static IServiceCollection AddIncomingWebhooks(this IServiceCollection services)
    {
        services.AddSingleton<InMemoryIncomingWebhookStore>();
        return services;
    }

    /// <summary>
    /// Maps all incoming webhook API endpoints.
    /// </summary>
    public static IEndpointRouteBuilder MapIncomingWebhooks(this IEndpointRouteBuilder app)
    {
        app.MapIncomingWebhookEndpoints();
        return app;
    }
}
