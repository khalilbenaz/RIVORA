using RVR.Framework.Billing.Interfaces;
using RVR.Framework.Billing.Services;
using RVR.Framework.Billing.Webhooks;
using Microsoft.Extensions.DependencyInjection;

namespace RVR.Framework.Billing.Extensions;

/// <summary>
/// Extension methods for configuring billing services in the dependency injection container.
/// </summary>
public static class BillingServiceCollectionExtensions
{
    /// <summary>
    /// Adds Rivora billing services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">An action to configure billing options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddRvrBilling(this IServiceCollection services, Action<BillingOptions> configure)
    {
        services.Configure(configure);

        services.AddSingleton<IBillingStore, InMemoryBillingStore>();
        services.AddScoped<IPaymentProvider, StripePaymentProvider>();
        services.AddScoped<IBillingService, BillingService>();
        services.AddScoped<StripeWebhookHandler>();

        return services;
    }

    /// <summary>
    /// Adds Rivora billing services with a custom billing store implementation.
    /// </summary>
    /// <typeparam name="TStore">The billing store implementation type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">An action to configure billing options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddRvrBilling<TStore>(this IServiceCollection services, Action<BillingOptions> configure)
        where TStore : class, IBillingStore
    {
        services.Configure(configure);

        services.AddScoped<IBillingStore, TStore>();
        services.AddScoped<IPaymentProvider, StripePaymentProvider>();
        services.AddScoped<IBillingService, BillingService>();
        services.AddScoped<StripeWebhookHandler>();

        return services;
    }
}

/// <summary>
/// Configuration options for the billing module.
/// </summary>
public sealed class BillingOptions
{
    /// <summary>
    /// Gets or sets the Stripe API secret key.
    /// </summary>
    public string StripeApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Stripe webhook signing secret.
    /// </summary>
    public string StripeWebhookSecret { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the default currency code (e.g., "usd", "eur").
    /// </summary>
    public string DefaultCurrency { get; set; } = "usd";

    /// <summary>
    /// Gets or sets the default number of trial days for new subscriptions.
    /// </summary>
    public int TrialDays { get; set; }

    /// <summary>
    /// Gets or sets the number of grace period days after a failed payment before suspension.
    /// </summary>
    public int GracePeriodDays { get; set; } = 3;
}
