# Migration Guide - RVR.Framework v1.x to v2.0.0

Ce document décrit les changements majeurs entre l'ancienne version de RVR.Framework (v1.x - architecture microservices) et la nouvelle version RVR.Framework 2.0.0.

---

## 📋 Table des matières

- [Vue d'ensemble des changements](#vue-densemble-des-changements)
- [Architecture](#architecture)
- [Structure du projet](#structure-du-projet)
- [Modules et fonctionnalités](#modules-et-fonctionnalités)
- [Guide de migration](#guide-de-migration)
- [FAQ](#faq)

---

## 🎯 Vue d'ensemble des changements

| Aspect | v1.x (Ancien) | v2.0.0 (Nouveau) |
|--------|---------------|------------------|
| **Architecture** | Microservices complexes | Clean Architecture modulaire |
| **Déploiement** | Docker Compose lourd | Docker léger + samples |
| **Documentation** | 30+ fichiers dispersés | docs/ structurés (13 fichiers) |
| **Samples** | Microservices complets | 3 samples légers (SaaS, Microservices, AI-RAG) |
| **CLI** | Aucun | `rvr` CLI complet |
| **AI-Native** | Aucun | 8+ providers AI supportés |
| **Multi-Tenancy** | Basique | 3 modes (row, schema, database) |
| **Security** | JWT basique | 2FA, RBAC, RateLimiting, Audit |
| **Jobs** | Aucun | Hangfire + Quartz unifiés |
| **Feature Flags** | Aucun | Hot-reload complet |

---

## 🏗 Architecture

### v1.x - Architecture Microservices

```
┌─────────────────────────────────────────────────────────────┐
│                    API Gateway (Ocelot)                      │
└─────────────────────────────────────────────────────────────┘
         │         │              │              │
         ▼         ▼              ▼              ▼
┌─────────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐
│  Identity   │ │ Product  │ │  Order   │ │ Tenant   │
│  Service    │ │ Service  │ │ Service  │ │ Service  │
└─────────────┘ └──────────┘ └──────────┘ └──────────┘
     │               │              │             │
     └───────────────┴──────────────┴─────────────┘
                     │
                     ▼
         ┌───────────────────────┐
         │  gRPC + RabbitMQ      │
         └───────────────────────┘
```

**Problèmes:**
- Complexe à déployer (6+ services)
- Difficile à debugger en local
- Documentation éparpillée
- Courbe d'apprentissage élevée

---

### v2.0.0 - Clean Architecture Modulaire

```
┌─────────────────────────────────────────────────────────────┐
│                    Presentation Layer                        │
│         (API, Blazor, Mobile, AI Agents)                     │
└─────────────────────────────────────────────────────────────┘
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                   Application Layer                          │
│    (CQRS, Saga, Workflow, AI RAG, Feature Flags)            │
└─────────────────────────────────────────────────────────────┘
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                     Domain Layer                             │
│         (Entities, Aggregates, Domain Events)               │
└─────────────────────────────────────────────────────────────┘
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                  Infrastructure Layer                        │
│   (EF Core, Redis, RabbitMQ, AI Providers, Jobs)           │
└─────────────────────────────────────────────────────────────┘
```

**Avantages:**
- Modulaire (choisissez les modules nécessaires)
- Simple à déployer (1 seul service ou microservices)
- Documentation centralisée
- Courbe d'apprentissage réduite

---

## 📁 Structure du projet

### v1.x (Supprimé)

```
microservices/
├── RVR.ApiGateway/           # Ocelot gateway
├── RVR.IdentityService/      # Service identité
├── RVR.ProductService/       # Service produits
├── RVR.OrderService/         # Service commandes
├── RVR.TenantService/        # Service tenants
├── RVR.PermissionService/    # Service permissions
├── RVR.Framework.Domain/     # Domain partagé
├── RVR.Framework.Grpc/       # Protos gRPC
├── docs/                     # 30+ fichiers docs
└── docker-compose.yml        # 10+ services
```

### v2.0.0 (Nouveau)

```
src/
├── RVR.Framework.Core/              # Entités de base
├── RVR.Framework.Application/       # CQRS, Mediator
├── RVR.Framework.Domain/            # Domain (existant)
├── RVR.Framework.Infrastructure/    # Infrastructure (existant)
├── RVR.Framework.Api/               # API (existant)
├── RVR.Framework.MultiTenancy/      # Multi-tenancy
├── RVR.Framework.AI/                # AI-Native (8+ providers)
├── RVR.Framework.Data.* /           # Database providers
├── RVR.Framework.Jobs.* /           # Job scheduling
├── RVR.Framework.Features/          # Feature Flags
├── RVR.Framework.Security/          # Security enhancements
├── RVR.Framework.HealthChecks/      # Health checks
├── RVR.Framework.Caching/           # Response caching
└── RVR.Framework.ApiVersioning/     # API versioning

samples/
├── saas-starter/              # Sample SaaS complet
├── microservices-demo/        # Sample microservices léger
└── ai-rag-app/                # Sample AI-RAG

tools/
└── RVR.CLI/                   # CLI tool (kba)

docs/
├── modules/                   # Documentation des modules
└── cli/                       # Documentation CLI
```

---

## 📦 Modules et fonctionnalités

### Nouveautés v2.0.0

| Module | Description | Migration |
|--------|-------------|-----------|
| **RVR.Framework.AI** | IChatClient avec 8+ providers (OpenAI, Claude, etc.) | Nouveau |
| **RVR.Framework.Data.Abstractions** | Abstraction multi-database | Nouveau |
| **RVR.Framework.Data.SqlServer** | Provider SQL Server | Nouveau |
| **RVR.Framework.Data.PostgreSQL** | Provider PostgreSQL | Nouveau |
| **RVR.Framework.Data.MySQL** | Provider MySQL | Nouveau |
| **RVR.Framework.Data.SQLite** | Provider SQLite | Nouveau |
| **RVR.Framework.Jobs.Abstractions** | Interface unifiée jobs | Nouveau |
| **RVR.Framework.Jobs.Hangfire** | Implementation Hangfire | Nouveau |
| **RVR.Framework.Jobs.Quartz** | Implementation Quartz | Nouveau |
| **RVR.Framework.Features** | Feature Flags avec hot-reload | Nouveau |
| **RVR.Framework.Security** | 2FA, RBAC, RateLimiting, Audit | Nouveau |
| **RVR.Framework.HealthChecks** | 80+ health checks | Nouveau |
| **RVR.Framework.Caching** | Cache avec invalidation par tag | Nouveau |
| **RVR.Framework.ApiVersioning** | Versioning API | Nouveau |
| **RVR.CLI** | CLI tool (kba) | Nouveau |

### Modules conservés

| Module | Changements |
|--------|-------------|
| **RVR.Framework.Domain** | Nettoyé, simplifié |
| **RVR.Framework.Infrastructure** | Ajout EF Core 8, nouvelles features |
| **RVR.Framework.Api** | Ajout OpenTelemetry, HealthChecks |
| **RVR.Framework.Application** | Ajout CQRS, MediatR |
| **RVR.Framework.MultiTenancy** | Étendu avec 3 modes |

### Modules supprimés

| Module | Raison | Alternative |
|--------|--------|-------------|
| **RVR.ApiGateway** | Trop complexe | YARP dans samples/microservices-demo |
| **RVR.IdentityService** | Fusionné | RVR.Framework.Security |
| **RVR.ProductService** | Exemple | samples/saas-starter |
| **RVR.OrderService** | Exemple | samples/saas-starter |
| **RVR.TenantService** | Exemple | samples/saas-starter |
| **RVR.PermissionService** | Exemple | samples/saas-starter |
| **RVR.Framework.Grpc** | Trop spécifique | samples/microservices-demo |

---

## 🔧 Guide de migration

### Étape 1: Sauvegarder l'existant

```bash
# Sauvegarder votre code actuel
git clone https://github.com/khalilbenaz/RIVORA.git kba-backup
cd kba-backup
git checkout <votre-version>
```

### Étape 2: Identifier les dépendances

Listez les services/fonctionnalités que vous utilisez:

- [ ] Identity/Authentification
- [ ] Multi-Tenancy
- [ ] Products/Catalogue
- [ ] Orders/Commandes
- [ ] Permissions
- [ ] gRPC
- [ ] RabbitMQ

### Étape 3: Mapper vers v2.0.0

| Ancien | Nouveau |
|--------|---------|
| `RVR.IdentityService` | `RVR.Framework.Security` + `samples/saas-starter` |
| `RVR.ProductService` | `samples/saas-starter/src/RVR.SaaS.Starter.Domain/Entities/Product.cs` |
| `RVR.OrderService` | `samples/saas-starter/src/RVR.SaaS.Starter.Domain/Entities/Order.cs` |
| `RVR.TenantService` | `RVR.Framework.MultiTenancy` |
| `RVR.PermissionService` | `RVR.Framework.Security` (RBAC) |
| `RVR.ApiGateway` | `samples/microservices-demo/src/RVR.Microservices.ApiGateway` |
| `RVR.Framework.Grpc` | `samples/microservices-demo/src/RVR.Microservices.Shared.Grpc` |

### Étape 4: Migrer le code

#### Authentification

**Avant (v1.x):**
```csharp
// RVR.IdentityService/Controllers/AuthController.cs
[HttpPost("login")]
public async Task<IActionResult> Login(LoginDto dto) {
    var user = await _userService.Validate(dto.Username, dto.Password);
    var token = _jwtService.GenerateToken(user);
    return Ok(new { token });
}
```

**Après (v2.0.0):**
```csharp
// RVR.Framework.Security + samples/saas-starter
[HttpPost("login")]
public async Task<IActionResult> Login(LoginDto dto) {
    var result = await _identityService.SignInAsync(dto.Username, dto.Password);
    if (result.RequiresTwoFactor) {
        return Challenge("2FA");
    }
    return Ok(new { token = result.Token, refreshToken = result.RefreshToken });
}
```

**Nouveautés:**
- 2FA TOTP supporté
- Refresh tokens avec rotation
- Backup codes

#### Multi-Tenancy

**Avant (v1.x):**
```csharp
// RVR.TenantService
var tenant = await _tenantService.GetById(tenantId);
var connectionStrings = tenant.ConnectionStrings;
```

**Après (v2.0.0):**
```csharp
// RVR.Framework.MultiTenancy
services.AddRvrDbContextWithAutoDetection<MyDbContext>(Configuration, options =>
{
    options.AutoMigrate = true;
    options.EnableRetryOnFailure = true;
});

// 3 modes: row-level, schema-per-tenant, database-per-tenant
```

### Étape 5: Tester

```bash
# Lancer les samples
cd samples/saas-starter
docker-compose up -d

# Ou microservices
cd samples/microservices-demo
docker-compose up -d

# Tester l'API
curl http://localhost:5000/health
```

---

## ❓ FAQ

### Q: Puis-je toujours utiliser l'architecture microservices ?

**R:** Oui ! La nouvelle version inclut `samples/microservices-demo/` qui est une version simplifiée et modernisée de l'ancienne architecture.

### Q: Que faire de mon code existant ?

**R:** Vous avez 3 options:
1. **Rester sur v1.x** - Le tag `v1.0.0` reste disponible
2. **Migrer progressivement** - Commencez par les samples
3. **Hybride** - Gardez v1.x pour l'existant, utilisez v2.0.0 pour les nouveaux projets

### Q: Les anciennes fonctionnalités sont-elles toujours disponibles ?

**R:** Oui, toutes les fonctionnalités de base sont conservées et améliorées:
- ✅ JWT Authentication (amélioré avec 2FA)
- ✅ Multi-Tenancy (amélioré avec 3 modes)
- ✅ Audit Logging (amélioré avec EF Core interceptor)
- ✅ Repository Pattern (conservé)

### Q: Comment migrer ma base de données ?

**R:** Les nouvelles migrations EF Core 8 sont rétro-compatibles:
```bash
dotnet ef database update --project src/RVR.Framework.Infrastructure
```

### Q: Où est passée la documentation ?

**R:** Toute la documentation a été consolidée dans `docs/`:
- `docs/modules/` - Documentation des modules
- `docs/cli/` - Documentation CLI
- `README.md` - Documentation principale

---

## 📊 Comparaison finale

| Critère | v1.x | v2.0.0 |
|---------|------|--------|
| **Fichiers** | 200+ | 250+ (mais structurés) |
| **Documentation** | 30+ fichiers dispersés | 13 fichiers organisés |
| **Samples** | 1 (microservices lourds) | 3 (légers et focalisés) |
| **CLI** | Aucun | `rvr` complet |
| **AI** | Aucun | 8+ providers |
| **Security** | JWT basique | 2FA, RBAC, RateLimiting |
| **Jobs** | Aucun | Hangfire + Quartz |
| **Feature Flags** | Aucun | Hot-reload |
| **Health Checks** | Aucun | 80+ checks |
| **CI/CD** | Basique | GitHub Actions complet |

---

## 🔗 Liens utiles

- [README.md](../README.md) - Documentation principale
- [docs/](../docs/) - Documentation complète
- [samples/](../samples/) - Applications de référence
- [CHANGELOG.md](../CHANGELOG.md) - Historique des versions
- [CONTRIBUTING.md](../CONTRIBUTING.md) - Guide de contribution

---

**Version du guide:** 1.0  
**Date:** 2026-03-08  
**Auteur:** RVR.Framework Team
