# KBA.CLI v2.1.0 - Studio Integration Edition

**KBA Framework CLI** - Outil en ligne de commande pour KBA.Framework 2.1.0.

Le compagnon indispensable pour le développement avec le **KBA Framework**, désormais intégré avec **KBA Studio**.

## 📦 Installation

```bash
dotnet tool install --global KBA.CLI
```

## 🚀 Nouveautés v2.1.0
- **Intégration Studio** : Support complet des commandes pour le KBA Studio.
- **Scaffolding Physique** : Amélioration de la génération de fichiers directement dans les couches Clean Architecture.
- **Enhanced Templates** : Mise à jour des templates pour le mode Monolithe et Microservices.

## 📋 Commandes disponibles

### Créer une solution complète

```bash
# Monolithe Standard
kba new MyProject --template saas-starter --tenancy row

# Microservices Cloud-Native
kba new MyCloudApp --template microservices-base
```

### Générer du code (Scaffolding)

```bash
kba generate aggregate Product Catalog
kba generate crud Order
kba generate command CreateProduct
kba generate query GetProductById
```

### Commandes AI (nécessite une clé API)

```bash
kba ai chat --provider openai "How to implement CQRS?"
kba ai generate "Create a service for sending emails"
kba ai review ./src --focus security
```

### Diagnostics et Utilitaires

```bash
kba migrate    # Appliquer les migrations EF Core
kba doctor     # Diagnostiquer la santé du projet
kba benchmark  # Load testing avec k6
kba dev        # Lancer le serveur de développement
```

## 🔗 Liens

- **Site Web**: https://khalilbenaz.github.io/KBA.Framework/
- **Repository**: https://github.com/khalilbenaz/KBA.Framework
- **Documentation**: https://github.com/khalilbenaz/KBA.Framework/docs/INDEX.md

## 🔧 Prérequis

- .NET 8 SDK
- Pour les commandes AI : clé API (OpenAI, Anthropic, etc.)

## 📄 License

MIT License
