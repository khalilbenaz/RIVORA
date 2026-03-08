namespace KBA.Framework.Security.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KBA.Framework.Security.Interfaces;
using KBA.Framework.Security.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

/// <summary>
/// Service for evaluating rate limits using various strategies.
/// </summary>
public class RateLimitService : IRateLimitService
{
    private readonly IRateLimitStore _store;
    private readonly RateLimitOptions _options;
    private readonly ILogger<RateLimitService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="RateLimitService"/> class.
    /// </summary>
    /// <param name="store">The rate limit store.</param>
    /// <param name="options">The rate limit options.</param>
    /// <param name="logger">The logger.</param>
    public RateLimitService(
        IRateLimitStore store,
        IOptions<RateLimitOptions> options,
        ILogger<RateLimitService> logger)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<RateLimitResult> CheckAsync(
        string key,
        RateLimitRule rule,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Key cannot be empty.", nameof(key));
        }

        if (rule == null)
        {
            throw new ArgumentNullException(nameof(rule));
        }

        if (!rule.Enabled)
        {
            return RateLimitResult.Allowed(rule.Limit, GetResetTime(0), rule.Limit, rule.Name);
        }

        var fullKey = $"{rule.Name}:{key}";

        return rule.Strategy switch
        {
            RateLimitStrategy.FixedWindow => await CheckFixedWindowAsync(fullKey, rule, cancellationToken),
            RateLimitStrategy.SlidingWindow => await CheckSlidingWindowAsync(fullKey, rule, cancellationToken),
            RateLimitStrategy.TokenBucket => await CheckTokenBucketAsync(fullKey, rule, cancellationToken),
            _ => throw new ArgumentOutOfRangeException(nameof(rule), $"Unknown strategy: {rule.Strategy}")
        };
    }

    /// <inheritdoc/>
    public async Task<RateLimitResult> CheckAsync(
        string endpoint,
        string? ipAddress,
        string? userId,
        string? tenantId,
        CancellationToken cancellationToken = default)
    {
        // Find applicable rules
        var applicableRules = _options.Rules
            .Where(r => r.Enabled && MatchesEndpoint(r, endpoint))
            .OrderBy(r => r.Order)
            .ToList();

        if (applicableRules.Count == 0)
        {
            // No rules apply, allow the request
            return RateLimitResult.Allowed(long.MaxValue, GetResetTime(0), long.MaxValue, null);
        }

        // Check each rule
        foreach (var rule in applicableRules)
        {
            var key = BuildKey(rule, endpoint, ipAddress, userId, tenantId);
            var result = await CheckAsync(key, rule, cancellationToken);

            if (!result.IsAllowed)
            {
                _logger.LogWarning(
                    "Rate limit exceeded for rule {RuleName}, key {Key}, endpoint {Endpoint}",
                    rule.Name, key, endpoint);

                return result;
            }
        }

        // All rules passed
        return applicableRules.Count > 0
            ? RateLimitResult.Allowed(
                  applicableRules.First().Limit,
                  GetResetTime(applicableRules.First().WindowSeconds),
                  applicableRules.First().Limit,
                  applicableRules.First().Name)
            : RateLimitResult.Allowed(long.MaxValue, GetResetTime(0), long.MaxValue, null);
    }

    /// <inheritdoc/>
    public string BuildKey(RateLimitRule rule, string endpoint, string? ipAddress, string? userId, string? tenantId)
    {
        return rule.KeyType switch
        {
            RateLimitKeyType.IpAddress => ipAddress ?? "anonymous",
            RateLimitKeyType.UserId => userId ?? "anonymous",
            RateLimitKeyType.TenantId => tenantId ?? "default",
            RateLimitKeyType.Endpoint => endpoint,
            RateLimitKeyType.IpAndEndpoint => $"{ipAddress ?? "anonymous"}:{endpoint}",
            RateLimitKeyType.UserAndEndpoint => $"{userId ?? "anonymous"}:{endpoint}",
            RateLimitKeyType.Custom => $"{rule.CustomKeySelector}:{endpoint}",
            _ => ipAddress ?? "anonymous"
        };
    }

    private bool MatchesEndpoint(RateLimitRule rule, string endpoint)
    {
        if (string.IsNullOrWhiteSpace(rule.EndpointPattern))
        {
            return true; // No pattern means all endpoints
        }

        // Simple wildcard matching
        var pattern = rule.EndpointPattern;

        if (pattern.EndsWith("*"))
        {
            var prefix = pattern[..^1];
            return endpoint.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
        }

        return string.Equals(endpoint, pattern, StringComparison.OrdinalIgnoreCase);
    }

    private async Task<RateLimitResult> CheckFixedWindowAsync(
        string key,
        RateLimitRule rule,
        CancellationToken cancellationToken)
    {
        var count = await _store.IncrementAsync(key, rule.WindowSeconds, cancellationToken);
        var resetTime = GetResetTime(rule.WindowSeconds);

        if (count > rule.Limit)
        {
            return RateLimitResult.Denied(
                resetTime,
                rule.WindowSeconds,
                rule.Limit,
                rule.Name);
        }

        return RateLimitResult.Allowed(
            rule.Limit - count,
            resetTime,
            rule.Limit,
            rule.Name);
    }

    private async Task<RateLimitResult> CheckSlidingWindowAsync(
        string key,
        RateLimitRule rule,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var windowStart = now.AddSeconds(-rule.WindowSeconds);
        var timestampKey = $"{key}:ts";

        var lastTimestamp = await _store.GetTimestampAsync(timestampKey, cancellationToken);

        if (lastTimestamp == null || lastTimestamp < windowStart)
        {
            // New window, reset count
            await _store.SetAsync(key, 1, rule.WindowSeconds, cancellationToken);
            await _store.SetTimestampAsync(timestampKey, now, rule.WindowSeconds, cancellationToken);

            return RateLimitResult.Allowed(
                rule.Limit - 1,
                GetResetTime(rule.WindowSeconds),
                rule.Limit,
                rule.Name);
        }

        var count = await _store.IncrementAsync(key, rule.WindowSeconds, cancellationToken);
        var resetTime = GetResetTime(rule.WindowSeconds);

        if (count > rule.Limit)
        {
            return RateLimitResult.Denied(
                resetTime,
                rule.WindowSeconds,
                rule.Limit,
                rule.Name);
        }

        return RateLimitResult.Allowed(
            rule.Limit - count,
            resetTime,
            rule.Limit,
            rule.Name);
    }

    private async Task<RateLimitResult> CheckTokenBucketAsync(
        string key,
        RateLimitRule rule,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var bucketKey = $"{key}:bucket";

        var currentTokens = await _store.GetTokenCountAsync(bucketKey, cancellationToken);

        if (currentTokens < 1)
        {
            // No tokens available, calculate retry time
            var retryAfterSeconds = (int)Math.Ceiling(1.0 / rule.RefillRate);

            return RateLimitResult.Denied(
                GetResetTime(rule.WindowSeconds),
                retryAfterSeconds,
                rule.BucketCapacity,
                rule.Name);
        }

        // Consume one token
        var newTokenCount = currentTokens - 1;
        await _store.SetTokenBucketAsync(bucketKey, newTokenCount, now, rule.WindowSeconds, cancellationToken);

        return RateLimitResult.Allowed(
            (long)newTokenCount,
            GetResetTime(rule.WindowSeconds),
            rule.BucketCapacity,
            rule.Name);
    }

    private static long GetResetTime(int windowSeconds)
    {
        var resetAt = DateTime.UtcNow.AddSeconds(windowSeconds);
        return new DateTimeOffset(resetAt).ToUnixTimeSeconds();
    }
}

