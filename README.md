# KBA Framework

![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet&style=flat-square)
![License](https://img.shields.io/badge/license-MIT-green?style=flat-square)
![Build](https://img.shields.io/badge/build-passing-brightgreen?style=flat-square)
![Coverage](https://img.shields.io/badge/coverage-85%25-success?style=flat-square)
![Version](https://img.shields.io/badge/version-2.1.0-blue?style=flat-square)

**Framework d'entreprise .NET 8 basé sur Clean Architecture, DDD et multi-tenancy pour applications SaaS professionnelles.**

---

## 📋 Table des Matières

- [Démarrage Rapide](#-démarrage-rapide)
- [Architecture](#-architecture)
- [Features Waves 1-5](#-features-waves-1-5)
- [KBA Studio (UI Tools)](#-kba-studio-ui-tools)
- [Modules](#-modules)
- [CLI KBA](#-cli-kba)
- [Documentation](#-documentation)
- [Contributing](#-contributing)
- [License](#-license)

---

## 🚀 Démarrage Rapide

### Prérequis

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server, PostgreSQL, MySQL ou SQLite
- Git
- (Optionnel) Redis pour le caching distribué

### Installation Standard

```bash
# 1. Cloner le repository
git clone https://github.com/khalilbenaz/KBA.Framework.git
cd KBA.Framework

# 2. Restaurer les packages
dotnet restore

# 3. Configurer la connexion database
# Éditer src/KBA.Framework.Api/appsettings.json

# 4. Créer la base de données
dotnet ef database update --project src/KBA.Framework.Infrastructure --startup-project src/KBA.Framework.Api

# 5. Lancer l'API
dotnet run --project src/KBA.Framework.Api
```

### Installation avec KBA.CLI

```bash
# Installer le CLI globalement
dotnet tool install -g KBA.CLI

# Créer un nouveau projet
kba new MyProject --template saas-starter --tenancy row

# Démarrer le serveur de développement
kba dev
```

### Premier Endpoint

```bash
# Créer le premier administrateur
curl -X POST http://localhost:5220/api/init/first-admin \
  -H "Content-Type: application/json" \
  -d '{
    "userName": "admin",
    "email": "admin@kba-framework.com",
    "password": "Admin@123456",
    "firstName": "Admin",
    "lastName": "System"
  }'

# S'authentifier
curl -X POST http://localhost:5220/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "userName": "admin",
    "password": "Admin@123456"
  }'
```

📖 **Guide complet** → [docs/quickstart.md](docs/quickstart.md)

---

## 🛠️ KBA Studio (UI Tools)

KBA Studio est une interface web de gestion pour les développeurs utilisant le framework.

### Visual Entity Builder (Full-Stack)

Le **Visual Entity Builder** permet de concevoir vos entités de domaine graphiquement et de générer l'intégralité du code source nécessaire :

- **Domain** : Entité riche (AggregateRoot), support Audit & Multitenant.
- **Infrastructure** : Configuration EF Core (Fluent API).
- **Application** : DTOs de requête et réponse.
- **API** : Controller REST prêt à l'emploi.

🚀 **Nouveau** : Génération **physique** des fichiers directement dans votre solution `src/` en un clic.

---

## 🏗️ Architecture

### Clean Architecture Layers

```
┌─────────────────────────────────────────────────────────────────┐
│                      PRESENTATION                                │
│                (KBA.Framework.Api)                               │
│         Controllers • DTOs • Middleware • Swagger                │
│         • API Versioning • Feature Gates • Rate Limiting         │
└────────────────────────────┬────────────────────────────────────┘
                             │ dépend de
┌────────────────────────────▼────────────────────────────────────┐
│                     APPLICATION                                  │
│            (KBA.Framework.Application)                           │
│    Services • DTOs • Interfaces • Validators • Mappings          │
│    • CQRS Handlers • Feature Management • Audit                  │
└────────────────────────────┬────────────────────────────────────┘
                             │ dépend de
┌────────────────────────────▼────────────────────────────────────┐
│                    INFRASTRUCTURE                                │
│          (KBA.Framework.Infrastructure)                          │
│   DbContext • Repositories • Configurations • Migrations         │
│   • Health Checks • Jobs (Hangfire/Quartz) • Caching            │
└────────────────────────────┬────────────────────────────────────┘
                             │ dépend de
┌────────────────────────────▼────────────────────────────────────┐
│                       DOMAIN                                     │
│             (KBA.Framework.Domain)                               │
│   Entities • Value Objects • Events • Repositories (I)           │
│   • Aggregates • Specifications • Multi-Tenancy                 │
└─────────────────────────────────────────────────────────────────┘
```

### Module Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                         API LAYER                               │
│  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐ ┌───────────┐ │
│  │ Versioning  │ │  RateLimit  │ │  Features   │ │  Health   │ │
│  │   v1/v2/v3  │ │  Middleware │ │   Gates     │ │  Checks   │ │
│  └─────────────┘ └─────────────┘ └─────────────┘ └───────────┘ │
└─────────────────────────────────────────────────────────────────┘
                              │
        ┌─────────────────────┼─────────────────────┐
        │                     │                     │
┌───────▼────────┐   ┌───────▼────────┐   ┌───────▼────────┐
│   SECURITY     │   │    CACHING     │   │     JOBS       │
│  ┌──────────┐  │   │  ┌──────────┐  │   │  ┌──────────┐  │
│  │   2FA    │  │   │  │  Memory  │  │   │  │ Hangfire │  │
│  │   RBAC   │  │   │  │  Redis   │  │   │  │  Quartz  │  │
│  │  Audit   │  │   │  │  Tags    │  │   │  │ Scheduler│  │
│  │RateLimit │  │   │  │Invalidation│ │   │  └──────────┘  │
│  └──────────┘  │   │  └──────────┘  │   └────────────────┘
└────────────────┘   └────────────────┘
        │                     │                     │
        └─────────────────────┼─────────────────────┘
                              │
        ┌─────────────────────┼─────────────────────┐
        │                     │                     │
┌───────▼────────┐   ┌───────▼────────┐   ┌───────▼────────┐
│  DATA LAYER    │   │    DOMAIN      │   │   FEATURES     │
│ ┌────────────┐ │   │ ┌────────────┐ │   │ ┌────────────┐ │
│ │  SqlServer │ │   │ │  Entities  │ │   │ │   Config   │ │
│ │ PostgreSQL │ │   │ │   Value    │ │   │ │  Database  │ │
│ │    MySQL   │ │   │ │  Objects   │ │   │ │   Azure    │ │
│ │   SQLite   │ │   │ │ Aggregates │ │   │ │  Providers │ │
│ └────────────┘ │   │ └────────────┘ │   │ └────────────┘ │
└────────────────┘   └────────────────┘   └────────────────┘
```

### Project Structure

```
KBA.Framework/
├── src/
│   ├── KBA.Framework.Domain/          # Entités métier, DDD
│   ├── KBA.Framework.Application/     # Services, DTOs, Validators
│   ├── KBA.Framework.Infrastructure/  # EF Core, Repositories
│   ├── KBA.Framework.Api/             # API REST, Controllers
│   ├── KBA.Framework.Security/        # 2FA, RBAC, RateLimiting, Audit
│   ├── KBA.Framework.Caching/         # Memory/Redis caching
│   ├── KBA.Framework.Features/        # Feature Flags
│   ├── KBA.Framework.HealthChecks/    # Health monitoring
│   ├── KBA.Framework.ApiVersioning/   # API versioning
│   ├── KBA.Framework.Jobs.*           # Hangfire, Quartz
│   └── KBA.Framework.Data.*           # SqlServer, PostgreSQL, MySQL, SQLite
├── tests/
│   ├── KBA.Framework.Domain.Tests/
│   ├── KBA.Framework.Application.Tests/
│   └── KBA.Framework.Api.IntegrationTests/
├── tools/
│   └── KBA.CLI/                       # CLI scaffolding & AI
├── docs/                              # Documentation
│   ├── modules/                       # Module docs
│   └── cli/                           # CLI docs
├── ops/                               # DevOps, monitoring
└── infra/                             # Infrastructure as Code
```

---

## ✨ Features Waves 1-5

### 🌊 Wave 1 - Documentation & Foundation

| Feature | Description | Status |
|---------|-------------|--------|
| **Clean Architecture** | Séparation stricte Domain/Application/Infrastructure/Api | ✅ |
| **Domain-Driven Design** | Entités riches, value objects, domain events, aggregates | ✅ |
| **Repository Pattern** | Abstraction complète de la couche de données | ✅ |
| **Dependency Injection** | Injection native .NET 8 avec lifetimes configurés | ✅ |
| **Multi-Tenancy** | Isolation des données par tenant (TenantId) | ✅ |
| **JWT Authentication** | Tokens HMAC-SHA256 avec refresh tokens | ✅ |
| **Authorization** | Rôles, permissions, claims personnalisées | ✅ |
| **Audit Logging** | Traçabilité automatique de toutes les opérations | ✅ |
| **Entity Framework Core 8** | ORM moderne avec configurations fluent | ✅ |
| **FluentValidation** | Validation robuste avec règles métier | ✅ |
| **Serilog** | Logging structuré avec rotation de fichiers | ✅ |
| **Swagger/OpenAPI** | Documentation interactive | ✅ |
| **Docker** | Containerisation prête | ✅ |
| **Health Checks** | Endpoints de monitoring | ✅ |

### 🌊 Wave 2 - Advanced Features

| Feature | Description | Status |
|---------|-------------|--------|
| **Feature Flags** | Config, Database, Azure App Configuration providers | ✅ |
| **Feature Gates** | Attribute-based feature toggling | ✅ |
| **Feature Dashboard** | UI de gestion des features | ✅ |
| **Caching** | Memory et Redis avec tags invalidation | ✅ |
| **Response Caching** | Middleware de cache HTTP | ✅ |
| **API Versioning** | URL path, header, query string, media type | ✅ |
| **Health Checks Extended** | 80+ checks (DB, Redis, RabbitMQ, AI, etc.) | ✅ |
| **Health UI** | Dashboard de monitoring | ✅ |

### 🌊 Wave 3 - Jobs & Background Processing

| Feature | Description | Status |
|---------|-------------|--------|
| **Hangfire Integration** | Jobs background avec dashboard | ✅ |
| **Quartz.NET Integration** | Scheduling avancé avec cron | ✅ |
| **Job Abstractions** | Interface commune IJobScheduler | ✅ |
| **Job Monitoring** | Health checks pour jobs | ✅ |
| **Retry Strategy** | Retry automatique avec backoff | ✅ |
| **Job Scheduling** | API unifiée pour scheduler jobs | ✅ |

### 🌊 Wave 4 - Security Enhancements

| Feature | Description | Status |
|---------|-------------|--------|
| **2FA/TOTP** | Authentication à deux facteurs | ✅ |
| **QR Code Generation** | Setup 2FA avec QR codes | ✅ |
| **Backup Codes** | Codes de récupération 2FA | ✅ |
| **RBAC** | Role-Based Access Control hiérarchique | ✅ |
| **Permission System** | Permissions granulaires | ✅ |
| **Rate Limiting** | Limitation de débit configurable | ✅ |
| **Audit Trail** | Interceptor EF Core pour audit | ✅ |
| **Refresh Token Cleanup** | Nettoyage automatique tokens | ✅ |

### 🌊 Wave 5 - AI & CLI

| Feature | Description | Status |
|---------|-------------|--------|
| **KBA.CLI** | CLI de scaffolding et génération | ✅ |
| **kba new** | Création de projets templates | ✅ |
| **kba generate** | Génération de code (aggregate, CRUD, CQRS) | ✅ |
| **kba ai chat** | Chat interactif avec LLM (OpenAI/Claude) | ✅ |
| **kba ai generate** | Génération de code avec AI | ✅ |
| **kba ai review** | Review de code avec AI | ✅ |
| **kba benchmark** | Load testing avec k6 | ✅ |
| **kba doctor** | Diagnostic de projet | ✅ |
| **kba add-module** | Ajout de modules complets | ✅ |

---

## 📦 Modules

### Database Providers

| Module | Package | Description |
|--------|---------|-------------|
| **SqlServer** | `KBA.Framework.Data.SqlServer` | Support SQL Server avec migrations EF Core |
| **PostgreSQL** | `KBA.Framework.Data.PostgreSQL` | Support PostgreSQL avec Npgsql |
| **MySQL** | `KBA.Framework.Data.MySQL` | Support MySQL avec Pomelo |
| **SQLite** | `KBA.Framework.Data.SQLite` | Support SQLite pour dev/testing |

📖 **Documentation** → [docs/modules/database.md](docs/modules/database.md)

### Jobs & Background Processing

| Module | Package | Description |
|--------|---------|-------------|
| **Jobs.Abstractions** | `KBA.Framework.Jobs.Abstractions` | Interfaces et modèles communs |
| **Jobs.Hangfire** | `KBA.Framework.Jobs.Hangfire` | Implementation avec Hangfire |
| **Jobs.Quartz** | `KBA.Framework.Jobs.Quartz` | Implementation avec Quartz.NET |

📖 **Documentation** → [docs/modules/jobs.md](docs/modules/jobs.md)

### Feature Management

| Module | Package | Description |
|--------|---------|-------------|
| **Features** | `KBA.Framework.Features` | Feature flags avec multiples providers |

📖 **Documentation** → [docs/modules/features.md](docs/modules/features.md)

### Security

| Module | Package | Description |
|--------|---------|-------------|
| **Security** | `KBA.Framework.Security` | 2FA, RBAC, RateLimiting, Audit |

📖 **Documentation** → [docs/modules/security.md](docs/modules/security.md)

### Infrastructure

| Module | Package | Description |
|--------|---------|-------------|
| **HealthChecks** | `KBA.Framework.HealthChecks` | 80+ health checks |
| **Caching** | `KBA.Framework.Caching` | Memory/Redis caching |
| **ApiVersioning** | `KBA.Framework.ApiVersioning` | API versioning strategies |

📖 **Documentation** → [docs/modules/health-checks.md](docs/modules/health-checks.md)  
📖 **Documentation** → [docs/modules/caching.md](docs/modules/caching.md)  
📖 **Documentation** → [docs/modules/api-versioning.md](docs/modules/api-versioning.md)

### AI Native

| Module | Package | Description |
|--------|---------|-------------|
| **KBA.CLI** | `tools/KBA.CLI` | CLI avec commandes AI |

📖 **Documentation** → [docs/modules/ai-native.md](docs/modules/ai-native.md)

---

## 🛠️ CLI KBA

Le CLI KBA fournit des commandes de scaffolding et d'assistance AI.

### Installation

```bash
dotnet tool install -g KBA.CLI
```

### Commandes Principales

| Commande | Alias | Description |
|----------|-------|-------------|
| `kba new <name>` | - | Créer un nouveau projet |
| `kba generate` | `gen`, `g` | Générer du code |
| `kba ai` | - | Commandes AI |
| `kba add-module` | - | Ajouter un module |
| `kba benchmark` | - | Load testing |
| `kba doctor` | - | Diagnostic |
| `kba dev` | - | Serveur de développement |
| `kba migrate` | - | Migrations database |
| `kba seed` | - | Seed database |

📖 **Documentation complète** → [docs/cli/](docs/cli/)

#### Exemples

```bash
# Créer un nouveau projet
kba new MyProject --template saas-starter --tenancy row

# Générer un aggregate
kba generate aggregate Product Catalog

# Générer CRUD
kba generate crud User --props "Name:string,Email:string,Age:int"

# Chat AI
kba ai chat --provider openai --model gpt-4o

# Générer code avec AI
kba ai generate "Create a repository pattern for Product entity" --output ProductRepository.cs

# Review de code
kba ai review src/MyProject/Controllers --focus security

# Load testing
kba benchmark http://localhost:5000/api/products --duration 1m --vus 50

# Diagnostic
kba doctor
```

---

## 📚 Documentation

### Guides Principaux

| Document | Description |
|----------|-------------|
| [Quickstart](docs/quickstart.md) | Installation et premier endpoint en 5 minutes |
| [Guide Pas-à-Pas](docs/TUTORIAL-STEP-BY-STEP.md) | Créez votre premier module avec KBA Studio (10 min) |
| [Guide Complet](docs/GUIDE-COMPLET.md) | Documentation approfondie du framework |
| [Initialization](docs/INITIALIZATION-GUIDE.md) | Configuration initiale et premier admin |

### Documentation Modules

| Module | Lien |
|--------|------|
| Database | [docs/modules/database.md](docs/modules/database.md) |
| Jobs | [docs/modules/jobs.md](docs/modules/jobs.md) |
| Features | [docs/modules/features.md](docs/modules/features.md) |
| Security | [docs/modules/security.md](docs/modules/security.md) |
| Health Checks | [docs/modules/health-checks.md](docs/modules/health-checks.md) |
| Caching | [docs/modules/caching.md](docs/modules/caching.md) |
| API Versioning | [docs/modules/api-versioning.md](docs/modules/api-versioning.md) |
| AI Native | [docs/modules/ai-native.md](docs/modules/ai-native.md) |

### Documentation CLI

| Commande | Lien |
|----------|------|
| kba new | [docs/cli/kba-new.md](docs/cli/kba-new.md) |
| kba generate | [docs/cli/kba-generate.md](docs/cli/kba-generate.md) |
| kba doctor | [docs/cli/kba-doctor.md](docs/cli/kba-doctor.md) |
| kba ai | [docs/cli/kba-ai.md](docs/cli/kba-ai.md) |
| kba benchmark | [docs/cli/kba-benchmark.md](docs/cli/kba-benchmark.md) |

### Documentation Technique

| Document | Description |
|----------|-------------|
| [Authorization](docs/AUTHORIZATION_SUMMARY.md) | JWT, rôles et permissions |
| [Multi-Tenancy](docs/TENANTID_IMPLEMENTATION.md) | Isolation des données par tenant |
| [Improvements](docs/AMELIORATIONS_IMPLEMENTEES.md) | Optimisations et améliorations |
| [Contributing](CONTRIBUTING.md) | Comment contribuer au projet |
| [Changelog](CHANGELOG.md) | Historique des versions |

---

## 🤝 Contributing

Nous acceptons les contributions de la communauté ! Consultez notre guide :

1. Fork le repository
2. Créez une branche feature (`git checkout -b feature/amazing-feature`)
3. Committez vos changements (`git commit -m 'Add amazing feature`)
4. Push vers la branche (`git push origin feature/amazing-feature`)
5. Ouvrez une Pull Request

📖 **Guide complet** → [CONTRIBUTING.md](CONTRIBUTING.md)

### Code Style

- Suivre les [conventions C#](https://docs.microsoft.com/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- Utiliser `var` quand le type est évident
- Noms de méthodes en PascalCase, variables en camelCase
- Commentaires XML pour les API publiques

---

## 📄 License

Distribué sous la licence MIT. Voir [LICENSE](LICENSE) pour plus d'informations.

---

## 📞 Support

- 💬 Issues: [GitHub Issues](https://github.com/khalilbenaz/KBA.Framework/issues)
- 📖 Docs: [Documentation complète](docs/INDEX.md)

---

**KBA Framework** - Production-Ready Clean Architecture pour .NET 8

*Built with ❤️ using .NET 8*
