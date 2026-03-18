using Microsoft.Extensions.DependencyInjection;
using RVR.Framework.SaaS.Onboarding.Steps;

namespace RVR.Framework.SaaS.Onboarding.Extensions;

/// <summary>
/// Extension methods for registering tenant onboarding services and default pipeline steps.
/// </summary>
public static class OnboardingServiceCollectionExtensions
{
    /// <summary>
    /// Adds the tenant onboarding wizard to the service collection, including the
    /// orchestrator and all default onboarding steps.
    /// </summary>
    /// <remarks>
    /// <para>Default steps (executed in order):</para>
    /// <list type="number">
    ///   <item><description><see cref="CreateTenantStep"/> (100) — creates the tenant record</description></item>
    ///   <item><description><see cref="CreateAdminUserStep"/> (200) — creates the admin user</description></item>
    ///   <item><description><see cref="AssignSubscriptionPlanStep"/> (300) — assigns the subscription plan</description></item>
    ///   <item><description><see cref="SendWelcomeEmailStep"/> (400) — sends the welcome email</description></item>
    ///   <item><description><see cref="NotifyWebhookStep"/> (500) — publishes a webhook notification</description></item>
    /// </list>
    /// <para>
    /// Custom steps can be registered by adding additional <see cref="ITenantOnboardingStep"/>
    /// implementations to the service collection after calling this method.
    /// </para>
    /// </remarks>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddTenantOnboarding(this IServiceCollection services)
    {
        // Register the orchestrator.
        services.AddScoped<ITenantOnboardingService, TenantOnboardingService>();

        // Register all default onboarding steps.
        services.AddScoped<ITenantOnboardingStep, CreateTenantStep>();
        services.AddScoped<ITenantOnboardingStep, CreateAdminUserStep>();
        services.AddScoped<ITenantOnboardingStep, AssignSubscriptionPlanStep>();
        services.AddScoped<ITenantOnboardingStep, SendWelcomeEmailStep>();
        services.AddScoped<ITenantOnboardingStep, NotifyWebhookStep>();

        return services;
    }

    /// <summary>
    /// Adds a custom onboarding step to the pipeline.
    /// </summary>
    /// <typeparam name="TStep">The step implementation type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddOnboardingStep<TStep>(this IServiceCollection services)
        where TStep : class, ITenantOnboardingStep
    {
        services.AddScoped<ITenantOnboardingStep, TStep>();
        return services;
    }
}
