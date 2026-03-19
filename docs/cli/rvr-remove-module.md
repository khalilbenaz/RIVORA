# rvr remove-module

Remove a RIVORA module cleanly from a project. Symmetric to `rvr add-module`.

## Syntax

```bash
rvr remove-module <name> [options]
```

## Arguments

| Argument | Description |
|----------|-------------|
| `name` | Module name (e.g., `Caching`, `RVR.Framework.Caching`) |

## Options

| Option | Description |
|--------|-------------|
| `--dry-run` | Preview changes without applying them |
| `--force` | Ignore dependency warnings |

## Operations

1. Removes `<PackageReference>` from `.csproj` files
2. Removes `Add*()` and `Use*()` calls from `Program.cs`
3. Removes configuration section from `appsettings*.json`
4. Deletes dedicated config files for the module
5. Checks if other modules depend on the removed module

## Examples

```bash
# Remove Caching module
rvr remove-module Caching

# Preview removal without changes
rvr remove-module Caching --dry-run

# Force removal despite dependency warnings
rvr remove-module Email --force

# Works with full package name
rvr remove-module RVR.Framework.Notifications
```

## Dependency Warnings

The command detects known dependencies between modules:
- `Caching` is used by: Notifications, Jobs, Search
- `Events` is used by: Notifications, Workflow, Webhooks
- `Security` is used by: MultiTenancy, Privacy, AuditLogging

When a dependency is found, you'll be prompted to confirm the removal.
