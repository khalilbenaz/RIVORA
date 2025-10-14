namespace RVR.Framework.ApiVersioning.Conventions;

using System;
using System.Linq;
using RVR.Framework.ApiVersioning.Attributes;
using RVR.Framework.ApiVersioning.Models;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.Options;

/// <summary>
/// Convention for automatically versioning controllers based on their namespace or folder structure.
/// </summary>
public class AutomaticVersioningConvention : IControllerModelConvention
{
    private readonly VersioningOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="AutomaticVersioningConvention"/> class.
    /// </summary>
    /// <param name="options">The versioning options.</param>
    public AutomaticVersioningConvention(IOptions<VersioningOptions> options)
    {
        _options = options?.Value ?? new VersioningOptions();
    }

    /// <inheritdoc/>
    public void Apply(ControllerModel controller)
    {
        if (!_options.EnableVersioning || !_options.EnableAutomaticControllerVersioning)
        {
            return;
        }

        // Skip if the controller already has an ApiVersion attribute
        if (controller.Attributes.OfType<ApiVersionAttribute>().Any() ||
            controller.Attributes.OfType<Asp.Versioning.ApiVersionAttribute>().Any())
        {
            return;
        }

        // Try to extract version from controller name (e.g., ProductsV2Controller -> v2)
        var versionFromName = ExtractVersionFromControllerName(controller.ControllerName);

        // Try to extract version from namespace (e.g., MyApi.V2.Controllers -> v2)
        var versionFromNamespace = ExtractVersionFromNamespace(controller.ControllerType.Namespace);

        // Use the extracted version or default
        var version = versionFromName ?? versionFromNamespace ?? _options.DefaultVersion;

        // Apply the version attribute via Filters (Attributes is IReadOnlyList)
        var apiVersion = new Asp.Versioning.ApiVersion(double.Parse(version));
        controller.Properties[typeof(Asp.Versioning.ApiVersion)] = apiVersion;

        // Apply route prefix if using URL path versioning
        if (_options.Strategies.HasFlag(VersioningStrategies.UrlPath))
        {
            ApplyRoutePrefix(controller, version);
        }
    }

    private string? ExtractVersionFromControllerName(string controllerName)
    {
        // Look for patterns like "ProductsV2" or "Products_V2"
        var match = System.Text.RegularExpressions.Regex.Match(
            controllerName,
            @"(?:V|_V)(\d+(?:\.\d+)?)$",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        if (match.Success)
        {
            return match.Groups[1].Value;
        }

        return null;
    }

    private string? ExtractVersionFromNamespace(string? namespaceName)
    {
        if (string.IsNullOrWhiteSpace(namespaceName))
        {
            return null;
        }

        // Look for patterns like "MyApi.V2.Controllers" or "MyApi.V2_0.Controllers"
        var match = System.Text.RegularExpressions.Regex.Match(
            namespaceName,
            @"\.V(\d+(?:\.\d+)?)(?:\.|$)",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        if (match.Success)
        {
            return match.Groups[1].Value;
        }

        return null;
    }

    private void ApplyRoutePrefix(ControllerModel controller, string version)
    {
        var versionPrefix = $"{_options.UrlPathPrefix}{version.Split('.')[0]}";

        // Check if there's already a route attribute
        var routeAttribute = controller.Selectors
            .FirstOrDefault(s => s.AttributeRouteModel != null)
            ?.AttributeRouteModel;

        if (routeAttribute != null)
        {
            // Prepend version prefix to existing route
            routeAttribute.Template = $"{versionPrefix}/{routeAttribute.Template}";
        }
        else
        {
            // Add new route with version prefix
            var selector = controller.Selectors.FirstOrDefault();
            if (selector == null)
            {
                selector = new SelectorModel();
                controller.Selectors.Add(selector);
            }
            selector.AttributeRouteModel = new AttributeRouteModel
            {
                Template = $"{versionPrefix}/[controller]"
            };
        }
    }
}

/// <summary>
/// Convention for applying version-specific route templates.
/// </summary>
public class VersionedRouteConvention : IControllerModelConvention
{
    private readonly VersioningOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="VersionedRouteConvention"/> class.
    /// </summary>
    /// <param name="options">The versioning options.</param>
    public VersionedRouteConvention(IOptions<VersioningOptions> options)
    {
        _options = options?.Value ?? new VersioningOptions();
    }

    /// <inheritdoc/>
    public void Apply(ControllerModel controller)
    {
        if (!_options.EnableVersioning)
        {
            return;
        }

        // Get the API version from attributes
        var apiVersionAttr = controller.Attributes
            .OfType<Asp.Versioning.ApiVersionAttribute>()
            .FirstOrDefault();

        if (apiVersionAttr == null)
        {
            return;
        }

        var version = apiVersionAttr.Versions.FirstOrDefault()?.ToString();
        if (string.IsNullOrWhiteSpace(version))
        {
            return;
        }

        // Apply versioned route template
        var versionPrefix = $"{_options.UrlPathPrefix}{version.Split('.')[0]}";

        foreach (var selector in controller.Selectors)
        {
            if (selector.AttributeRouteModel != null &&
                !selector.AttributeRouteModel.IsAbsoluteTemplate)
            {
                var existingTemplate = selector.AttributeRouteModel.Template;
                if (!string.IsNullOrWhiteSpace(existingTemplate) &&
                    !existingTemplate.StartsWith(versionPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    selector.AttributeRouteModel.Template = $"{versionPrefix}/{existingTemplate}";
                }
            }
        }
    }
}

/// <summary>
/// Convention for adding version-specific selectors.
/// </summary>
public class VersionSelectorConvention : IControllerModelConvention
{
    private readonly VersioningOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="VersionSelectorConvention"/> class.
    /// </summary>
    /// <param name="options">The versioning options.</param>
    public VersionSelectorConvention(IOptions<VersioningOptions> options)
    {
        _options = options?.Value ?? new VersioningOptions();
    }

    /// <inheritdoc/>
    public void Apply(ControllerModel controller)
    {
        if (!_options.EnableVersioning)
        {
            return;
        }

        // Get all API version attributes
        var apiVersionAttrs = controller.Attributes
            .OfType<Asp.Versioning.ApiVersionAttribute>()
            .ToList();

        if (apiVersionAttrs.Count == 0)
        {
            return;
        }

        // Add version constraints to selectors
        foreach (var selector in controller.Selectors)
        {
            foreach (var attr in apiVersionAttrs)
            {
                foreach (var version in attr.Versions)
                {
                    selector.EndpointMetadata.Add(new Asp.Versioning.ApiVersionAttribute(version.ToString()));
                }
            }
        }
    }
}
