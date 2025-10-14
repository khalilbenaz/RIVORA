namespace RVR.Framework.HealthChecks.Models;

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

/// <summary>
/// Represents the overall health status of the application.
/// </summary>
public class HealthStatus
{
    /// <summary>
    /// Gets or sets the overall status (Healthy, Degraded, Unhealthy).
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the total duration of all health checks.
    /// </summary>
    public TimeSpan TotalDuration { get; set; }

    /// <summary>
    /// Gets or sets the individual health check results.
    /// </summary>
    public IEnumerable<HealthCheckResult> Results { get; set; } = Enumerable.Empty<HealthCheckResult>();
}

/// <summary>
/// Represents the result of a single health check.
/// </summary>
public class HealthCheckResult
{
    /// <summary>
    /// Gets or sets the name of the health check.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the status of the health check (Healthy, Degraded, Unhealthy).
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the duration of the health check.
    /// </summary>
    public TimeSpan Duration { get; set; }

    /// <summary>
    /// Gets or sets the exception message if the health check failed.
    /// </summary>
    public string? Exception { get; set; }

    /// <summary>
    /// Gets or sets the description of the health check result.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the tags associated with this health check.
    /// </summary>
    public IEnumerable<string> Tags { get; set; } = Enumerable.Empty<string>();

    /// <summary>
    /// Gets or sets additional data returned by the health check.
    /// </summary>
    public Dictionary<string, object> Data { get; set; } = new();
}

/// <summary>
/// Represents the readiness status response.
/// </summary>
public class ReadinessStatus
{
    /// <summary>
    /// Gets or sets whether the application is ready to receive traffic.
    /// </summary>
    public bool IsReady { get; set; }

    /// <summary>
    /// Gets or sets the list of checks that are not ready.
    /// </summary>
    public IEnumerable<string> NotReadyChecks { get; set; } = Enumerable.Empty<string>();

    /// <summary>
    /// Gets or sets the detailed results of readiness checks.
    /// </summary>
    public IEnumerable<HealthCheckResult> Results { get; set; } = Enumerable.Empty<HealthCheckResult>();
}

/// <summary>
/// Represents the configuration for a health check.
/// </summary>
public class HealthCheckConfiguration
{
    /// <summary>
    /// Gets or sets the name of the health check.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the tags for filtering health checks.
    /// </summary>
    public IEnumerable<string> Tags { get; set; } = Enumerable.Empty<string>();

    /// <summary>
    /// Gets or sets the timeout for the health check.
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets or sets whether this check is required for readiness.
    /// </summary>
    public bool IsRequiredForReadiness { get; set; } = true;
}
