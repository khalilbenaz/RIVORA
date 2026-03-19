# Creer son projet avec RIVORA

Ce guide detaille etape par etape comment creer un nouveau projet a partir du framework RIVORA, du scaffold initial au premier deploiement.

## Prerequis

Avant de commencer, verifiez que vous avez :

| Outil | Commande de verification |
|-------|--------------------------|
| .NET 9 SDK | `dotnet --version` (9.0+) |
| SQL Server / LocalDB | `sqllocaldb info` |
| EF Core CLI | `dotnet ef --version` |
| Git | `git --version` |

Si EF Core CLI n'est pas installe :

```bash
dotnet tool install --global dotnet-ef
```

---

## Etape 1 : Cloner le framework

```bash
git clone https://github.com/khalilbenaz/RIVORA.git MonProjet
cd MonProjet
```

Supprimez l'historique Git pour repartir de zero :

```bash
rm -rf .git
git init
git add .
git commit -m "Initial commit from RIVORA Framework"
```

---

## Etape 2 : Renommer le projet (optionnel)

Si vous souhaitez renommer le namespace pour votre entreprise ou produit, remplacez `RVR.Framework` par votre propre namespace dans les fichiers suivants :

| Fichier | Ce qu'il faut changer |
|---------|----------------------|
| `*.csproj` | `<RootNamespace>`, `<AssemblyName>` |
| `*.cs` | Namespaces `using` et declarations |
| `appsettings.json` | Noms de service, titres |
| `Directory.Packages.props` | Noms de packages si publies |

::: tip RVR CLI
Si vous avez installe le CLI RIVORA, vous pouvez utiliser la commande de scaffolding :
```bash
rvr new MonProjet --namespace MaSociete.MonProduit
```
:::

---

## Etape 3 : Configurer la base de donnees

### 3.1 Choisir un provider

Editez `src/api/RVR.Framework.Api/appsettings.json` selon votre provider :

::: code-group

```json [SQL Server]
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=MonProjetDb;Trusted_Connection=true;TrustServerCertificate=true"
  }
}
```

```json [PostgreSQL]
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=MonProjetDb;Username=postgres;Password=secret"
  },
  "DatabaseProvider": "PostgreSQL"
}
```

```json [SQLite (dev rapide)]
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=MonProjet.db"
  },
  "DatabaseProvider": "SQLite"
}
```

:::

### 3.2 Securiser avec User Secrets

Ne stockez **jamais** de mots de passe dans les fichiers commites :

```bash
cd src/api/RVR.Framework.Api

# Initialiser User Secrets
dotnet user-secrets init

# Definir la chaine de connexion
dotnet user-secrets set "ConnectionStrings:DefaultConnection" \
  "Server=localhost;Database=MonProjetDb;User Id=sa;Password=MonMotDePasse;TrustServerCertificate=true"

# Definir la cle JWT
dotnet user-secrets set "JwtSettings:SecretKey" \
  "MaCleSecreteSuperLongueEtUniqueDeMinimum32Caracteres!"

# Verifier
dotnet user-secrets list
```

::: info Comment ca marche ?
Les User Secrets surchargent `appsettings.json` en environnement `Development`. Ils sont stockes dans :
- **Windows** : `%APPDATA%\Microsoft\UserSecrets\<id>\secrets.json`
- **Linux/macOS** : `~/.microsoft/usersecrets/<id>/secrets.json`
:::

### 3.3 Creer la base de donnees

```bash
# Depuis la racine du projet
dotnet ef database update \
  --project src/core/RVR.Framework.Infrastructure \
  --startup-project src/api/RVR.Framework.Api
```

Sortie attendue :
```
Build started...
Build succeeded.
Applying migration '20251014201749_InitialCreate'.
Done.
```

---

## Etape 4 : Configurer la securite

### 4.1 JWT

Dans `appsettings.json`, configurez les parametres JWT (la cle secrete est dans User Secrets) :

```json
{
  "JwtSettings": {
    "Issuer": "MonProjet",
    "Audience": "MonProjet.Client",
    "ExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 7
  }
}
```

### 4.2 CORS

Pour le developpement, CORS accepte toutes les origines. En production, specifiez les origines autorisees :

```json
{
  "Cors": {
    "AllowedOrigins": [
      "https://monapp.com",
      "https://admin.monapp.com"
    ]
  }
}
```

### 4.3 Rate Limiting

Le rate limiting est configure par defaut dans `Program.cs` :

| Politique | Limite | Usage |
|-----------|--------|-------|
| `fixed` | 100 req / 10s | Endpoints standards |
| `concurrency` | 10 simultanes | Protection charge |
| `strict` | 5 req / min | Login, endpoints sensibles |

---

## Etape 5 : Lancer l'application

```bash
dotnet run --project src/api/RVR.Framework.Api
```

