# rvr publish

Build and publish your RIVORA project to various targets.

## Usage

```bash
rvr publish [options]
```

## Options

| Flag | Description | Default |
|------|-------------|---------|
| `--target` | Publish target (`docker`, `nuget`, `azure`, `self-contained`) | Required |
| `--configuration` | Build configuration | `Release` |
| `--runtime` | Target runtime identifier | Auto-detected |
| `--tag` | Docker image tag or NuGet version | Auto from git |
| `--registry` | Docker registry URL | Docker Hub |
| `--skip-tests` | Skip running tests before publish | `false` |
| `--dry-run` | Preview the publish process | `false` |
| `--push` | Push to remote registry after build | `false` |

## Targets

### Docker

Build a Docker image for your project.

```bash
rvr publish --target docker
rvr publish --target docker --tag 1.2.0 --push
rvr publish --target docker --registry ghcr.io/myorg --tag latest
```

### NuGet

Package and publish NuGet packages.

```bash
rvr publish --target nuget
rvr publish --target nuget --tag 1.0.0 --push
```

### Azure

Deploy to Azure App Service or Azure Container Apps.

```bash
rvr publish --target azure
```

### Self-Contained

Build self-contained binaries with no runtime dependency.

```bash
rvr publish --target self-contained
rvr publish --target self-contained --runtime linux-x64
rvr publish --target self-contained --runtime win-x64
```

## Examples

Preview a Docker publish:

```bash
rvr publish --target docker --dry-run
```

Publish to Docker Hub, skipping tests:

```bash
rvr publish --target docker --tag v2.0.0 --push --skip-tests
```

Build self-contained binaries for Linux and Windows:

```bash
rvr publish --target self-contained --runtime linux-x64
rvr publish --target self-contained --runtime win-x64
```

Full CI/CD publish pipeline:

```bash
rvr publish --target docker \
  --registry ghcr.io/myorg \
  --tag $(git describe --tags) \
  --push
```
