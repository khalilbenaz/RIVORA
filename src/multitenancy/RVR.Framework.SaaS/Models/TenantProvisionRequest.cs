namespace RVR.Framework.SaaS.Models;

/// <summary>
/// Request model for provisioning a new tenant.
/// </summary>
public sealed class TenantProvisionRequest
{
    /// <summary>
    /// Gets or sets the display name of the tenant.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the email address of the initial admin user.
    /// </summary>
    public required string AdminEmail { get; set; }

    /// <summary>
    /// Gets or sets the subscription plan identifier (e.g., "free", "pro", "enterprise").
    /// </summary>
    public required string Plan { get; set; }

    /// <summary>
    /// Gets or sets the subdomain used to identify this tenant.
    /// </summary>
    public required string Subdomain { get; set; }
}
