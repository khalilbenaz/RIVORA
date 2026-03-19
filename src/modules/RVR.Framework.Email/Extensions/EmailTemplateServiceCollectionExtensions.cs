using Microsoft.Extensions.DependencyInjection;
using RVR.Framework.Email.Templates;

namespace RVR.Framework.Email.Extensions;

/// <summary>
/// Extension methods for registering email template services.
/// </summary>
public static class EmailTemplateServiceCollectionExtensions
{
    /// <summary>
    /// Adds the Rivora email template rendering services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddEmailTemplates(this IServiceCollection services)
    {
        services.AddSingleton<IEmailTemplateRenderer, InlineEmailTemplateRenderer>();
        return services;
    }
}
