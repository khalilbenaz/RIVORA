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
