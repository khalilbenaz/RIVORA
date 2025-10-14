# Contributing to RIVORA Framework

Merci de votre intérêt pour contribuer à RIVORA Framework ! Ce guide vous explique comment contribuer efficacement au projet.

---

## 📋 Table des Matières

- [Code de conduite](#code-de-conduite)
- [Comment contribuer](#comment-contribuer)
- [Processus de Pull Request](#processus-de-pull-request)
- [Code Style](#code-style)
- [Standards de développement](#standards-de-développement)
- [Tests](#tests)
- [Documentation](#documentation)

---

## Code de conduite

- Soyez respectueux et inclusif
- Acceptez les critiques constructives
- Concentrez-vous sur ce qui est meilleur pour la communauté
- Montrez de l'empathie envers les autres membres

---

## Comment contribuer

### Types de contributions

Nous acceptons plusieurs types de contributions :

| Type | Description |
|------|-------------|
| 🐛 Bug fixes | Correction de bugs ou problèmes |
| ✨ Features | Nouvelles fonctionnalités |
| 📝 Documentation | Amélioration de la documentation |
| 🎨 Style | Corrections de style, formatting |
| ♻️ Refactoring | Refactoring de code existant |
| ⚡ Performance | Optimisations de performance |
| ✅ Tests | Ajout ou correction de tests |
| 🔧 CI/CD | Améliorations de l'intégration continue |

### Premiers pas

#### 1. Fork le repository

```bash
# Sur GitHub, cliquez sur "Fork" pour créer votre copie
```

#### 2. Clonez votre fork

```bash
git clone https://github.com/VOTRE_USERNAME/RVR.Framework.git
cd RVR.Framework
```

#### 3. Configurez le remote upstream

```bash
git remote add upstream https://github.com/khalilbenaz/RIVORA.git
git fetch upstream
```

#### 4. Créez une branche

```bash
# Toujours créer une branche depuis main/master
git checkout -b feature/ma-nouvelle-feature
```

**Convention de nommage des branches :**

| Type | Préfixe | Exemple |
|------|---------|---------|
| Feature | `feature/` | `feature/jwt-refresh-token` |
| Bug fix | `fix/` | `fix/product-validation` |
| Documentation | `docs/` | `docs/api-documentation` |
| Refactoring | `refactor/` | `refactor/repository-pattern` |
| Performance | `perf/` | `perf/ef-optimizations` |
| Tests | `test/` | `test/integration-tests` |

---

## Processus de Pull Request

### Workflow complet

```
1. Créer une issue (si nécessaire)
         ↓
2. Fork et créer une branche
         ↓
3. Développer et tester
         ↓
4. Commit avec messages conventionnels
         ↓
5. Push vers GitHub
         ↓
6. Ouvrir une Pull Request
         ↓
7. Review et corrections
         ↓
8. Merge par un maintainer
```

### 1. Créer une issue

Pour les features majeures ou les changements importants, créez d'abord une issue GitHub pour discuter de la proposition.

### 2. Développer et tester

- Suivez le code style du projet
- Écrivez des tests pour les nouvelles fonctionnalités
- Assurez-vous que tous les tests passent

```bash
# Exécuter tous les tests
dotnet test

# Vérifier le build
dotnet build
```

### 3. Messages de commit conventionnels

Nous utilisons le format **Conventional Commits** :

```
<type>(<scope>): <description>

[optional body]

[optional footer]
```

**Types :**

| Type | Description |
|------|-------------|
| `feat` | Nouvelle fonctionnalité |
| `fix` | Correction de bug |
| `docs` | Documentation uniquement |
| `style` | Formatage, point-virgules, etc. |
| `refactor` | Refactoring de code |
| `perf` | Optimisation de performance |
| `test` | Ajout ou correction de tests |
| `chore` | Maintenance, outils, build |

**Exemples :**

```bash
# Feature
git commit -m "feat(auth): add JWT refresh token support

Co-authored-by: Qwen-Coder <qwen-coder@alibabacloud.com>"

# Bug fix
git commit -m "fix(products): resolve null reference in product search"

# Documentation
git commit -m "docs(readme): update quickstart section"

# Refactor
git commit -m "refactor(domain): extract value objects to separate classes"

# With scope and body
git commit -m "feat(multi-tenancy): add tenant isolation layer

- Implement ITenantProvider interface
- Add TenantId filter to all queries
- Update repository pattern for tenant awareness

Closes #123"
```

### 4. Ouvrir une Pull Request

#### Template de PR

Lorsque vous ouvrez une PR, remplissez le template suivant :

```markdown
## Description
<!-- Décrivez vos changements en quelques phrases -->

## Type de changement
<!-- Cochez la case appropriée -->
- [ ] Bug fix (changement non cassant qui corrige un problème)
- [ ] Nouvelle feature (changement non cassant qui ajoute une fonctionnalité)
- [ ] Breaking change (correction ou feature qui cassera une fonctionnalité existante)
- [ ] Documentation mise à jour

## Checklist
<!-- Vérifiez chaque point -->
- [ ] Mon code suit les standards du projet
- [ ] J'ai commenté mon code si nécessaire
- [ ] J'ai mis à jour la documentation
- [ ] Les tests passent localement
- [ ] J'ai ajouté des tests pour ma feature

## Issues liées
<!-- Liez les issues GitHub concernées -->
Closes #123
```

### 5. Review process

- Un maintainer reviewera votre PR
- Des commentaires peuvent être demandés
- Des CI checks seront exécutés automatiquement
- Une fois approuvé, votre PR sera mergée

---

## Code Style

### C# Conventions

Nous suivons les [conventions C# Microsoft](https://docs.microsoft.com/dotnet/csharp/fundamentals/coding-style/coding-conventions).

#### Naming

```csharp
// ✅ CORRECT
public class ProductService { }           // PascalCase pour classes
public interface IUserService { }         // I-prefix pour interfaces
public const string MaxLength = "100";    // PascalCase pour constantes
public string UserName { get; set; }      // PascalCase pour propriétés
public void CalculateTotal() { }          // PascalCase pour méthodes
var products = new List<Product>();       // camelCase pour variables
private readonly IRepository _repository; // _prefix pour champs privés

// ❌ INCORRECT
public class productService { }           // Pas de camelCase pour classes
public interface user_service { }         // Pas de snake_case
public const string max_length = "100";   // Pas de snake_case pour constantes
```

#### Properties et Auto-properties

```csharp
// ✅ Utiliser auto-properties quand possible
public string Name { get; set; }

// ✅ Utiliser expression-bodied members pour les propriétés calculées
public string FullName => $"{FirstName} {LastName}";

// ✅ Initializers pour valeurs par défaut
public int Status { get; set; } = 1;
```

#### Async/Await

```csharp
// ✅ CORRECT
public async Task<User> GetUserAsync(Guid id, CancellationToken ct = default)
{
    return await _repository.GetByIdAsync(id, ct);
}

// ❌ INCORRECT - async void (sauf pour event handlers)
public async void DoSomething() { }

// ❌ INCORRECT - .Result ou .Wait()
var user = _repository.GetByIdAsync(id).Result;
```

#### Dependency Injection

```csharp
// ✅ CORRECT - Constructor injection
public class ProductService : IProductService
{
    private readonly IRepository _repository;
    
    public ProductService(IRepository repository)
    {
        _repository = repository;
    }
}

// ❌ INCORRECT - Service Locator
public class ProductService
{
    public void DoSomething()
    {
        var repo = ServiceProvider.GetService<IRepository>();
    }
}
```

#### Error Handling

```csharp
// ✅ CORRECT - Exceptions spécifiques
try
{
    await _repository.GetByIdAsync(id);
}
catch (NotFoundException ex)
{
    _logger.LogWarning(ex, "Entity not found: {Id}", id);
    throw;
}

// ❌ INCORRECT - Catch général
try
{
    // code
}
catch (Exception)
{
    // swallow exception
}
```

### Architecture Patterns

#### Repository Pattern

```csharp
// Interface dans Domain
public interface IProductRepository : IRepository<Product, Guid>
{
    Task<List<Product>> GetByCategoryAsync(string category);
}

// Implémentation dans Infrastructure
public class ProductRepository : Repository<Product, Guid>, IProductRepository
{
    public ProductRepository(RVRDbContext context) : base(context) { }
    
    public async Task<List<Product>> GetByCategoryAsync(string category)
    {
        return await _dbSet
            .Where(p => p.Category == category)
            .AsNoTracking()
            .ToListAsync();
    }
}
```

#### Service Layer

```csharp
// Interface
public interface IProductService
{
    Task<ProductDto> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<ProductDto>> GetAllAsync(CancellationToken ct = default);
    Task<ProductDto> CreateAsync(CreateProductDto dto, CancellationToken ct = default);
}

// Implémentation
public class ProductService : IProductService
{
    private readonly IProductRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<ProductService> _logger;

    public ProductService(
        IProductRepository repository,
        IMapper mapper,
        ILogger<ProductService> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ProductDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var product = await _repository.GetByIdAsync(id, ct);
        return product == null ? null : _mapper.Map<ProductDto>(product);
    }
}
```

---

## Standards de développement

### Clean Architecture

Respectez la séparation des couches :

```
Domain (Entities, Value Objects, Events)
   ↑
Application (Services, DTOs, Interfaces)
   ↑
Infrastructure (EF Core, Repositories, External Services)
   ↑
API (Controllers, Middleware, Configuration)
```

**Règles :**

- Domain ne dépend de rien
- Application dépend uniquement de Domain
- Infrastructure dépend de Domain et Application
- API dépend de toutes les couches

### Multi-Tenancy

Toutes les entités doivent supporter le multi-tenancy :

```csharp
public class Product : Entity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }
    
    // Toutes les queries doivent filtrer par TenantId
}
```

### Audit Logging

Les entités auditées doivent implémenter `IAuditedEntity` :

```csharp
public class Product : Entity<Guid>, IAuditedEntity
{
    public DateTime CreationTime { get; set; }
    public Guid? CreatorUserId { get; set; }
    public DateTime? LastModificationTime { get; set; }
    public Guid? LastModifierUserId { get; set; }
}
```

---

## Tests

### Écriture de tests

```csharp
public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _mockRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly ProductService _service;

    public ProductServiceTests()
    {
        _mockRepository = new Mock<IProductRepository>();
        _mockMapper = new Mock<IMapper>();
        _service = new ProductService(
            _mockRepository.Object,
            _mockMapper.Object,
            NullLogger<ProductService>.Instance
        );
    }

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsProduct()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = new Product { Id = productId, Name = "Test" };
        _mockRepository
            .Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act
        var result = await _service.GetByIdAsync(productId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test", result.Name);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingId_ReturnsNull()
    {
        // Arrange
        var productId = Guid.NewGuid();
        _mockRepository
            .Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product)null);

        // Act
        var result = await _service.GetByIdAsync(productId);

        // Assert
        Assert.Null(result);
    }
}
```

### Exécution des tests

```bash
# Tous les tests
dotnet test

# Avec couverture
dotnet test /p:CollectCoverage=true

# Tests spécifiques
dotnet test --filter "FullyQualifiedName~ProductService"
```

---

## Documentation

### Code Comments

```csharp
/// <summary>
/// Crée un nouveau produit dans le système.
/// </summary>
/// <param name="dto">Les données du produit à créer.</param>
/// <param name="cancellationToken">Token d'annulation.</param>
/// <returns>Le produit créé avec son ID généré.</returns>
/// <exception cref="ValidationException">Lorsque les données sont invalides.</exception>
public async Task<ProductDto> CreateAsync(
    CreateProductDto dto,
    CancellationToken cancellationToken = default)
{
    // Implementation
}
```

### README Updates

Pour les nouvelles features, mettez à jour :

1. Le README principal si la feature est majeure
2. La documentation dans `docs/`
3. Les exemples de code si nécessaire

---

## 👥 Contributeurs Principaux

- **Khalil Benazzouz** ([@khalilbenaz](https://github.com/khalilbenaz)) - Créateur et Maintainer.
- **Gemini CLI** (Google AI) - Co-développeur et Assistant IA.

---

## 📞 Questions ?

- 💬 Discussions: [GitHub Discussions](https://github.com/khalilbenaz/RIVORA/discussions)
- 🐛 Issues: [GitHub Issues](https://github.com/khalilbenaz/RIVORA/issues)

---

Merci de contribuer à RIVORA Framework ! 🎉
