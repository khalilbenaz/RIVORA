# API Versioning - RIVORA Framework

Système de versioning d'API avec multiples stratégies.

## Table des Matières

- [Vue d'ensemble](#vue-densemble)
- [Installation](#installation)
- [Configuration](#configuration)
- [Stratégies de Versioning](#stratégies-de-versioning)
- [Usage](#usage)
- [Swagger Integration](#swagger-integration)
- [Best Practices](#best-practices)

---

## Vue d'ensemble

RIVORA Framework API Versioning supporte :

| Stratégie | Description | Exemple |
|-----------|-------------|---------|
| **URL Path** | Version dans l'URL | `/v1/products` |
| **Header** | Version dans le header | `X-API-Version: 1` |
| **Query String** | Version en paramètre | `?api-version=1` |
| **Media Type** | Version dans Accept | `application/vnd.api.v1+json` |

---

## Installation

```bash
dotnet add package RVR.Framework.ApiVersioning
```

---

## Configuration

### Configuration de base

```csharp
using RVR.Framework.ApiVersioning.Extensions;

// Dans Program.cs
builder.Services.AddRvrApiVersioning();

// Avec configuration
builder.Services.AddRvrApiVersioning(options =>
{
    options.DefaultVersion = "1.0";
    options.SupportedVersions = new[] { "1.0", "2.0" };
    options.Strategies = VersioningStrategies.UrlPath | VersioningStrategies.Header;
});
```

### Configuration complète

```csharp
builder.Services.AddRvrApiVersioning(options =>
{
    // Versions supportées
    options.DefaultVersion = "1.0";
    options.SupportedVersions = new[] { "1.0", "2.0", "3.0" };
    
    // Stratégies
    options.Strategies = VersioningStrategies.UrlPath;
    
    // Header configuration
    options.HeaderName = "X-API-Version";
    
    // Query string configuration
    options.QueryStringName = "api-version";
    
    // URL path configuration
    options.UrlPathPrefix = "v";
    
    // Media type configuration
    options.MediaTypeFormat = "application/vnd.api.v{version}+json";
});

// Activer les conventions
builder.Services.AddRvrApiVersioningConventions();
```

---

## Stratégies de Versioning

### URL Path (Recommandé)

```csharp
builder.Services.AddRvrApiVersioningUrlPath();

// Routes:
// GET /v1/products
// GET /v2/products
```

### Header

```csharp
builder.Services.AddRvrApiVersioningHeader();

// Request:
// GET /products
// X-API-Version: 1

// Request:
// GET /products
// X-API-Version: 2
```

### Query String

```csharp
builder.Services.AddRvrApiVersioningQueryString();

// Routes:
// GET /products?api-version=1
// GET /products?api-version=2
```

### Media Type

```csharp
builder.Services.AddRvrApiVersioningMediaType();

// Request:
// GET /products
// Accept: application/vnd.api.v1+json

// Request:
// GET /products
// Accept: application/vnd.api.v2+json
```

### Multiple Stratégies

```csharp
builder.Services.AddRvrApiVersioning(options =>
{
    options.Strategies = VersioningStrategies.UrlPath | VersioningStrategies.Header;
});

// Supporte les deux:
// GET /v1/products
// GET /products (avec header X-API-Version: 1)
```

---

## Usage

### Controller Versioning

```csharp
using RVR.Framework.ApiVersioning.Attributes;
using Asp.Versioning;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class ProductsController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetProducts()
    {
        var products = await _productService.GetAllAsync();
        return Ok(products);
    }
}

// Version 2.0 avec breaking changes
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("2.0")]
public class ProductsController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetProductsV2()
    {
        var products = await _productService.GetAllV2Async();
        return Ok(products);
    }
}
```

### Controller avec Multiple Versions

```csharp
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[ApiVersion("2.0")]
public class ProductsController : ControllerBase
{
    // Endpoint pour v1.0 et v2.0
    [HttpGet]
    [MapToApiVersion("1.0")]
    [MapToApiVersion("2.0")]
    public async Task<IActionResult> GetProducts()
    {
        var products = await _productService.GetAllAsync();
        return Ok(products);
    }

    // Endpoint spécifique à v2.0
    [HttpGet("stats")]
    [MapToApiVersion("2.0")]
    public async Task<IActionResult> GetStats()
    {
        var stats = await _productService.GetStatsAsync();
        return Ok(stats);
    }
}
```

### Versioning avec Attributes

```csharp
using RVR.Framework.ApiVersioning.Attributes;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    [HttpGet]
    [ApiVersion("1.0")]
    public async Task<IActionResult> GetOrdersV1()
    {
        return Ok(await _orderService.GetOrdersV1());
    }

    [HttpGet]
    [ApiVersion("2.0")]
    public async Task<IActionResult> GetOrdersV2()
    {
        return Ok(await _orderService.GetOrdersV2());
    }

    [HttpPost]
    [ApiVersion("1.0", "2.0")]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        var order = await _orderService.CreateAsync(request);
        return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
    }
}
```

### Deprecated Version

```csharp
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0", Deprecated = true)]
[ApiVersion("2.0")]
public class ProductsController : ControllerBase
{
    [HttpGet]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> GetProductsV1()
    {
        // Ancienne version - dépréciée
        return Ok(await _productService.GetAllV1());
    }

    [HttpGet]
    [MapToApiVersion("2.0")]
    public async Task<IActionResult> GetProductsV2()
    {
        // Nouvelle version
        return Ok(await _productService.GetAllV2());
    }
}
```

---

## Swagger Integration

### Configuration Swagger

```csharp
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "My API V1",
        Version = "v1",
        Description = "API version 1.0"
    });

    options.SwaggerDoc("v2", new OpenApiInfo
    {
        Title = "My API V2",
        Version = "v2",
        Description = "API version 2.0 - New features"
    });

    // Add versioning support
    options.OperationFilter<ApiVersioningOperationFilter>();
});

// Dans Program.cs
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "API V1");
    options.SwaggerEndpoint("/swagger/v2/swagger.json", "API V2");
});
```

### Versions Endpoint

```csharp
// Endpoint pour lister les versions supportées
app.MapRvrApiVersions(path: "/api/versions");

// Response:
// {
//   "supportedVersions": ["1.0", "2.0"],
//   "defaultVersion": "1.0",
//   "strategies": "UrlPath",
//   "headerName": "X-API-Version",
//   "queryStringName": "api-version",
//   "urlPathPrefix": "v"
// }
```

---

## Best Practices

### Versioning Strategy

```csharp
// ✅ Recommandé: URL Path pour API publique
builder.Services.AddRvrApiVersioningUrlPath();

// ✅ Header pour API interne
builder.Services.AddRvrApiVersioningHeader();

// ❌ Éviter: Query string pour API publique
// builder.Services.AddRvrApiVersioningQueryString();
```

### Version Numbering

```csharp
// ✅ Utiliser Semantic Versioning
[ApiVersion("1.0")]
[ApiVersion("1.1")]
[ApiVersion("2.0")]

// ❌ Éviter les versions trop granulaires
// [ApiVersion("1.0.1")]
// [ApiVersion("1.0.2")]
```

### Breaking Changes

```csharp
// v1.0 - Structure originale
public class ProductDtoV1
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
}

// v2.0 - Breaking change: prix avec devise
public class ProductDtoV2
{
    public int Id { get; set; }
    public string Name { get; set; }
    public PriceValue Price { get; set; } // Nouveau type
}

public class PriceValue
{
    public decimal Amount { get; set; }
    public string Currency { get; set; }
}
```

### Non-Breaking Changes

```csharp
// ✅ Ajouter des champs optionnels (non-breaking)
public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string? Description { get; set; } // Nouveau champ optionnel
}

// ❌ Changer le type d'un champ (breaking)
// public int Price { get; set; } -> public decimal Price { get; set; }
```

### Version Sunset

```csharp
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0", Deprecated = true)]
[ApiVersion("2.0")]
public class ProductsController : ControllerBase
{
    [HttpGet]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> GetProductsV1()
    {
        Response.Headers.Add("X-API-Deprecation", "true");
        Response.Headers.Add("X-API-Sunset", "2024-12-31");
        
        return Ok(await _productService.GetAllV1());
    }

    [HttpGet]
    [MapToApiVersion("2.0")]
    public async Task<IActionResult> GetProductsV2()
    {
        return Ok(await _productService.GetAllV2());
    }
}
```

---

## Configuration

### appsettings.json

```json
{
  "VersioningOptions": {
    "DefaultVersion": "1.0",
    "SupportedVersions": ["1.0", "2.0"],
    "Strategies": "UrlPath",
    "HeaderName": "X-API-Version",
    "QueryStringName": "api-version",
    "UrlPathPrefix": "v",
    "MediaTypeFormat": "application/vnd.api.v{version}+json"
  }
}
```

### Error Response

```json
// Version non supportée
HTTP/1.1 400 Bad Request
Content-Type: application/json

{
  "error": "Unsupported API version",
  "message": "The requested API version '3.0' is not supported.",
  "supportedVersions": ["1.0", "2.0"]
}
```

---

## Voir aussi

- [Features](features.md) - Feature flags avec versioning
- [Security](security.md) - Authorization avec versioning
- [Swagger](../quickstart.md) - Documentation API
