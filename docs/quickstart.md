# Quickstart - RIVORA Framework

**Temps de lecture : 5 minutes**

Ce guide vous accompagne pas à pas pour installer RIVORA Framework, créer votre premier projet et tester votre premier endpoint API.

---

## 📋 Table des Matières

- [Installation](#installation)
- [Création du projet](#création-du-projet)
- [Premier endpoint](#premier-endpoint)
- [Tests](#tests)
- [Étapes suivantes](#étapes-suivantes)

---

## Installation

### Prérequis

Avant de commencer, assurez-vous d'avoir installé :

| Logiciel | Version | Lien |
|----------|---------|------|
| .NET SDK | 8.0+ | [Télécharger](https://dotnet.microsoft.com/download/dotnet/8.0) |
| SQL Server | 2019+ ou LocalDB | [Télécharger](https://www.microsoft.com/sql-server) |
| Git | Dernière version | [Télécharger](https://git-scm.com) |
| VS Code / Visual Studio | Optionnel | [Télécharger](https://code.visualstudio.com) |

### Vérification des prérequis

```bash
# Vérifier .NET SDK
dotnet --version
# Doit afficher : 8.0.x

# Vérifier Git
git --version

# Vérifier LocalDB (si utilisé)
sqllocaldb info
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

## Création du projet

### Option 1 : Utiliser le template existant

Le projet est déjà configuré. Passez directement à la configuration.

### Option 2 : Créer un nouveau projet à partir du framework

```bash
# Créer un nouveau dossier pour votre projet
mkdir MonProjet
cd MonProjet

# Copier la structure du framework
cp -r ../RVR.Framework/src ./src
cp -r ../RVR.Framework/tests ./tests
cp ../RVR.Framework/Directory.Packages.props ./

# Initialiser le repository Git
git init
```

### Configuration de la base de données

Éditez le fichier `src/RVR.Framework.Api/appsettings.json` :

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=RVRFrameworkDb;Trusted_Connection=true;MultipleActiveResultSets=true;TrustServerCertificate=true"
  },
  "DatabaseSettings": {
    "CommandTimeout": 30,
    "EnableRetryOnFailure": true,
    "MaxRetryCount": 3,
    "MaxRetryDelay": "00:00:05",
    "EnableSensitiveDataLogging": true,
    "EnableDetailedErrors": true,
    "MigrationsAssembly": "RVR.Framework.Infrastructure"
  },
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyThatShouldBeAtLeast32CharactersLong!",
    "Issuer": "RVR.Framework",
    "Audience": "RVR.Framework.Client",
    "ExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 7
  }
}
```

> ⚠️ **Important** : En production, utilisez des variables d'environnement pour les secrets.

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

- [ ] Changer la `JwtSettings.SecretKey`
- [ ] Configurer HTTPS
- [ ] Mettre `EnableSensitiveDataLogging` à `false`
- [ ] Configurer CORS avec origines spécifiques
- [ ] Activer le rate limiting
- [ ] Configurer le logging centralisé
- [ ] Mettre en place les health checks

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
