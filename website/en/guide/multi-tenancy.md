# Multi-Tenancy

## Isolation Strategies

RIVORA Framework supports 3 multi-tenancy strategies:

| Strategy | Isolation | Performance | Complexity |
|----------|-----------|-------------|------------|
| **Column** (`TenantId`) | Logical | High | Low |
| **Schema** | DB Schema | Medium | Medium |
| **Separate Database** | Physical | Variable | High |

## Configuration

```json
{
  "MultiTenancy": {
    "Strategy": "Column",
    "TenantResolution": "Header",
    "HeaderName": "X-Tenant-Id",
    "DefaultTenantId": "default"
  }
}
```

```csharp
builder.Services.AddRvrMultiTenancy(options =>
{
    options.Strategy = TenantStrategy.Column;
    options.ResolutionStrategy = TenantResolution.Header;
});
```

## Tenant Resolution

The tenant is resolved in this order:

1. **HTTP Header**: `X-Tenant-Id`
2. **JWT Claim**: `tenant_id`
3. **Subdomain**: `acme.app.com` -> `acme`
4. **Route**: `/api/v1/tenants/{tenantId}/...`

## Multi-tenant Entities

```csharp
public class Product : BaseEntity, ITenantEntity
{
    public string TenantId { get; set; } = default!;
    public string Name { get; set; } = default!;
    public decimal Price { get; set; }
}
```

The EF Core global filter automatically applies `WHERE TenantId = @currentTenant` on all queries.
