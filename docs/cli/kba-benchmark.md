# rvr benchmark - RVR CLI

Load testing avec k6.

## Syntaxe

```bash
rvr benchmark <url> [options]
```

## Arguments

| Argument | Description | Required |
|----------|-------------|----------|
| `url` | URL cible à tester | Oui |

## Options

| Option | Description | Default |
|--------|-------------|---------|
| --duration | Durée du test (ex: 30s, 5m) | 1m |
| --vus | Nombre de virtual users | 10 |
| --output | Format de sortie | console |
| --scenario | Scénario de test | load |

## Scénarios Disponibles

| Scénario | Description |
|----------|-------------|
| `smoke` | Test rapide de validation |
| `load` | Test de charge normal |
| `stress` | Test de stress extrême |
| `spike` | Test de pic de charge |
| `soak` | Test d'endurance longue durée |

## Exemples

### Test de charge basique

```bash
rvr benchmark http://localhost:5000/api/products
```

### Test avec durée et VUS personnalisés

```bash
rvr benchmark http://localhost:5000/api/products --duration 5m --vus 50
```

### Test de stress

```bash
rvr benchmark http://localhost:5000/api/products --scenario stress --duration 2m --vus 100
```

### Test de smoke

```bash
rvr benchmark http://localhost:5000/health --scenario smoke --duration 30s --vus 5
```

### Test avec output JSON

```bash
rvr benchmark http://localhost:5000/api/products --output json --duration 1m
```

## Output Example

```
╭────────────────────────────────────────╮
│          RVR Benchmark                 │
│   Load Testing with k6                 │
╰────────────────────────────────────────╯

Target: http://localhost:5000/api/products
Scenario: load
Duration: 1m
VUs: 10

Running...

     ✓ HTTP Request Duration
       avg: 45.2ms
       min: 12ms
       med: 42ms
       max: 156ms
       p(90): 78ms
       p(95): 95ms

     ✓ HTTP Request Failed
       count: 0

     ✓ HTTP Request Count
       count: 1250

     ✓ Data Received
       total: 2.5 MB

     ✓ Data Sent
       total: 125 KB

Summary:
  Requests: 1250
  Duration: 1m 0s
  Avg Response: 45.2ms
  P95 Response: 95ms
  Errors: 0
  Throughput: 20.8 req/s

Status: PASSED
```

## Scénarios Détaillés

### Smoke Test

```bash
# Validation rapide (30s, 5 VUs)
rvr benchmark http://localhost:5000/health --scenario smoke
```

Paramètres smoke :
- Duration: 30s
- VUs: 5
- Objectif: Validation basique

### Load Test

```bash
# Test de charge normal (1m, 10 VUs)
rvr benchmark http://localhost:5000/api/products --scenario load
```

Paramètres load :
- Duration: 1m
- VUs: 10
- Objectif: Performance normale

### Stress Test

```bash
# Test de stress (2m, 100 VUs)
rvr benchmark http://localhost:5000/api/products --scenario stress --vus 100
```

Paramètres stress :
- Duration: 2m
- VUs: 100
- Objectif: Point de rupture

### Spike Test

```bash
# Test de pic (1m, 50-200 VUs)
rvr benchmark http://localhost:5000/api/products --scenario spike
```

Paramètres spike :
- Duration: 1m
- VUs: 50 -> 200 -> 50
- Objectif: Réaction aux pics

### Soak Test

```bash
# Test d'endurance (30m, 20 VUs)
rvr benchmark http://localhost:5000/api/products --scenario soak --duration 30m --vus 20
```

Paramètres soak :
- Duration: 30m
- VUs: 20
- Objectif: Fuites mémoire

## Metrics

| Metric | Description |
|--------|-------------|
| avg | Temps de réponse moyen |
| min | Temps de réponse minimum |
| med | Médiane |
| max | Temps de réponse maximum |
| p(90) | 90ème percentile |
| p(95) | 95ème percentile |
| count | Nombre de requêtes |
| failed | Nombre d'échecs |

## Output Formats

| Format | Description |
|--------|-------------|
| `console` | Output texte dans le terminal |
| `json` | Output JSON pour processing |
| `html` | Rapport HTML (si disponible) |

## Integration CI/CD

### GitHub Actions

```yaml
name: Performance Test

on:
  push:
    branches: [main]

jobs:
  benchmark:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      
      - name: Install RVR.CLI
        run: dotnet tool install -g RVR.CLI
      
      - name: Start Application
        run: dotnet run --project src/MyApp.Api &
      
      - name: Wait for app
        run: sleep 10
      
      - name: Run Benchmark
        run: rvr benchmark http://localhost:5000/health --scenario smoke
      
      - name: Run Load Test
        run: rvr benchmark http://localhost:5000/api/products --duration 2m --vus 20 --output json > results.json
      
      - name: Upload Results
        uses: actions/upload-artifact@v4
        with:
          name: benchmark-results
          path: results.json
```

## Voir aussi

- [Health Checks](../modules/health-checks.md) - Monitoring
- [Performance](../quickstart.md) - Optimisation performance
- [k6 Documentation](https://k6.io/docs/) - k6 reference
