namespace KBA.Framework.ApiVersioning.Models;

using System;
using System.Collections.Generic;

/// <summary>
/// Configuration options for API versioning.
/// </summary>
public class VersioningOptions
{
    /// <summary>
    /// Gets or sets the default API version.
    /// </summary>
    public string DefaultVersion { get; set; } = "1.0";

    /// <summary>
    /// Gets or sets whether to assume a default version when none is specified.
    /// </summary>
    public bool AssumeDefaultVersionWhenUnspecified { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to enable API versioning.
    /// </summary>
    public bool EnableVersioning { get; set; } = true;

    /// <summary>
    /// Gets or sets the versioning strategies to support.
    /// </summary>
    public VersioningStrategies Strategies { get; set; } = VersioningStrategies.All;

    /// <summary>
    /// Gets or sets the URL path segment for versioning (e.g., "v1", "v2").
    /// Used when UrlPath strategy is enabled.
    /// </summary>
    public string UrlPathPrefix { get; set; } = "v";

    /// <summary>
    /// Gets or sets the header name for versioning.
    /// Used when Header strategy is enabled.
    /// </summary>
    public string HeaderName { get; set; } = "X-API-Version";

    /// <summary>
    /// Gets or sets the query string parameter name for versioning.
    /// Used when QueryString strategy is enabled.
    /// </summary>
    public string QueryStringName { get; set; } = "api-version";

    /// <summary>
    /// Gets or sets the media type versioning format (e.g., "application/vnd.api.v{version}+json").
    /// Used when MediaType strategy is enabled.
    /// </summary>
    public string? MediaTypeFormat { get; set; }

    /// <summary>
    /// Gets or sets whether to include API version in response headers.
    /// </summary>
    public bool IncludeVersionInResponse { get; set; } = true;

    /// <summary>
    /// Gets or sets the response header name for the API version.
    /// </summary>
    public string ResponseHeaderName { get; set; } = "X-API-Version";

    /// <summary>
    /// Gets or sets whether to report API versions in responses.
    /// </summary>
    public bool ReportApiVersions { get; set; } = true;

    /// <summary>
    /// Gets or sets the supported API versions.
    /// </summary>
    public IEnumerable<string> SupportedVersions { get; set; } = new[] { "1.0", "2.0" };

    /// <summary>
    /// Gets or sets whether to enable automatic controller versioning conventions.
    /// </summary>
    public bool EnableAutomaticControllerVersioning { get; set; } = true;

    /// <summary>
    /// Gets or sets the controller template pattern for versioning.
    /// Default is "v{version}/[controller]".
    /// </summary>
    public string ControllerTemplate { get; set; } = "v{version}/[controller]";

    /// <summary>
    /// Gets or sets whether to use route constraints for versioning.
    /// </summary>
    public bool UseRouteConstraints { get; set; } = true;

    /// <summary>
    /// Gets or sets the route constraint pattern for versions.
    /// </summary>
    public string VersionRouteConstraint { get; set; } = @"^\d+(\.\d+)?$";

    /// <summary>
    /// Gets or sets whether to enable deprecation headers.
    /// </summary>
    public bool EnableDeprecationHeaders { get; set; } = true;

    /// <summary>
    /// Gets or sets the header name for deprecation warnings.
    /// </summary>
    public string DeprecationHeaderName { get; set; } = "Deprecation";

    /// <summary>
    /// Gets or sets the header name for sunset date.
    /// </summary>
    public string SunsetHeaderName { get; set; } = "Sunset";

    /// <summary>
    /// Gets or sets whether to enable version negotiation.
    /// </summary>
    public bool EnableVersionNegotiation { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to return 400 Bad Request for unsupported versions.
    /// If false, returns 404 Not Found.
    /// </summary>
    public bool Return400ForUnsupportedVersion { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to log versioning information.
    /// </summary>
    public bool EnableLogging { get; set; } = true;
}

/// <summary>
/// Specifies the API versioning strategies.
/// </summary>
[Flags]
public enum VersioningStrategies
{
    /// <summary>
    /// No versioning strategy.
    /// </summary>
    None = 0,

    /// <summary>
    /// Version via URL path segment (e.g., /v1/products).
    /// </summary>
    UrlPath = 1,

    /// <summary>
    /// Version via HTTP header.
    /// </summary>
    Header = 2,

    /// <summary>
    /// Version via query string parameter.
    /// </summary>
    QueryString = 4,

    /// <summary>
    /// Version via media type (Accept header).
    /// </summary>
    MediaType = 8,

    /// <summary>
    /// All versioning strategies.
    /// </summary>
    All = UrlPath | Header | QueryString | MediaType
}

/// <summary>
/// Represents API version information.
/// </summary>
public class ApiVersionInfo
{
    /// <summary>
    /// Gets or sets the version number.
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether this version is deprecated.
    /// </summary>
    public bool IsDeprecated { get; set; }

    /// <summary>
    /// Gets or sets the deprecation message.
    /// </summary>
    public string? DeprecationMessage { get; set; }

    /// <summary>
    /// Gets or sets the sunset date.
    /// </summary>
    public DateTime? SunsetDate { get; set; }

    /// <summary>
    /// Gets or sets the version status.
    /// </summary>
    public VersionStatus Status { get; set; } = VersionStatus.Stable;

    /// <summary>
    /// Gets or sets the supported HTTP methods.
    /// </summary>
    public IEnumerable<string> SupportedMethods { get; set; } = Enumerable.Empty<string>();

    /// <summary>
    /// Gets or sets the endpoint paths.
    /// </summary>
    public IEnumerable<string> Endpoints { get; set; } = Enumerable.Empty<string>();
}

/// <summary>
/// Specifies the version status.
/// </summary>
public enum VersionStatus
{
    /// <summary>
    /// Alpha version.
    /// </summary>
    Alpha,

    /// <summary>
    /// Beta version.
    /// </summary>
    Beta,

    /// <summary>
    /// Stable version.
    /// </summary>
    Stable,

    /// <summary>
    /// Deprecated version.
    /// </summary>
    Deprecated,

    /// <summary>
    /// Retired version.
    /// </summary>
    Retired
}