Verifiez que tout fonctionne :

| URL | Resultat attendu |
|-----|-------------------|
| `http://localhost:5220/alive` | `200 OK` |
| `http://localhost:5220/healthz` | `200 OK` |
| `http://localhost:5220/health` | `200 Healthy` (avec DB) |
| `http://localhost:5220/swagger` | Interface Swagger UI |

---

## Etape 6 : Creer le premier administrateur

```bash
curl -X POST http://localhost:5220/api/init/first-admin \
  -H "Content-Type: application/json" \
  -d '{
    "userName": "admin",
    "email": "admin@monprojet.com",
    "password": "Admin@123456",
    "firstName": "Admin",
    "lastName": "System"
  }'
```

Puis authentifiez-vous :

```bash
curl -X POST http://localhost:5220/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "userName": "admin",
    "password": "Admin@123456"
  }'
```

Conservez le `token` retourne pour les appels suivants.

---

## Etape 7 : Ajouter vos entites metier

### 7.1 Creer une entite Domain

Ajoutez votre entite dans `src/core/RVR.Framework.Domain/Entities/` :

```csharp
namespace RVR.Framework.Domain.Entities;

public class Client : BaseEntity<Guid>, IMultiTenant
{
    public string Nom { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telephone { get; set; } = string.Empty;
    public DateTime DateInscription { get; set; } = DateTime.UtcNow;
    public Guid? TenantId { get; set; }
}
```

### 7.2 Creer le repository (interface)

Dans `src/core/RVR.Framework.Domain/Repositories/` :

```csharp
namespace RVR.Framework.Domain.Repositories;

public interface IClientRepository : IRepository<Client, Guid>
{
    Task<Client?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<IReadOnlyList<Client>> SearchAsync(string term, CancellationToken ct = default);
}
```

### 7.3 Creer le repository (implementation)

Dans `src/core/RVR.Framework.Infrastructure/Repositories/` :

```csharp
namespace RVR.Framework.Infrastructure.Repositories;

public class ClientRepository : Repository<Client, Guid>, IClientRepository
{
    public ClientRepository(KBADbContext context) : base(context) { }

    public async Task<Client?> GetByEmailAsync(string email, CancellationToken ct = default)
        => await DbSet.FirstOrDefaultAsync(c => c.Email == email, ct);

    public async Task<IReadOnlyList<Client>> SearchAsync(string term, CancellationToken ct = default)
        => await DbSet
            .Where(c => c.Nom.Contains(term) || c.Email.Contains(term))
            .ToListAsync(ct);
}
```

### 7.4 Creer le service applicatif

Dans `src/core/RVR.Framework.Application/Services/` :

```csharp
namespace RVR.Framework.Application.Services;

public interface IClientService
{
    Task<ClientDto> CreateAsync(CreateClientDto dto);
    Task<ClientDto?> GetByIdAsync(Guid id);
    Task<IReadOnlyList<ClientDto>> GetAllAsync();
}

public class ClientService : IClientService
{
    private readonly IClientRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public ClientService(IClientRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ClientDto> CreateAsync(CreateClientDto dto)
    {
        var client = new Client
        {
            Id = Guid.NewGuid(),
            Nom = dto.Nom,
            Email = dto.Email,
            Telephone = dto.Telephone
        };

        await _repository.AddAsync(client);
        await _unitOfWork.SaveChangesAsync();

        return new ClientDto(client.Id, client.Nom, client.Email, client.Telephone);
    }

    public async Task<ClientDto?> GetByIdAsync(Guid id)
    {
        var client = await _repository.GetByIdAsync(id);
        return client is null ? null : new ClientDto(client.Id, client.Nom, client.Email, client.Telephone);
    }

    public async Task<IReadOnlyList<ClientDto>> GetAllAsync()
    {
        var clients = await _repository.GetAllAsync();
        return clients.Select(c => new ClientDto(c.Id, c.Nom, c.Email, c.Telephone)).ToList();
    }
}

public record ClientDto(Guid Id, string Nom, string Email, string Telephone);
public record CreateClientDto(string Nom, string Email, string Telephone);
```

### 7.5 Creer le controleur API

Dans `src/api/RVR.Framework.Api/Controllers/` :

```csharp
namespace RVR.Framework.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ClientsController : ControllerBase
{
    private readonly IClientService _clientService;

    public ClientsController(IClientService clientService)
    {
        _clientService = clientService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var clients = await _clientService.GetAllAsync();
        return Ok(clients);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var client = await _clientService.GetByIdAsync(id);
        return client is null ? NotFound() : Ok(client);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateClientDto dto)
    {
        var client = await _clientService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = client.Id }, client);
    }
}
```

### 7.6 Enregistrer les services

Dans `Program.cs`, ajoutez :

