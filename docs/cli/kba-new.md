# kba new - KBA CLI

Créer un nouveau projet KBA Framework.

## Syntaxe

```bash
kba new <name> [options]
```

## Arguments

| Argument | Description | Required |
|----------|-------------|----------|
| `name` | Nom du projet | Oui |

## Options

| Option | Description | Default |
|--------|-------------|---------|
| `--template` | Template à utiliser | minimal |
| `--tenancy` | Mode multi-tenancy | row |

## Templates Disponibles

| Template | Description |
|----------|-------------|
| `minimal` | Projet minimal avec structure de base |
| `saas-starter` | Starter SaaS avec authentication, multi-tenancy |
| `ai-rag` | Projet avec intégration AI RAG |

## Modes Multi-Tenancy

| Mode | Description |
|------|-------------|
| `row` | Isolation par ligne (TenantId column) |
| `schema` | Isolation par schema database |
| `database` | Isolation par database séparée |

## Exemples

### Créer un projet minimal

```bash
kba new MyProject
```

### Créer un projet SaaS

```bash
kba new MySaaS --template saas-starter --tenancy row
```

### Créer un projet AI

```bash
kba new MyAIApp --template ai-rag
```

## Structure Générée

### Template Minimal

```
MyProject/
├── src/
│   ├── MyProject.Api/
│   ├── MyProject.Application/
│   ├── MyProject.Domain/
│   └── MyProject.Infrastructure/
├── tests/
│   └── MyProject.Tests/
├── docs/
└── README.md
```

### Template SaaS Starter

```
MyProject/
├── src/
│   ├── MyProject.Api/
│   ├── MyProject.Application/
│   ├── MyProject.Domain/
│   └── MyProject.Infrastructure/
├── tests/
│   ├── MyProject.Domain.Tests/
│   ├── MyProject.Application.Tests/
│   └── MyProject.Api.IntegrationTests/
├── docs/
├── docker-compose.yml
├── Dockerfile
└── README.md
```

## Next Steps

Après création du projet :

```bash
cd MyProject

# Restaurer les packages
dotnet restore

# Configurer la database
# Éditer src/MyProject.Api/appsettings.json

# Créer la database
dotnet ef database update

# Lancer le projet
kba dev
# ou
dotnet run --project src/MyProject.Api
```

## Voir aussi

- [kba dev](kba-doctor.md) - Serveur de développement
- [Quickstart](../quickstart.md) - Guide de démarrage
- [Database](../modules/database.md) - Configuration database
