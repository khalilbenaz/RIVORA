# Health Checks - KBA Framework

Système de monitoring complet avec 80+ health checks pour surveiller l'état de votre application.

## Table des Matières

- [Vue d'ensemble](#vue-densemble)
- [Installation](#installation)
- [Configuration](#configuration)
- [Health Checks Disponibles](#health-checks-disponibles)
- [Endpoints](#endpoints)
- [Dashboard](#dashboard)
- [Custom Health Checks](#custom-health-checks)
- [Alerting](#alerting)

---

## Vue d'ensemble

KBA Framework Health Checks fournit :

| Feature | Description |
|---------|-------------|
| **80+ Checks** | Database, Redis, RabbitMQ, AI providers, etc. |
| **Multiple Endpoints** | /health, /health/ready, /health/detailed |
| **Custom Writers** | JSON, Prometheus, UI |
| **Dashboard** | Health Checks UI intégré |
| **Alerting** | Integration avec Prometheus, Datadog, etc. |

---

## Installation

```bash
dotnet add package KBA.Framework.HealthChecks
```

---

## Configuration

### Configuration de base

```csharp
using KBA.Framework.HealthChecks.Extensions;

// Dans Program.cs
builder.Services.AddKbaHealthChecks();

// Configuration des endpoints
app.UseKbaHealthChecks(healthPath: "/health", readyPath: "/health/ready");
```

### Configuration complète

```csharp
builder.Services.AddKbaHealthChecks(options =>
{
    options.IncludeDatabase = true;
    options.IncludeRedis = true;
    options.IncludeRabbitMq = true;
    options.IncludeAiProviders = true;
    options.IncludeJobs = true;
    options.DefaultTimeout = TimeSpan.FromSeconds(30);
    options.EnableDetailedResponses = true;
});
```

---

## Health Checks Disponibles

### Database Health Checks

```csharp
// SQL Server
builder.Services.AddDatabaseHealthCheck(
    builder.Configuration.GetConnectionString("DefaultConnection"),
    databaseType: "SqlServer",
    name: "sqlserver");

// PostgreSQL
builder.Services.AddDatabaseHealthCheck(
    builder.Configuration.GetConnectionString("PostgresConnection"),
    databaseType: "PostgreSQL",
    name: "postgres");

// MySQL
builder.Services.AddDatabaseHealthCheck(
    builder.Configuration.GetConnectionString("MySqlConnection"),
    databaseType: "MySQL",
    name: "mysql");

// EF Core
builder.Services.AddEfCoreHealthCheck<AppDbContext>(
    name: "database-efcore");
```

### Redis Health Check

```csharp
builder.Services.AddRedisHealthCheck(
    builder.Configuration.GetConnectionString("Redis"),
    name: "redis");
```

### RabbitMQ Health Check

```csharp
builder.Services.AddRabbitMqHealthCheck(
    builder.Configuration.GetConnectionString("RabbitMQ"),
    name: "rabbitmq");
```

### AI Provider Health Check

```csharp
builder.Services.AddAiProviderHealthCheck(
    providerName: "OpenAI",
    apiKey: builder.Configuration["OpenAI:ApiKey"],
    endpoint: "https://api.openai.com/v1",
    name: "openai");

builder.Services.AddAiProviderHealthCheck(
    providerName: "Azure OpenAI",
    apiKey: builder.Configuration["AzureOpenAI:ApiKey"],
    endpoint: builder.Configuration["AzureOpenAI:Endpoint"],
    name: "azure-openai");
```

### Jobs Health Check

```csharp
builder.Services.AddJobsHealthCheck(
    jobMonitor: sp => sp.GetRequiredService<IJobHealthMonitor>(),
    criticalQueues: new[] { "critical" },
    name: "jobs");
```

---

## Endpoints

### Endpoints disponibles

| Endpoint | Description |
|----------|-------------|
| `/health` | Health check principal |
| `/health/ready` | Readiness probe (checks tagged "ready") |
| `/health/detailed` | Health check détaillé avec toutes les informations |
| `/health/live` | Liveness probe (check basique) |

### Configuration des endpoints

```csharp
// Endpoints de base
app.UseKbaHealthChecks(
    healthPath: "/health",
    readyPath: "/health/ready");

// Endpoint détaillé
app.UseKbaDetailedHealthChecks(path: "/health/detailed");

// Endpoint personnalisé
app.MapHealthChecks("/healthz", new HealthCheckOptions
{
    ResponseWriter = HealthCheckResponseWriter.WriteHealthCheckResponseAsync,
    Predicate = check => check.Tags.Contains("critical")
});
```

### Response Format

```json
// /health
{
  "status": "Healthy",
  "duration": "00:00:00.1234567",
  "checks": {
    "database": {
      "status": "Healthy",
      "duration": "00:00:00.0123456"
    },
    "redis": {
      "status": "Healthy",
      "duration": "00:00:00.0034567"
    }
  }
}

// /health/detailed
{
  "status": "Healthy",
  "duration": "00:00:00.1234567",
  "checks": {
    "database": {
      "status": "Healthy",
      "description": "SQL Server connection successful",
      "duration": "00:00:00.0123456",
      "data": {
        "connectionString": "Server=localhost;Database=MyApp;",
        "serverVersion": "SQL Server 2019"
      }
    }
  }
}
```

---

## Dashboard

### Health Checks UI

```csharp
// Activer le dashboard UI
builder.Services.AddHealthChecksUI(options =>
{
    options.SetEvaluationTimeInSeconds(60);
    options.MaximumHistoryEntriesPerEndpoint(100);
    options.AddHealthCheckEndpoint("MyApp", "/health");
});

builder.Services.AddHealthChecksUIInMemoryStorage();

// Mapper le dashboard
app.MapHealthChecksUI("/health-ui");
```

### Configuration du dashboard

```json
{
  "HealthChecksUI": {
    "EvaluationTimeInSeconds": 60,
    "MaximumHistoryEntriesPerEndpoint": 100,
    "HealthCheckDatabaseConnectionString": "Data Source=healthchecks.db"
  }
}
```

---

## Custom Health Checks

### Créer un health check personnalisé

```csharp
using Microsoft.Extensions.Diagnostics.HealthChecks;

public class ExternalApiHealthCheck : IHealthCheck
{
    private readonly HttpClient _httpClient;
    private readonly string _endpoint;

    public ExternalApiHealthCheck(HttpClient httpClient, string endpoint)
    {
        _httpClient = httpClient;
        _endpoint = endpoint;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.GetAsync(_endpoint, cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                return HealthCheckResult.Healthy(
                    "External API is responding");
            }

            return HealthCheckResult.Degradated(
                $"External API returned {response.StatusCode}");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(
                "External API is not responding", ex);
        }
    }
}

// Enregistrement
builder.Services.AddHttpClient("ExternalApi", client =>
{
    client.BaseAddress = new Uri("https://api.external.com");
});

builder.Services.AddHealthChecks()
    .Add(new HealthCheckRegistration(
        "external-api",
        sp => new ExternalApiHealthCheck(
            sp.GetRequiredService<IHttpClientFactory>().CreateClient("ExternalApi"),
            "/health"),
        timeout: TimeSpan.FromSeconds(10),
        tags: new[] { "external", "api" }));
```

### Health Check avec données

```csharp
public class DatabaseHealthCheck : IHealthCheck
{
    private readonly AppDbContext _context;

    public DatabaseHealthCheck(AppDbContext context)
    {
        _context = context;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken)
    {
        try
        {
            var canConnect = await _context.Database.CanConnectAsync(cancellationToken);
            
            if (!canConnect)
            {
                return HealthCheckResult.Unhealthy("Cannot connect to database");
            }

            var pendingMigrations = await _context.Database.GetPendingMigrationsAsync(cancellationToken);
            
            return HealthCheckResult.Healthy(
                "Database connection successful",
                new Dictionary<string, object>
                {
                    { "PendingMigrations", pendingMigrations.Count() },
                    { "ServerVersion", _context.Database.GetDbConnection().ServerVersion }
                });
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Database health check failed", ex);
        }
    }
}
```

---

## Alerting

### Prometheus Metrics

```csharp
// Exporter les metrics Prometheus
builder.Services.AddHealthChecksPrometheusMetrics();

app.MapPrometheusScrapingEndpoint("/metrics");
```

### Integration avec Application Insights

```csharp
builder.Services.AddHealthChecks()
    .AddApplicationInsightsPublisher();
```

### Integration avec Datadog

```csharp
builder.Services.AddHealthChecks()
    .AddDatadogPublisher(options =>
    {
        options.ApiKey = builder.Configuration["Datadog:ApiKey"];
        options.AppName = "MyApp";
    });
```

### Integration avec Seq

```csharp
builder.Services.AddHealthChecks()
    .AddSeqPublisher(options =>
    {
        options.Endpoint = builder.Configuration["Seq:Endpoint"];
        options.ApiKey = builder.Configuration["Seq:ApiKey"];
    });
```

---

## Kubernetes Integration

### Probes configuration

```yaml
# deployment.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: myapp
spec:
  template:
    spec:
      containers:
      - name: myapp
        livenessProbe:
          httpGet:
            path: /health/live
            port: 8080
          initialDelaySeconds: 30
          periodSeconds: 10
        readinessProbe:
          httpGet:
            path: /health/ready
            port: 8080
          initialDelaySeconds: 5
          periodSeconds: 5
```

---

## Bonnes Pratiques

### Tags pour organiser les checks

```csharp
builder.Services.AddHealthChecks()
    .AddCheck("database", () => HealthCheckResult.Healthy(), tags: new[] { "critical", "data" })
    .AddCheck("redis", () => HealthCheckResult.Healthy(), tags: new[] { "critical", "cache" })
    .AddCheck("external-api", () => HealthCheckResult.Healthy(), tags: new[] { "external" })
    .AddCheck("self", () => HealthCheckResult.Healthy(), tags: new[] { "ready" });
```

### Timeout configuration

```csharp
builder.Services.AddHealthChecks()
    .AddCheck("slow-check", new SlowHealthCheck(), 
        timeout: TimeSpan.FromSeconds(30),
        tags: new[] { "slow" });
```

### Health Check Result statuses

```csharp
// Healthy - Tout fonctionne
HealthCheckResult.Healthy("Everything is working");

// Degraded - Fonctionne mais avec problèmes
HealthCheckResult.Degraded("Running in degraded mode");

// Unhealthy - Problème critique
HealthCheckResult.Unhealthy("Critical failure");
```

---

## Voir aussi

- [Database](database.md) - Health checks database
- [Jobs](jobs.md) - Health checks pour jobs
- [Caching](caching.md) - Health checks pour cache
