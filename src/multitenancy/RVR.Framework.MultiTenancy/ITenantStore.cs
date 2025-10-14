namespace RVR.Framework.MultiTenancy;

/// <summary>
/// Abstraction for looking up tenant information by identifier.
/// Implementations may query a database, cache, or configuration source.
/// </summary>
public interface ITenantStore
{
    /// <summary>
    /// Retrieves a <see cref="TenantInfo"/> by its identifier.
    /// Returns <c>null</c> if the tenant does not exist or is inactive.
    /// </summary>
    Task<TenantInfo?> GetTenantAsync(string tenantId);
}