/// <summary>
/// Configuration options for rate limiting.
/// </summary>
public class RateLimitOptions
{
    /// <summary>
    /// Gets or sets the list of rate limiting rules.
    /// </summary>
    public List<RateLimitRule> Rules { get; set; } = new();

    /// <summary>
    /// Gets or sets whether rate limiting is enabled globally.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the default rule to apply when no specific rule matches.
    /// </summary>
    public RateLimitRule? DefaultRule { get; set; }

    /// <summary>
    /// Gets or sets whether to bypass rate limiting for authenticated administrators.
    /// </summary>
    public bool BypassForAdmins { get; set; } = false;

    /// <summary>
    /// Gets or sets whether to include rate limit headers in the response.
    /// </summary>
    public bool IncludeHeaders { get; set; } = true;
}

/// <summary>
/// Defines the contract for rate limit service operations.
/// </summary>
public interface IRateLimitService
{
    /// <summary>
    /// Checks if a request is allowed under the specified rate limit rule.
    /// </summary>
    /// <param name="key">The rate limit key.</param>
    /// <param name="rule">The rate limit rule.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The rate limit result.</returns>
    Task<RateLimitResult> CheckAsync(string key, RateLimitRule rule, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a request is allowed based on configured rules.
    /// </summary>
    /// <param name="endpoint">The endpoint being accessed.</param>
    /// <param name="ipAddress">The client IP address.</param>
    /// <param name="userId">The user ID (if authenticated).</param>
    /// <param name="tenantId">The tenant ID (for multi-tenancy).</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The rate limit result.</returns>
    Task<RateLimitResult> CheckAsync(
        string endpoint,
        string? ipAddress,
        string? userId,
        string? tenantId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Builds a rate limit key based on the rule configuration.
    /// </summary>
    /// <param name="rule">The rate limit rule.</param>
    /// <param name="endpoint">The endpoint.</param>
    /// <param name="ipAddress">The IP address.</param>
    /// <param name="userId">The user ID.</param>
    /// <param name="tenantId">The tenant ID.</param>
    /// <returns>The rate limit key.</returns>
    string BuildKey(RateLimitRule rule, string endpoint, string? ipAddress, string? userId, string? tenantId);
}
