# Security - RIVORA Framework

Système de sécurité complet avec 2FA, RBAC, Rate Limiting et Audit Trail.

## Table des Matières

- [Vue d'ensemble](#vue-densemble)
- [2FA/TOTP](#2fatotp)
- [RBAC & Permissions](#rbac--permissions)
- [Rate Limiting](#rate-limiting)
- [Audit Trail](#audit-trail)
- [Refresh Tokens](#refresh-tokens)
- [Configuration](#configuration)

---

## Vue d'ensemble

RIVORA Framework Security inclut :

| Feature | Description | Status |
|---------|-------------|--------|
| **2FA/TOTP** | Authentication à deux facteurs | ✅ |
| **QR Code** | Setup 2FA avec QR codes | ✅ |
| **Backup Codes** | Codes de récupération | ✅ |
| **RBAC** | Role-Based Access Control | ✅ |
| **Permissions** | Permissions granulaires | ✅ |
| **Rate Limiting** | Limitation de débit | ✅ |
| **Audit Trail** | Logging des opérations | ✅ |
| **Refresh Tokens** | Token management | ✅ |

---

## 2FA/TOTP

### Installation

```bash
dotnet add package RVR.Framework.Security
```

### Configuration

```csharp
using RVR.Framework.Security.Extensions;

// Dans Program.cs
builder.Services.AddRvrSecurity(options =>
{
    options.Totp.Issuer = "MyApp";
    options.Totp.Digits = 6;
    options.Totp.Period = 30;
    options.Totp.BackupCodeCount = 10;
});

// Avec Redis pour distributed deployments
builder.Services.AddRvrSecurityWithRedis(
    builder.Configuration.GetConnectionString("Redis"));
```

### Setup 2FA pour un utilisateur

```csharp
using RVR.Framework.Security.Interfaces;

public class AccountController : ControllerBase
{
    private readonly ITotpService _totpService;

    public AccountController(ITotpService totpService)
    {
        _totpService = totpService;
    }

    [HttpPost("2fa/enable")]
    public async Task<IActionResult> Enable2FA()
    {
        var userId = GetCurrentUserId();
        
        // Générer une nouvelle clé secrète
        var setupInfo = await _totpService.GenerateSecretKeyAsync(userId);
        
        // Retourner le QR code et la clé secrète
        return Ok(new
        {
            SecretKey = setupInfo.SecretKey,
            QrCodeSetupUrl = setupInfo.QrCodeSetupUrl,
            ManualEntryKey = setupInfo.ManualEntryKey
        });
    }

    [HttpPost("2fa/verify")]
    public async Task<IActionResult> Verify2FA([FromBody] Verify2FARequest request)
    {
        var userId = GetCurrentUserId();
        
        var isValid = await _totpService.VerifyCodeAsync(userId, request.Code);
        
        if (!isValid)
        {
            return BadRequest("Invalid 2FA code");
        }

        // Activer 2FA
        await _totpService.Enable2FAAsync(userId);
        
        return Ok("2FA enabled successfully");
    }
}
```

### Vérification du code 2FA

```csharp
[HttpPost("login")]
public async Task<IActionResult> Login([FromBody] LoginRequest request)
{
    var user = await _userService.FindByNameAsync(request.UserName);
    
    if (user == null || !await _userService.CheckPasswordAsync(user, request.Password))
    {
        return Unauthorized("Invalid credentials");
    }

    // Vérifier si 2FA est activé
    if (user.TwoFactorEnabled)
    {
        return Challenge("2FA Required", new AuthenticationProperties
        {
            Items = { { "UserId", user.Id } }
        });
    }

    // Générer les tokens
    var tokens = await _authService.GenerateTokensAsync(user);
    return Ok(tokens);
}

[HttpPost("2fa/login")]
public async Task<IActionResult> Login2FA([FromBody] Verify2FARequest request)
{
    var userId = GetPendingUserId();
    
    var isValid = await _totpService.VerifyCodeAsync(userId, request.Code);
    
    if (!isValid)
    {
        return Unauthorized("Invalid 2FA code");
    }

    var user = await _userService.FindByIdAsync(userId);
    var tokens = await _authService.GenerateTokensAsync(user);
    
    return Ok(tokens);
}
```

### Backup Codes

```csharp
[HttpPost("2fa/backup-codes")]
public async Task<IActionResult> GenerateBackupCodes()
{
    var userId = GetCurrentUserId();
    
    var backupCodes = await _totpService.GenerateBackupCodesAsync(userId);
    
    // Afficher les codes une seule fois
    return Ok(new
    {
        BackupCodes = backupCodes,
        Message = "Save these codes securely. They won't be shown again."
    });
}

[HttpPost("2fa/login/backup")]
public async Task<IActionResult> LoginWithBackupCode([FromBody] BackupCodeRequest request)
{
    var userId = GetPendingUserId();
    
    var isValid = await _totpService.VerifyBackupCodeAsync(userId, request.Code);
    
    if (!isValid)
    {
        return Unauthorized("Invalid backup code");
    }

    var user = await _userService.FindByIdAsync(userId);
    var tokens = await _authService.GenerateTokensAsync(user);
    
    return Ok(tokens);
}
```

---

## RBAC & Permissions

### Configuration des permissions

```csharp
builder.Services.AddPermissionServices(options =>
{
    options.Permissions.Add(new PermissionDefinition
    {
        Name = "Products.View",
        DisplayName = "View Products",
        Description = "Can view products"
    });
    
    options.Permissions.Add(new PermissionDefinition
    {
        Name = "Products.Create",
        DisplayName = "Create Products",
        Description = "Can create new products"
    });
    
    options.Permissions.Add(new PermissionDefinition
    {
        Name = "Products.Edit",
        DisplayName = "Edit Products",
        Description = "Can edit existing products"
    });
    
    options.Permissions.Add(new PermissionDefinition
    {
        Name = "Products.Delete",
        DisplayName = "Delete Products",
        Description = "Can delete products"
    });

    options.EnableHierarchicalPermissions = true;
});
```

### Attribute RequirePermission

```csharp
using RVR.Framework.Security.Attributes;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    [HttpGet]
    [RequirePermission("Products.View")]
    public async Task<IActionResult> GetProducts()
    {
        var products = await _productService.GetAllAsync();
        return Ok(products);
    }

    [HttpPost]
    [RequirePermission("Products.Create")]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request)
    {
        var product = await _productService.CreateAsync(request);
        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
    }

    [HttpPut("{id}")]
    [RequirePermission("Products.Edit")]
    public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductRequest request)
    {
        await _productService.UpdateAsync(id, request);
        return NoContent();
    }

    [HttpDelete("{id}")]
    [RequirePermission("Products.Delete")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        await _productService.DeleteAsync(id);
        return NoContent();
    }
}
```

### Vérification des permissions dans le code

```csharp
public class ProductService
{
    private readonly IPermissionService _permissionService;
    private readonly ICurrentUser _currentUser;

    public ProductService(
        IPermissionService permissionService,
        ICurrentUser currentUser)
    {
        _permissionService = permissionService;
        _currentUser = currentUser;
    }

    public async Task<Product> CreateAsync(CreateProductRequest request)
    {
        // Vérifier la permission
        var hasPermission = await _permissionService.HasPermissionAsync(
            _currentUser.UserId, "Products.Create");

        if (!hasPermission)
        {
            throw new UnauthorizedAccessException("Missing permission: Products.Create");
        }

        // Création du produit
        return await _repository.CreateAsync(request.ToEntity());
    }
}
```

### Rôles et permissions hiérarchiques

```csharp
// Configuration des rôles
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("ManagerOrAdmin", policy => 
        policy.RequireRole("Manager", "Admin"));
});

// Utilisation des policies
[Authorize(Policy = "AdminOnly")]
[HttpPost("admin")]
public async Task<IActionResult> AdminAction()
{
    // Action réservée aux admins
}

// Permissions hiérarchiques
// Products.Delete inclut automatiquement Products.Edit, Products.Create, Products.View
options.EnableHierarchicalPermissions = true;
```

---

## Rate Limiting

### Configuration

```csharp
builder.Services.AddRateLimitingServices(options =>
{
    options.EnableRateLimiting = true;
    options.DefaultLimit = 100;
    options.DefaultWindow = TimeSpan.FromMinutes(1);
    options.StoreType = "InMemory"; // ou "Redis"
});
```

### Rate Limiting par endpoint

```csharp
using RVR.Framework.Security.Attributes;

[ApiController]
[Route("api/[controller]")]
public class SearchController : ControllerBase
{
    [HttpGet]
    [RateLimit(10, 60)] // 10 requêtes par minute
    public async Task<IActionResult> Search([FromQuery] string query)
    {
        var results = await _searchService.SearchAsync(query);
        return Ok(results);
    }

    [HttpPost("bulk")]
    [RateLimit(5, 60)] // 5 requêtes par minute
    public async Task<IActionResult> BulkSearch([FromBody] BulkSearchRequest request)
    {
        var results = await _searchService.BulkSearchAsync(request.Queries);
        return Ok(results);
    }
}
```

### Rate Limiting global

```csharp
// Middleware de rate limiting
app.UseRvrRateLimiting();

// Configuration des règles
builder.Services.Configure<RateLimitOptions>(options =>
{
    options.Rules.Add(new RateLimitRule
    {
        Endpoint = "/api/search",
        Limit = 10,
        Window = TimeSpan.FromMinutes(1),
        Description = "Search endpoint limit"
    });

    options.Rules.Add(new RateLimitRule
    {
        Endpoint = "/api/auth/login",
        Limit = 5,
        Window = TimeSpan.FromMinutes(5),
        Description = "Login attempt limit"
    });

    options.Rules.Add(new RateLimitRule
    {
        Endpoint = "*",
        Limit = 1000,
        Window = TimeSpan.FromHours(1),
        Description = "Global hourly limit"
    });
});
```

### Response Rate Limiting

```csharp
// Headers de rate limiting dans la réponse
HTTP/1.1 200 OK
X-RateLimit-Limit: 100
X-RateLimit-Remaining: 95
X-RateLimit-Reset: 1647360000

// Response quand la limite est atteinte
HTTP/1.1 429 Too Many Requests
Retry-After: 60
Content-Type: application/json

{
    "error": "Rate limit exceeded",
    "message": "Too many requests. Please try again later.",
    "retryAfter": 60
}
```

---

## Audit Trail

### Configuration

```csharp
builder.Services.AddAuditTrailServices(options =>
{
    options.EnableAudit = true;
    options.IncludeRequestBody = true;
    options.IncludeResponseBody = false;
    options.ExcludePaths = new[] { "/health", "/metrics" };
    options.MaxRequestBodySize = 1024 * 1024; // 1MB
});
```

### Audit des opérations

```csharp
using RVR.Framework.Security.Interfaces;

public class OrderService
{
    private readonly IAuditService _auditService;
    private readonly ICurrentUser _currentUser;

    public OrderService(
        IAuditService auditService,
        ICurrentUser currentUser)
    {
        _auditService = auditService;
        _currentUser = currentUser;
    }

    public async Task<Order> CreateAsync(CreateOrderRequest request)
    {
        var order = await _repository.CreateAsync(request.ToEntity());

        // Audit entry
        await _auditService.LogAsync(new AuditEntry
        {
            UserId = _currentUser.UserId,
            Action = "Create",
            Entity = "Order",
            EntityId = order.Id.ToString(),
            OldValue = null,
            NewValue = order,
            IpAddress = _currentUser.IpAddress,
            UserAgent = _currentUser.UserAgent
        });

        return order;
    }

    public async Task UpdateAsync(int id, UpdateOrderRequest request)
    {
        var existing = await _repository.GetByIdAsync(id);
        var updated = request.ApplyTo(existing);
        await _repository.UpdateAsync(updated);

        // Audit entry
        await _auditService.LogAsync(new AuditEntry
        {
            UserId = _currentUser.UserId,
            Action = "Update",
            Entity = "Order",
            EntityId = id.ToString(),
            OldValue = existing,
            NewValue = updated,
            IpAddress = _currentUser.IpAddress,
            UserAgent = _currentUser.UserAgent
        });
    }
}
```

### Query des audit logs

```csharp
[ApiController]
[Route("api/audit")]
[Authorize(Roles = "Admin")]
public class AuditController : ControllerBase
{
    private readonly IAuditService _auditService;

    public AuditController(IAuditService auditService)
    {
        _auditService = auditService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAuditLogs(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] string? userId,
        [FromQuery] string? entity)
    {
        var logs = await _auditService.GetAsync(new AuditQuery
        {
            StartDate = startDate,
            EndDate = endDate,
            UserId = userId,
            Entity = entity
        });

        return Ok(logs);
    }
}
```

---

## Refresh Tokens

### Configuration

```csharp
builder.Services.AddRefreshTokenServices(options =>
{
    options.ExpirationDays = 30;
    options.CleanupIntervalHours = 24;
    options.EnableSlidingExpiration = true;
    options.MaxRefreshTokenUsage = 10;
});
```

### Usage des refresh tokens

```csharp
[HttpPost("refresh")]
public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
{
    var tokens = await _refreshTokenService.RefreshAsync(
        request.RefreshToken,
        request.AccessToken);

    return Ok(tokens);
}

[HttpPost("logout")]
public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request)
{
    await _refreshTokenService.RevokeAsync(request.RefreshToken);
    return Ok("Logged out successfully");
}
```

---

## Configuration

### appsettings.json complet

```json
{
  "Security": {
    "RefreshToken": {
      "ExpirationDays": 30,
      "CleanupIntervalHours": 24,
      "EnableSlidingExpiration": true,
      "MaxRefreshTokenUsage": 10
    },
    "RateLimit": {
      "EnableRateLimiting": true,
      "DefaultLimit": 100,
      "DefaultWindow": "00:01:00",
      "StoreType": "InMemory"
    },
    "AuditTrail": {
      "EnableAudit": true,
      "IncludeRequestBody": true,
      "IncludeResponseBody": false,
      "ExcludePaths": ["/health", "/metrics"],
      "MaxRequestBodySize": 1048576
    },
    "Totp": {
      "Issuer": "MyApp",
      "Digits": 6,
      "Period": 30,
      "BackupCodeCount": 10
    },
    "Permission": {
      "EnableHierarchicalPermissions": true,
      "Permissions": [
        {
          "Name": "Products.View",
          "DisplayName": "View Products"
        },
        {
          "Name": "Products.Create",
          "DisplayName": "Create Products"
        }
      ]
    }
  }
}
```

---

## Voir aussi

- [Feature Flags](features.md) - Authorization basée sur les features
- [Jobs](jobs.md) - Nettoyage automatique des tokens
- [Health Checks](health-checks.md) - Monitoring de la sécurité
