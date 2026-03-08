# AI Native - KBA Framework

Fonctionnalités AI intégrées avec support OpenAI et Claude.

## Table des Matières

- [Vue d'ensemble](#vue-densemble)
- [KBA.CLI](#kbacli)
- [AI Chat](#ai-chat)
- [AI Code Generation](#ai-code-generation)
- [AI Code Review](#ai-code-review)
- [Configuration](#configuration)
- [Best Practices](#best-practices)

---

## Vue d'ensemble

KBA Framework inclut des fonctionnalités AI natives :

| Feature | Description | Status |
|---------|-------------|--------|
| **KBA.CLI** | CLI avec commandes AI | ✅ |
| **AI Chat** | Chat interactif avec LLM | ✅ |
| **AI Generate** | Génération de code | ✅ |
| **AI Review** | Review de code | ✅ |
| **OpenAI** | Support GPT-4, GPT-4o | ✅ |
| **Claude** | Support Claude 3 | ✅ |

---

## KBA.CLI

### Installation

```bash
dotnet tool install -g KBA.CLI
```

### Commandes Disponibles

```bash
kba --help

# Commandes principales:
#   new           Créer un nouveau projet
#   generate      Générer du code (alias: gen, g)
#   ai            Commandes AI (chat, generate, review)
#   add-module    Ajouter un module
#   benchmark     Load testing
#   doctor        Diagnostic
#   dev           Serveur de développement
#   migrate       Migrations database
#   seed          Seed database
```

---

## AI Chat

### Commande

```bash
kba ai chat [options]
```

### Options

| Option | Description | Default |
|--------|-------------|---------|
| --provider | LLM provider (openai, claude) | openai |
| --model | Model à utiliser | gpt-4o |
| --api-key | API key | Env var |

### Exemples

```bash
# Chat avec OpenAI GPT-4o
kba ai chat --provider openai --model gpt-4o

# Chat avec Claude
kba ai chat --provider claude --model claude-3-5-sonnet-20241022

# Avec API key explicite
kba ai chat --provider openai --api-key sk-xxx

# Utiliser les variables d'environnement
export OPENAI_API_KEY=sk-xxx
kba ai chat
```

### Session Interactive

```
AI Chat - Interactive chat with LLM

Provider: openai
Model: gpt-4o
Type your message (or 'quit' to exit)

> Comment créer un repository pattern en C# ?

Assistant:
Voici un exemple de repository pattern en C# :

public interface IRepository<T> where T : class
{
    Task<T> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
}

public class EfRepository<T> : IRepository<T> where T : class
{
    protected readonly AppDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public EfRepository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<T> GetByIdAsync(int id)
    {
        return await _dbSet.FindAsync(id);
    }
}

> quit
Goodbye!
```

### Commandes de Session

| Commande | Description |
|----------|-------------|
| quit, exit | Quitter la session |
| clear | Effacer l'historique |

---

## AI Code Generation

### Commande

```bash
kba ai generate <prompt> [options]
```

### Options

| Option | Description | Default |
|--------|-------------|---------|
| --output | Fichier de sortie | Console |
| --provider | LLM provider | openai |
| --model | Model à utiliser | gpt-4o |
| --api-key | API key | Env var |
| --language | Langage cible | csharp |

### Exemples

```bash
# Générer un repository
kba ai generate "Create a repository pattern for Product entity with async methods" --output ProductRepository.cs

# Générer un controller
kba ai generate "Create a REST API controller for User management with CRUD operations" --output UsersController.cs --language csharp

# Générer avec Claude
kba ai generate "Generate a React component for a data table with sorting" --output DataTable.tsx --provider claude --language typescript

# Générer des tests
kba ai generate "Write unit tests for ProductService using xUnit and Moq" --output ProductServiceTests.cs
```

---

## AI Code Review

### Commande

```bash
kba ai review <path> [options]
```

### Options

| Option | Description | Default |
|--------|-------------|---------|
| --provider | LLM provider | openai |
| --model | Model à utiliser | gpt-4o |
| --api-key | API key | Env var |
| --focus | Focus area | all |

### Focus Areas

| Focus | Description |
|-------|-------------|
| all | Tous les aspects |
| security | Vulnérabilités sécurité |
| performance | Optimisations performance |
| style | Code style et conventions |

### Exemples

```bash
# Review complète
kba ai review src/MyProject/Controllers

# Review sécurité
kba ai review src/MyProject --focus security

# Review performance
kba ai review src/MyProject/Services --focus performance

# Review avec output fichier
kba ai review src/MyProject --focus all --output review-report.md

# Review avec Claude
kba ai review src/MyProject --provider claude --focus security
```

### Output Example

```
AI Review - AI-powered code review

Provider: openai
Model: gpt-4o
Path: src/MyProject
Focus: security

Analyzing code...
Review complete

=== Security Review ===

HIGH: SQL Injection vulnerability in UserRepository.cs:42
   - Raw SQL query with string concatenation
   - Recommendation: Use parameterized queries

MEDIUM: Missing authorization check in UserController.cs:78
   - Endpoint accessible without authentication
   - Recommendation: Add [Authorize] attribute

LOW: Hardcoded connection string in appsettings.json
   - Consider using Azure Key Vault or environment variables

Summary:
- High: 1
- Medium: 1
- Low: 1
```

---

## Configuration

### Variables d'Environnement

```bash
# OpenAI
export OPENAI_API_KEY=sk-xxx
export OPENAI_MODEL=gpt-4o

# Claude/Anthropic
export ANTHROPIC_API_KEY=sk-ant-xxx
export ANTHROPIC_MODEL=claude-3-5-sonnet-20241022
```

### appsettings.json

```json
{
  "AI": {
    "OpenAI": {
      "ApiKey": "sk-xxx",
      "DefaultModel": "gpt-4o",
      "Endpoint": "https://api.openai.com/v1",
      "MaxTokens": 4096,
      "Temperature": 0.7
    },
    "Anthropic": {
      "ApiKey": "sk-ant-xxx",
      "DefaultModel": "claude-3-5-sonnet-20241022",
      "Endpoint": "https://api.anthropic.com",
      "MaxTokens": 4096
    }
  }
}
```

---

## Best Practices

### Prompt Engineering

```bash
# Spécifique et contextuel
kba ai generate "Create a C# repository pattern for Product entity with async methods, CancellationToken support, EF Core 8"

# Trop vague (à éviter)
kba ai generate "Create repository"
```

### Code Review

```bash
# Review ciblée
kba ai review src/Controllers --focus security
kba ai review src/Services --focus performance

# Review avant merge
kba ai review src/Critical --focus all
```

### Cost Optimization

```bash
# Utiliser des modèles moins chers pour les tâches simples
kba ai generate "Create a simple DTO" --model gpt-3.5-turbo

# Utiliser GPT-4/Claude pour les tâches complexes
kba ai review src/Critical --model gpt-4o
```

### Security

```bash
# Ne jamais committer les API keys
# Mauvais: kba ai chat --api-key sk-xxx (visible dans l'historique)

# Bon: utiliser les variables d'environnement
export OPENAI_API_KEY=sk-xxx
kba ai chat
```

---

## Integration CI/CD

### GitHub Actions

```yaml
name: AI Code Review

on:
  pull_request:
    branches: [main]

jobs:
  ai-review:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      
      - name: Install KBA.CLI
        run: dotnet tool install -g KBA.CLI
      
      - name: AI Security Review
        run: kba ai review src --focus security --output review.md
        env:
          OPENAI_API_KEY: ${{ secrets.OPENAI_API_KEY }}
      
      - name: Upload Review
        uses: actions/upload-artifact@v4
        with:
          name: ai-review
          path: review.md
```

---

## Voir aussi

- [CLI Commands](../cli/kba-new.md) - Documentation CLI complète
- [Security](security.md) - Sécurité des API keys
- [Contributing](../../CONTRIBUTING.md) - Contribuer au CLI
