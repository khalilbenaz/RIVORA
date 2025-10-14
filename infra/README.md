# RIVORA Framework - DevOps & Observability Infrastructure

Wave 1: DevOps & Observability Stack implementation for RIVORA Framework.

## Stack Components

| Component | Port | Description |
|-----------|------|-------------|
| **Grafana** | 3000 | Visualization & Dashboards |
| **Prometheus** | 9090 | Metrics Collection & Alerting |
| **Loki** | 3100 | Log Aggregation |
| **Tempo** | 3200 | Distributed Tracing |
| **OpenTelemetry Collector** | 4317 (gRPC), 4318 (HTTP) | Telemetry Pipeline |

## Quick Start

### 1. Start the Observability Stack

```bash
cd infra
docker-compose -f docker-compose.dev.yml up -d
```

### 2. Verify Services

```bash
# Check all containers are running
docker-compose -f docker-compose.dev.yml ps

# Test endpoints
curl http://localhost:3000/api/health  # Grafana
curl http://localhost:9090/-/healthy   # Prometheus
curl http://localhost:3100/ready       # Loki
curl http://localhost:3200/ready       # Tempo
```

### 3. Access Grafana

- URL: http://localhost:3000
- Username: `admin`
- Password: `admin`

Datasources and dashboards are auto-provisioned.

## Configuration Files

### Docker Compose
- `docker-compose.dev.yml` - Main compose file with all services

### Prometheus
- `prometheus/prometheus.yml` - Scrape configurations
- `prometheus/alerts.yml` - Alert rules

### OpenTelemetry Collector
- `otel-collector-config.yml` - Telemetry routing configuration

### Grafana
- `grafana/datasources.yml` - Auto-provisioned datasources
- `grafana/dashboards.yml` - Dashboard provisioning config
- `grafana/dashboards/overview.json` - Main overview dashboard

### Loki
- `loki-config.yml` - Log aggregation configuration

### Tempo
- `tempo-config.yml` - Distributed tracing configuration

## Application Integration

### Serilog Configuration (Console + File + OTLP)

The application is configured to send logs to three sinks:

1. **Console** - For local development
2. **File** - Rolling file logs in `logs/` directory
3. **OpenTelemetry (OTLP)** - Sends to Loki via OTel Collector

### OpenTelemetry Configuration

Tracing and metrics are automatically collected for:
- ASP.NET Core requests
- HTTP Client calls
- SQL Client queries
- .NET Runtime metrics
- Process metrics

## Grafana Dashboards

### RIVORA Framework - Overview

The overview dashboard includes:

1. **Summary Stats**
   - Request Rate
   - Error Rate
   - P95 Latency
   - Services Up

2. **HTTP Metrics**
   - Requests by Status Code
   - Request Latency Percentiles (P50, P90, P95, P99)

3. **Logs (Loki)**
   - Application Logs viewer
   - Log Volume by Level

4. **Traces (Tempo)**
   - Recent Traces search

5. **Infrastructure**
   - Container Memory Usage
   - Container CPU Usage

## Prometheus Metrics

Available metrics include:

- `http_requests_total` - Total HTTP requests
- `http_request_duration_seconds` - Request duration histogram
- `kba_*` - Custom application metrics
- `dotnet_*` - .NET runtime metrics
- `process_*` - Process metrics

## Alerting Rules

Pre-configured alerts in `prometheus/alerts.yml`:

- **HighErrorRate** - Error rate > 5%
- **HighResponseTime** - P95 latency > 1s
- **ServiceDown** - Service unavailable
- **HighMemoryUsage** - Memory > 80%
- **HighCPUUsage** - CPU > 80%
- **LokiDown** - Loki unavailable
- **TempoDown** - Tempo unavailable

## Local Development Workflow

1. Start the observability stack:
   ```bash
   docker-compose -f docker-compose.dev.yml up -d
   ```

2. Run your .NET application:
   ```bash
   cd src/RVR.Framework.Api
   dotnet run
   ```

3. The application will automatically send:
   - Logs to OTel Collector (→ Loki)
   - Traces to OTel Collector (→ Tempo)
   - Metrics to OTel Collector (→ Prometheus)

4. View everything in Grafana at http://localhost:3000

## Stopping the Stack

```bash
# Stop all services
docker-compose -f docker-compose.dev.yml down

# Stop and remove volumes (clean slate)
docker-compose -f docker-compose.dev.yml down -v
```

## Troubleshooting

### Grafana datasources not loading
- Check Grafana logs: `docker logs kba-grafana`
- Verify datasources.yml syntax

### No logs in Loki
- Check OTel Collector logs: `docker logs kba-otel-collector`
- Verify application is sending to correct endpoint

### No traces in Tempo
- Verify OTLP endpoint in application: `http://localhost:4317`
- Check Tempo logs: `docker logs kba-tempo`

### Prometheus targets down
- Check Prometheus targets: http://localhost:9090/targets
- Verify scrape configs in prometheus.yml

## Next Steps (Future Waves)

- Wave 2: CI/CD Pipeline Enhancement
- Wave 3: Production Deployment (Kubernetes)
- Wave 4: Advanced Alerting & On-Call
- Wave 5: Cost Optimization & Retention Policies
