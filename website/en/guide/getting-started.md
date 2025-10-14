# Quick Start

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download) (9.0+)
- SQL Server, PostgreSQL, MySQL or SQLite
- (Optional) Docker Desktop for the full dev environment

## Installation

```bash
# Clone the repository
git clone https://github.com/khalilbenaz/RIVORA.git
cd RVR.Framework

# Restore packages
dotnet restore

# Run the API
dotnet run --project src/RVR.Framework.Api
```

The API starts at `http://localhost:5220`.

## Available Endpoints

| Endpoint | URL |
|----------|-----|
| REST API | `http://localhost:5220` |
| Swagger UI | `http://localhost:5220/swagger` |
| ReDoc | `http://localhost:5220/api-docs` |
| Health Check | `http://localhost:5220/health` |
| Admin Blazor | `http://localhost:5200` |

## First Steps

### 1. Create a user

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

### 2. Login

```bash
curl -X POST http://localhost:5220/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@example.com",
    "password": "P@ssw0rd!123"
  }'
```

The response contains a JWT `accessToken` and a `refreshToken`.

### 3. Call the API

```bash
curl http://localhost:5220/api/v1/products \
  -H "Authorization: Bearer <your-token>"
```

## Docker Compose

To start the full environment (SQL Server, PostgreSQL, Redis, RabbitMQ, Prometheus, Grafana, Jaeger, Seq):

```bash
docker compose -f docker-compose.dev.yml up -d
```

Available services:

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

## Next Steps

- [Architecture](/en/guide/architecture) to understand the project structure
- [Security](/en/guide/security) to configure JWT and 2FA
- [Multi-Tenancy](/en/guide/multi-tenancy) to enable data isolation
