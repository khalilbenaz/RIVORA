# Installation

## Depuis les sources

```bash
git clone https://github.com/khalilbenaz/RIVORA.git
cd RVR.Framework
dotnet restore
dotnet build
```

## Configuration de la base de donnees

RIVORA Framework supporte 4 providers. Configurez `appsettings.json` :

### SQL Server (defaut)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=RVRFrameworkDb;Trusted_Connection=true;TrustServerCertificate=true"
  },
  "DatabaseProvider": "SqlServer"
}
```

### PostgreSQL

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=RVRFrameworkDb;Username=kba;Password=kba_dev_password"
  },
  "DatabaseProvider": "PostgreSQL"
}
```

### MySQL

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=RVRFrameworkDb;User=root;Password=password"
  },
  "DatabaseProvider": "MySQL"
}
```

### SQLite

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=RVRFramework.db"
  },
  "DatabaseProvider": "SQLite"
}
```

## Securiser les secrets avec User Secrets

En developpement, ne stockez jamais de mots de passe ou cles dans `appsettings.json`. Utilisez `dotnet user-secrets` :

```bash
# Initialiser User Secrets (une seule fois)
cd src/api/RVR.Framework.Api
dotnet user-secrets init
```

### Chaine de connexion

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" \
  "Server=localhost;Database=RVRFrameworkDb;User Id=sa;Password=MonMotDePasse;TrustServerCertificate=true"
```

### Cle JWT

```bash
dotnet user-secrets set "JwtSettings:SecretKey" \
  "MaCleSecreteSuperLongueEtComplexe123!"
```

### OAuth (Azure AD, Keycloak, Auth0)

```bash
dotnet user-secrets set "OAuth:AzureAd:ClientSecret" "mon-client-secret"
dotnet user-secrets set "OAuth:AzureAd:TenantId" "mon-tenant-id"
```

### Commandes utiles

```bash
# Lister les secrets
dotnet user-secrets list

# Supprimer un secret
dotnet user-secrets remove "ConnectionStrings:DefaultConnection"

# Tout supprimer
dotnet user-secrets clear
```

::: tip Ou sont stockes les secrets ?
Les User Secrets sont stockes hors du projet :
- **Windows** : `%APPDATA%\Microsoft\UserSecrets\<UserSecretsId>\secrets.json`
- **Linux/macOS** : `~/.microsoft/usersecrets/<UserSecretsId>/secrets.json`

Ils ne sont jamais inclus dans le code source et sont charges automatiquement en environnement `Development`.
:::

::: warning Production
En production, utilisez des variables d'environnement ou un coffre-fort :
- Azure Key Vault
- AWS Secrets Manager
- HashiCorp Vault
- Variables d'environnement du systeme
:::

## Appliquer les migrations

```bash
dotnet ef database update --project src/RVR.Framework.Infrastructure
```

## RVR CLI (optionnel)

```bash
dotnet tool install --global RVR.CLI
rvr doctor  # Verifie l'environnement
```

## Verification

```bash
dotnet run --project src/RVR.Framework.Api
# Ouvrir http://localhost:5220/health
```

Si le health check retourne `Healthy`, l'installation est reussie.

## Etape suivante

Consultez le guide [Creer son projet](/guide/create-project) pour apprendre a construire votre propre application etape par etape avec RIVORA.
