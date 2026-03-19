# Monitoring avec Grafana

RIVORA integre une stack d'observabilite complete basee sur Prometheus, Grafana, OpenTelemetry et Loki. Ce guide explique comment configurer et utiliser le monitoring en developpement et en production.

## Architecture de la stack

```
Application RIVORA
    |
    v
OTEL Collector (port 4317/4318)
    |
    +---> Prometheus (metriques)   ---> Grafana (dashboards)
    +---> Loki (logs)              ---> Grafana (logs)
    +---> Tempo (traces)           ---> Grafana (traces)
    +---> Jaeger (traces alt.)
```

## Demarrage rapide

### Lancer la stack d'observabilite

```bash
# Stack complete (recommande)
docker compose -f infra/docker-compose.dev.yml up -d

# Verifier que tout tourne
docker compose -f infra/docker-compose.dev.yml ps
```

Services disponibles :

| Service | URL | Identifiants |
|---------|-----|--------------|
| Grafana | `http://localhost:3000` | `admin` / `admin` |
| Prometheus | `http://localhost:9090` | - |
| Jaeger UI | `http://localhost:16686` | - |
| Seq | `http://localhost:8081` | - |

## Configuration OpenTelemetry

### Installation des packages

```xml
<ItemGroup>
  <PackageReference Include="OpenTelemetry.Extensions.Hosting" Version="1.10.0" />
  <PackageReference Include="OpenTelemetry.Instrumentation.AspNetCore" Version="1.10.0" />
  <PackageReference Include="OpenTelemetry.Instrumentation.Http" Version="1.10.0" />
  <PackageReference Include="OpenTelemetry.Instrumentation.SqlClient" Version="1.10.0" />
  <PackageReference Include="OpenTelemetry.Instrumentation.EntityFrameworkCore" Version="1.0.0" />
  <PackageReference Include="OpenTelemetry.Exporter.OpenTelemetryProtocol" Version="1.10.0" />
  <PackageReference Include="OpenTelemetry.Exporter.Prometheus.AspNetCore" Version="1.10.0" />
</ItemGroup>
```

### Configuration dans Program.cs

```csharp
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService(
            serviceName: "RVR.Framework.Api",
            serviceVersion: typeof(Program).Assembly.GetName().Version?.ToString() ?? "1.0.0"))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation(options =>
        {
            options.RecordException = true;
            options.Filter = (ctx) => !ctx.Request.Path.StartsWithSegments("/health");
        })
        .AddHttpClientInstrumentation()
        .AddEntityFrameworkCoreInstrumentation()
        .AddSqlClientInstrumentation(options => options.SetDbStatementForText = true)
        .AddSource("RVR.Framework")
        .AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri("http://localhost:4317");
        }))
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddRuntimeInstrumentation()
        .AddProcessInstrumentation()
        .AddMeter("RVR.Framework")
        .AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri("http://localhost:4317");
        })
        .AddPrometheusExporter());

// Exposer les metriques Prometheus
app.MapPrometheusScrapingEndpoint();
```

### Configuration via appsettings.json

```json
{
  "OpenTelemetry": {
    "ServiceName": "RVR.Framework.Api",
    "Endpoint": "http://localhost:4317",
    "Tracing": {
      "Enabled": true,
      "SamplingRatio": 1.0,
      "ExcludedPaths": ["/health", "/metrics", "/swagger"]
    },
    "Metrics": {
      "Enabled": true,
      "ExportInterval": 15000
    },
    "Logging": {
      "Enabled": true,
      "IncludeScopes": true
    }
  }
}
```

## Metriques Prometheus

### Metriques par defaut

L'application expose automatiquement ces metriques sur `/metrics` :

| Metrique | Type | Description |
|----------|------|-------------|
| `http_server_request_duration_seconds` | Histogram | Duree des requetes HTTP |
| `http_server_active_requests` | Gauge | Requetes actives |
| `http_client_request_duration_seconds` | Histogram | Duree des appels HTTP sortants |
| `process_cpu_usage` | Gauge | Utilisation CPU |
| `process_memory_usage` | Gauge | Utilisation memoire |
| `dotnet_gc_collections_total` | Counter | Collections GC |

### Metriques business personnalisees

Creez vos propres metriques pour suivre les indicateurs metier :

```csharp
using System.Diagnostics.Metrics;

public class BusinessMetrics
{
    private readonly Counter<long> _ordersCreated;
    private readonly Histogram<double> _orderAmount;
    private readonly UpDownCounter<long> _activeUsers;
    private readonly ObservableGauge<int> _pendingOrders;

    public BusinessMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create("RVR.Framework.Business");

        _ordersCreated = meter.CreateCounter<long>(
            "orders.created",
            unit: "orders",
            description: "Nombre de commandes creees");

        _orderAmount = meter.CreateHistogram<double>(
            "orders.amount",
            unit: "EUR",
            description: "Montant des commandes");

        _activeUsers = meter.CreateUpDownCounter<long>(
            "users.active",
            description: "Utilisateurs actifs");

        _pendingOrders = meter.CreateObservableGauge(
            "orders.pending",
            () => GetPendingOrderCount(),
            description: "Commandes en attente");
    }

    public void RecordOrderCreated(decimal amount, string tenantId)
    {
        _ordersCreated.Add(1,
            new KeyValuePair<string, object?>("tenant", tenantId));
        _orderAmount.Record((double)amount,
            new KeyValuePair<string, object?>("tenant", tenantId));
    }

    public void UserConnected() => _activeUsers.Add(1);
    public void UserDisconnected() => _activeUsers.Add(-1);
}
```

