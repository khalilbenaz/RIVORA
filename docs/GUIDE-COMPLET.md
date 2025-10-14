# Guide Complet - RIVORA Framework

## Table des Matières

1. [Introduction](#1-introduction)
2. [Architecture détaillée](#2-architecture-détaillée)
3. [Patterns implémentés](#3-patterns-implémentés)
4. [Multi-Tenancy](#4-multi-tenancy)
5. [Audit Logging](#5-audit-logging)
6. [Sécurité](#6-sécurité)
7. [Performance](#7-performance)
8. [Best Practices](#8-best-practices)

## 1. Introduction

RIVORA Framework est un framework d'entreprise complet basé sur les principes de Clean Architecture et Domain-Driven Design (DDD). Il fournit une base solide pour développer des applications SaaS multi-tenant avec .NET 8.

### 1.1 Objectifs du Framework

- Fournir une architecture scalable et maintenable
- Implémenter les best practices .NET
- Supporter le multi-tenancy natif
- Offrir un audit logging complet
- Faciliter les tests et le déploiement

### 1.2 Principes de conception

- **Separation of Concerns** - Chaque couche a une responsabilité claire
- **Dependency Inversion** - Les dépendances pointent vers le domaine
- **Single Responsibility** - Une classe = une responsabilité
- **Open/Closed Principle** - Ouvert à l'extension, fermé à la modification
- **DRY (Don't Repeat Yourself)** - Éviter la duplication de code

## 2. Architecture détaillée

### 2.1 Couche Domain

La couche Domain est le cœur de l'application. Elle ne dépend d'aucune autre couche.

#### Entités de base

**Entity<TKey>**
```csharp
public abstract class Entity<TKey> : IEquatable<Entity<TKey>>
{
    public TKey Id { get; protected set; }
    // Logique d'égalité basée sur l'ID
}
```

**AuditedEntity<TKey>**
```csharp
public abstract class AuditedEntity<TKey> : Entity<TKey>
{
    public DateTime CreatedAt { get; protected set; }
    public Guid? CreatorId { get; protected set; }
    public DateTime? UpdatedAt { get; protected set; }
    public Guid? LastModifierId { get; protected set; }
}
```

**FullAuditedEntity<TKey>**
```csharp
public abstract class FullAuditedEntity<TKey> : AuditedEntity<TKey>
{
    public bool IsDeleted { get; protected set; }
    public DateTime? DeletedAt { get; protected set; }
    public Guid? DeleterId { get; protected set; }
    
    public virtual void Delete(Guid? userId = null) { }
    public virtual void Restore() { }
}
```

**AggregateRoot<TKey>**
```csharp
public abstract class AggregateRoot<TKey> : FullAuditedEntity<TKey>
{
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    
    protected void AddDomainEvent(IDomainEvent domainEvent) { }
    public void ClearDomainEvents() { }
}
```

#### Règles métier

Les entités doivent encapsuler leurs règles métier:

```csharp
public class Product : AggregateRoot<Guid>
{
    // Constructeur privé pour EF Core
    private Product() { }

    // Constructeur public avec validation
    public Product(Guid? tenantId, string name, decimal price, int stock)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Le nom ne peut pas être vide.");
        
        if (price < 0)
            throw new ArgumentException("Le prix ne peut pas être négatif.");
            
        // Initialisation...
    }

    // Méthodes métier au lieu de setters publics
    public void AdjustStock(int quantity, Guid? userId = null)
    {
        var newStock = Stock + quantity;
        if (newStock < 0)
            throw new InvalidOperationException("Stock insuffisant.");
            
        Stock = newStock;
        SetModificationInfo(userId);
    }
}
```

### 2.2 Couche Application

La couche Application orchestre les opérations métier.

#### Services

```csharp
public interface IProductService
{
    Task<ProductDto?> GetAsync(Guid id, CancellationToken ct = default);
    Task<ProductDto> CreateAsync(CreateProductDto dto, CancellationToken ct = default);
}

public class ProductService : IProductService
{
    private readonly IProductRepository _repository;

    public ProductService(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<ProductDto> CreateAsync(CreateProductDto dto, CancellationToken ct = default)
    {
        // 1. Validation
        if (dto == null) throw new ArgumentNullException(nameof(dto));

        // 2. Création de l'entité
        var product = new Product(null, dto.Name, dto.Price, dto.Stock);

        // 3. Persistance
        var created = await _repository.InsertAsync(product, ct);

        // 4. Mapping vers DTO
        return MapToDto(created);
    }
}
```

#### DTOs (Data Transfer Objects)

Utilisez des records C# pour l'immutabilité:

```csharp
// DTO de lecture
public record ProductDto(
    Guid Id,
    string Name,
    decimal Price,
    int Stock,
    DateTime CreatedAt
);

// DTO de création
public record CreateProductDto(
    string Name,
    decimal Price,
    int Stock
);

// DTO de mise à jour
public record UpdateProductDto(
    string Name,
    decimal Price
);
```

### 2.3 Couche Infrastructure

#### DbContext

```csharp
public class RVRDbContext : DbContext
{
    public RVRDbContext(DbContextOptions<RVRDbContext> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<User> Users => Set<User>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(RVRDbContext).Assembly);
    }
}
```

#### Configuration Fluent API

```csharp
public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable(RVRConsts.TablePrefix + "Products");
        
        builder.HasKey(p => p.Id);
        
        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(256);
            
        builder.Property(p => p.Price)
            .HasPrecision(18, 2);
            
        builder.HasIndex(p => p.Name);
        builder.HasIndex(p => p.TenantId);
        
        // Ignorer les événements de domaine
        builder.Ignore(p => p.DomainEvents);
        
        // Query filter pour soft delete
        builder.HasQueryFilter(p => !p.IsDeleted);
    }
}
```

#### Repository Pattern

```csharp
public class Repository<TEntity, TKey> : IRepository<TEntity, TKey> 
    where TEntity : Entity<TKey>
{
    protected readonly RVRDbContext _context;
    protected readonly DbSet<TEntity> _dbSet;

    public Repository(RVRDbContext context)
    {
        _context = context;
        _dbSet = context.Set<TEntity>();
    }

    public virtual async Task<TEntity?> GetByIdAsync(TKey id, CancellationToken ct = default)
    {
        return await _dbSet.FindAsync(new object[] { id! }, ct);
    }

    public virtual async Task<TEntity> InsertAsync(TEntity entity, CancellationToken ct = default)
    {
        await _dbSet.AddAsync(entity, ct);
        await _context.SaveChangesAsync(ct);
        return entity;
    }
}
```

### 2.4 Couche API

#### Contrôleurs

```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _service;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(IProductService service, ILogger<ProductsController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProductDto>> Create(
        [FromBody] CreateProductDto dto, 
        CancellationToken ct)
    {
        try
        {
            var product = await _service.CreateAsync(dto, ct);
            return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
```

## 3. Patterns implémentés

### 3.1 Repository Pattern

Le Repository Pattern abstrait l'accès aux données:

**Avantages:**
- Découplage de la logique métier et de l'accès aux données
- Facilite les tests (mocking)
- Centralise la logique de requêtage
- Permet de changer facilement de technologie d'accès aux données

### 3.2 Unit of Work Pattern

Géré automatiquement par EF Core via `SaveChangesAsync()`.

### 3.3 CQRS (Command Query Responsibility Segregation)

Séparation des opérations de lecture et d'écriture (peut être étendu).

### 3.4 Specification Pattern

Pour des requêtes complexes réutilisables:

```csharp
public class ActiveProductsSpecification : Specification<Product>
{
    public override Expression<Func<Product, bool>> ToExpression()
    {
        return p => p.IsActive && !p.IsDeleted;
    }
}
```

## 4. Multi-Tenancy

### 4.1 Architecture Multi-Tenant

RIVORA Framework implémente le **multi-tenancy au niveau de la ligne** (shared database, shared schema):

- Une seule base de données
- Tables partagées avec colonne `TenantId`
- Isolation des données par filtres de requêtes

### 4.2 Configuration

```csharp
public class Tenant : FullAuditedEntity<Guid>
{
    public string Name { get; private set; }
    public bool IsActive { get; private set; }
}
```

Toutes les entités métier doivent avoir:

```csharp
public Guid? TenantId { get; private set; }
```

### 4.3 Filtrage automatique

```csharp
// Dans la configuration EF Core
builder.HasQueryFilter(p => p.TenantId == CurrentTenantId);
```

### 4.4 Résolution du Tenant

Implémenter un service pour identifier le tenant actuel:

```csharp
public interface ITenantResolver
{
    Guid? GetCurrentTenantId();
}

public class TenantResolver : ITenantResolver
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public Guid? GetCurrentTenantId()
    {
        // Depuis un header HTTP, claim, subdomain, etc.
        return _httpContextAccessor.HttpContext?.User
            ?.FindFirst("TenantId")?.Value;
    }
}
```

## 5. Audit Logging

### 5.1 Structure des Audit Logs

```
AuditLog (requête HTTP globale)
├── AuditLogAction (appel de méthode)
├── EntityChange (modification d'entité)
│   └── EntityPropertyChange (changement de propriété)
```

### 5.2 Capture automatique

Intercepter `SaveChangesAsync()` pour capturer les changements:

```csharp
public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
{
    var entries = ChangeTracker.Entries()
        .Where(e => e.State == EntityState.Added || 
                    e.State == EntityState.Modified || 
                    e.State == EntityState.Deleted)
        .ToList();

    foreach (var entry in entries)
    {
        // Créer EntityChange et EntityPropertyChange
    }

    return await base.SaveChangesAsync(ct);
}
```

### 5.3 Middleware d'Audit

```csharp
public class AuditLoggingMiddleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            // Enregistrer l'audit log
        }
    }
}
```

## 6. Sécurité

### 6.1 Authentification JWT

```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = configuration["Jwt:Issuer"],
            ValidAudience = configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(configuration["Jwt:Key"]))
        };
    });
```

### 6.2 Autorisation basée sur les permissions

```csharp
[Authorize]
[RequirePermission("Products.Create")]
public async Task<IActionResult> Create([FromBody] CreateProductDto dto)
{
    // ...
}
```

### 6.3 Validation des entrées

Utilisez Data Annotations et FluentValidation:

```csharp
public class CreateProductDtoValidator : AbstractValidator<CreateProductDto>
{
    public CreateProductDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Stock).GreaterThanOrEqualTo(0);
    }
}
```

## 7. Performance

### 7.1 Caching

```csharp
public class CachedProductService : IProductService
{
    private readonly IProductService _innerService;
    private readonly IMemoryCache _cache;

    public async Task<ProductDto?> GetAsync(Guid id, CancellationToken ct = default)
    {
        var cacheKey = $"product_{id}";
        
        if (!_cache.TryGetValue(cacheKey, out ProductDto? product))
        {
            product = await _innerService.GetAsync(id, ct);
            
            _cache.Set(cacheKey, product, TimeSpan.FromMinutes(5));
        }
        
        return product;
    }
}
```

### 7.2 Pagination

```csharp
public record PagedResult<T>(
    List<T> Items,
    int TotalCount,
    int PageNumber,
    int PageSize
);

public async Task<PagedResult<ProductDto>> GetPagedAsync(
    int pageNumber, 
    int pageSize, 
    CancellationToken ct = default)
{
    var query = _dbSet.AsQueryable();
    
    var totalCount = await query.CountAsync(ct);
    
    var items = await query
        .Skip((pageNumber - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync(ct);
    
    return new PagedResult<ProductDto>(
        items.Select(MapToDto).ToList(),
        totalCount,
        pageNumber,
        pageSize
    );
}
```

### 7.3 Async/Await

Toujours utiliser async/await pour les opérations I/O:

```csharp
// ✅ BON
public async Task<Product> GetProductAsync(Guid id)
{
    return await _repository.GetByIdAsync(id);
}

// ❌ MAUVAIS
public Task<Product> GetProductAsync(Guid id)
{
    return Task.Run(() => _repository.GetById(id)); // Bloque un thread
}
```

## 8. Best Practices

### 8.1 Naming Conventions

- **Classes**: PascalCase (`ProductService`)
- **Méthodes**: PascalCase (`GetProductAsync`)
- **Variables**: camelCase (`productId`)
- **Constantes**: PascalCase (`MaxNameLength`)
- **Privé**: _camelCase (`_repository`)

### 8.2 Gestion des exceptions

```csharp
public async Task<ProductDto> GetAsync(Guid id)
{
    var product = await _repository.GetByIdAsync(id);
    
    if (product == null)
        throw new KeyNotFoundException($"Produit {id} non trouvé.");
    
    return MapToDto(product);
}
```

### 8.3 Logging

```csharp
public async Task<ProductDto> CreateAsync(CreateProductDto dto)
{
    _logger.LogInformation("Création du produit {ProductName}", dto.Name);
    
    try
    {
        var product = await _repository.InsertAsync(new Product(...));
        
        _logger.LogInformation(
            "Produit {ProductId} créé avec succès", 
            product.Id);
        
        return MapToDto(product);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Erreur lors de la création du produit");
        throw;
    }
}
```

### 8.4 Tests

**Arrange-Act-Assert Pattern:**

```csharp
[Fact]
public async Task CreateAsync_ShouldCreateProduct_WithValidData()
{
    // Arrange
    var dto = new CreateProductDto("Product", 100m, 10);
    var mockRepo = new Mock<IProductRepository>();
    var service = new ProductService(mockRepo.Object);

    // Act
    var result = await service.CreateAsync(dto);

    // Assert
    Assert.NotNull(result);
    Assert.Equal("Product", result.Name);
    mockRepo.Verify(r => r.InsertAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once);
}
```

### 8.5 Dependency Injection

Toujours injecter les dépendances via le constructeur:

```csharp
public class ProductService : IProductService
{
    private readonly IProductRepository _repository;
    private readonly ILogger<ProductService> _logger;

    // ✅ BON: Injection par constructeur
    public ProductService(
        IProductRepository repository,
        ILogger<ProductService> logger)
    {
        _repository = repository;
        _logger = logger;
    }
}
```

### 8.6 Configuration

Utilisez le pattern Options:

```csharp
public class JwtSettings
{
    public string Key { get; set; }
    public string Issuer { get; set; }
    public string Audience { get; set; }
    public int ExpirationMinutes { get; set; }
}

// Dans Program.cs
builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("Jwt"));

// Dans un service
public class TokenService
{
    private readonly JwtSettings _jwtSettings;

    public TokenService(IOptions<JwtSettings> jwtSettings)
    {
        _jwtSettings = jwtSettings.Value;
    }
}
```

---

## Commandes utiles

### Entity Framework

```bash
# Créer une migration
dotnet ef migrations add MigrationName --project src/RVR.Framework.Infrastructure --startup-project src/RVR.Framework.Api

# Appliquer les migrations
dotnet ef database update --project src/RVR.Framework.Infrastructure --startup-project src/RVR.Framework.Api

# Supprimer la dernière migration
dotnet ef migrations remove --project src/RVR.Framework.Infrastructure --startup-project src/RVR.Framework.Api

# Générer un script SQL
dotnet ef migrations script --project src/RVR.Framework.Infrastructure --startup-project src/RVR.Framework.Api --output migration.sql
```

### Build et Test

```bash
# Build
dotnet build

# Test
dotnet test

# Publish
dotnet publish -c Release

# Watch (développement)
dotnet watch run --project src/RVR.Framework.Api
```

---

## 9. Génération de Code (RVR Studio)

RVR Studio est une suite d'outils graphiques intégrée pour accélérer le développement.

### 9.1 Visual Entity Builder (Full-Stack)

Le Visual Entity Builder permet de définir une entité et ses propriétés, puis de générer physiquement tous les fichiers nécessaires au bon fonctionnement du framework.

#### Fonctionnalités principales :
- **Définition visuelle** : Ajout de champs avec types C# (string, int, decimal, bool, DateTime, Guid).
- **Options d'architecture** : 
    - Choix entre `AggregateRoot` ou `Entity`.
    - Support natif du Multi-Tenancy (`IMustHaveTenant`).
    - Support natif de l'Audit (`IAuditableEntity`).
- **Génération Physique** : Les fichiers sont écrits directement dans les projets correspondants de la solution `src/`.

#### Fichiers générés :
1. **Domain** (`src/RVR.Framework.Domain/Entities/{Entity}s/`) :
    - Classe de l'entité avec constructeurs (privé pour EF, public pour la création) et méthode `Update`.
2. **Infrastructure** (`src/RVR.Framework.Infrastructure/Data/Configurations/`) :
    - Classe de configuration Fluent API (`IEntityTypeConfiguration`).
3. **Application** (`src/RVR.Framework.Application/DTOs/{Entity}s/`) :
    - DTOs de réponse, création et mise à jour.
4. **API** (`src/RVR.Framework.Api/Controllers/`) :
    - Controller REST complet injectant le service correspondant.

### 9.2 Workflow de génération
1. Lancer **RVR Studio**.
2. Accéder à la page **Entity Builder**.
3. Saisir le nom de l'entité et ajouter les champs.
4. Cliquer sur **Prévisualiser** pour vérifier le code généré.
5. Cliquer sur **Générer PHYSIQUEMENT les fichiers** pour finaliser.
6. (Optionnel) Créer une migration EF Core pour mettre à jour la base de données.

---

**RIVORA Framework** - Guide Complet v1.1
