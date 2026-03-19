# rvr remove-module

Remove a RIVORA module from your project. This uninstalls NuGet packages, removes configuration entries, and cleans up generated files.

## Usage

```bash
rvr remove-module <module-name> [options]
```

## Options

| Flag | Description | Default |
|------|-------------|---------|
| `--dry-run` | Preview what would be removed without making changes | `false` |
| `--force` | Skip confirmation prompt and remove even if other modules depend on it | `false` |
| `--keep-config` | Keep configuration entries in `appsettings.json` | `false` |
| `--keep-data` | Preserve database tables (skip migration generation) | `false` |

## Dependency Check

Before removing a module, the CLI checks for dependencies. If other modules depend on the one being removed, you will see a warning:

```bash
rvr remove-module Caching
```

```
WARNING: The following modules depend on Caching:
  - Jobs (uses distributed cache for deduplication)
  - Billing (uses cache for rate limit state)

Use --force to remove anyway, or remove dependent modules first.
```

## Examples

Preview removal before executing:

```bash
rvr remove-module Caching --dry-run
```

```
Dry run - the following changes would be made:
  - Remove NuGet package: Rivora.Modules.Caching
  - Remove configuration section: Caching
  - Remove file: src/MyProject.Infrastructure/Caching/CacheService.cs
  - Generate migration: RemoveCachingTables
```

Force remove a module with dependencies:

```bash
rvr remove-module Caching --force
```

Remove a module but keep its database tables:

```bash
rvr remove-module Audit --keep-data
```

Remove a module but keep its configuration:

```bash
rvr remove-module SMS --keep-config
```
