namespace RVR.Framework.Alerting.Extensions;

using System;
using Microsoft.Extensions.DependencyInjection;
using RVR.Framework.Alerting.Channels;
using RVR.Framework.Alerting.Interfaces;
using RVR.Framework.Alerting.Models;
using RVR.Framework.Alerting.Services;

/// <summary>
/// Extension methods for configuring RIVORA Framework Alerting services.
/// </summary>
public static class AlertingServiceCollectionExtensions
{
    /// <summary>
    /// Adds RIVORA Framework Alerting services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Action to configure alert options.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddRvrAlerting(
        this IServiceCollection services,
        Action<AlertOptions>? configureOptions = null)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        services.AddOptions<AlertOptions>()
            .Configure(configureOptions ?? (_ => { }));

        services.AddSingleton<IAlertService, AlertService>();

        return services;
    }

    /// <summary>
    /// Adds the Slack alert channel.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="webhookUrl">The Slack webhook URL.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddRvrSlackAlerts(
        this IServiceCollection services,
        string webhookUrl)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(webhookUrl);

        services.AddHttpClient<SlackAlertChannel>();

        services.AddSingleton<IAlertChannel>(sp =>
        {
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient(nameof(SlackAlertChannel));
            var logger = sp.GetService<Microsoft.Extensions.Logging.ILogger<SlackAlertChannel>>();
            return new SlackAlertChannel(httpClient, webhookUrl, logger);
        });

        return services;
    }

    /// <summary>
    /// Adds the Microsoft Teams alert channel.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="webhookUrl">The Teams webhook URL.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddRvrTeamsAlerts(
        this IServiceCollection services,
        string webhookUrl)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(webhookUrl);

        services.AddHttpClient<TeamsAlertChannel>();

        services.AddSingleton<IAlertChannel>(sp =>
        {
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient(nameof(TeamsAlertChannel));
            var logger = sp.GetService<Microsoft.Extensions.Logging.ILogger<TeamsAlertChannel>>();
            return new TeamsAlertChannel(httpClient, webhookUrl, logger);
        });

        return services;
    }

    /// <summary>
    /// Adds the email alert channel.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="recipientEmail">The recipient email address.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddRvrEmailAlerts(
        this IServiceCollection services,
        string recipientEmail)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(recipientEmail);

        services.AddSingleton<IAlertChannel>(sp =>
        {
            var emailSender = sp.GetRequiredService<IAlertEmailSender>();
            var logger = sp.GetService<Microsoft.Extensions.Logging.ILogger<EmailAlertChannel>>();
            return new EmailAlertChannel(emailSender, recipientEmail, logger);
        });

        return services;
    }

    /// <summary>
    /// Adds the console (logger) alert channel.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddRvrConsoleAlerts(
        this IServiceCollection services)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        services.AddSingleton<IAlertChannel, ConsoleAlertChannel>();

        return services;
    }
}
