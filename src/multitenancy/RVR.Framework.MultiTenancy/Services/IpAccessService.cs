using System.Collections.Concurrent;
using System.Net;
using RVR.Framework.MultiTenancy.Models;

namespace RVR.Framework.MultiTenancy.Services;

/// <summary>
/// In-memory implementation of <see cref="IIpAccessService"/>.
/// Blocklist takes precedence over allowlist.
/// </summary>
public sealed class IpAccessService : IIpAccessService
{
    private readonly ConcurrentDictionary<Guid, IpAccessRule> _rules = new();

    /// <inheritdoc />
    public bool IsAllowed(string tenantId, string ipAddress)
    {
        var tenantRules = _rules.Values
            .Where(r => r.TenantId == tenantId)
            .ToList();

        // If no rules exist for this tenant, allow by default
        if (tenantRules.Count == 0)
            return true;

        var blockRules = tenantRules.Where(r => r.RuleType == IpRuleType.Block).ToList();
        var allowRules = tenantRules.Where(r => r.RuleType == IpRuleType.Allow).ToList();

        // Blocklist takes precedence: if any block rule matches, deny
        if (blockRules.Any(r => MatchesIpPattern(ipAddress, r.IpPattern)))
            return false;

        // If there are allow rules, the IP must match at least one
        if (allowRules.Count > 0)
            return allowRules.Any(r => MatchesIpPattern(ipAddress, r.IpPattern));

        // No allow rules defined and not blocked => allow
        return true;
    }

    /// <inheritdoc />
    public void AddRule(IpAccessRule rule)
    {
        _rules[rule.Id] = rule;
    }

    /// <inheritdoc />
    public void RemoveRule(Guid ruleId)
    {
        _rules.TryRemove(ruleId, out _);
    }

    /// <inheritdoc />
    public IReadOnlyList<IpAccessRule> GetRules(string tenantId)
    {
        return _rules.Values
            .Where(r => r.TenantId == tenantId)
            .ToList()
            .AsReadOnly();
    }

    private static bool MatchesIpPattern(string ipAddress, string pattern)
    {
        // Exact match
        if (string.Equals(ipAddress, pattern, StringComparison.OrdinalIgnoreCase))
            return true;

        // CIDR match
        if (pattern.Contains('/'))
        {
            return IsInCidrRange(ipAddress, pattern);
        }

        return false;
    }

    private static bool IsInCidrRange(string ipAddress, string cidr)
    {
        var parts = cidr.Split('/');
        if (parts.Length != 2 || !int.TryParse(parts[1], out var prefixLength))
            return false;

        if (!IPAddress.TryParse(ipAddress, out var ip) || !IPAddress.TryParse(parts[0], out var networkAddress))
            return false;

        var ipBytes = ip.GetAddressBytes();
        var networkBytes = networkAddress.GetAddressBytes();

        if (ipBytes.Length != networkBytes.Length)
            return false;

        var totalBits = ipBytes.Length * 8;
        if (prefixLength < 0 || prefixLength > totalBits)
            return false;

        for (var i = 0; i < ipBytes.Length; i++)
        {
            var bitsToCheck = Math.Min(8, Math.Max(0, prefixLength - (i * 8)));
            if (bitsToCheck <= 0) break;

            var mask = (byte)(0xFF << (8 - bitsToCheck));
            if ((ipBytes[i] & mask) != (networkBytes[i] & mask))
                return false;
        }

        return true;
    }
}
