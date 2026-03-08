# Migration Guide - KBA.Framework v1.x to v2.0.0

Ce document décrit les changements majeurs entre l'ancienne version de KBA.Framework (v1.x - architecture microservices) et la nouvelle version KBA.Framework 2.0.0.

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
| **CLI** | Aucun | `kba` CLI complet |
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
├── KBA.ApiGateway/           # Ocelot gateway
├── KBA.IdentityService/      # Service identité
├── KBA.ProductService/       # Service produits
├── KBA.OrderService/         # Service commandes
├── KBA.TenantService/        # Service tenants
├── KBA.PermissionService/    # Service permissions
├── KBA.Framework.Domain/     # Domain partagé
├── KBA.Framework.Grpc/       # Protos gRPC
├── docs/                     # 30+ fichiers docs
└── docker-compose.yml        # 10+ services
```

### v2.0.0 (Nouveau)

```
src/
├── KBA.Framework.Core/              # Entités de base
├── KBA.Framework.Application/       # CQRS, Mediator
├── KBA.Framework.Domain/            # Domain (existant)
├── KBA.Framework.Infrastructure/    # Infrastructure (existant)
├── KBA.Framework.Api/               # API (existant)
├── KBA.Framework.MultiTenancy/      # Multi-tenancy
├── KBA.Framework.AI/                # AI-Native (8+ providers)
├── KBA.Framework.Data.* /           # Database providers
├── KBA.Framework.Jobs.* /           # Job scheduling
├── KBA.Framework.Features/          # Feature Flags
├── KBA.Framework.Security/          # Security enhancements
├── KBA.Framework.HealthChecks/      # Health checks
├── KBA.Framework.Caching/           # Response caching
└── KBA.Framework.ApiVersioning/     # API versioning

samples/
├── saas-starter/              # Sample SaaS complet
├── microservices-demo/        # Sample microservices léger
└── ai-rag-app/                # Sample AI-RAG

tools/
└── KBA.CLI/                   # CLI tool (kba)

docs/
├── modules/                   # Documentation des modules
└── cli/                       # Documentation CLI
```

---

## 📦 Modules et fonctionnalités

### Nouveautés v2.0.0

| Module | Description | Migration |
|--------|-------------|-----------|
| **KBA.Framework.AI** | IChatClient avec 8+ providers (OpenAI, Claude, etc.) | Nouveau |
| **KBA.Framework.Data.Abstractions** | Abstraction multi-database | Nouveau |
| **KBA.Framework.Data.SqlServer** | Provider SQL Server | Nouveau |
| **KBA.Framework.Data.PostgreSQL** | Provider PostgreSQL | Nouveau |
| **KBA.Framework.Data.MySQL** | Provider MySQL | Nouveau |
| **KBA.Framework.Data.SQLite** | Provider SQLite | Nouveau |
| **KBA.Framework.Jobs.Abstractions** | Interface unifiée jobs | Nouveau |
| **KBA.Framework.Jobs.Hangfire** | Implementation Hangfire | Nouveau |
| **KBA.Framework.Jobs.Quartz** | Implementation Quartz | Nouveau |
| **KBA.Framework.Features** | Feature Flags avec hot-reload | Nouveau |
| **KBA.Framework.Security** | 2FA, RBAC, RateLimiting, Audit | Nouveau |
| **KBA.Framework.HealthChecks** | 80+ health checks | Nouveau |
| **KBA.Framework.Caching** | Cache avec invalidation par tag | Nouveau |
| **KBA.Framework.ApiVersioning** | Versioning API | Nouveau |
| **KBA.CLI** | CLI tool (kba) | Nouveau |

### Modules conservés

| Module | Changements |
|--------|-------------|
| **KBA.Framework.Domain** | Nettoyé, simplifié |
| **KBA.Framework.Infrastructure** | Ajout EF Core 8, nouvelles features |
| **KBA.Framework.Api** | Ajout OpenTelemetry, HealthChecks |
| **KBA.Framework.Application** | Ajout CQRS, MediatR |
| **KBA.Framework.MultiTenancy** | Étendu avec 3 modes |

### Modules supprimés

| Module | Raison | Alternative |
|--------|--------|-------------|
| **KBA.ApiGateway** | Trop complexe | YARP dans samples/microservices-demo |
| **KBA.IdentityService** | Fusionné | KBA.Framework.Security |
| **KBA.ProductService** | Exemple | samples/saas-starter |
| **KBA.OrderService** | Exemple | samples/saas-starter |
| **KBA.TenantService** | Exemple | samples/saas-starter |
| **KBA.PermissionService** | Exemple | samples/saas-starter |
| **KBA.Framework.Grpc** | Trop spécifique | samples/microservices-demo |

---

## 🔧 Guide de migration

### Étape 1: Sauvegarder l'existant

```bash
# Sauvegarder votre code actuel
git clone https://github.com/khalilbenaz/KBA.Framework.git kba-backup
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
| `KBA.IdentityService` | `KBA.Framework.Security` + `samples/saas-starter` |
| `KBA.ProductService` | `samples/saas-starter/src/KBA.SaaS.Starter.Domain/Entities/Product.cs` |
| `KBA.OrderService` | `samples/saas-starter/src/KBA.SaaS.Starter.Domain/Entities/Order.cs` |
| `KBA.TenantService` | `KBA.Framework.MultiTenancy` |
| `KBA.PermissionService` | `KBA.Framework.Security` (RBAC) |
| `KBA.ApiGateway` | `samples/microservices-demo/src/KBA.Microservices.ApiGateway` |
| `KBA.Framework.Grpc` | `samples/microservices-demo/src/KBA.Microservices.Shared.Grpc` |

### Étape 4: Migrer le code

#### Authentification

**Avant (v1.x):**
```csharp
// KBA.IdentityService/Controllers/AuthController.cs
[HttpPost("login")]
public async Task<IActionResult> Login(LoginDto dto) {
    var user = await _userService.Validate(dto.Username, dto.Password);
    var token = _jwtService.GenerateToken(user);
    return Ok(new { token });
}
```

**Après (v2.0.0):**
```csharp
// KBA.Framework.Security + samples/saas-starter
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
// KBA.TenantService
var tenant = await _tenantService.GetById(tenantId);
var connectionStrings = tenant.ConnectionStrings;
```

**Après (v2.0.0):**
```csharp
// KBA.Framework.MultiTenancy
services.AddKbaDbContextWithAutoDetection<MyDbContext>(Configuration, options =>
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
dotnet ef database update --project src/KBA.Framework.Infrastructure
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
| **CLI** | Aucun | `kba` complet |
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
**Auteur:** KBA.Framework Team
