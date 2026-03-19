using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RVR.Framework.SMS.Configuration;
using RVR.Framework.SMS.Models;
using RVR.Framework.SMS.Providers;

namespace RVR.Framework.SMS.Extensions;

/// <summary>
/// Extension methods for registering SMS services in the dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the RVR SMS module services including <see cref="ISmsService"/>,
    /// the configured <see cref="ISmsProvider"/>, and named <see cref="HttpClient"/> instances.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="config">The application configuration (reads the "SMS" section).</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddRvrSms(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<SmsOptions>(config.GetSection(SmsOptions.SectionName));

        // Register named HttpClients for each provider
        services.AddHttpClient("RVR.SMS.Twilio");
        services.AddHttpClient("RVR.SMS.Vonage");
        services.AddHttpClient("RVR.SMS.OVH");
        services.AddHttpClient("RVR.SMS.Azure");

        // Determine the configured provider
        var options = new SmsOptions();
        config.GetSection(SmsOptions.SectionName).Bind(options);

        // Register the appropriate provider based on configuration
        switch (options.DefaultProvider)
        {
            case SmsProvider.Twilio:
                services.AddSingleton<ISmsProvider, TwilioSmsProvider>();
                break;
            case SmsProvider.Vonage:
                services.AddSingleton<ISmsProvider, VonageSmsProvider>();
                break;
            case SmsProvider.OVH:
                services.AddSingleton<ISmsProvider, OvhSmsProvider>();
                break;
            case SmsProvider.Azure:
                services.AddSingleton<ISmsProvider, AzureSmsProvider>();
                break;
            case SmsProvider.Console:
            case SmsProvider.None:
            default:
                services.AddSingleton<ISmsProvider, ConsoleSmsProvider>();
                break;
        }

        services.AddSingleton<ISmsService, SmsService>();

        return services;
    }
}
