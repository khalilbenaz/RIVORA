# rvr benchmark

Run performance and load tests against your RIVORA application.

## Usage

```bash
rvr benchmark [options]
```

## Options

| Flag | Description | Default |
|------|-------------|---------|
| `--scenario` | Test scenario type | `smoke` |
| `--duration` | Test duration (e.g., `30s`, `5m`, `1h`) | `30s` |
| `--users` | Number of concurrent virtual users | `10` |
| `--endpoint` | Target endpoint URL | `http://localhost:5220` |
| `--output` | Output format (`console`, `json`, `html`) | `console` |
| `--output-file` | Write results to file | None |

## Scenarios

### smoke

Quick validation that the application responds correctly under minimal load.

```bash
rvr benchmark --scenario smoke
```

### load

Simulate normal expected traffic patterns.

```bash
rvr benchmark --scenario load --users 50 --duration 5m
```

### stress

Push the application beyond normal capacity to find breaking points.

```bash
rvr benchmark --scenario stress --users 200 --duration 10m
```

### spike

Test sudden traffic spikes to evaluate auto-scaling and recovery.

```bash
rvr benchmark --scenario spike --users 500 --duration 2m
```

### soak

Long-running test to detect memory leaks and resource exhaustion.

```bash
rvr benchmark --scenario soak --users 20 --duration 1h
```

## Output

```
RIVORA Benchmark - Load Test
─────────────────────────────

Scenario:    load
Duration:    5m 00s
VUsers:      50

Results:
  Total requests:      14,832
  Requests/sec:        49.4
  Avg response time:   42ms
  P95 response time:   128ms
  P99 response time:   312ms
  Error rate:          0.02%
  Throughput:          2.1 MB/s
```

## Examples

Run a smoke test and export HTML report:

```bash
rvr benchmark --scenario smoke --output html --output-file report.html
```

Stress test a specific endpoint:

```bash
rvr benchmark --scenario stress \
  --endpoint http://localhost:5220/api/products \
  --users 200 \
  --duration 5m
```

Soak test with JSON output for automated analysis:

```bash
rvr benchmark --scenario soak \
  --duration 1h \
  --output json \
  --output-file soak-results.json
```
