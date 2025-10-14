# Multi-Tenancy

## Strategies d'isolation

RIVORA Framework supporte 3 strategies de multi-tenancy :

| Strategie | Isolation | Performance | Complexite |
|-----------|-----------|-------------|------------|
| **Colonne** (`TenantId`) | Logique | Haute | Faible |
| **Schema** | Schema DB | Moyenne | Moyenne |
| **Base separee** | Physique | Variable | Haute |

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

### Enregistrement

```csharp
builder.Services.AddRvrMultiTenancy(options =>
{
    options.Strategy = TenantStrategy.Column;
    options.ResolutionStrategy = TenantResolution.Header;
});
```

## Resolution du tenant

Le tenant est resolu dans cet ordre :

1. **Header HTTP** : `X-Tenant-Id`
2. **Claim JWT** : `tenant_id`
3. **Sous-domaine** : `acme.app.com` -> `acme`
4. **Route** : `/api/v1/tenants/{tenantId}/...`

### Resolver personnalise

```csharp
public class CustomTenantResolver : ITenantResolver
{
    public Task<string?> ResolveAsync(HttpContext context)
    {
        // Logique personnalisee
        return Task.FromResult(context.Request.Headers["X-Tenant-Id"].FirstOrDefault());
    }
}
```

## Entites multi-tenant

Les entites implementent `ITenantEntity` :

```csharp
public class Product : BaseEntity, ITenantEntity
{
    public string TenantId { get; set; } = default!;
    public string Name { get; set; } = default!;
    public decimal Price { get; set; }
}
```

Le filtre global EF Core applique automatiquement `WHERE TenantId = @currentTenant` sur toutes les requetes.

## Isolation des donnees

### Strategie Colonne

```csharp
// Filtre applique automatiquement par l'intercepteur
modelBuilder.Entity<Product>()
    .HasQueryFilter(p => p.TenantId == _currentTenant.Id);
```

### Strategie Schema

```csharp
options.Strategy = TenantStrategy.Schema;
// Chaque tenant a son schema : tenant_acme.Products, tenant_beta.Products
```

### Strategie Base separee

```csharp
options.Strategy = TenantStrategy.Database;
// Connection string resolue dynamiquement par tenant
```

## Administration des tenants

```bash
# Via CLI
kba tenant create --name "Acme Corp" --plan enterprise
kba tenant list
kba tenant migrate --all  # Appliquer les migrations a tous les tenants
```

```bash
# Via API
curl -X POST http://localhost:5220/api/v1/admin/tenants \
  -H "Authorization: Bearer <admin-token>" \
  -d '{"name": "Acme Corp", "plan": "enterprise"}'
```
