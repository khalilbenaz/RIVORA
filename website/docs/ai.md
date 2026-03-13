# Intelligence Artificielle

KBA Framework est un framework **AI-First**. Il intègre nativement la puissance des modèles de langage (LLM) pour assister les développeurs dans la génération de code, l'audit de sécurité et l'architecture logicielle.

## Fournisseurs Supportés

Le framework supporte quatre fournisseurs majeurs, couvrant aussi bien les solutions Cloud que les installations Locales (pour une confidentialité totale).

### Cloud (SaaS)
*   **OpenAI** : Utilise les modèles GPT-4o et GPT-4o-mini.
*   **Anthropic Claude** : Support de Claude 3.5 Sonnet pour une ingénierie logicielle de haute précision.

### Local (Auto-hébergé)
*   **Ollama** : Pour faire tourner des modèles comme Llama 3 localement.
*   **Kilo Code** : Intégration optimisée pour le développement.

---

## Configuration

L'intégration se configure via des variables d'environnement simples dans votre terminal.

### Pour le Cloud (OpenAI / Claude)
```bash
# OpenAI
export OPENAI_API_KEY="sk-..."

# Anthropic Claude
export ANTHROPIC_API_KEY="sk-ant-..."
```

### Pour le Local (Ollama / Kilo)
```bash
# Ollama (adresse par défaut : http://localhost:11434/v1)
export OLLAMA_API_URL="http://localhost:11434/v1"

# Activer l'IA Locale pour KBA Studio
export ENABLE_LOCAL_AI="ollama" 
```

---

## Utilisation : KBA Studio (Baguette Magique)

Dans l'**Entity Builder** de KBA Studio, vous trouverez une zone de saisie en langage naturel.

**Exemple de prompt :**
> "Génère une entité Commande pour un e-commerce avec numéro, date, prix total, id client et statut."

L'IA va alors :
1. Analyser votre besoin.
2. Déterminer les types C# appropriés (Guid, decimal, DateTime).
3. Configurer les options (Aggregate Root, Audit).
4. Remplir instantanément le formulaire visuel pour vous.

---

## Utilisation : KBA CLI

La commande `kba ai` est votre assistant de poche.

### Chat Interactif
```bash
# Par défaut (OpenAI)
kba ai chat

# Utiliser Claude
kba ai chat --provider claude

# Utiliser votre IA locale
kba ai chat --provider ollama
```

### Génération de code
```bash
kba ai generate "Crée un pattern spécification pour filtrer les commandes payées" --output OrderSpec.cs
```

### Revue de Code
```bash
kba ai review src/Api/Controllers/OrderController.cs --focus performance
```
