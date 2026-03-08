# KBA.CLI

**KBA Framework CLI** - Outil en ligne de commande pour KBA.Framework 2.0.1

## 📦 Installation

```bash
dotnet tool install --global KBA.CLI
```

## 🚀 Commandes disponibles

### Créer un projet

```bash
kba new MyProject --template minimal
kba new MySaaS --template saas-starter --tenancy row
```

### Générer du code

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
kba ai review ./src "Check for security issues"
```

### Ajouter un module

```bash
kba add-module Product --api --tests
```

### Load testing

```bash
kba benchmark http://localhost:5000 --scenario load
```

### Diagnostic

```bash
kba doctor
```

### Complétion shell

```bash
kba completion bash >> ~/.bashrc
kba completion zsh >> ~/.zshrc
kba completion powershell >> $PROFILE
```

## 🔗 Liens

- **Repository**: https://github.com/khalilbenaz/KBA.Framework
- **Issues**: https://github.com/khalilbenaz/KBA.Framework/issues
- **Releases**: https://github.com/khalilbenaz/KBA.Framework/releases

## 📖 Documentation

- [CLI Documentation](https://github.com/khalilbenaz/KBA.Framework/blob/main/docs/cli/kba-new.md)
- [KBA.Framework](https://github.com/khalilbenaz/KBA.Framework)

## 🔧 Prérequis

- .NET 8 SDK
- Pour les commandes AI : clé API (OpenAI, Anthropic, etc.)

## 📄 License

MIT License
