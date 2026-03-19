# rvr doctor

Run diagnostic checks on your RIVORA project to identify configuration issues, missing dependencies, and potential problems.

## Usage

```bash
rvr doctor [options]
```

## Options

| Flag | Description | Default |
|------|-------------|---------|
| `--fix` | Attempt to auto-fix detected issues | `false` |
| `--verbose` | Show detailed check output | `false` |
| `--json` | Output results as JSON | `false` |

## Health Checks

The doctor command verifies the following:

| Check | Description |
|-------|-------------|
| .NET SDK | Verifies the correct .NET SDK version is installed |
| NuGet packages | Checks for outdated or vulnerable packages |
| Database | Tests database connectivity |
| Redis | Tests Redis connection (if configured) |
| RabbitMQ | Tests RabbitMQ connection (if configured) |
| Docker | Verifies Docker is available and running |
| Configuration | Validates `rivora.json` and `appsettings.json` |
| Architecture | Checks project structure conforms to Clean Architecture |
| Migrations | Detects pending database migrations |

## Output

```bash
rvr doctor
```

```
RIVORA Doctor - Project Health Check
─────────────────────────────────────

  .NET SDK 9.0.1          OK
  NuGet packages           OK (32 packages up to date)
  PostgreSQL               OK (localhost:5432)
  Redis                    OK (localhost:6379)
  RabbitMQ                 WARNING (not configured)
  Docker                   OK (Docker 27.1.1)
  Configuration            OK
  Architecture             OK (Clean Architecture validated)
  Pending migrations       WARNING (1 pending)

Score: 8/9  (89%)

Recommendations:
  - Apply pending migration: 20260320_AddOrderTable
  - Consider configuring RabbitMQ for async messaging
```

## Examples

Run doctor with auto-fix:

```bash
rvr doctor --fix
```

Output as JSON for CI integration:

```bash
rvr doctor --json > doctor-report.json
```

Use in CI to fail on issues:

```bash
rvr doctor --json | jq -e '.score == .total'
```
