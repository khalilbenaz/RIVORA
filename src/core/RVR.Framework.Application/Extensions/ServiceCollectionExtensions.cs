using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using RVR.Framework.Application.Behaviors;

namespace RVR.Framework.Application.Extensions;

/// <summary>
/// DI registration extensions for the Application layer.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers MediatR, FluentValidation validators, and all pipeline behaviors
    /// from the Application assembly.
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        var assembly = typeof(ServiceCollectionExtensions).Assembly;

        // Register MediatR handlers from the calling assembly
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));

        // Register all FluentValidation validators from the assembly
        services.AddValidatorsFromAssembly(assembly);

        // Register pipeline behaviors in execution order:
        // 1. Unhandled exception logging (outermost - catches everything)
        // 2. Performance monitoring
        // 3. Logging (request/response with timing)
        // 4. Validation (innermost - closest to handler)
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }
}
