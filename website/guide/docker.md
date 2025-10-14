# Docker Development Guide

The RIVORA Framework includes Docker Compose configurations for running the full infrastructure stack locally.

## Docker Compose Files

| File | Purpose |
|------|---------|
| `docker-compose.dev.yml` | Core services (SQL Server, PostgreSQL, Redis, RabbitMQ, etc.) |
| `infra/docker-compose.dev.yml` | Observability stack (Loki, Tempo, Prometheus, Grafana, OTEL Collector) |

## Starting the Full Stack

### Core Services

```bash
docker compose -f docker-compose.dev.yml up -d
```

This starts:

| Service | Port | Description |
|---------|------|-------------|
| SQL Server | `1433` | Primary database |
| PostgreSQL | `5432` | Alternative database |
| Redis | `6379` | Caching and rate limiting |
| RabbitMQ | `5672` / `15672` | Messaging / Management UI |
| Prometheus | `9090` | Metrics collection |
| Grafana | `3000` | Dashboards |
| Jaeger | `4317` / `16686` | Distributed tracing / UI |
| Seq | `5341` / `8081` | Structured log viewer |
| Elasticsearch | `9200` | Full-text search / logs |
| Kibana | `5601` | Elasticsearch dashboard |

### Observability Stack

```bash
docker compose -f infra/docker-compose.dev.yml up -d
```

This starts the advanced observability pipeline:

| Service | Port | Description |
|---------|------|-------------|
| Loki | `3100` | Log aggregation |
| Tempo | `3200` | Distributed tracing |
| OTEL Collector | `4317` / `4318` | OpenTelemetry pipeline |
| Prometheus | `9090` | Metrics |
| Grafana | `3000` | Unified dashboard |

## Connecting to Services

### SQL Server

```
Server=localhost,1433;Database=RVRFrameworkDb;User Id=sa;Password=RVR_Dev_P@ssw0rd!;TrustServerCertificate=True
```

appsettings.Development.json:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=RVRFrameworkDb;User Id=sa;Password=RVR_Dev_P@ssw0rd!;TrustServerCertificate=True"
  }
}
```

### PostgreSQL

```
Host=localhost;Port=5432;Database=RVRFrameworkDb;Username=kba;Password=kba_dev_password
```

appsettings.Development.json:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=RVRFrameworkDb;Username=kba;Password=kba_dev_password"
  }
}
```

### Redis

```json
{
  "Redis": {
    "ConnectionString": "localhost:6379"
  }
}
```

### RabbitMQ

```json
{
  "RabbitMQ": {
    "HostName": "localhost",
    "Port": 5672,
    "UserName": "kba",
    "Password": "kba_dev_password"
  }
}
```

Management UI: `http://localhost:15672` (login: `kba` / `kba_dev_password`)

## Service Management

### Check service status

```bash
docker compose -f docker-compose.dev.yml ps
```

### View logs

```bash
# All services
docker compose -f docker-compose.dev.yml logs -f

# Specific service
docker compose -f docker-compose.dev.yml logs -f redis
```

### Stop all services

```bash
docker compose -f docker-compose.dev.yml down
```

### Reset data (remove volumes)

```bash
docker compose -f docker-compose.dev.yml down -v
```

## Accessing Dashboards

| Dashboard | URL | Credentials |
|-----------|-----|-------------|
| Grafana | `http://localhost:3000` | `admin` / `admin` |
| RabbitMQ | `http://localhost:15672` | `kba` / `kba_dev_password` |
| Jaeger UI | `http://localhost:16686` | None |
| Seq | `http://localhost:8081` | None |
| Kibana | `http://localhost:5601` | None |
| Prometheus | `http://localhost:9090` | None |

## Running the API with Docker Services

Start the infrastructure, then run the API:

```bash
# 1. Start infrastructure
docker compose -f docker-compose.dev.yml up -d

# 2. Wait for health checks to pass
docker compose -f docker-compose.dev.yml ps

# 3. Run the API
cd src/api/RVR.Framework.Api
dotnet run
```

The API will be available at `http://localhost:5220`.

## Health Checks

Verify services are healthy:

```bash
# SQL Server
docker compose -f docker-compose.dev.yml exec sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "RVR_Dev_P@ssw0rd!" -Q "SELECT 1" -C

# Redis
docker compose -f docker-compose.dev.yml exec redis redis-cli ping

# Elasticsearch
curl http://localhost:9200/_cluster/health

# RabbitMQ
curl -u kba:kba_dev_password http://localhost:15672/api/overview
```

## Troubleshooting

### Port conflicts

If a port is already in use, modify the port mapping in `docker-compose.dev.yml`:

```yaml
services:
  redis:
    ports:
      - "6380:6379"  # Map to 6380 on host
```

### SQL Server on ARM/Mac

SQL Server does not have native ARM images. Use Azure SQL Edge instead:

```yaml
services:
  sqlserver:
    image: mcr.microsoft.com/azure-sql-edge:latest
    environment:
      ACCEPT_EULA: "Y"
      MSSQL_SA_PASSWORD: "RVR_Dev_P@ssw0rd!"
    ports:
      - "1433:1433"
```

### Insufficient memory

If containers crash due to memory, increase Docker's memory allocation to at least 4 GB in Docker Desktop settings. For Elasticsearch, the compose file limits JVM heap to 512 MB:

```yaml
environment:
  - "ES_JAVA_OPTS=-Xms512m -Xmx512m"
```