Enregistrement :

```csharp
builder.Services.AddSingleton<BusinessMetrics>();
builder.Services.AddOpenTelemetry()
    .WithMetrics(metrics => metrics.AddMeter("RVR.Framework.Business"));
```

## Dashboards Grafana

### Import des dashboards RIVORA

RIVORA fournit des dashboards pre-configures dans `infra/grafana/dashboards/` :

1. **API Overview** : Requetes/sec, latence P95, taux d'erreur, requetes actives
2. **Database Performance** : Temps de query, connexions actives, slow queries
3. **Business Metrics** : Commandes, utilisateurs, revenus par tenant
4. **Infrastructure** : CPU, memoire, GC, threads

Pour les importer :

```bash
# Les dashboards sont montes automatiquement via docker-compose
# Ou importez manuellement via Grafana UI > Dashboards > Import
```

### Creer un dashboard personnalise

Exemple de requete PromQL pour un panel :

```promql
# Requetes par seconde
rate(http_server_request_duration_seconds_count[5m])

# Latence P95
histogram_quantile(0.95, rate(http_server_request_duration_seconds_bucket[5m]))

# Taux d'erreur (%)
100 * sum(rate(http_server_request_duration_seconds_count{http_response_status_code=~"5.."}[5m]))
  / sum(rate(http_server_request_duration_seconds_count[5m]))

# Commandes par tenant
sum by (tenant) (rate(orders_created_total[5m]))
```

### Alertes Grafana

Configurez des alertes dans Grafana pour etre notifie en cas de probleme :

```yaml
# infra/grafana/provisioning/alerting/rules.yml
groups:
  - name: rivora-alerts
    rules:
      - alert: HighErrorRate
        expr: |
          sum(rate(http_server_request_duration_seconds_count{http_response_status_code=~"5.."}[5m]))
          / sum(rate(http_server_request_duration_seconds_count[5m])) > 0.05
        for: 5m
        labels:
          severity: critical
        annotations:
          summary: "Taux d'erreur superieur a 5%"

      - alert: HighLatency
        expr: |
          histogram_quantile(0.95, rate(http_server_request_duration_seconds_bucket[5m])) > 2
        for: 5m
        labels:
          severity: warning
        annotations:
          summary: "Latence P95 superieure a 2 secondes"

      - alert: HighMemoryUsage
        expr: process_memory_usage > 1073741824
        for: 10m
        labels:
          severity: warning
        annotations:
          summary: "Utilisation memoire superieure a 1 GB"
```

## Configuration OTEL Collector

Le fichier de configuration du collecteur OpenTelemetry :

```yaml
# infra/otel-collector-config.yml
receivers:
  otlp:
    protocols:
      grpc:
        endpoint: 0.0.0.0:4317
      http:
        endpoint: 0.0.0.0:4318

processors:
  batch:
    timeout: 10s
    send_batch_size: 1024
  memory_limiter:
    check_interval: 5s
    limit_mib: 512

exporters:
  prometheus:
    endpoint: 0.0.0.0:8889
  otlp/tempo:
    endpoint: tempo:4317
    tls:
      insecure: true
  loki:
    endpoint: http://loki:3100/loki/api/v1/push

service:
  pipelines:
    traces:
      receivers: [otlp]
      processors: [batch, memory_limiter]
      exporters: [otlp/tempo]
    metrics:
      receivers: [otlp]
      processors: [batch]
      exporters: [prometheus]
    logs:
      receivers: [otlp]
      processors: [batch]
      exporters: [loki]
```

## Production

### Bonnes pratiques

- **Sampling** : En production, reduisez le taux d'echantillonnage des traces a 10-20% :

```csharp
.WithTracing(tracing => tracing
    .SetSampler(new TraceIdRatioBasedSampler(0.1))) // 10%
```

- **Retention** : Configurez la retention des donnees (par defaut 15 jours pour Prometheus)
- **Ressources** : Allouez au moins 2 GB de RAM a Prometheus et 1 GB a Loki
- **Securite** : Placez Grafana derriere un reverse proxy avec authentification

### Health checks

RIVORA expose des health checks sur `/health` :

```bash
curl http://localhost:5220/health
```

Reponse :

```json
{
  "status": "Healthy",
  "checks": {
    "database": "Healthy",
    "redis": "Healthy",
    "rabbitmq": "Healthy",
    "disk": "Healthy"
  },
  "duration": "00:00:00.0234567"
}
```

## Etape suivante

- [Docker](/guide/docker) pour la configuration de l'infrastructure
- [CI/CD](/guide/ci-cd) pour integrer le monitoring dans votre pipeline
- [Native AOT](/guide/native-aot) pour optimiser les performances
