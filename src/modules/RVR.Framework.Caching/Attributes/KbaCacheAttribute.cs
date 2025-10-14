namespace RVR.Framework.Caching.Attributes;

using System;

/// <summary>
/// Attribute to mark a method or controller action for caching.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public class KbaCacheAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the cache duration in seconds.
    /// </summary>
    public int Duration { get; set; } = 300;

    /// <summary>
    /// Gets or sets the cache tags for invalidation grouping.
    /// </summary>
    public string[] Tags { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets a custom cache key. If not provided, a key will be generated automatically.
    /// </summary>
    public string? Key { get; set; }

    /// <summary>
    /// Gets or sets the cache key prefix.
    /// </summary>
    public string? KeyPrefix { get; set; }

    /// <summary>
    /// Gets or sets whether to cache only successful responses (status code 200-299).
    /// </summary>
    public bool CacheOnlySuccess { get; set; } = true;

    /// <summary>
    /// Gets or sets the HTTP methods to cache. Defaults to GET only.
    /// </summary>
    public string[] HttpMethods { get; set; } = new[] { "GET" };

    /// <summary>
    /// Gets or sets whether to vary the cache by query string parameters.
    /// </summary>
    public bool VaryByQuery { get; set; } = true;

    /// <summary>
    /// Gets or sets specific query parameters to vary by. If null or empty, varies by all.
    /// </summary>
    public string[]? VaryByQueryParams { get; set; }

    /// <summary>
    /// Gets or sets whether to vary the cache by header values.
    /// </summary>
    public bool VaryByHeader { get; set; } = false;

    /// <summary>
    /// Gets or sets the headers to vary by. If null or empty, varies by none.
    /// </summary>
    public string[]? VaryByHeaders { get; set; }

    /// <summary>
    /// Gets or sets whether to vary the cache by user identity.
    /// </summary>
    public bool VaryByUser { get; set; } = false;

    /// <summary>
    /// Gets or sets the cache location (public, private, or none).
    /// </summary>
    public CacheLocation Location { get; set; } = CacheLocation.Public;

    /// <summary>
    /// Gets or sets the sliding expiration. If set, the cache entry will expire after the specified time of inactivity.
    /// </summary>
    public TimeSpan? SlidingExpiration { get; set; }

    /// <summary>
    /// Gets or sets the absolute expiration. If set, the cache entry will expire at the specified time.
    /// </summary>
    public TimeSpan? AbsoluteExpirationRelativeToNow { get; set; }

    /// <summary>
    /// Gets or sets the priority of the cache entry.
    /// </summary>
    public CachePriority Priority { get; set; } = CachePriority.Normal;

    /// <summary>
    /// Gets or sets whether to bypass cache for authenticated users.
    /// </summary>
    public bool BypassForAuthenticatedUsers { get; set; } = false;

    /// <summary>
    /// Gets or sets the condition for caching (e.g., a policy name).
    /// </summary>
    public string? Policy { get; set; }
}

/// <summary>
/// Specifies the cache location for HTTP caching.
/// </summary>
public enum CacheLocation
{
    /// <summary>
    /// The response can be cached by any cache (public).
    /// </summary>
    Public,

    /// <summary>
    /// The response can only be cached by the client (private).
    /// </summary>
    Private,

    /// <summary>
    /// The response should not be cached.
    /// </summary>
    None
}

/// <summary>
/// Specifies the priority of a cache entry.
/// </summary>
public enum CachePriority
{
    /// <summary>
    /// Low priority, can be removed first when memory is low.
    /// </summary>
    Low,

    /// <summary>
    /// Normal priority.
    /// </summary>
    Normal,

    /// <summary>
    /// High priority, should be kept as long as possible.
    /// </summary>
    High,

    /// <summary>
    /// Never remove automatically.
    /// </summary>
    NeverRemove
}
