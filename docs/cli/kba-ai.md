# kba ai - KBA CLI

Commandes AI pour chat, génération et review de code.

## Syntaxe

```bash
kba ai <command> [options]
```

## Commandes Disponibles

| Commande | Description |
|----------|-------------|
| `chat` | Chat interactif avec LLM |
| `generate` | Génération de code avec AI |
| `review` | Review de code avec AI |

---

## kba ai chat

Chat interactif avec les LLM OpenAI ou Claude.

### Syntaxe

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
# Chat avec OpenAI
kba ai chat --provider openai --model gpt-4o

# Chat avec Claude
kba ai chat --provider claude --model claude-3-5-sonnet-20241022

# Avec API key
kba ai chat --api-key sk-xxx
```

### Commandes de Session

| Commande | Description |
|----------|-------------|
| quit, exit | Quitter la session |
| clear | Effacer l'historique |

---

## kba ai generate

Générer du code avec AI.

### Syntaxe

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
kba ai generate "Create a repository pattern for Product entity" --output ProductRepository.cs

# Générer un controller
kba ai generate "REST API controller for User CRUD" --output UsersController.cs

# Générer avec Claude
kba ai generate "React data table component" --output DataTable.tsx --provider claude --language typescript
```

---

## kba ai review

Review de code avec AI.

### Syntaxe

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
kba ai review src/Controllers

# Review sécurité
kba ai review src --focus security

# Review performance
kba ai review src/Services --focus performance

# Review avec output
kba ai review src --output review.md
```

---

## Configuration

### Variables d'Environnement

```bash
# OpenAI
export OPENAI_API_KEY=sk-xxx
export OPENAI_MODEL=gpt-4o

# Anthropic/Claude
export ANTHROPIC_API_KEY=sk-ant-xxx
export ANTHROPIC_MODEL=claude-3-5-sonnet-20241022
```

### Models Supportés

| Provider | Models |
|----------|--------|
| OpenAI | gpt-4o, gpt-4-turbo, gpt-3.5-turbo |
| Claude | claude-3-5-sonnet, claude-3-opus, claude-3-haiku |

---

## Voir aussi

- [AI Native](../modules/ai-native.md) - Documentation AI complète
- [kba generate](kba-generate.md) - Génération sans AI
- [Security](../modules/security.md) - Sécurité des API keys
