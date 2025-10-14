namespace RVR.Framework.SaaS.Models;

/// <summary>
/// Result of a tenant provisioning operation.
/// </summary>
public sealed class TenantProvisionResult
{
    /// <summary>
    /// Gets or sets the unique identifier assigned to the new tenant.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets or sets whether the provisioning was initiated successfully.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the current status of the tenant.
    /// </summary>
    public TenantStatus Status { get; set; }

    /// <summary>
    /// Gets or sets any error message if provisioning failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the tenant was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    public static TenantProvisionResult Succeeded(Guid tenantId) => new()
    {
        TenantId = tenantId,
        Success = true,
        Status = TenantStatus.Provisioning,
        CreatedAt = DateTime.UtcNow
    };

    public static TenantProvisionResult Failed(string error) => new()
    {
        Success = false,
        Status = TenantStatus.Deactivated,
        ErrorMessage = error,
        CreatedAt = DateTime.UtcNow
    };
}
