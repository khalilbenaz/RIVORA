# rvr publish

Unified publish pipeline for RIVORA projects — build, test, and deploy in one command.

## Syntax

```bash
rvr publish [options]
```

## Options

| Option | Description | Default |
|--------|-------------|---------|
| `--target` | Publish target (docker, nuget, azure, self-contained, auto) | `auto` |
| `--skip-tests` | Skip running tests before publishing | `false` |
| `--dry-run` | Show commands without executing them | `false` |
| `--registry` | Container or package registry URL | default |
| `--tag` | Version tag override | auto-detected |

## Targets

### Docker
- Generates optimized Dockerfile if missing
- Builds multi-arch image
- Pushes to Docker Hub / ACR / GHCR
- Auto-tags with project version

### NuGet
- Packs all `IsPackable=true` projects
- Pushes to NuGet.org / GitHub Packages / private feed

### Self-contained
- Publishes standalone binaries for win-x64, linux-x64, osx-x64

### Azure
- Publishes and deploys to Azure App Service / Container Apps

## Examples

```bash
# Auto-detect and publish
rvr publish

# Docker image to custom registry
rvr publish --target docker --registry ghcr.io/myorg

# NuGet packages
rvr publish --target nuget --tag 2.0.0

# Preview all commands
rvr publish --dry-run

# Publish without tests
rvr publish --target docker --skip-tests
```

## Pipeline

1. `dotnet build` (Release)
2. `dotnet test` (unless `--skip-tests`)
3. Target-specific commands (docker build/push, dotnet pack/push, etc.)
