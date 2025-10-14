# Implémentation du TenantId dans RVR.Framework

## Résumé des modifications

Ce document décrit l'implémentation complète du système de multi-tenancy basé sur le TenantId récupéré du contexte utilisateur JWT.

## Architecture

### 1. Service de contexte utilisateur

**Interface**: `ICurrentUserContext`
- **Emplacement**: `src/RVR.Framework.Application/Services/ICurrentUserContext.cs`
- **Propriétés**:
  - `TenantId`: Récupère le TenantId de l'utilisateur connecté depuis les claims JWT
  - `UserId`: Récupère l'UserId depuis les claims JWT
  - `UserName`: Récupère le nom d'utilisateur
  - `IsAuthenticated`: Indique si l'utilisateur est authentifié

**Implémentation**: `CurrentUserContext`
- **Emplacement**: `src/RVR.Framework.Infrastructure/Services/CurrentUserContext.cs`
- **Fonctionnement**: 
  - Utilise `IHttpContextAccessor` pour accéder au contexte HTTP
  - Extrait les claims du token JWT
  - Parse le TenantId depuis le claim "TenantId"
  - Parse l'UserId depuis le claim `ClaimTypes.NameIdentifier`

### 2. Génération du token JWT avec TenantId

**Service**: `JwtTokenService`
- **Emplacement**: `src/RVR.Framework.Infrastructure/Services/JwtTokenService.cs`
- **Modification**: Ajout automatique du claim "TenantId" lors de la génération du token si l'utilisateur a un TenantId

```csharp
// Ajouter le TenantId si présent
if (user.TenantId.HasValue)
{
    claims.Add(new Claim("TenantId", user.TenantId.Value.ToString()));
}
```

### 3. Utilisation dans les services métier

#### ProductService
- **Emplacement**: `src/RVR.Framework.Application/Services/ProductService.cs`
- **Modification**: 
  - Injection de `ICurrentUserContext` dans le constructeur
  - Utilisation de `_currentUserContext.TenantId` lors de la création de produits
  
**Avant**:
```csharp
var product = new Product(
    tenantId: null, // À remplacer par le TenantId du contexte
    name: dto.Name,
    price: dto.Price,
    stock: dto.Stock
);
```

**Après**:
```csharp
var product = new Product(
    tenantId: _currentUserContext.TenantId,
    name: dto.Name,
    price: dto.Price,
    stock: dto.Stock
);
```

#### UserService
- **Emplacement**: `src/RVR.Framework.Application/Services/UserService.cs`
- **Modification**: Identique à ProductService
  - Injection de `ICurrentUserContext`
  - Utilisation lors de la création d'utilisateurs

### 4. Configuration dans Program.cs

**Enregistrement des services**:
```csharp
// Enregistrement du contexte utilisateur
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserContext, CurrentUserContext>();
```

**Ordre d'importance**:
1. `AddHttpContextAccessor()` doit être appelé en premier
2. `AddScoped<ICurrentUserContext, CurrentUserContext>()` peut ensuite être enregistré
3. Les services métier qui dépendent de `ICurrentUserContext` sont injectés automatiquement

### 5. Dépendances ajoutées

**Infrastructure Project** (`RVR.Framework.Infrastructure.csproj`):
```xml
<PackageReference Include="Microsoft.AspNetCore.Http.Abstractions" Version="2.2.0" />
```

### 6. Tests unitaires

**ProductServiceTests**:
- **Emplacement**: `tests/RVR.Framework.Application.Tests/Services/ProductServiceTests.cs`
- **Modification**: Ajout du mock `ICurrentUserContext` dans le constructeur des tests

```csharp
private readonly Mock<ICurrentUserContext> _mockUserContext;

public ProductServiceTests()
{
    _mockRepository = new Mock<IProductRepository>();
    _mockUserContext = new Mock<ICurrentUserContext>();
    
    // Configuration par défaut du contexte utilisateur
    _mockUserContext.Setup(x => x.TenantId).Returns((Guid?)null);
    _mockUserContext.Setup(x => x.UserId).Returns(Guid.NewGuid());
    _mockUserContext.Setup(x => x.IsAuthenticated).Returns(true);
    
    _service = new ProductService(_mockRepository.Object, _mockUserContext.Object);
}
```

## Fonctionnement en production

1. **Authentification**: L'utilisateur se connecte via `/api/auth/login`
2. **Génération du token**: Le service JWT génère un token incluant le TenantId dans les claims
3. **Requêtes authentifiées**: L'utilisateur envoie des requêtes avec le token JWT dans l'en-tête `Authorization: Bearer {token}`
4. **Extraction du contexte**: Le middleware d'authentification valide le token et remplit `HttpContext.User`
5. **Accès au TenantId**: Les services métier utilisent `ICurrentUserContext` pour récupérer le TenantId
6. **Isolation des données**: Les entités créées sont automatiquement associées au TenantId de l'utilisateur connecté

## Avantages de cette implémentation

✅ **Séparation des responsabilités**: Le contexte utilisateur est géré dans un service dédié  
✅ **Testabilité**: Facile à mocker dans les tests unitaires  
✅ **Sécurité**: Le TenantId ne peut pas être falsifié car il provient du token JWT validé  
✅ **Transparence**: Les services métier n'ont pas besoin de connaître HttpContext  
✅ **Flexibilité**: Facile d'ajouter d'autres informations de contexte (rôles, permissions, etc.)  

## Remarques importantes

- Si un utilisateur n'a pas de TenantId (ex: super admin), `TenantId` sera `null`
- Les requêtes non authentifiées auront `IsAuthenticated = false` et `TenantId = null`
- Le filtrage par TenantId dans les requêtes doit être implémenté au niveau du repository (futur)
