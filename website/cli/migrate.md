# rvr migrate

Manage database migrations: generate, apply, list, and rollback.

## Usage

```bash
rvr migrate <subcommand> [options]
```

## Subcommands

### generate

Create a new migration from pending model changes.

```bash
rvr migrate generate AddOrderTable
rvr migrate generate AddIndexOnEmail --context IdentityDbContext
```

### apply

Apply all pending migrations to the database.

```bash
rvr migrate apply
rvr migrate apply --to AddOrderTable    # Apply up to a specific migration
rvr migrate apply --context AppDbContext # Target a specific DbContext
```

### list

List all migrations and their status.

```bash
rvr migrate list
```

Output:

```
Migration                    Status      Applied At
──────────────────────────────────────────────────────
20260101_InitialCreate       Applied     2026-01-01 10:00
20260115_AddProductTable     Applied     2026-01-15 14:30
20260320_AddOrderTable       Pending     --
```

### rollback

Roll back the last applied migration or roll back to a specific migration.

```bash
rvr migrate rollback                        # Rollback last migration
rvr migrate rollback --to AddProductTable   # Rollback to specific point
rvr migrate rollback --steps 3              # Rollback last 3 migrations
```

## Options

| Flag | Description | Default |
|------|-------------|---------|
| `--context` | Target DbContext name | Auto-detected |
| `--to` | Target migration name | Latest (apply) / Previous (rollback) |
| `--steps` | Number of migrations to rollback | `1` |
| `--connection` | Override connection string | From configuration |
| `--dry-run` | Preview SQL without executing | `false` |
| `--verbose` | Show generated SQL | `false` |

## Examples

Generate and apply a migration:

```bash
rvr migrate generate AddCustomerAddress
rvr migrate apply
```

Preview the SQL that would be executed:

```bash
rvr migrate apply --dry-run --verbose
```

Rollback the last migration in a specific context:

```bash
rvr migrate rollback --context TenantDbContext
```

## Seeding

Seeders work alongside migrations to populate data:

```bash
rvr seed --profile demo               # Seed with demo profile
rvr seed --reset --profile test        # Truncate and reseed
rvr seed --dry-run                     # Preview seed operations
rvr generate seed Product              # Scaffold a new seeder
```
