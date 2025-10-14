# Module Caching

**Package** : `RVR.Framework.Caching`

## Description

Cache a 2 niveaux : Memory (L1) pour la vitesse, Redis (L2) pour le partage entre instances.

## Enregistrement

```csharp
builder.Services.AddRvrCaching(options =>
{
    options.EnableL1 = true;              // IMemoryCache
    options.EnableL2 = true;              // Redis
    options.DefaultExpirationMinutes = 5;
    options.RedisConnection = "localhost:6379";
});
```

## Utilisation

```csharp
public class ProductService
{
    private readonly ICacheService _cache;

    public async Task<ProductDto?> GetProductAsync(Guid id)
    {
        return await _cache.GetOrSetAsync(
            $"product:{id}",
            () => _repo.GetByIdAsync(id),
            TimeSpan.FromMinutes(10)
        );
    }

    public async Task InvalidateProductAsync(Guid id)
    {
        await _cache.RemoveAsync($"product:{id}");
    }
}
```

## Cache via MediatR

```csharp
[Cached(Duration = 300, Key = "products:all")]
public record GetProductsQuery : IRequest<List<ProductDto>>;
```

Le `CachingBehavior` du pipeline MediatR intercepte automatiquement les queries decorees.

## ETag Caching

Middleware pour reponses conditionnelles HTTP 304 :

```csharp
app.UseRvrETagCaching();
```
