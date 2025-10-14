# rvr doctor - RVR CLI

Diagnostiquer les problèmes du projet.

## Syntaxe

```bash
rvr doctor
```

## Description

La commande `rvr doctor` analyse votre projet et détecte les problèmes potentiels :

- Configuration manquante
- Dépendances non résolues
- Problèmes de build
- Migrations database pending
- Tests failing
- Code smells

## Exemple d'Output

```
╭────────────────────────────────────────╮
│            RVR Doctor                  │
│   Project Diagnostics                  │
╰────────────────────────────────────────╯

Analyzing project...

=== Project Info ===
Name: MyProject
SDK: .NET 8.0
Location: /path/to/MyProject

=== Checks ===

[✓] .NET SDK installed
[✓] Project restores successfully
[✓] Build succeeds
[✓] All tests pass
[✓] No pending migrations
[✓] Configuration valid

=== Warnings ===

[!] appsettings.json: Connection string uses LocalDB
    Recommendation: Use SQL Server for production

[!] No rate limiting configured
    Recommendation: Add rate limiting middleware

=== Errors ===

[✗] Missing environment variable: OPENAI_API_KEY
    Recommendation: Set OPENAI_API_KEY in environment

=== Summary ===

Checks: 6 passed, 0 failed
Warnings: 2
Errors: 1

Health Score: 85/100
```

## Checks Performed

| Check | Description |
|-------|-------------|
| SDK | .NET SDK version check |
| Restore | Package restore verification |
| Build | Build compilation check |
| Tests | Unit tests execution |
| Migrations | Database migrations status |
| Configuration | appsettings.json validation |
| Environment | Environment variables check |
| Dependencies | Project references check |

## Health Score

| Score | Status |
|-------|--------|
| 90-100 | Excellent |
| 70-89 | Good |
| 50-69 | Fair |
| 0-49 | Poor |

## Fixing Issues

### Missing Environment Variables

```bash
# Set the missing variable
export OPENAI_API_KEY=sk-xxx

# Or add to .env file
echo "OPENAI_API_KEY=sk-xxx" >> .env
```

### Pending Migrations

```bash
# Apply migrations
rvr migrate

# Or manually
dotnet ef database update
```

### Failing Tests

```bash
# Run tests to see details
dotnet test

# Fix failing tests and re-run
```

## Voir aussi

- [rvr migrate](kba-doctor.md) - Appliquer migrations
- [rvr dev](kba-doctor.md) - Serveur de développement
- [Troubleshooting](../quickstart.md) - Guide de troubleshooting
