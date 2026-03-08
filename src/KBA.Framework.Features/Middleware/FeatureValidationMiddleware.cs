namespace KBA.Framework.Features.Middleware;

using KBA.Framework.Features.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

/// <summary>
/// Middleware that validates feature flags for incoming requests.
/// Can check required features from headers, query strings, or route data.
/// </summary>
public class FeatureValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<FeatureValidationMiddleware> _logger;
    private readonly FeatureValidationOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="FeatureValidationMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next delegate in the pipeline.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="options">The validation options.</param>
    public FeatureValidationMiddleware(
        RequestDelegate next,
        ILogger<FeatureValidationMiddleware> logger,
        FeatureValidationOptions options)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// Invokes the middleware.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <param name="featureManager">The feature manager.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task InvokeAsync(HttpContext context, IFeatureManager featureManager)
    {
        // Check for required features from various sources
        var requiredFeatures = GetRequiredFeatures(context);

        foreach (var featureName in requiredFeatures)
        {
            try
            {
                var isEnabled = await featureManager.IsEnabledAsync(featureName, context.RequestAborted);
                
                if (!isEnabled)
                {
                    _logger.LogInformation("Request blocked: feature {FeatureName} is disabled", featureName);
                    
                    context.Response.StatusCode = _options.BlockingStatusCode;
                    context.Response.Headers.ContentType = "application/json";
                    
                    var responseBody = _options.BlockingResponse ?? new
                    {
                        error = "Feature Disabled",
                        message = $"The requested feature '{featureName}' is not available",
                        feature = featureName
                    };

                    await context.Response.WriteAsJsonAsync(responseBody, context.RequestAborted);
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating feature {FeatureName}", featureName);
                
                if (_options.FailClosed)
                {
                    context.Response.StatusCode = 503;
                    await context.Response.WriteAsJsonAsync(new
                    {
                        error = "Service Unavailable",
                        message = "Unable to validate feature flags"
                    }, context.RequestAborted);
                    return;
                }
            }
        }

        // Add feature status to response headers for debugging
        if (_options.AddDebugHeaders)
        {
            var allFeatures = featureManager.GetAllFeatures();
            foreach (var feature in allFeatures)
            {
                var headerName = $"X-Feature-{feature.Name}";
                if (!context.Response.Headers.ContainsKey(headerName))
                {
                    context.Response.Headers[headerName] = feature.Enabled ? "enabled" : "disabled";
                }
            }
        }

        await _next(context);
    }

    private IEnumerable<string> GetRequiredFeatures(HttpContext context)
    {
        var features = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Check header
        if (_options.CheckHeaders && 
            context.Request.Headers.TryGetValue(_options.HeaderName, out var headerValues))
        {
            foreach (var value in headerValues)
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    features.Add(value);
                }
            }
        }

        // Check query string
        if (_options.CheckQueryString && 
            context.Request.Query.TryGetValue(_options.QueryStringName, out var queryValues))
        {
            foreach (var value in queryValues)
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    features.Add(value);
                }
            }
        }

        // Check for path-based feature requirements
        if (_options.PathFeatureMappings != null)
        {
            var path = context.Request.Path.Value;
            if (!string.IsNullOrEmpty(path))
            {
                foreach (var mapping in _options.PathFeatureMappings)
                {
                    if (path.StartsWith(mapping.Key, StringComparison.OrdinalIgnoreCase))
                    {
                        features.Add(mapping.Value);
                    }
                }
            }
        }

        return features;
    }
}

/// <summary>
/// Options for configuring feature validation middleware.
/// </summary>
public class FeatureValidationOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether to check headers for required features.
    /// Default is false.
    /// </summary>
    public bool CheckHeaders { get; set; } = false;

    /// <summary>
    /// Gets or sets the header name to check for required features.
    /// Default is "X-Required-Features".
    /// </summary>
    public string HeaderName { get; set; } = "X-Required-Features";

    /// <summary>
    /// Gets or sets a value indicating whether to check query string for required features.
    /// Default is false.
    /// </summary>
    public bool CheckQueryString { get; set; } = false;

    /// <summary>
    /// Gets or sets the query string parameter name to check for required features.
    /// Default is "features".
    /// </summary>
    public string QueryStringName { get; set; } = "features";

    /// <summary>
    /// Gets or sets a dictionary of path prefixes to required features.
    /// </summary>
    public IDictionary<string, string>? PathFeatureMappings { get; set; }

    /// <summary>
    /// Gets or sets the HTTP status code to return when a required feature is disabled.
    /// Default is 404 (NotFound).
    /// </summary>
    public int BlockingStatusCode { get; set; } = 404;

    /// <summary>
    /// Gets or sets a custom response object when a feature is disabled.
    /// </summary>
    public object? BlockingResponse { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to fail closed (return 503) on errors.
    /// If false, errors are logged but requests proceed.
    /// Default is true.
    /// </summary>
    public bool FailClosed { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to add debug headers showing feature states.
    /// Default is false (only in development).
    /// </summary>
    public bool AddDebugHeaders { get; set; } = false;
}

/// <summary>
/// Extension methods for registering the feature validation middleware.
/// </summary>
public static class FeatureValidationMiddlewareExtensions
{
    /// <summary>
    /// Adds the feature validation middleware to the pipeline.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <param name="options">The validation options.</param>
    /// <returns>The application builder.</returns>
    public static IApplicationBuilder UseFeatureValidation(
        this IApplicationBuilder app, 
        FeatureValidationOptions? options = null)
    {
        return app.UseMiddleware<FeatureValidationMiddleware>(options ?? new FeatureValidationOptions());
    }
}
