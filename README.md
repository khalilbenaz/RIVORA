# KBA Framework

![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet&style=flat-square)
![License](https://img.shields.io/badge/license-MIT-green?style=flat-square)
![Build](https://img.shields.io/badge/build-passing-brightgreen?style=flat-square)
![Coverage](https://img.shields.io/badge/coverage-85%25-success?style=flat-square)

**Framework d'entreprise .NET 8 basé sur Clean Architecture, DDD et multi-tenancy pour applications SaaS professionnelles.**

---

## 📋 Table des Matières

- [Démarrage Rapide](#-démarrage-rapide)
- [Architecture](#-architecture)
- [Features](#-features)
- [Documentation](#-documentation)
- [Contributing](#-contributing)
- [License](#-license)

---

## 🚀 Démarrage Rapide (5 minutes)

### Prérequis

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server ou LocalDB
- Git

### Installation

```bash
# 1. Cloner le repository
git clone https://github.com/khalilbenaz/KBA.Framework.git
cd KBA.Framework

# 2. Restaurer les packages
dotnet restore

# 3. Configurer la connexion (optionnel - LocalDB par défaut)
# Éditer src/KBA.Framework.Api/appsettings.json

# 4. Créer la base de données
dotnet ef database update --project src/KBA.Framework.Infrastructure --startup-project src/KBA.Framework.Api

# 5. Lancer l'API
dotnet run --project src/KBA.Framework.Api
```

### Premier endpoint

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

## 🏗️ Architecture

### Clean Architecture Layers

```
┌─────────────────────────────────────────────────────────┐
│                    PRESENTATION                          │
│              (KBA.Framework.Api)                         │
│         Controllers • DTOs • Middleware • Swagger        │
└───────────────────────┬─────────────────────────────────┘
                        │ dépend de
┌───────────────────────▼─────────────────────────────────┐
│                   APPLICATION                            │
│           (KBA.Framework.Application)                    │
│    Services • DTOs • Interfaces • Validators • Mappings  │
└───────────────────────┬─────────────────────────────────┘
                        │ dépend de
┌───────────────────────▼─────────────────────────────────┐
│                   INFRASTRUCTURE                         │
│         (KBA.Framework.Infrastructure)                   │
│   DbContext • Repositories • Configurations • Migrations │
└───────────────────────┬─────────────────────────────────┘
                        │ dépend de
┌───────────────────────▼─────────────────────────────────┐
│                      DOMAIN                               │
│            (KBA.Framework.Domain)                        │
│   Entities • Value Objects • Events • Repositories (I)   │
└─────────────────────────────────────────────────────────┘
```

### Project Structure

```
KBA.Framework/
├── src/
│   ├── KBA.Framework.Domain/          # Entités métier, DDD
│   ├── KBA.Framework.Application/     # Services, DTOs, Validators
│   ├── KBA.Framework.Infrastructure/  # EF Core, Repositories
│   └── KBA.Framework.Api/             # API REST, Controllers
├── tests/
│   ├── KBA.Framework.Domain.Tests/
│   ├── KBA.Framework.Application.Tests/
│   └── KBA.Framework.Api.IntegrationTests/
├── docs/                              # Documentation
├── ops/                               # DevOps, monitoring
└── infra/                             # Infrastructure as Code
```

---

## ✨ Features

### Core Architecture

- ✅ **Clean Architecture** - Séparation stricte des responsabilités
- ✅ **Domain-Driven Design (DDD)** - Entités riches, value objects, domain events
- ✅ **Repository Pattern** - Abstraction complète de la couche de données
- ✅ **Dependency Injection** - Injection de dépendances native .NET 8

### Multi-Tenancy & Security

- ✅ **Multi-Tenancy complet** - Isolation des données par tenant
- ✅ **JWT Authentication** - Tokens signés HMAC-SHA256 avec refresh
- ✅ **Authorization** - Rôles, permissions, claims personnalisées
- ✅ **Audit Logging** - Traçabilité automatique de toutes les opérations

### Data & Performance

- ✅ **Entity Framework Core 8** - ORM moderne avec configurations fluent
- ✅ **Optimisations EF** - AsNoTracking, split queries, retry logic
- ✅ **SQL Server** - Support natif avec migrations EF Core
- ✅ **Connection pooling** - Gestion optimisée des connexions

### Validation & Quality

- ✅ **FluentValidation** - Validation robuste avec règles métier
- ✅ **Global Error Handling** - Middleware d'exceptions centralisé
- ✅ **Response standardization** - Format de réponse cohérent
- ✅ **Development mode** - Détails complets des erreurs en dev

### Developer Experience

- ✅ **Swagger/OpenAPI** - Documentation interactive
- ✅ **ReDoc** - Documentation élégante en lecture seule
- ✅ **API Explorer** - Interface de test moderne
- ✅ **Serilog** - Logging structuré avec rotation de fichiers

### Testing

- ✅ **xUnit** - Framework de tests unitaires
- ✅ **Moq** - Bibliothèque de mocking
- ✅ **Integration tests** - Tests d'intégration complets
- ✅ **Test helpers** - Utilities pour tests isolés

### DevOps & Deployment

- ✅ **Docker** - Containerisation prête
- ✅ **IIS Deployment** - Script PowerShell automatisé
- ✅ **Health checks** - Endpoints de monitoring
- ✅ **Configuration environments** - Dev, staging, production

---

## 📚 Documentation

| Document | Description |
|----------|-------------|
| [Quickstart](docs/quickstart.md) | Installation et premier endpoint en 5 minutes |
| [Guide Complet](docs/GUIDE-COMPLET.md) | Documentation approfondie du framework |
| [Initialization](docs/INITIALIZATION-GUIDE.md) | Configuration initiale et premier admin |
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
3. Committez vos changements (`git commit -m 'Add amazing feature

Co-authored-by: Qwen-Coder <qwen-coder@alibabacloud.com>'`)
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

- 📧 Email: support@kba-framework.com
- 💬 Issues: [GitHub Issues](https://github.com/khalilbenaz/KBA.Framework/issues)
- 📖 Docs: [Documentation complète](docs/INDEX.md)

---

**KBA Framework** - Production-Ready Clean Architecture pour .NET 8

*Built with ❤️ using .NET 8*
