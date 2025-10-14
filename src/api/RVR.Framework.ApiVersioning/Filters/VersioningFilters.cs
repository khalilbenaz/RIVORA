namespace RVR.Framework.ApiVersioning.Filters;

using System;
using System.Linq;
using System.Threading.Tasks;
using RVR.Framework.ApiVersioning.Attributes;
using RVR.Framework.ApiVersioning.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;

/// <summary>
/// Action filter for adding API version information to responses.
/// </summary>
public class ApiVersionResponseFilter : IAsyncResultFilter
{
    private readonly VersioningOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiVersionResponseFilter"/> class.
    /// </summary>
    /// <param name="options">The versioning options.</param>
    public ApiVersionResponseFilter(IOptions<VersioningOptions> options)
    {
        _options = options?.Value ?? new VersioningOptions();
    }

    /// <inheritdoc/>
    public Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        if (!_options.EnableVersioning || !_options.IncludeVersionInResponse)
        {
            return next();
        }

        var httpContext = context.HttpContext;
        var version = GetApiVersion(httpContext);

        if (!string.IsNullOrWhiteSpace(version))
        {
            httpContext.Response.Headers[_options.ResponseHeaderName] = version;
        }

        // Add deprecation headers if applicable
        AddDeprecationHeaders(context, httpContext);

        return next();
    }

    private string? GetApiVersion(HttpContext httpContext)
    {
        // Try to get version from route data
        if (httpContext.GetRouteData().Values.TryGetValue("version", out var routeVersion))
        {
            return routeVersion?.ToString();
        }

        // Try from header
        if (httpContext.Request.Headers.TryGetValue(_options.HeaderName, out var headerVersion))
        {
            return headerVersion.ToString();
        }

        // Try from query string
        if (httpContext.Request.Query.TryGetValue(_options.QueryStringName, out var queryVersion))
        {
            return queryVersion.ToString();
        }

        return _options.DefaultVersion;
    }

    private void AddDeprecationHeaders(ResultExecutingContext context, HttpContext httpContext)
    {
        if (!_options.EnableDeprecationHeaders)
        {
            return;
        }

        var controller = context.Controller.GetType();
        var action = context.ActionDescriptor.DisplayName;

        // Check for Deprecated attribute on controller
        var controllerDeprecatedAttr = controller.GetCustomAttributes(true)
            .OfType<DeprecatedAttribute>()
            .FirstOrDefault();

        if (controllerDeprecatedAttr != null)
        {
            httpContext.Response.Headers[_options.DeprecationHeaderName] = "true";

            if (controllerDeprecatedAttr.SunsetDate.HasValue)
            {
                httpContext.Response.Headers[_options.SunsetHeaderName] =
                    controllerDeprecatedAttr.SunsetDate.Value.ToString("R");
            }

            if (!string.IsNullOrWhiteSpace(controllerDeprecatedAttr.Message))
            {
                httpContext.Response.Headers["Deprecation-Message"] = controllerDeprecatedAttr.Message;
            }
        }

        // Check for ApiVersion attribute with deprecation
        var apiVersionAttr = controller.GetCustomAttributes(true)
            .OfType<ApiVersionAttribute>()
            .FirstOrDefault(a => a.Deprecated);

        if (apiVersionAttr != null)
        {
            httpContext.Response.Headers[_options.DeprecationHeaderName] = "true";

            if (apiVersionAttr.SunsetDate.HasValue)
            {
                httpContext.Response.Headers[_options.SunsetHeaderName] =
                    apiVersionAttr.SunsetDate.Value.ToString("R");
            }

            if (!string.IsNullOrWhiteSpace(apiVersionAttr.DeprecationMessage))
            {
                httpContext.Response.Headers["Deprecation-Message"] = apiVersionAttr.DeprecationMessage;
            }
        }
    }
}

