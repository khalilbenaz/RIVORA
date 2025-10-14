# RVR.CLI v3.3.0 - Rivora Framework CLI

**Enterprise .NET 9 CLI** pour scaffolding, AI code review, domain design et migrations.

## Installation

```bash
dotnet tool install --global RVR.CLI
```

## Commandes

### Scaffolding
```bash
rvr new MyApp --template saas-starter    # Nouveau projet SaaS
rvr generate crud Invoice                # CRUD complet
rvr generate aggregate Order             # Aggregate root DDD
rvr generate client --url http://api     # Client API type depuis OpenAPI
rvr generate test Product                # Tests unitaires scaffoldes
rvr add-module Inventory                 # Module complet (Domain/App/Infra/API/Tests)
```

### AI
```bash
rvr ai chat --provider claude            # Chat interactif LLM
rvr ai generate "Create a payment service"
rvr ai design --provider openai          # Domain design interactif
rvr ai review --all                      # Review complet (Architecture, DDD, Performance, Security)
rvr ai review --architecture             # Clean Architecture conformance
rvr ai review --ddd                      # DDD anti-patterns
rvr ai review --performance              # N+1, missing async, EF anti-patterns
rvr ai review --security                 # Vulnerabilites OWASP
rvr ai review --output sarif             # CI integration (SARIF 2.1.0)
rvr ai review --provider ollama          # Offline avec Ollama
```

### Migrations
```bash
rvr migrate generate AddOrderTable       # Generer migration EF Core
rvr migrate apply                        # Appliquer les migrations
rvr migrate list                         # Lister les migrations
rvr migrate rollback                     # Annuler derniere migration
```

### DevOps
```bash
rvr doctor                               # Diagnostic du projet
rvr benchmark                            # Load testing k6
rvr dev                                  # Serveur de dev
```

## LLM Backends

| Provider | Usage | Offline |
|----------|-------|---------|
| OpenAI | `--provider openai` | Non |
| Claude | `--provider claude` | Non |
| Ollama | `--provider ollama` | Oui |

## Liens

- [GitHub](https://github.com/khalilbenaz/RIVORA)
- [Documentation](https://khalilbenaz.github.io/RIVORA/)
- [Release Notes](https://github.com/khalilbenaz/RIVORA/releases/tag/v3.3.0)

## License

MIT
