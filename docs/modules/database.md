# Database Providers - KBA Framework

Support multi-bases de données pour KBA Framework avec Entity Framework Core 8.

## Table des Matières

- [Vue d'ensemble](#vue-densemble)
- [SqlServer](#sqlserver)
- [PostgreSQL](#postgresql)
- [MySQL](#mysql)
- [SQLite](#sqlite)
- [Configuration](#configuration)
- [Migration](#migration)

---

## Vue d'ensemble

KBA Framework supporte 4 providers de base de données :

| Provider | Package | Use Case |
|----------|---------|----------|
| **SqlServer** | `KBA.Framework.Data.SqlServer` | Production enterprise |
| **PostgreSQL** | `KBA.Framework.Data.PostgreSQL` | Open source production |
| **MySQL** | `KBA.Framework.Data.MySQL` | Web applications |
| **SQLite** | `KBA.Framework.Data.SQLite` | Development/Testing |

---

## SqlServer

### Installation

```bash
dotnet add package KBA.Framework.Data.SqlServer
```

### Configuration

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=MyApp;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "DatabaseSettings": {
    "Provider": "SqlServer",
    "EnableRetryOnFailure": true,
    "MaxRetryCount": 3,
    "CommandTimeout": 30
  }
}
```

### Usage

```csharp
using KBA.Framework.Data.SqlServer;

// Dans Program.cs
builder.Services.AddKbaSqlServer(builder.Configuration);

// Ou avec connection string explicite
builder.Services.AddKbaSqlServer(
    builder.Configuration.GetConnectionString("DefaultConnection"),
    options =>
    {
        options.EnableRetryOnFailure = true;
        options.MaxRetryCount = 3;
        options.CommandTimeout = 30;
    });
```

### DbContext

```csharp
public class AppDbContext : SqlServerDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Configurations métier
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
```

---

## PostgreSQL

### Installation

```bash
dotnet add package KBA.Framework.Data.PostgreSQL
```

### Configuration

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=MyApp;Username=postgres;Password=secret"
  },
  "DatabaseSettings": {
    "Provider": "PostgreSQL",
    "EnableRetryOnFailure": true,
    "MaxRetryCount": 3,
    "CommandTimeout": 30
  }
}
```

### Usage

```csharp
using KBA.Framework.Data.PostgreSQL;

// Dans Program.cs
builder.Services.AddKbaPostgreSQL(builder.Configuration);

// Ou avec connection string explicite
builder.Services.AddKbaPostgreSQL(
    builder.Configuration.GetConnectionString("DefaultConnection"),
    options =>
    {
        options.EnableRetryOnFailure = true;
        options.MaxRetryCount = 3;
    });
```

### DbContext

```csharp
public class AppDbContext : PostgreSQLDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }
}
```

---

## MySQL

### Installation

```bash
dotnet add package KBA.Framework.Data.MySQL
```

### Configuration

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=MyApp;User=root;Password=secret;"
  },
  "DatabaseSettings": {
    "Provider": "MySQL",
    "EnableRetryOnFailure": true,
    "MaxRetryCount": 3,
    "CommandTimeout": 30
  }
}
```

### Usage

```csharp
using KBA.Framework.Data.MySQL;

// Dans Program.cs
builder.Services.AddKbaMySql(builder.Configuration);
```

### DbContext

```csharp
public class AppDbContext : MySqlDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }
}
```

---

## SQLite

### Installation

```bash
dotnet add package KBA.Framework.Data.SQLite
```

### Configuration

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=app.db"
  },
  "DatabaseSettings": {
    "Provider": "SQLite",
    "CommandTimeout": 30
  }
}
```

### Usage

```csharp
using KBA.Framework.Data.SQLite;

// Dans Program.cs
builder.Services.AddKbaSqlite(builder.Configuration);
```

### DbContext

```csharp
public class AppDbContext : SqliteDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }
}
```

---

## Configuration

### appsettings.json complet

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=MyApp;Trusted_Connection=True;",
    "ReadOnlyConnection": "Server=localhost;Database=MyApp;ApplicationIntent=ReadOnly;"
  },
  "DatabaseSettings": {
    "Provider": "SqlServer",
    "EnableRetryOnFailure": true,
    "MaxRetryCount": 3,
    "MaxRetryDelay": 30,
    "CommandTimeout": 30,
    "EnableSensitiveDataLogging": false,
    "EnableDetailedErrors": false
  }
}
```

### Options de configuration

| Option | Type | Default | Description |
|--------|------|---------|-------------|
| `Provider` | string | SqlServer | Provider de base de données |
| `EnableRetryOnFailure` | bool | true | Retry sur erreurs transitoires |
| `MaxRetryCount` | int | 3 | Nombre maximum de retries |
| `MaxRetryDelay` | int | 30 | Délai maximum entre retries (secondes) |
| `CommandTimeout` | int | 30 | Timeout des commandes (secondes) |
| `EnableSensitiveDataLogging` | bool | false | Logging des données sensibles |
| `EnableDetailedErrors` | bool | false | Détails des erreurs EF Core |

---

## Migration

### Créer une migration

```bash
# SqlServer
dotnet ef migrations add InitialCreate --project src/KBA.Framework.Infrastructure --startup-project src/KBA.Framework.Api

