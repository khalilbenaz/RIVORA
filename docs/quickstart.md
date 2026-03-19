# Quickstart - RIVORA Framework v4.0

**Temps de lecture : 5 minutes**

Ce guide vous accompagne pas a pas pour installer RIVORA Framework, lancer le backend, le frontend React, et creer votre premier projet.

---

## Table des Matieres

- [Installation](#installation)
- [Lancer le backend](#lancer-le-backend)
- [Lancer le frontend](#lancer-le-frontend)
- [Creer un projet avec le wizard](#creer-un-projet-avec-le-wizard)
- [Tests](#tests)
- [Etapes suivantes](#etapes-suivantes)

---

## Installation

### Prerequis

| Logiciel | Version | Lien |
|----------|---------|------|
| .NET SDK | 9.0+ | [Telecharger](https://dotnet.microsoft.com/download/dotnet/9.0) |
| Node.js | 20+ | [Telecharger](https://nodejs.org) |
| SQL Server, PostgreSQL, MySQL ou SQLite | Au choix | - |
| Git | Derniere version | [Telecharger](https://git-scm.com) |
| Docker | Optionnel | [Telecharger](https://www.docker.com) |

### Verification

```bash
dotnet --version   # 9.0.x
node --version     # v20+
git --version
```

### Cloner le repository

```bash
git clone https://github.com/khalilbenaz/RIVORA.git
cd RVR.Framework
```

### Restaurer les packages

```bash
dotnet restore
```

---

## Lancer le backend

```bash
# Configurer les secrets (obligatoire depuis v4.0)
cd src/api/RVR.Framework.Api
dotnet user-secrets init
dotnet user-secrets set "JwtSettings:SecretKey" "VotreCleSecreteDauMoins32Caracteres!!"
dotnet user-secrets set "ConnectionStrings:DefaultConnection" \
  "Server=localhost;Database=RVRFrameworkDb;Trusted_Connection=True;Encrypt=True"

# Lancer
cd ../../..
dotnet run --project src/api/RVR.Framework.Api
```

API disponible sur `http://localhost:5220` (Swagger: `/swagger`, ReDoc: `/api-docs`)

---

## Lancer le frontend

```bash
cd frontend
npm install
npm run dev
```

Frontend disponible sur `http://localhost:3000` (proxy automatique vers l'API).

### Pages principales

| URL | Description |
|-----|-------------|
| `http://localhost:3000` | Landing page publique |
| `http://localhost:3000/admin` | Back Office (28 pages) |
| `http://localhost:3000/app` | App client SaaS |
| `http://localhost:3000/admin/projects/new` | Project Wizard (6 templates) |
| `http://localhost:3000/admin/generator` | Entity/CRUD Generator |
| `http://localhost:3000/admin/flows` | Flow Builder |
| `http://localhost:3000/admin/kanban` | Kanban Board |

---

## Creer un projet avec le wizard

### Option 1 : Via le Front End (recommande)

1. Ouvrir `http://localhost:3000/admin/projects/new`
2. Choisir un template (SaaS, E-commerce, CRM, Blog, Internal Tools, API)
3. Configurer le nom, namespace, base de donnees
4. Selectionner les modules (securite, tenancy, billing, etc.)
5. Definir les entites et champs
6. Generer et telecharger le ZIP

### Option 2 : Via le CLI

```bash
# Installer le CLI
dotnet tool install --global RVR.CLI

# Wizard interactif
rvr new

# Ou avec des flags
rvr new MonSaaS --db postgresql --modules security,tenancy,jobs --auth jwt+2fa
```

### Option 3 : Via l'Entity Generator

1. Ouvrir `http://localhost:3000/admin/generator`
2. Definir vos entites visuellement (champs, types, relations)
3. Previsualiser le code genere (8 fichiers : Entity, DTOs, Validator, Repository, Service, Controller, Page React, Client API)
4. Telecharger tous les fichiers

### Configuration de la base de donnees

Les secrets ne doivent **jamais** etre dans `appsettings.json`. Utilisez `dotnet user-secrets` :

```bash
cd src/api/RVR.Framework.Api
dotnet user-secrets init
dotnet user-secrets set "JwtSettings:SecretKey" "VotreCleSecreteDauMoins32Caracteres!!"
```

#### Configurer OAuth (Azure AD, Keycloak, Auth0)

```bash
dotnet user-secrets set "OAuth:AzureAd:ClientSecret" "mon-client-secret"
dotnet user-secrets set "OAuth:AzureAd:TenantId" "mon-tenant-id"
```

#### Commandes utiles

```bash
# Lister tous les secrets configurés
dotnet user-secrets list

# Supprimer un secret
dotnet user-secrets remove "ConnectionStrings:DefaultConnection"

# Supprimer tous les secrets
dotnet user-secrets clear
```

> 💡 **Note** : Les User Secrets sont stockés localement dans `%APPDATA%\Microsoft\UserSecrets\` (Windows) ou `~/.microsoft/usersecrets/` (Linux/macOS). Ils ne sont **jamais** inclus dans le code source et sont automatiquement chargés en environnement `Development`.

### Créer la base de données

```bash
# Appliquer les migrations
dotnet ef database update \
  --project src/RVR.Framework.Infrastructure \
  --startup-project src/RVR.Framework.Api
```

**Sortie attendue :**
```
Build started...
Build succeeded.
Done. To undo this action, use 'ef migrations remove'.
```

---

## Premier endpoint

### 1. Démarrer l'API

```bash
dotnet run --project src/RVR.Framework.Api
```

**Sortie attendue :**
```
Building...
Now listening on: http://localhost:5220
Application started. Press Ctrl+C to shut down.
```

### 2. Initialiser le premier administrateur

L'API nécessite un utilisateur administrateur pour accéder aux endpoints protégés.

#### Via cURL

```bash
curl -X POST http://localhost:5220/api/init/first-admin \
  -H "Content-Type: application/json" \
  -d '{
    "userName": "admin",
    "email": "admin@RIVORA-framework.com",
    "password": "Admin@123456",
    "firstName": "Admin",
    "lastName": "System"
  }'
```

#### Via PowerShell

```powershell
$body = @{
    userName = "admin"
    email = "admin@RIVORA-framework.com"
    password = "Admin@123456"
    firstName = "Admin"
    lastName = "System"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5220/api/init/first-admin" \
  -Method POST \
  -ContentType "application/json" \
  -Body $body
```

#### Réponse attendue

```json
{
  "success": true,
  "message": "First admin user created successfully",
  "user": {
    "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
    "userName": "admin",
    "email": "admin@RIVORA-framework.com"
  }
}
```

### 3. S'authentifier

Obtenez un token JWT pour accéder aux endpoints protégés.

```bash
curl -X POST http://localhost:5220/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "userName": "admin",
    "password": "Admin@123456"
  }'
```

**Réponse :**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "...",
  "expiresAt": "2025-03-08T16:30:00Z",
  "userName": "admin",
  "email": "admin@RIVORA-framework.com"
}
```

### 4. Créer un produit (endpoint protégé)

```bash
# Remplacez VOTRE_TOKEN par le token reçu
TOKEN="eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."

curl -X POST http://localhost:5220/api/products \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "name": "Mon Premier Produit",
    "description": "Description du produit",
    "price": 99.99,
    "stock": 10,
    "sku": "PROD-001",
    "category": "Electronics"
  }'
```

**Réponse :**
```json
{
  "id": "12345678-1234-1234-1234-123456789012",
  "tenantId": null,
  "name": "Mon Premier Produit",
  "description": "Description du produit",
  "price": 99.99,
  "stock": 10,
  "sku": "PROD-001",
  "category": "Electronics",
  "creationTime": "2025-03-08T15:30:00Z"
}
```

### 5. Lister les produits (endpoint public)

```bash
curl http://localhost:5220/api/products
```

**Réponse :**
```json
[
  {
    "id": "12345678-1234-1234-1234-123456789012",
    "name": "Mon Premier Produit",
    "price": 99.99,
    "stock": 10
  }
]
```

---

## Tests

### Exécuter tous les tests

```bash
dotnet test
```

**Sortie attendue :**
```
Build started...
Build succeeded.
  RVR.Framework.Domain.Tests -> ...
  RVR.Framework.Application.Tests -> ...
  RVR.Framework.Api.IntegrationTests -> ...
Test run for ...
Passed!  - Failed: 0, Passed: 150, Skipped: 0
```

### Tests par catégorie

```bash
# Tests du domaine (entités, value objects)
dotnet test tests/RVR.Framework.Domain.Tests \
  --logger "console;verbosity=detailed"

# Tests de l'application (services, validators)
dotnet test tests/RVR.Framework.Application.Tests \
  --logger "console;verbosity=detailed"

# Tests d'intégration (API, base de données)
dotnet test tests/RVR.Framework.Api.IntegrationTests \
  --logger "console;verbosity=detailed"
```

### Exécuter un test spécifique

```bash
# Par nom de test
dotnet test --filter "FullyQualifiedName~ProductService"

# Par catégorie
dotnet test --filter "Category=Unit"
```

### Couverture de code

```bash
# Installer coverlet (si nécessaire)
dotnet tool install --global coverlet.console

# Exécuter avec couverture
dotnet test /p:CollectCoverage=true \
  /p:CoverletOutputFormat=lcov \
  /p:CoverletOutput=./coverage/lcov.info
```

### Tests manuels avec Swagger

1. Ouvrez votre navigateur : `http://localhost:5220/swagger`
2. Section **Authentication** → `POST /api/auth/login`
3. Cliquez **Execute** avec les credentials admin
4. Copiez le token de la réponse
5. Cliquez **Authorize** en haut
6. Collez `Bearer VOTRE_TOKEN`
7. Testez les autres endpoints

---

## Étapes suivantes

### 📚 Aller plus loin

| Sujet | Documentation |
|-------|---------------|
| Ajouter un nouveau service | [GUIDE-COMPLET.md](GUIDE-COMPLET.md#comment-ajouter-un-nouveau-service) |
| Comprendre le multi-tenancy | [TENANTID_IMPLEMENTATION.md](TENANTID_IMPLEMENTATION.md) |
| Configurer l'autorisation | [AUTHORIZATION_SUMMARY.md](AUTHORIZATION_SUMMARY.md) |
| Déployer sur IIS | [../README.md](../README.md#-déploiement-iis) |

### 🔧 Personnalisation

1. **Modifier les entités** : `src/RVR.Framework.Domain/Entities/`
2. **Ajouter des validators** : `src/RVR.Framework.Application/Validators/`
3. **Créer des services** : `src/RVR.Framework.Application/Services/`
4. **Configurer l'API** : `src/RVR.Framework.Api/Program.cs`

### 🚀 Production checklist

- [ ] Utiliser `dotnet user-secrets` pour le développement local (ne jamais committer de secrets)
- [ ] Changer la `JwtSettings.SecretKey`
- [ ] Configurer HTTPS
- [ ] Mettre `EnableSensitiveDataLogging` à `false`
- [ ] Configurer CORS avec origines spécifiques
- [ ] Activer le rate limiting
- [ ] Configurer le logging centralisé
- [ ] Mettre en place les health checks
- [ ] Utiliser des variables d'environnement ou Azure Key Vault pour les secrets en production

---

## 🆘 Troubleshooting

### La base de données ne se crée pas

```bash
# Vérifier LocalDB
sqllocaldb info

# Créer si nécessaire
sqllocaldb create MSSQLLocalDB
sqllocaldb start MSSQLLocalDB
```

### Port déjà utilisé

Éditez `src/RVR.Framework.Api/Properties/launchSettings.json` :
```json
{
  "applicationUrl": "http://localhost:5221"
}
```

### Erreur de migration

```bash
# Supprimer la dernière migration
dotnet ef migrations remove --project src/RVR.Framework.Infrastructure

# Recréer la migration
dotnet ef migrations add InitialCreate \
  --project src/RVR.Framework.Infrastructure \
  --startup-project src/RVR.Framework.Api
```

### Tests échouent

```bash
# Nettoyer et reconstruire
dotnet clean
dotnet restore
dotnet build
```

---

**Prochaine étape** → [Guide Complet](GUIDE-COMPLET.md)

*RIVORA Framework - v2.0.0*