```csharp
builder.Services.AddScoped<IClientRepository, ClientRepository>();
builder.Services.AddScoped<IClientService, ClientService>();
```

### 7.7 Ajouter la migration

```bash
dotnet ef migrations add AddClientEntity \
  --project src/core/RVR.Framework.Infrastructure \
  --startup-project src/api/RVR.Framework.Api

dotnet ef database update \
  --project src/core/RVR.Framework.Infrastructure \
  --startup-project src/api/RVR.Framework.Api
```

---

## Etape 8 : Activer les modules optionnels

RIVORA est modulaire. Activez les modules selon vos besoins dans `Program.cs` :

```csharp
// Multi-tenancy
builder.Services.AddRvrModule<MultiTenancyModule>(builder.Configuration);

// Cache L1 (Memory) + L2 (Redis)
builder.Services.AddRvrModule<CachingModule>(builder.Configuration);

// Export PDF/Excel/CSV
builder.Services.AddRvrModule<ExportModule>(builder.Configuration);

// Webhooks
builder.Services.AddRvrModule<WebhooksModule>(builder.Configuration);

// GraphQL
builder.Services.AddRvrModule<GraphQLModule>(builder.Configuration);

// AI & RAG
builder.Services.AddRvrModule<AIModule>(builder.Configuration);
```

Chaque module a sa propre section de configuration dans `appsettings.json`. Consultez la [documentation des modules](/modules/) pour les details.

---

## Etape 9 : Tests

### Lancer les tests existants

```bash
dotnet test
```

### Ajouter vos propres tests

Creez un test unitaire dans `tests/` :

```csharp
namespace RVR.Framework.Application.Tests;

public class ClientServiceTests
{
    [Fact]
    public async Task CreateAsync_ShouldReturnClientDto()
    {
        // Arrange
        var mockRepo = new Mock<IClientRepository>();
        var mockUow = new Mock<IUnitOfWork>();
        var service = new ClientService(mockRepo.Object, mockUow.Object);

        var dto = new CreateClientDto("Dupont", "dupont@email.com", "+33612345678");

        // Act
        var result = await service.CreateAsync(dto);

        // Assert
        Assert.Equal("Dupont", result.Nom);
        Assert.Equal("dupont@email.com", result.Email);
        mockRepo.Verify(r => r.AddAsync(It.IsAny<Client>(), default), Times.Once);
        mockUow.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }
}
```

---

## Etape 10 : Preparer la production

### Checklist avant deploiement

- [ ] Configurer les secrets via variables d'environnement ou coffre-fort (pas User Secrets en prod)
- [ ] Changer la `JwtSettings:SecretKey` (minimum 32 caracteres, unique)
- [ ] Mettre `EnableSensitiveDataLogging` a `false`
- [ ] Mettre `EnableDetailedErrors` a `false`
- [ ] Configurer CORS avec les origines specifiques
- [ ] Configurer HTTPS
- [ ] Activer le rate limiting adapte a la charge
- [ ] Configurer le logging centralise (Seq, Elasticsearch, Azure Monitor)
- [ ] Configurer les health checks pour le load balancer
- [ ] Generer le script SQL de migration (`dotnet ef migrations script`)

### Variables d'environnement en production

```bash
# La convention .NET : double underscore pour les sections imbriquees
export ConnectionStrings__DefaultConnection="Server=prod-server;Database=MonProjetDb;..."
export JwtSettings__SecretKey="CleDeProductionTresLongueEtSecurisee!"
export ASPNETCORE_ENVIRONMENT="Production"
```

### Docker

```bash
docker compose -f docker-compose.dev.yml up -d
```

---

## Resume de la structure

```
MonProjet/
  src/
    api/RVR.Framework.Api/              # Point d'entree, controllers
      Controllers/                      # Vos controllers API
      Program.cs                        # Configuration et DI
      appsettings.json                  # Config (sans secrets !)
    core/
      RVR.Framework.Domain/             # Entites, interfaces repositories
        Entities/                       # Vos entites metier
        Repositories/                   # Interfaces des repositories
      RVR.Framework.Application/        # Services, DTOs, validators
        Services/                       # Logique metier
        Validators/                     # FluentValidation
      RVR.Framework.Infrastructure/     # EF Core, implementations
        Repositories/                   # Implementations des repositories
        Data/Migrations/                # Migrations EF Core
  tests/                                # Tests unitaires et integration
  tools/                                # CLI et Studio
```

## Etape suivante

- [Architecture](/guide/architecture) pour comprendre les couches en detail
- [Securite](/guide/security) pour configurer JWT, 2FA et le chiffrement
- [Multi-Tenancy](/guide/multi-tenancy) pour l'isolation des donnees par tenant
- [Modules](/modules/) pour la liste complete des fonctionnalites disponibles
