# Installation

## From Source

```bash
git clone https://github.com/khalilbenaz/RIVORA.git
cd RVR.Framework
dotnet restore
dotnet build
```

## Database Configuration

RIVORA Framework supports 4 providers. Configure `appsettings.json`:

### SQL Server (default)

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

## Apply Migrations

```bash
dotnet ef database update --project src/RVR.Framework.Infrastructure
```

## RVR CLI (optional)

```bash
dotnet tool install --global RVR.CLI
rvr doctor  # Verify environment
```

## Verification

```bash
dotnet run --project src/RVR.Framework.Api
# Open http://localhost:5220/health
```

If the health check returns `Healthy`, the installation is successful.
