# Multi-tenancy & SaaS

The RIVORA Framework provides comprehensive multi-tenancy support with flexible tenant isolation strategies, lifecycle management, and SaaS-ready features.

## Tenant Resolution

The `TenantMiddleware` resolves the current tenant from HTTP requests using three strategies (in priority order):

1. **Header**: `X-Tenant-Id: tenant-abc`
2. **Query string**: `?tenant=tenant-abc`
3. **Subdomain**: `tenant-abc.app.example.com`

```csharp
// In Program.cs
app.UseTenancy();
```

## ITenantStore

The `ITenantStore` interface validates and retrieves tenant information:

```csharp
public interface ITenantStore
{
    Task<TenantInfo?> GetTenantAsync(string tenantId);
}
```

Implement this interface to back tenant resolution with your preferred storage:

```csharp
public class DatabaseTenantStore : ITenantStore
{
    private readonly AppDbContext _db;
    private readonly IMemoryCache _cache;

    public async Task<TenantInfo?> GetTenantAsync(string tenantId)
    {
        return await _cache.GetOrCreateAsync($"tenant:{tenantId}", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);

            var tenant = await _db.Tenants
                .FirstOrDefaultAsync(t => t.Identifier == tenantId && t.IsActive);

            if (tenant == null) return null;

            return new TenantInfo
            {
                Id = tenant.Id.ToString(),
                Name = tenant.Name,
                Identifier = tenant.Identifier,
                ConnectionString = tenant.ConnectionString
            };
        });
    }
}
```

## Tenant Isolation Strategies

### Strategy 1: Shared Database with Discriminator Column

All tenants share one database. Each entity has a `TenantId` column, and queries are automatically filtered.

```csharp
builder.Services.AddRvrMultiTenancy(options =>
{
    options.IsolationStrategy = TenantIsolation.SharedDatabase;
});
```

```csharp
// Automatic query filtering in the DbContext
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // Automatically applied by RIVORA Framework
    modelBuilder.Entity<Product>()
        .HasQueryFilter(p => p.TenantId == _currentTenantId);
}
```

### Strategy 2: Database per Tenant

Each tenant gets a dedicated database. The connection string is resolved from the `TenantInfo`.

```csharp
builder.Services.AddRvrMultiTenancy(options =>
{
    options.IsolationStrategy = TenantIsolation.DatabasePerTenant;
});
```

### Strategy 3: Schema per Tenant

All tenants share one database, but each gets a separate schema.

```csharp
builder.Services.AddRvrMultiTenancy(options =>
{
    options.IsolationStrategy = TenantIsolation.SchemaPerTenant;
    options.DefaultSchema = "shared";
});
```

## Tenant Lifecycle

### Provisioning a Tenant

```csharp
public class TenantManagementService
{
    private readonly ITenantStore _store;
    private readonly ITenantProvisioner _provisioner;

    public async Task<TenantInfo> CreateTenantAsync(CreateTenantDto dto, CancellationToken ct)
    {
        // Validate tenant identifier uniqueness
        var existing = await _store.GetTenantAsync(dto.Identifier);
        if (existing != null)
            throw new InvalidOperationException($"Tenant '{dto.Identifier}' already exists.");

        // Create tenant record
        var tenant = new TenantInfo
        {
            Id = Guid.NewGuid().ToString(),
            Name = dto.Name,
            Identifier = dto.Identifier,
            IsActive = true
        };

        // Provision resources (database, schema, storage, etc.)
        await _provisioner.ProvisionAsync(tenant, ct);

        return tenant;
    }
}
```

### Suspending a Tenant

```csharp
public async Task SuspendTenantAsync(string tenantId, string reason, CancellationToken ct)
{
    var tenant = await _store.GetTenantAsync(tenantId)
        ?? throw new KeyNotFoundException($"Tenant '{tenantId}' not found.");

    tenant.IsActive = false;
    tenant.SuspensionReason = reason;
    tenant.SuspendedAt = DateTime.UtcNow;

    await _store.UpdateTenantAsync(tenant, ct);

    // Revoke all active sessions for this tenant
    await _sessionManager.RevokeAllTenantSessionsAsync(tenantId, ct);
}
```

### Deleting a Tenant

```csharp
public async Task DeleteTenantAsync(string tenantId, CancellationToken ct)
{
    var tenant = await _store.GetTenantAsync(tenantId)
        ?? throw new KeyNotFoundException($"Tenant '{tenantId}' not found.");

    // Anonymize personal data (GDPR compliance)
    await _dsarService.AnonymizeTenantDataAsync(tenantId, ct);

    // Deprovision resources
    await _provisioner.DeprovisionAsync(tenant, ct);

    // Soft-delete or hard-delete the tenant record
    await _store.DeleteTenantAsync(tenantId, ct);
}
```

## Tenant Validation in Middleware

The `TenantMiddleware` performs two validations:

1. **Existence**: The tenant must exist and be active in `ITenantStore`
2. **JWT match**: For authenticated requests, the tenant ID from the request must match the `TenantId` claim in the JWT

```csharp
// If validation fails:
// - Unknown/inactive tenant -> 403 Forbidden
// - JWT claim mismatch -> 403 Forbidden
```

## Accessing Current Tenant

```csharp
public class ProductService
{
    private readonly ITenantProvider _tenantProvider;

    public async Task<List<Product>> GetProductsAsync(CancellationToken ct)
    {
        var tenantId = _tenantProvider.GetCurrentTenantId();
        // Query is automatically filtered by tenantId
        return await _repository.GetAllAsync(ct);
    }
}
```

## Registration

```csharp
builder.Services.AddRvrMultiTenancy(options =>
{
    options.IsolationStrategy = TenantIsolation.SharedDatabase;
    options.TenantHeaderName = "X-Tenant-Id";
    options.TenantQueryStringName = "tenant";
    options.EnableSubdomainResolution = true;
    options.CacheTenantInfo = true;
    options.CacheDuration = TimeSpan.FromMinutes(5);
});

builder.Services.AddScoped<ITenantStore, DatabaseTenantStore>();

// In the pipeline
app.UseTenancy();
```
