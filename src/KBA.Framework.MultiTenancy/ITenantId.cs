namespace KBA.Framework.MultiTenancy;

/// <summary>
/// Interface for entities that belong to a specific tenant.
/// Implementing this interface enables automatic tenant filtering in queries.
/// </summary>
public interface ITenantId
{
    /// <summary>
    /// Gets or sets the unique identifier of the tenant.
    /// </summary>
    string TenantId { get; set; }
}
