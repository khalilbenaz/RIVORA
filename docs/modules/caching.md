# Caching - RIVORA Framework

Système de caching performant avec support Memory et Redis.

## Table des Matières

- [Vue d'ensemble](#vue-densemble)
- [Installation](#installation)
- [Configuration](#configuration)
- [Memory Caching](#memory-caching)
- [Redis Caching](#redis-caching)
- [Response Caching](#response-caching)
- [Cache Invalidation](#cache-invalidation)
- [Best Practices](#best-practices)

---

## Vue d'ensemble

RIVORA Framework Caching fournit :

| Feature | Description |
|---------|-------------|
| **Memory Cache** | Cache en mémoire locale |
| **Redis Cache** | Cache distribué avec Redis |
| **Response Caching** | Cache HTTP des réponses |
| **Tag Invalidation** | Invalidation par tags |
| **Serialization** | MessagePack pour performance |

---

## Installation

```bash
dotnet add package RVR.Framework.Caching
```

---

## Configuration

### Configuration de base

```csharp
using RVR.Framework.Caching.Extensions;

// Dans Program.cs
builder.Services.AddRvrCaching();

// Avec options
builder.Services.AddRvrCaching(options =>
{
    options.DefaultExpirationMinutes = 30;
    options.EnableLogging = true;
});
```

### Configuration avec Redis

```csharp
// Avec Redis
builder.Services.AddRvrCachingWithRedis(
    builder.Configuration.GetConnectionString("Redis"),
    options =>
    {
        options.DefaultExpirationMinutes = 30;
    },
    redisOptions =>
    {
        redisOptions.Ssl = false;
        redisOptions.ConnectTimeout = 5000;
        redisOptions.SyncTimeout = 5000;
    });
```

---

## Memory Caching

### Configuration

```csharp
builder.Services.AddRvrMemoryCache(options =>
{
    options.SizeLimit = 1024;
    options.CompactionPercentage = 0.5;
});

builder.Services.AddRvrMemoryCacheServices(options =>
{
    options.ExpirationScanFrequency = TimeSpan.FromMinutes(1);
});
```

### Usage

```csharp
using RVR.Framework.Caching.Interfaces;

public class ProductService
{
    private readonly IResponseCacheService _cacheService;
    private readonly ILogger<ProductService> _logger;

    public ProductService(
        IResponseCacheService cacheService,
        ILogger<ProductService> logger)
    {
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<Product> GetByIdAsync(int id)
    {
        var cacheKey = $"product:{id}";
        
        // Try get from cache
        var cached = await _cacheService.GetAsync<Product>(cacheKey);
        if (cached != null)
        {
            _logger.LogInformation("Cache hit for product {Id}", id);
            return cached;
        }

        // Get from database
        var product = await _repository.GetByIdAsync(id);
        
        // Store in cache
        await _cacheService.SetAsync(
            cacheKey,
            product,
            TimeSpan.FromMinutes(30));

        return product;
    }

    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        var cacheKey = "products:all";
        
        var cached = await _cacheService.GetAsync<IEnumerable<Product>>(cacheKey);
        if (cached != null)
        {
            return cached;
        }

        var products = await _repository.GetAllAsync();
        
        await _cacheService.SetAsync(
            cacheKey,
            products,
            TimeSpan.FromMinutes(10),
            tags: new[] { "products" });

        return products;
    }
}
```

---

## Redis Caching

### Configuration

```csharp
builder.Services.AddRvrRedisCache(
    builder.Configuration.GetConnectionString("Redis"),
    options =>
    {
        options.DefaultExpirationMinutes = 30;
        options.UseMessagePack = true;
    },
    redisOptions =>
    {
        redisOptions.Configuration = "localhost:6379";
        redisOptions.InstanceName = "MyApp_";
        redisOptions.Ssl = false;
        redisOptions.ConnectTimeout = 5000;
        redisOptions.SyncTimeout = 5000;
        redisOptions.AsyncTimeout = 5000;
        redisOptions.AllowAdmin = true;
        redisOptions.AbortOnConnectFail = false;
        redisOptions.KeepAlive = 180;
    });
```

### Configuration avancée Redis

```csharp
builder.Services.AddRvrDistributedCache(
    builder.Configuration.GetConnectionString("Redis"));

// Ou avec configuration complète
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "MyApp_";
});
```

### Usage avec Redis

```csharp
public class UserService
{
    private readonly IResponseCacheService _cacheService;

    public UserService(IResponseCacheService cacheService)
    {
        _cacheService = cacheService;
    }

    public async Task<User> GetByIdAsync(int id)
    {
        var cacheKey = $"user:{id}";
        
        return await _cacheService.GetOrSetAsync(
            cacheKey,
            async () => await _repository.GetByIdAsync(id),
            TimeSpan.FromMinutes(30),
            tags: new[] { "users" });
    }

    public async Task<User> GetByTokenAsync(string token)
    {
        var cacheKey = $"user:token:{token}";
        
        return await _cacheService.GetOrSetAsync(
            cacheKey,
            async () => await _repository.GetByTokenAsync(token),
            TimeSpan.FromHours(1),
            tags: new[] { "users", "tokens" });
    }
}
```

---

## Response Caching

### Middleware

```csharp
// Activer le middleware de response caching
app.UseRvrResponseCaching();
```

### Attribute KbaCache

```csharp
using RVR.Framework.Caching.Attributes;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    [HttpGet]
    [KbaCache(Duration = 300)] // Cache 5 minutes
    public async Task<IActionResult> GetProducts()
    {
        var products = await _productService.GetAllAsync();
        return Ok(products);
    }

    [HttpGet("{id}")]
    [KbaCache(Duration = 600, Tags = new[] { "products" })]
    public async Task<IActionResult> GetProduct(int id)
    {
        var product = await _productService.GetByIdAsync(id);
        return Ok(product);
    }

    [HttpGet("category/{categoryId}")]
    [KbaCache(Duration = 300, VaryByQuery = "categoryId,page,pageSize")]
    public async Task<IActionResult> GetByCategory(
        int categoryId,
        int page = 1,
        int pageSize = 10)
    {
        var products = await _productService.GetByCategoryAsync(
            categoryId, page, pageSize);
        return Ok(products);
    }
}
```

### Configuration du Response Caching

```csharp
builder.Services.AddResponseCaching();

app.UseResponseCaching();

// Configuration avancée
builder.Services.Configure<ResponseCachingOptions>(options =>
{
    options.MaximumBodySize = 1024 * 1024; // 1MB
    options.UseCaseSensitivePaths = false;
    options.SizeLimit = 1024 * 1024 * 100; // 100MB
});
```

---

## Cache Invalidation

### Invalidation par tags

```csharp
public class ProductController : ControllerBase
{
    private readonly IResponseCacheService _cacheService;

    public ProductController(IResponseCacheService cacheService)
    {
        _cacheService = cacheService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductRequest request)
    {
        var product = await _productService.CreateAsync(request);
        
        // Invalider le cache des products
        await _cacheService.InvalidateByTagsAsync("products");
        
        return CreatedAtAction(nameof(Get), new { id = product.Id }, product);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductRequest request)
    {
        await _productService.UpdateAsync(id, request);
        
        // Invalider le cache spécifique et les tags
        await _cacheService.RemoveAsync($"product:{id}");
        await _cacheService.InvalidateByTagsAsync("products");
        
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _productService.DeleteAsync(id);
        
        // Invalider le cache
        await _cacheService.RemoveAsync($"product:{id}");
        await _cacheService.InvalidateByTagsAsync("products");
        
        return NoContent();
    }
}
```

### Cache Helper

```csharp
using static RVR.Framework.Caching.Extensions.CacheHelper;

public class OrderService
{
    private readonly IResponseCacheService _cacheService;

    public async Task<Order> GetByIdAsync(int id)
    {
        var cacheKey = GenerateKeyWithPrefix("order", id.ToString());
        // cacheKey = "order:123"
        
        return await _cacheService.GetOrSetAsync(
            cacheKey,
            async () => await _repository.GetByIdAsync(id),
            TimeSpan.FromMinutes(30));
    }

    public async Task InvalidateUserOrdersAsync(int userId)
    {
        // Invalider tous les caches liés aux commandes de l'utilisateur
        await _cacheService.InvalidateByTagsAsync($"user:{userId}:orders");
    }
}
```

---

## Best Practices

### Cache-Aside Pattern

```csharp
public async Task<Product> GetByIdAsync(int id)
{
    var cacheKey = $"product:{id}";
    
    // 1. Try cache
    var cached = await _cacheService.GetAsync<Product>(cacheKey);
    if (cached != null)
    {
        return cached;
    }

    // 2. Cache miss - load from database
    var product = await _repository.GetByIdAsync(id);
    if (product == null)
    {
        return null;
    }

    // 3. Populate cache
    await _cacheService.SetAsync(
        cacheKey,
        product,
        TimeSpan.FromMinutes(30));

    return product;
}
```

### Write-Through Pattern

```csharp
public async Task UpdateAsync(int id, UpdateProductRequest request)
{
    // 1. Update database
    await _repository.UpdateAsync(id, request);

    // 2. Update cache
    var product = await _repository.GetByIdAsync(id);
    await _cacheService.SetAsync(
        $"product:{id}",
        product,
        TimeSpan.FromMinutes(30));

    // 3. Invalidate list cache
    await _cacheService.InvalidateByTagsAsync("products");
}
```

### Cache Stampede Prevention

```csharp
public async Task<Product> GetByIdAsync(int id)
{
    var cacheKey = $"product:{id}";
    var lockKey = $"lock:{cacheKey}";

    // Try cache first
    var cached = await _cacheService.GetAsync<Product>(cacheKey);
    if (cached != null)
    {
        return cached;
    }

    // Acquire lock to prevent stampede
    using (await _distributedLock.AcquireAsync(lockKey, TimeSpan.FromSeconds(5)))
    {
        // Double-check cache after acquiring lock
        cached = await _cacheService.GetAsync<Product>(cacheKey);
        if (cached != null)
        {
            return cached;
        }

        // Load from database
        var product = await _repository.GetByIdAsync(id);
        
        // Populate cache
        await _cacheService.SetAsync(
            cacheKey,
            product,
            TimeSpan.FromMinutes(30));

        return product;
    }
}
```

### Expiration Strategies

```csharp
// Absolute expiration
await _cacheService.SetAsync(
    "key",
    value,
    TimeSpan.FromMinutes(30));

// Sliding expiration
await _cacheService.SetAsync(
    "key",
    value,
    slidingExpiration: TimeSpan.FromMinutes(5));

// Absolute + Sliding
await _cacheService.SetAsync(
    "key",
    value,
    absoluteExpiration: TimeSpan.FromHours(1),
    slidingExpiration: TimeSpan.FromMinutes(10));
```

---

## Configuration

### appsettings.json

```json
{
  "CacheOptions": {
    "DefaultExpirationMinutes": 30,
    "EnableLogging": true,
    "UseMessagePack": true,
    "MaxCacheSize": 1073741824
  },
  "RedisCacheOptions": {
    "ConnectionString": "localhost:6379",
    "InstanceName": "MyApp_",
    "Ssl": false,
    "ConnectTimeout": 5000,
    "SyncTimeout": 5000,
    "AsyncTimeout": 5000,
    "AllowAdmin": true,
    "AbortOnConnectFail": false,
    "KeepAlive": 180
  }
}
```

---

## Voir aussi

- [Health Checks](health-checks.md) - Monitoring du cache
- [Security](security.md) - Authorization avec cache
- [Database](database.md) - Cache des requêtes database
