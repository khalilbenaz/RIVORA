# Health Checks

The RIVORA framework provides configurable health check endpoints via `AddRvrHealthChecks()`.

## GET `/health`

Main health check endpoint. Returns the overall health status including all registered checks.

- **Auth required**: No

```bash
curl http://localhost:5220/health
```

**Response `200 OK`:**

```json
{
  "status": "Healthy",
  "totalDuration": "00:00:00.0234567",
  "entries": {
    "self": { "status": "Healthy" },
    "database": { "status": "Healthy" },
    "redis": { "status": "Healthy" },
    "rabbitmq": { "status": "Healthy" }
  }
}
```

**Response `503 Service Unavailable`** when any check is unhealthy:

```json
{
  "status": "Unhealthy",
  "totalDuration": "00:00:05.0012345",
  "entries": {
    "self": { "status": "Healthy" },
    "database": { "status": "Unhealthy", "description": "Connection refused" },
    "redis": { "status": "Healthy" }
  }
}
```

---

## GET `/health/ready`

Readiness probe. Returns only checks tagged with `ready`. Use this for Kubernetes readiness probes.

- **Auth required**: No

```bash
curl http://localhost:5220/health/ready
```

**Response `200 OK`:**

```json
{
  "status": "Healthy",
  "entries": {
    "database": { "status": "Healthy" },
    "redis": { "status": "Healthy" }
  }
}
```

Use in Kubernetes:

```yaml
readinessProbe:
  httpGet:
    path: /health/ready
    port: 5220
  initialDelaySeconds: 5
  periodSeconds: 10
```

---

## GET `/health/detailed`

Detailed health check with full diagnostics. Must be enabled via `UseRvrDetailedHealthChecks()`.

- **Auth required**: Yes (recommended to restrict in production)

```bash
curl http://localhost:5220/health/detailed \
  -H "Authorization: Bearer <token>"
```

**Response `200 OK`:**

```json
{
  "status": "Healthy",
  "totalDuration": "00:00:00.0456789",
  "entries": {
    "database": {
      "status": "Healthy",
      "duration": "00:00:00.0120000",
      "data": {
        "provider": "PostgreSQL",
        "server": "localhost:5432",
        "database": "rivora_db",
        "pendingMigrations": 0
      }
    },
    "redis": {
      "status": "Healthy",
      "duration": "00:00:00.0030000",
      "data": {
        "endpoint": "localhost:6379",
        "connectedClients": 5
      }
    },
    "jobs": {
      "status": "Healthy",
      "duration": "00:00:00.0010000",
      "data": {
        "enqueuedJobs": 3,
        "failedJobs": 0
      }
    }
  }
}
```

---

## Registered Checks

The following checks are auto-registered based on your configured modules:

| Check | Condition | Tags |
|-------|-----------|------|
| `self` | Always registered | `live` |
| `database` | EF Core configured | `ready` |
| `redis` | Caching module enabled | `ready` |
| `rabbitmq` | Messaging configured | `ready` |
| `ai` | AI provider configured | -- |
| `jobs` | Jobs module enabled | -- |
