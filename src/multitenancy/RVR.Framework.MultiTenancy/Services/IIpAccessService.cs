using RVR.Framework.MultiTenancy.Models;

namespace RVR.Framework.MultiTenancy.Services;

/// <summary>
/// Service for managing and evaluating IP access rules per tenant.
/// </summary>
public interface IIpAccessService
{
    /// <summary>
    /// Determines whether the given IP address is allowed for the specified tenant.
    /// Blocklist takes precedence over allowlist.
    /// </summary>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="ipAddress">The IP address to check.</param>
    /// <returns>True if the IP is allowed; false if blocked.</returns>
    bool IsAllowed(string tenantId, string ipAddress);

    /// <summary>
    /// Adds an IP access rule.
    /// </summary>
    /// <param name="rule">The rule to add.</param>
    void AddRule(IpAccessRule rule);

    /// <summary>
    /// Removes an IP access rule by its identifier.
    /// </summary>
    /// <param name="ruleId">The rule identifier.</param>
    void RemoveRule(Guid ruleId);

    /// <summary>
    /// Gets all rules for a tenant.
    /// </summary>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <returns>A read-only list of rules.</returns>
    IReadOnlyList<IpAccessRule> GetRules(string tenantId);
}