/// <summary>
/// Action filter for validating API version in requests.
/// </summary>
public class ApiVersionValidationFilter : IAsyncActionFilter
{
    private readonly VersioningOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiVersionValidationFilter"/> class.
    /// </summary>
    /// <param name="options">The versioning options.</param>
    public ApiVersionValidationFilter(IOptions<VersioningOptions> options)
    {
        _options = options?.Value ?? new VersioningOptions();
    }

    /// <inheritdoc/>
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!_options.EnableVersioning)
        {
            await next();
            return;
        }

        var httpContext = context.HttpContext;
        var version = GetRequestedVersion(httpContext);

        if (string.IsNullOrWhiteSpace(version))
        {
            if (!_options.AssumeDefaultVersionWhenUnspecified)
            {
                context.Result = new BadRequestObjectResult(new
                {
                    error = "API version is required",
                    message = "Please specify an API version using URL path, header, or query string"
                });
                return;
            }

            version = _options.DefaultVersion;
        }

        // Validate version format
        if (!IsValidVersionFormat(version))
        {
            context.Result = new BadRequestObjectResult(new
            {
                error = "Invalid API version format",
                message = "API version must be in the format 'major' or 'major.minor' (e.g., '1', '1.0', '2.5')"
            });
            return;
        }

        // Check if version is supported
        if (!_options.SupportedVersions.Contains(version, StringComparer.OrdinalIgnoreCase))
        {
            if (_options.Return400ForUnsupportedVersion)
            {
                context.Result = new BadRequestObjectResult(new
                {
                    error = "Unsupported API version",
                    message = $"The requested version '{version}' is not supported",
                    supportedVersions = _options.SupportedVersions
                });
            }
            else
            {
                context.Result = new NotFoundObjectResult(new
                {
                    error = "API version not found",
                    message = $"The requested version '{version}' was not found"
                });
            }
            return;
        }

        await next();
    }

    private string? GetRequestedVersion(HttpContext httpContext)
    {
        // Try URL path first (e.g., /v1/products)
        if (_options.Strategies.HasFlag(VersioningStrategies.UrlPath))
        {
            var path = httpContext.Request.Path.Value;
            if (!string.IsNullOrWhiteSpace(path))
            {
                var match = System.Text.RegularExpressions.Regex.Match(
                    path,
                    $@"/{_options.UrlPathPrefix}(\d+(?:\.\d+)?)",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                if (match.Success)
                {
                    return match.Groups[1].Value;
                }
            }
        }

        // Try header
        if (_options.Strategies.HasFlag(VersioningStrategies.Header))
        {
            if (httpContext.Request.Headers.TryGetValue(_options.HeaderName, out var headerVersion))
            {
                return headerVersion.ToString();
            }
        }

        // Try query string
        if (_options.Strategies.HasFlag(VersioningStrategies.QueryString))
        {
            if (httpContext.Request.Query.TryGetValue(_options.QueryStringName, out var queryVersion))
            {
                return queryVersion.ToString();
            }
        }

        return null;
    }

    private bool IsValidVersionFormat(string version)
    {
        return System.Text.RegularExpressions.Regex.IsMatch(version, _options.VersionRouteConstraint);
    }
}

/// <summary>
/// Filter for adding supported versions header to responses.
/// </summary>
public class SupportedVersionsFilter : IAsyncResultFilter
{
    private readonly VersioningOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="SupportedVersionsFilter"/> class.
    /// </summary>
    /// <param name="options">The versioning options.</param>
    public SupportedVersionsFilter(IOptions<VersioningOptions> options)
    {
        _options = options?.Value ?? new VersioningOptions();
    }

    /// <inheritdoc/>
    public Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        if (_options.EnableVersioning && _options.ReportApiVersions)
        {
            var httpContext = context.HttpContext;
            var supportedVersions = string.Join(", ", _options.SupportedVersions);
            httpContext.Response.Headers["X-Supported-API-Versions"] = supportedVersions;
        }

        return next();
    }
}
