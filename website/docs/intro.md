# Introduction

Bienvenue dans la documentation officielle de **KBA Framework**.

KBA Framework est un framework d'entreprise complet pour .NET 8, conçu pour accélérer le développement d'applications SaaS professionnelles. Il repose sur les principes de la **Clean Architecture** et du **Domain-Driven Design (DDD)**.

## Pourquoi KBA Framework ?

Développer des applications d'entreprise robustes nécessite souvent de réimplémenter les mêmes concepts (authentification, audit, multi-tenancy, validation). KBA Framework fournit ces fondations prêtes à l'emploi.

## Fonctionnalités Clés

- **Clean Architecture** : Séparation stricte des responsabilités.
- **Multi-Tenancy** : Isolation native des données.
- **Sécurité** : JWT, RBAC, 2FA, Rate Limiting.
- **Audit Logging** : Traçabilité automatique des modifications.
- **KBA Studio** : Outils visuels comme le **Visual Entity Builder**.

## Démarrer en 5 minutes

Le moyen le plus rapide de démarrer est d'utiliser notre guide de [Démarrage Rapide](./getting-started/quick-start.md).

```bash
dotnet tool install -g KBA.CLI
kba new MyProject --template saas-starter
```

## Structure de la Documentation

- **Architecture** : Comprendre les couches du framework.
- **Modules** : Détails sur le Caching, les Jobs, la Sécurité, etc.
- **CLI** : Guide complet sur les commandes `kba`.
- **KBA Studio** : Utilisation des outils visuels.
