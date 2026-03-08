namespace KBA.Framework.ApiVersioning.Attributes;

using System;

/// <summary>
/// Attribute to specify the API version for a controller or action.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
public class ApiVersionAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the version number as a string (e.g., "1.0", "2.0").
    /// </summary>
    public string Version { get; set; } = "1.0";

    /// <summary>
    /// Gets or sets whether this version is deprecated.
    /// </summary>
    public bool Deprecated { get; set; }

    /// <summary>
    /// Gets or sets the deprecation message shown in responses.
    /// </summary>
    public string? DeprecationMessage { get; set; }

    /// <summary>
    /// Gets or sets the sunset date when this version will be removed.
    /// </summary>
    public DateTime? SunsetDate { get; set; }

    /// <summary>
    /// Gets or sets the status of this version (stable, beta, alpha).
    /// </summary>
    public VersionStatus Status { get; set; } = VersionStatus.Stable;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiVersionAttribute"/> class.
    /// </summary>
    public ApiVersionAttribute()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiVersionAttribute"/> class with the specified version.
    /// </summary>
    /// <param name="version">The version number.</param>
    public ApiVersionAttribute(string version)
    {
        Version = version;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiVersionAttribute"/> class with the specified major and minor version.
    /// </summary>
    /// <param name="major">The major version number.</param>
    /// <param name="minor">The minor version number.</param>
    public ApiVersionAttribute(int major, int minor)
    {
        Version = $"{major}.{minor}";
    }
}

/// <summary>
/// Specifies the status of an API version.
/// </summary>
public enum VersionStatus
{
    /// <summary>
    /// The version is in alpha testing.
    /// </summary>
    Alpha,

    /// <summary>
    /// The version is in beta testing.
    /// </summary>
    Beta,

    /// <summary>
    /// The version is stable and production-ready.
    /// </summary>
    Stable,

    /// <summary>
    /// The version is deprecated and will be removed.
    /// </summary>
    Deprecated,

    /// <summary>
    /// The version is retired and no longer available.
    /// </summary>
    Retired
}

/// <summary>
/// Attribute to specify that a controller or action supports multiple API versions.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
public class MapToApiVersionAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the version numbers this endpoint maps to.
    /// </summary>
    public string[] Versions { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Initializes a new instance of the <see cref="MapToApiVersionAttribute"/> class.
    /// </summary>
    /// <param name="versions">The version numbers.</param>
    public MapToApiVersionAttribute(params string[] versions)
    {
        Versions = versions ?? Array.Empty<string>();
    }
}

/// <summary>
/// Attribute to mark a controller or action as deprecated.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
public class DeprecatedAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the deprecation message.
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Gets or sets the sunset date.
    /// </summary>
    public DateTime? SunsetDate { get; set; }

    /// <summary>
    /// Gets or sets the recommended alternative.
    /// </summary>
    public string? Alternative { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DeprecatedAttribute"/> class.
    /// </summary>
    public DeprecatedAttribute()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DeprecatedAttribute"/> class with a message.
    /// </summary>
    /// <param name="message">The deprecation message.</param>
    public DeprecatedAttribute(string message)
    {
        Message = message;
    }
}
