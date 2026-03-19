# rvr env

Manage environments, configuration variables, and secrets.

## Usage

```bash
rvr env <subcommand> [options]
```

## Subcommands

### list

List all configured environments.

```bash
rvr env list
```

Output:

```
Environment     Status
──────────────────────────
Development     Active
Staging         Configured
Production      Configured
```

### get

Get the value of a configuration variable.

```bash
rvr env get DB:Host
rvr env get ConnectionStrings:Default --env Production
```

### set

Set a configuration variable.

```bash
rvr env set DB:Host "localhost"
rvr env set DB:Port "5432" --env Staging
```

### remove

Remove a configuration variable.

```bash
rvr env remove DB:Port
rvr env remove DB:Port --env Staging
```

### switch

Switch the active environment.

```bash
rvr env switch Staging
rvr env switch Production
```

### diff

Compare configuration between two environments.

```bash
rvr env diff Development Production
```

Output:

```
Variable                Development          Production
─────────────────────────────────────────────────────────
DB:Host                 localhost            db.prod.internal
DB:Port                 5432                 5432
Redis:Endpoint          localhost:6379       redis.prod:6379
Logging:Level           Debug                Warning
```

### secrets

Manage user secrets for local development.

```bash
rvr env secrets init                     # Initialize User Secrets
rvr env secrets set "Stripe:ApiKey" "sk_test_..."
rvr env secrets list
rvr env secrets clear
```

### export

Export environment configuration to a file.

```bash
rvr env export --format dotenv           # Export as .env
rvr env export --format json             # Export as JSON
rvr env export --format yaml --env Production
```

### import

Import configuration from a file.

```bash
rvr env import --file .env
rvr env import --file config.json --env Staging
```

## Options

| Flag | Description | Default |
|------|-------------|---------|
| `--env` | Target environment | Active environment |
| `--format` | Export/import format (`dotenv`, `json`, `yaml`) | `dotenv` |
| `--file` | File path for import/export | Auto-generated |

## Examples

Set up a new environment from an existing `.env` file:

```bash
rvr env import --file .env.staging --env Staging
rvr env switch Staging
```

Compare and sync environments:

```bash
rvr env diff Development Staging
rvr env export --env Development --format dotenv > .env.dev
```
