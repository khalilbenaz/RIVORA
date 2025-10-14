using RVR.Framework.MultiTenancy;

namespace RVR.Framework.TestInfrastructure;

/// <summary>
/// Fake tenant provider for unit tests. Returns a fixed test tenant.
/// </summary>
public class FakeTenantProvider : ITenantProvider
{
    /// <summary>
    /// The default test tenant ID used across all test fixtures.
    /// </summary>
    public const string DefaultTenantId = "test-tenant-001";

    /// <summary>
    /// The default test tenant name.
    /// </summary>
    public const string DefaultTenantName = "Test Tenant";

    private TenantInfo _currentTenant;

    public FakeTenantProvider()
    {
        _currentTenant = new TenantInfo
        {
            Id = DefaultTenantId,
            Name = DefaultTenantName,
            Identifier = DefaultTenantId,
            IsActive = true
        };
    }

    public FakeTenantProvider(string tenantId, string tenantName = "Test Tenant")
    {
        _currentTenant = new TenantInfo
        {
            Id = tenantId,
            Name = tenantName,
            Identifier = tenantId,
            IsActive = true
        };
    }

    /// <inheritdoc />
    public TenantInfo? GetCurrentTenant() => _currentTenant;

    /// <inheritdoc />
    public string? GetConnectionString() => null;

    /// <summary>
    /// Changes the current tenant for the duration of a test.
    /// </summary>
    public void SetTenant(string tenantId, string tenantName = "Test Tenant")
    {
        _currentTenant = new TenantInfo
        {
            Id = tenantId,
            Name = tenantName,
            Identifier = tenantId,
            IsActive = true
        };
    }
}
