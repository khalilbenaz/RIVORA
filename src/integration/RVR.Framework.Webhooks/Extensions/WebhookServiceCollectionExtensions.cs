using RVR.Framework.Webhooks.Services;
using Microsoft.Extensions.DependencyInjection;

namespace RVR.Framework.Webhooks.Extensions;

/// <summary>
/// Extension methods for registering webhook services in the DI container.
/// </summary>
public static class WebhookServiceCollectionExtensions
{
    /// <summary>
    /// Adds the Rivora webhook system services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Optional configuration action for <see cref="WebhookOptions"/>.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddRvrWebhooks(
        this IServiceCollection services,
        Action<WebhookOptions>? configure = null)
    {
        services.Configure(configure ?? (_ => { }));
        services.AddHttpClient("webhooks");
        services.AddSingleton<IWebhookStore, InMemoryWebhookStore>();
        services.AddSingleton<WebhookSender>();
        services.AddSingleton<WebhookDeliveryChannel>();
        services.AddHostedService<WebhookDeliveryBackgroundService>();
        services.AddScoped<IWebhookService, WebhookService>();
        return services;
    }
}
