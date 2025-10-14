using Microsoft.Extensions.DependencyInjection;
using RVR.Framework.Email.Models;
using RVR.Framework.Email.Services;

namespace RVR.Framework.Email.Extensions;

/// <summary>
/// Extension methods for registering email services.
/// </summary>
public static class EmailServiceCollectionExtensions
{
    /// <summary>
    /// Adds the Rivora SMTP email services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Configuration action for <see cref="EmailOptions"/>.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddRvrEmail(
        this IServiceCollection services,
        Action<EmailOptions> configure)
    {
        services.Configure(configure);
        services.AddSingleton<IEmailSender, SmtpEmailSender>();
        return services;
    }
}
