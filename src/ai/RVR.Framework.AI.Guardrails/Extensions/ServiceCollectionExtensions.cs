using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RVR.Framework.AI.Guardrails.Configuration;
using RVR.Framework.AI.Guardrails.Guardrails;

namespace RVR.Framework.AI.Guardrails.Extensions;

/// <summary>
/// Extension methods for registering AI Guardrails services in the dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the RIVORA AI Guardrails services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="config">The application configuration containing guardrail settings.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddRvrAIGuardrails(this IServiceCollection services, IConfiguration config)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(config);

        var section = config.GetSection(GuardrailOptions.SectionPath);

        // Bind options
        services.Configure<GuardrailOptions>(section);
        services.Configure<PromptInjectionOptions>(section.GetSection(nameof(GuardrailOptions.PromptInjection)));
        services.Configure<PiiDetectionOptions>(section.GetSection(nameof(GuardrailOptions.PiiDetection)));
        services.Configure<ContentModerationOptions>(section.GetSection(nameof(GuardrailOptions.ContentModeration)));
        services.Configure<TokenBudgetOptions>(section.GetSection(nameof(GuardrailOptions.TokenBudget)));
        services.Configure<OutputValidationOptions>(section.GetSection(nameof(GuardrailOptions.OutputValidation)));

        // Register guardrails
        services.AddSingleton<IGuardrail, PromptInjectionGuardrail>();
        services.AddSingleton<IGuardrail, PiiDetectionGuardrail>();
        services.AddSingleton<IGuardrail, ContentModerationGuardrail>();
        services.AddSingleton<IGuardrail, TokenBudgetGuardrail>();
        services.AddSingleton<IGuardrail, OutputSchemaGuardrail>();

        // Register pipeline
        services.AddSingleton<IGuardrailPipeline, GuardrailPipeline>();

        return services;
    }
}