# Spécifier le provider
dotnet ef migrations add InitialCreate \
  --project src/KBA.Framework.Infrastructure \
  --startup-project src/KBA.Framework.Api \
  -- --provider SqlServer
```

### Appliquer les migrations

```bash
# Appliquer toutes les migrations
dotnet ef database update --project src/KBA.Framework.Infrastructure --startup-project src/KBA.Framework.Api

# Appliquer une migration spécifique
dotnet ef database update InitialCreate --project src/KBA.Framework.Infrastructure --startup-project src/KBA.Framework.Api
```

### Script SQL

```bash
# Générer le script SQL
dotnet ef migrations script --project src/KBA.Framework.Infrastructure --startup-project src/KBA.Framework.Api --output migrations.sql
```

---

## Multi-Tenancy

KBA Framework supporte le multi-tenancy avec isolation des données :

```csharp
// Dans votre DbContext
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);
    
    // Filtrage global par TenantId
    foreach (var entityType in modelBuilder.Model.GetEntityTypes())
    {
        if (typeof(IMultiTenant).IsAssignableFrom(entityType.ClrType))
        {
            var parameter = Expression.Parameter(entityType.ClrType, "e");
            var property = Expression.Property(parameter, nameof(IMultiTenant.TenantId));
            var currentTenantId = Expression.Constant(CurrentTenantId);
            var equality = Expression.Equal(property, currentTenantId);
            var lambda = Expression.Lambda(equality, parameter);
            
            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
        }
    }
}
```

---

## Performance

### Bonnes pratiques

```csharp
// Utiliser AsNoTracking pour les lectures
var products = await context.Products
    .AsNoTracking()
    .ToListAsync();

// Split queries pour éviter les cartesian explosions
var order = await context.Orders
    .Include(o => o.Items)
    .Include(o => o.Customer)
    .AsSplitQuery()
    .FirstOrDefaultAsync();

// Projection sélective
var productNames = await context.Products
    .Select(p => new { p.Id, p.Name })
    .ToListAsync();

// Batch operations
await context.Products
    .Where(p => p.IsActive)
    .ExecuteUpdateAsync(setters => setters
        .SetProperty(p => p.LastModified, DateTime.UtcNow));
```

---

## Voir aussi

- [Health Checks](health-checks.md) - Monitoring des bases de données
- [Caching](caching.md) - Cache des requêtes database
- [Multi-Tenancy](../../docs/TENANTID_IMPLEMENTATION.md) - Isolation des données
