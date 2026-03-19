# Exemples et Templates

RIVORA fournit plusieurs templates de projet preconfigures pour demarrer rapidement. Chaque template inclut une architecture complete, des entites metier, des tests et une configuration Docker.

## Templates disponibles

| Template | Description | Modules inclus |
|----------|-------------|---------------|
| [SaaS Starter](/samples/saas-starter) | Application SaaS multi-tenant complete | Identity, Multi-tenancy, Billing, Webhooks |
| [E-commerce](/samples/ecommerce) | Plateforme e-commerce avec panier et paiement | Export, Billing, Jobs, Caching |
| [AI RAG App](/samples/ai-rag) | Application RAG avec vector store et chat | AI, NaturalQuery, Guardrails |
| [Fintech Payment](/samples/fintech) | Plateforme de paiement avec Event Sourcing | Event Sourcing, Saga, Security |
| API Minimal | API REST minimale | Core, Security |
| Microservices | Architecture microservices avec gateway | Core, Caching, Jobs, RivoraApiClient |

## Utiliser un template

### Via la CLI

```bash
# Lister les templates disponibles
rvr new --list

# Creer un projet depuis un template
rvr new MonProjet --template saas-starter

# Creer avec des options
rvr new MonProjet --template ecommerce \
  --database postgresql \
  --auth jwt \
  --docker
```

### Via le Project Wizard (Frontend)

1. Lancez le frontend RIVORA
2. Accedez a la page **Project Wizard** (`/project-wizard`)
3. Selectionnez un template
4. Configurez les options (base de donnees, modules, authentification)
5. Generez le projet

Le wizard genere une archive ZIP contenant le projet configure.

## Structure d'un template

Chaque template genere une solution .NET complete :

```
MonProjet/
  src/
    MonProjet.Domain/            # Entites, Value Objects, interfaces
    MonProjet.Application/       # Use cases, CQRS handlers, DTOs
    MonProjet.Infrastructure/    # EF Core, services externes
    MonProjet.Api/               # Controllers, middleware, configuration
  tests/
    MonProjet.Domain.Tests/      # Tests du domaine
    MonProjet.Application.Tests/ # Tests des handlers
    MonProjet.Api.Tests/         # Tests d'integration
  docker-compose.yml             # Infrastructure locale
  .github/
    workflows/
      ci.yml                     # Pipeline CI/CD
  README.md                      # Documentation du projet
  MonProjet.sln                  # Solution .NET
```

## Personnaliser un template

Apres generation, vous pouvez ajouter ou retirer des modules :

```bash
# Ajouter un module
cd MonProjet
rvr add-module GraphQL
rvr add-module EventSourcing

# Retirer un module
rvr remove-module Caching --dry-run   # Previsualiser
rvr remove-module Caching             # Appliquer
```

## Generer des entites

Utilisez la CLI pour ajouter des entites CRUD :

```bash
# Generer un CRUD complet
rvr generate crud Invoice \
  --props "Reference:string,Amount:decimal,DueDate:DateTime,Status:InvoiceStatus"

# Cela genere :
#   - Entity (Domain)
#   - Commands : Create, Update, Delete
#   - Queries : GetById, GetList
#   - Controller REST
#   - Tests unitaires
#   - Migration EF Core
```

## Bonnes pratiques

- **Commencez par un template** : Ne partez pas de zero, choisissez le template le plus proche de votre besoin
- **Personnalisez progressivement** : Ajoutez les modules au fur et a mesure
- **Gardez les tests** : Les templates incluent des tests, maintenez-les a jour
- **Utilisez `rvr doctor`** : Verifiez regulierement que votre environnement est sain

## Etape suivante

- [SaaS Starter](/samples/saas-starter) pour une application multi-tenant
- [E-commerce](/samples/ecommerce) pour une plateforme de vente
- [AI RAG App](/samples/ai-rag) pour une application d'intelligence artificielle
- [Creer son projet](/guide/create-project) pour la documentation detaillee
