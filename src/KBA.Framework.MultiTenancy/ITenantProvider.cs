namespace KBA.Framework.MultiTenancy;

/// <summary>
/// Interface for providing the current tenant's information and connection string.
/// </summary>
public interface ITenantProvider
{
    /// <summary>
    /// Gets the current tenant information.
    /// </summary>
    TenantInfo? GetCurrentTenant();

    /// <summary>
    /// Gets the connection string for the current tenant.
    /// </summary>
    /// <returns>The tenant-specific connection string, or the default connection string if not found.</returns>
    string? GetConnectionString();
}
