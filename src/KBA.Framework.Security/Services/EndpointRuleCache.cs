namespace KBA.Framework.Security.Services;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using KBA.Framework.Security.Models;
using Microsoft.Extensions.Options;

/// <summary>
/// Pre-compiled cache for endpoint-to-rule mappings.
/// Eliminates runtime LINQ enumeration by building lookups at construction time.
/// Thread-safe using concurrent collections.
/// </summary>
public class EndpointRuleCache
{
    private readonly ConcurrentDictionary<string, List<RateLimitRule>> _exactMatchCache;
    private readonly List<WildcardPattern> _wildcardPatterns;
    private readonly List<RateLimitRule> _globalRules;

    /// <summary>
    /// Initializes a new instance of the <see cref="EndpointRuleCache"/> class.
    /// </summary>
    /// <param name="options">The rate limit options containing rules to cache.</param>
    public EndpointRuleCache(IOptions<RateLimitOptions> options)
    {
        if (options?.Value == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        var rateLimitOptions = options.Value;
        _exactMatchCache = new ConcurrentDictionary<string, List<RateLimitRule>>(StringComparer.OrdinalIgnoreCase);
        _wildcardPatterns = new List<WildcardPattern>();
        _globalRules = new List<RateLimitRule>();

        // Pre-compile rules at construction time
        CompileRules(rateLimitOptions.Rules);
    }

    /// <summary>
    /// Gets the applicable rules for the specified endpoint.
    /// Uses pre-compiled lookups for O(1) exact matches and O(k) wildcard matches
    /// where k is the number of wildcard patterns (not total rules).
    /// </summary>
    /// <param name="endpoint">The endpoint to match.</param>
    /// <returns>A list of applicable rules, ordered by priority.</returns>
    public List<RateLimitRule> GetRulesForEndpoint(string endpoint)
    {
        if (string.IsNullOrWhiteSpace(endpoint))
        {
            return new List<RateLimitRule>();
        }

        var applicableRules = new List<RateLimitRule>();

        // Add global rules (no pattern specified)
        if (_globalRules.Count > 0)
        {
            applicableRules.AddRange(_globalRules);
        }

        // Check exact match cache (O(1) lookup)
        if (_exactMatchCache.TryGetValue(endpoint, out var exactRules))
        {
            applicableRules.AddRange(exactRules);
        }

        // Check wildcard patterns (O(k) where k = number of wildcard patterns)
        foreach (var pattern in _wildcardPatterns)
        {
            if (endpoint.StartsWith(pattern.Prefix, StringComparison.OrdinalIgnoreCase))
            {
                applicableRules.AddRange(pattern.Rules);
            }
        }

        // Return rules already sorted by Order (sorted during compilation)
        return applicableRules;
    }

    /// <summary>
    /// Compiles the rules into optimized data structures at construction time.
    /// </summary>
    /// <param name="rules">The rules to compile.</param>
    private void CompileRules(List<RateLimitRule> rules)
    {
        if (rules == null || rules.Count == 0)
        {
            return;
        }

        // Filter to enabled rules and sort by Order once at construction
        var enabledRules = rules
            .Where(r => r.Enabled)
            .OrderBy(r => r.Order)
            .ToList();

        // Group rules by pattern type
        var exactMatchGroups = new Dictionary<string, List<RateLimitRule>>(StringComparer.OrdinalIgnoreCase);
        var wildcardGroups = new Dictionary<string, List<RateLimitRule>>(StringComparer.OrdinalIgnoreCase);

        foreach (var rule in enabledRules)
        {
            var pattern = rule.EndpointPattern;

            // Global rule (no pattern or empty pattern)
            if (string.IsNullOrWhiteSpace(pattern))
            {
                _globalRules.Add(rule);
                continue;
            }

            // Wildcard pattern (ends with *)
            if (pattern.EndsWith("*"))
            {
                var prefix = pattern[..^1];

                if (!wildcardGroups.ContainsKey(prefix))
                {
                    wildcardGroups[prefix] = new List<RateLimitRule>();
                }

                wildcardGroups[prefix].Add(rule);
            }
            else
            {
                // Exact match pattern
                if (!exactMatchGroups.ContainsKey(pattern))
                {
                    exactMatchGroups[pattern] = new List<RateLimitRule>();
                }

                exactMatchGroups[pattern].Add(rule);
            }
        }

        // Populate exact match cache
        foreach (var kvp in exactMatchGroups)
        {
            _exactMatchCache[kvp.Key] = kvp.Value;
        }

        // Populate wildcard patterns list, sorted by prefix length (longest first)
        // This ensures more specific patterns are checked first
        _wildcardPatterns.AddRange(
            wildcardGroups
                .OrderByDescending(kvp => kvp.Key.Length)
                .Select(kvp => new WildcardPattern
                {
                    Prefix = kvp.Key,
                    Rules = kvp.Value
                }));
    }

    /// <summary>
    /// Represents a wildcard pattern with its associated rules.
    /// </summary>
    private class WildcardPattern
    {
        public string Prefix { get; set; } = string.Empty;
        public List<RateLimitRule> Rules { get; set; } = new();
    }
}
