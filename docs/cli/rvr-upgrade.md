# rvr upgrade

Migration assistant between major RIVORA Framework versions. Automatically detects breaking changes and applies transformations.

## Syntax

```bash
rvr upgrade [options]
```

## Options

| Option | Description |
|--------|-------------|
| `--to <version>` | Target version to upgrade to |
| `--dry-run` | Preview changes without applying them |
| `--list` | List all available migrations |

## Available Migrations

| From | To | Breaking Changes | Auto-transforms |
|------|-----|-----------------|-----------------|
| 3.0.0 | 3.1.0 | IRvrRepository rename, AddRvrFramework split | 2 |
| 3.1.0 | 3.2.0 | MultiTenancyMode rename, IEventBus moved | 3 |
| 3.2.0 | 3.3.0 | API registration change, Privacy module extraction | 2 |
| 3.3.0 | 4.0.0 | TFM upgrade, entity simplification, KBA->RVR rename | 6 |

## Examples

```bash
# List all migrations and their status
rvr upgrade --list

# Preview migration to v4.0
rvr upgrade --to 4.0 --dry-run

# Apply migration to v4.0
rvr upgrade --to 4.0

# Auto-detect latest version and upgrade
rvr upgrade
```

## How It Works

1. **Detection** — Scans `.csproj` and `Directory.Build.props` to determine current version
2. **Planning** — Finds applicable migrations between current and target versions
3. **Analysis** — Scans project files for patterns that need transformation
4. **Execution** — Applies automatic transforms (string replacements, version bumps)
5. **Report** — Lists manual steps that require developer attention

## Automatic Transformations

- Package version updates in `.csproj` files
- Method renames in `.cs` files (e.g., `AddKba*` -> `AddRvr*`)
- Target framework updates (e.g., `net8.0` -> `net9.0`)
- Configuration key renames in `appsettings*.json`

## Manual Steps

Some breaking changes cannot be automated and are reported at the end:
- Entity base class changes
- Custom middleware updates
- Architecture pattern changes

After upgrade, always run:
```bash
dotnet build
dotnet test
```
