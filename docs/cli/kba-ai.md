# rvr ai - RVR CLI

Commandes AI pour chat, génération et review de code.

## Syntaxe

```bash
rvr ai <command> [options]
```

## Commandes Disponibles

| Commande | Description |
|----------|-------------|
| `chat` | Chat interactif avec LLM |
| `generate` | Génération de code avec AI |
| `review` | Review de code avec AI |

---

## rvr ai chat

Chat interactif avec les LLM OpenAI ou Claude.

### Syntaxe

```bash
rvr ai chat [options]
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
rvr ai chat --provider openai --model gpt-4o

# Chat avec Claude
rvr ai chat --provider claude --model claude-3-5-sonnet-20241022

# Avec API key
rvr ai chat --api-key sk-xxx
```

### Commandes de Session

| Commande | Description |
|----------|-------------|
| quit, exit | Quitter la session |
| clear | Effacer l'historique |

---

## rvr ai generate

Générer du code avec AI.

### Syntaxe

```bash
rvr ai generate <prompt> [options]
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
rvr ai generate "Create a repository pattern for Product entity" --output ProductRepository.cs

# Générer un controller
rvr ai generate "REST API controller for User CRUD" --output UsersController.cs

# Générer avec Claude
rvr ai generate "React data table component" --output DataTable.tsx --provider claude --language typescript
```

---

## rvr ai review

Review de code avec AI.

### Syntaxe

```bash
rvr ai review <path> [options]
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
rvr ai review src/Controllers

# Review sécurité
rvr ai review src --focus security

# Review performance
rvr ai review src/Services --focus performance

# Review avec output
rvr ai review src --output review.md
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
- [rvr generate](kba-generate.md) - Génération sans AI
- [Security](../modules/security.md) - Sécurité des API keys
