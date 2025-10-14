namespace RVR.Framework.SaaS.Models;

/// <summary>
/// Represents the lifecycle status of a tenant.
/// </summary>
public enum TenantStatus
{
    /// <summary>
    /// Tenant is being provisioned and not yet fully operational.
    /// </summary>
    Provisioning,

    /// <summary>
    /// Tenant is fully operational and accessible.
    /// </summary>
    Active,

    /// <summary>
    /// Tenant is temporarily suspended; all API access is blocked.
    /// </summary>
    Suspended,

    /// <summary>
    /// Tenant has been deactivated and is pending deletion or archival.
    /// </summary>
    Deactivated
}
