namespace RVR.Framework.MultiTenancy.Models;

/// <summary>
/// Represents an IP access rule for a tenant.
/// </summary>
public sealed class IpAccessRule
{
    /// <summary>
    /// Unique identifier for the rule.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// The tenant this rule belongs to.
    /// </summary>
    public required string TenantId { get; init; }

    /// <summary>
    /// The IP address pattern (exact IP or CIDR notation, e.g., "192.168.1.0/24").
    /// </summary>
    public required string IpPattern { get; init; }

    /// <summary>
    /// The type of access rule.
    /// </summary>
    public IpRuleType RuleType { get; init; }
}

/// <summary>
/// The type of IP access rule.
/// </summary>
public enum IpRuleType
{
    /// <summary>
    /// Allow access from this IP pattern.
    /// </summary>
    Allow,

    /// <summary>
    /// Block access from this IP pattern.
    /// </summary>
    Block
}
