# rvr upgrade

Upgrade your RIVORA project to a newer version. Handles NuGet package updates, configuration migrations, and breaking change detection.

## Usage

```bash
rvr upgrade [options]
```

## Options

| Flag | Description | Default |
|------|-------------|---------|
| `--list` | List available upgrade versions | `false` |
| `--to <version>` | Target version to upgrade to | Latest |
| `--dry-run` | Preview upgrade steps without applying | `false` |
| `--skip-backup` | Skip project backup before upgrading | `false` |
| `--force` | Skip confirmation prompts | `false` |

## List Available Versions

```bash
rvr upgrade --list
```

```
Available Upgrades
──────────────────
  3.1.0    Current
  3.2.0    Minor - New export formats, bug fixes
  4.0.0    Major - .NET 10 support, new module system

Use: rvr upgrade --to <version>
```

## Preview an Upgrade

```bash
rvr upgrade --to 4.0 --dry-run
```

```
Upgrade Plan: 3.1.0 -> 4.0.0
─────────────────────────────

Steps:
  1. Update 12 NuGet packages
  2. Migrate rivora.json schema (v3 -> v4)
  3. Rename namespace: Rivora.Core -> Rivora.Framework
  4. Update DI registration: AddRivora() -> AddRivoraFramework()
  5. Generate database migration for schema changes

Breaking Changes:
  - IRepository<T> interface updated (new method: GetPagedAsync)
  - ITenantStore renamed to ITenantProvider
  - Removed: AddRivoraLegacyAuth() (use AddRivoraSecurity())

Run without --dry-run to apply.
```

## Apply an Upgrade

```bash
rvr upgrade --to 4.0
```

The CLI will:

1. Create a backup of your project
2. Update all RIVORA NuGet packages
3. Apply automated code transformations
4. Update configuration files
5. Generate required database migrations
6. Display any manual steps needed

## Examples

Upgrade to the latest version:

```bash
rvr upgrade
```

Upgrade to a specific version with dry-run:

```bash
rvr upgrade --to 3.2.0 --dry-run
```

Upgrade without backup (CI environments):

```bash
rvr upgrade --to 4.0 --skip-backup --force
```
