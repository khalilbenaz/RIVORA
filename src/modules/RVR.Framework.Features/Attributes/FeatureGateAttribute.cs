namespace RVR.Framework.Features.Attributes;

using System.Diagnostics.CodeAnalysis;
using RVR.Framework.Features.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

/// <summary>
/// Attribute to gate controller actions or entire controllers based on feature flags.
/// When applied, the action/controller will only be accessible if the specified feature is enabled.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class FeatureGateAttribute : TypeFilterAttribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FeatureGateAttribute"/> class.
    /// </summary>
    /// <param name="featureName">The name of the feature that must be enabled.</param>
    public FeatureGateAttribute(string featureName) : base(typeof(FeatureGateFilter))
    {
        FeatureName = featureName;
        Arguments = new object[] { featureName };
    }

    /// <summary>
    /// Gets the name of the feature that must be enabled.
    /// </summary>
    public string FeatureName { get; }

    /// <summary>
    /// Gets or sets the HTTP status code to return when the feature is disabled.
    /// Default is 404 (NotFound) to hide the existence of the feature.
    /// </summary>
    public int StatusCode { get; set; } = 404;

    /// <summary>
    /// Gets or sets a custom message to return when the feature is disabled.
    /// </summary>
    public string? Message { get; set; }
}

/// <summary>
/// Filter that implements the feature gate logic.
/// </summary>
internal class FeatureGateFilter : IAsyncActionFilter
{
    private readonly string _featureName;
    private readonly ILogger<FeatureGateFilter> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="FeatureGateFilter"/> class.
    /// </summary>
    /// <param name="featureName">The name of the feature to check.</param>
    /// <param name="logger">The logger instance.</param>
    public FeatureGateFilter(string featureName, ILogger<FeatureGateFilter> logger)
    {
        _featureName = featureName ?? throw new ArgumentNullException(nameof(featureName));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var featureManager = context.HttpContext.RequestServices.GetRequiredService<IFeatureManager>();
        
        try
        {
            var isEnabled = await featureManager.IsEnabledAsync(_featureName, context.HttpContext.RequestAborted);
            
            if (!isEnabled)
            {
                _logger.LogInformation("Feature {FeatureName} is disabled, blocking access to {Controller}.{Action}", 
                    _featureName, 
                    context.Controller.GetType().Name, 
                    context.ActionDescriptor.DisplayName);

                var attribute = context.ActionDescriptor.EndpointMetadata
                    .OfType<FeatureGateAttribute>()
                    .FirstOrDefault(a => a.FeatureName == _featureName)
                    ?? GetFeatureGateAttributes(context.Controller.GetType())
                        .FirstOrDefault(a => a.FeatureName == _featureName);

                var statusCode = attribute?.StatusCode ?? 404;
                var message = attribute?.Message ?? $"Feature '{_featureName}' is not available";

                context.Result = new ObjectResult(new
                {
                    error = "Feature Disabled",
                    message = message,
                    feature = _featureName
                })
                {
                    StatusCode = statusCode
                };
                return;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking feature {FeatureName}", _featureName);
            
            // In case of error, we can either block or allow access based on configuration
            // Default behavior is to block for safety
            context.Result = new StatusCodeResult(503);
            return;
        }

        await next();
    }

    private static IEnumerable<FeatureGateAttribute> GetFeatureGateAttributes(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type controllerType)
    {
        return controllerType.GetCustomAttributes(typeof(FeatureGateAttribute), true)
            .Cast<FeatureGateAttribute>();
    }
}

/// <summary>
/// Extension methods for feature gate functionality.
/// </summary>
public static class FeatureGateExtensions
{
    /// <summary>
    /// Checks if a feature is enabled and throws an exception if not.
    /// </summary>
    /// <param name="featureManager">The feature manager.</param>
    /// <param name="featureName">The name of the feature to check.</param>
    /// <exception cref="FeatureDisabledException">Thrown when the feature is disabled.</exception>
    public static void RequireFeature(this IFeatureManager featureManager, string featureName)
    {
        if (!featureManager.IsEnabled(featureName))
        {
            throw new FeatureDisabledException(featureName);
        }
    }

    /// <summary>
    /// Checks if a feature is enabled and throws an exception if not.
    /// </summary>
    /// <param name="featureManager">The feature manager.</param>
    /// <param name="featureName">The name of the feature to check.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <exception cref="FeatureDisabledException">Thrown when the feature is disabled.</exception>
    public static async Task RequireFeatureAsync(this IFeatureManager featureManager, string featureName, CancellationToken cancellationToken = default)
    {
        if (!await featureManager.IsEnabledAsync(featureName, cancellationToken))
        {
            throw new FeatureDisabledException(featureName);
        }
    }
}

/// <summary>
/// Exception thrown when a required feature is disabled.
/// </summary>
public class FeatureDisabledException : Exception
{
    /// <summary>
    /// Gets the name of the disabled feature.
    /// </summary>
    public string FeatureName { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="FeatureDisabledException"/> class.
    /// </summary>
    /// <param name="featureName">The name of the disabled feature.</param>
    public FeatureDisabledException(string featureName)
        : base($"Feature '{featureName}' is disabled")
    {
        FeatureName = featureName;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FeatureDisabledException"/> class.
    /// </summary>
    /// <param name="featureName">The name of the disabled feature.</param>
    /// <param name="message">The error message.</param>
    public FeatureDisabledException(string featureName, string message)
        : base(message)
    {
        FeatureName = featureName;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FeatureDisabledException"/> class.
    /// </summary>
    /// <param name="featureName">The name of the disabled feature.</param>
    /// <param name="innerException">The inner exception.</param>
    public FeatureDisabledException(string featureName, Exception innerException)
        : base($"Feature '{featureName}' is disabled", innerException)
    {
        FeatureName = featureName;
    }
}
