namespace KBA.Framework.Security.Models;

using System;

/// <summary>
/// Configuration for rate limiting rules.
/// </summary>
public class RateLimitRule
{
    /// <summary>
    /// Gets or sets the name of the rule.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the endpoint pattern (e.g., "/api/orders/*").
    /// </summary>
    public string? EndpointPattern { get; set; }

    /// <summary>
    /// Gets or sets the rate limiting strategy.
    /// </summary>
    public RateLimitStrategy Strategy { get; set; } = RateLimitStrategy.SlidingWindow;

    /// <summary>
    /// Gets or sets the maximum number of requests allowed.
    /// </summary>
    public int Limit { get; set; } = 100;

    /// <summary>
    /// Gets or sets the time window in seconds.
    /// </summary>
    public int WindowSeconds { get; set; } = 60;

    /// <summary>
    /// Gets or sets the token bucket capacity (max tokens).
    /// Used only for TokenBucket strategy.
    /// </summary>
    public int BucketCapacity { get; set; } = 100;

    /// <summary>
    /// Gets or sets the token bucket refill rate (tokens per second).
    /// Used only for TokenBucket strategy.
    /// </summary>
    public double RefillRate { get; set; } = 1.0;

    /// <summary>
    /// Gets or sets the key selector type (IP, User, Tenant, Endpoint).
    /// </summary>
    public RateLimitKeyType KeyType { get; set; } = RateLimitKeyType.IpAddress;

    /// <summary>
    /// Gets or sets a custom key selector function name.
    /// </summary>
    public string? CustomKeySelector { get; set; }

    /// <summary>
    /// Gets or sets the HTTP status code to return when rate limited.
    /// Default is 429 (Too Many Requests).
    /// </summary>
    public int StatusCode { get; set; } = 429;

    /// <summary>
    /// Gets or sets the message to return when rate limited.
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Gets or sets whether to include rate limit headers in the response.
    /// </summary>
    public bool IncludeHeaders { get; set; } = true;

    /// <summary>
    /// Gets or sets the order of this rule (lower values are evaluated first).
    /// </summary>
    public int Order { get; set; } = 0;

    /// <summary>
    /// Gets or sets whether the rule is enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;
}

/// <summary>
/// Represents the rate limiting strategy.
/// </summary>
public enum RateLimitStrategy
{
    /// <summary>
    /// Fixed window counter - resets at fixed intervals.
    /// Simple but can allow bursts at window boundaries.
    /// </summary>
    FixedWindow = 0,

    /// <summary>
    /// Sliding window counter - more accurate, smooths out bursts.
    /// </summary>
    SlidingWindow = 1,

    /// <summary>
    /// Token bucket - allows controlled bursting while maintaining average rate.
    /// </summary>
    TokenBucket = 2
}

/// <summary>
/// Represents the type of key used for rate limiting.
/// </summary>
public enum RateLimitKeyType
{
    /// <summary>
    /// Rate limit by client IP address.
    /// </summary>
    IpAddress = 0,

    /// <summary>
    /// Rate limit by user ID (requires authentication).
    /// </summary>
    UserId = 1,

    /// <summary>
    /// Rate limit by tenant ID (for multi-tenancy).
    /// </summary>
    TenantId = 2,

    /// <summary>
    /// Rate limit by endpoint (global limit for the endpoint).
    /// </summary>
    Endpoint = 3,

    /// <summary>
    /// Rate limit by a combination of IP and endpoint.
    /// </summary>
    IpAndEndpoint = 4,

    /// <summary>
    /// Rate limit by a combination of user and endpoint.
    /// </summary>
    UserAndEndpoint = 5,

    /// <summary>
    /// Rate limit by a custom key selector.
    /// </summary>
    Custom = 6
}

/// <summary>
/// Result of a rate limit check.
/// </summary>
public class RateLimitResult
{
    /// <summary>
    /// Gets or sets a value indicating whether the request is allowed.
    /// </summary>
    public bool IsAllowed { get; set; }

    /// <summary>
    /// Gets or sets the remaining number of requests in the current window.
    /// </summary>
    public long Remaining { get; set; }

    /// <summary>
    /// Gets or sets the time when the rate limit window resets (Unix timestamp).
    /// </summary>
    public long ResetAt { get; set; }

    /// <summary>
    /// Gets or sets the total limit for the current window.
    /// </summary>
    public long Limit { get; set; }

    /// <summary>
    /// Gets or sets the retry-after time in seconds.
    /// </summary>
    public int? RetryAfterSeconds { get; set; }

    /// <summary>
    /// Gets or sets the name of the rule that was applied.
    /// </summary>
    public string? RuleName { get; set; }

    /// <summary>
    /// Creates an allowed result.
    /// </summary>
    /// <param name="remaining">The remaining requests.</param>
    /// <param name="resetAt">The reset time.</param>
    /// <param name="limit">The limit.</param>
    /// <param name="ruleName">The rule name.</param>
    /// <returns>An allowed <see cref="RateLimitResult"/>.</returns>
    public static RateLimitResult Allowed(long remaining, long resetAt, long limit, string? ruleName = null)
    {
        return new RateLimitResult
        {
            IsAllowed = true,
            Remaining = remaining,
            ResetAt = resetAt,
            Limit = limit,
            RuleName = ruleName
        };
    }

    /// <summary>
    /// Creates a denied result.
    /// </summary>
    /// <param name="resetAt">The reset time.</param>
    /// <param name="retryAfterSeconds">The retry after seconds.</param>
    /// <param name="limit">The limit.</param>
    /// <param name="ruleName">The rule name.</param>
    /// <returns>A denied <see cref="RateLimitResult"/>.</returns>
    public static RateLimitResult Denied(long resetAt, int retryAfterSeconds, long limit, string? ruleName = null)
    {
        return new RateLimitResult
        {
            IsAllowed = false,
            Remaining = 0,
            ResetAt = resetAt,
            Limit = limit,
            RetryAfterSeconds = retryAfterSeconds,
            RuleName = ruleName
        };
    }
}
