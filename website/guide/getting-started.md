# Demarrage rapide

## Prerequis

- [.NET 9 SDK](https://dotnet.microsoft.com/download) (9.0+)
- SQL Server, PostgreSQL, MySQL ou SQLite
- (Optionnel) Docker Desktop pour l'environnement complet

## Installation

```bash
# Cloner le repository
git clone https://github.com/khalilbenaz/RIVORA.git
cd RVR.Framework

# Restaurer les packages
dotnet restore

# Lancer l'API
dotnet run --project src/RVR.Framework.Api
```

L'API demarre sur `http://localhost:5220`.

## Endpoints disponibles

| Endpoint | URL |
|----------|-----|
| API REST | `http://localhost:5220` |
| Swagger UI | `http://localhost:5220/swagger` |
| ReDoc | `http://localhost:5220/api-docs` |
| Health Check | `http://localhost:5220/health` |
| Admin Blazor | `http://localhost:5200` |

## Premiers pas

### 1. Creer un utilisateur

```bash
curl -X POST http://localhost:5220/api/v1/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@example.com",
    "password": "P@ssw0rd!123",
    "firstName": "Admin",
    "lastName": "Rivora"
  }'
```

### 2. Se connecter

```bash
curl -X POST http://localhost:5220/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@example.com",
    "password": "P@ssw0rd!123"
  }'
```

La reponse contient un `accessToken` JWT et un `refreshToken`.

### 3. Appeler l'API

```bash
curl http://localhost:5220/api/v1/products \
  -H "Authorization: Bearer <votre-token>"
```

## Docker Compose

Pour lancer l'environnement complet (SQL Server, PostgreSQL, Redis, RabbitMQ, Prometheus, Grafana, Jaeger, Seq) :

```bash
docker compose -f docker-compose.dev.yml up -d
```

Services disponibles :

| Service | Port | UI |
|---------|------|----|
| SQL Server | 1433 | - |
| PostgreSQL | 5432 | - |
| Redis | 6379 | - |
| RabbitMQ | 5672 | `http://localhost:15672` |
| Prometheus | 9090 | `http://localhost:9090` |
| Grafana | 3000 | `http://localhost:3000` |
| Jaeger | 16686 | `http://localhost:16686` |
| Seq | 5341 | `http://localhost:8081` |

## Etape suivante

- [Creer son projet](/guide/create-project) pour demarrer votre propre application avec RIVORA
- [Installation](/guide/installation) pour configurer la base de donnees et les secrets
- [Architecture du projet](/guide/architecture) pour comprendre la structure
- [Securite](/guide/security) pour configurer JWT et 2FA
- [Multi-Tenancy](/guide/multi-tenancy) pour activer l'isolation des donnees
