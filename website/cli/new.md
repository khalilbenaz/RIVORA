# rvr new

Create a new RIVORA project from a template with optional module and configuration presets.

## Usage

```bash
rvr new <project-name> [options]
```

## Options

| Flag | Description | Default |
|------|-------------|---------|
| `--template`, `-t` | Project template | `saas-starter` |
| `--type` | Project type (`api`, `fullstack`, `microservices`) | `api` |
| `--db` | Database provider (`postgres`, `sqlserver`, `sqlite`, `mysql`) | `postgres` |
| `--modules` | Comma-separated list of modules to include | Core modules |
| `--security` | Security preset (`basic`, `standard`, `enterprise`) | `standard` |
| `--multitenancy` | Enable multi-tenancy (`none`, `schema`, `database`, `row`) | `none` |
| `--devops` | DevOps preset (`none`, `github`, `azure`, `gitlab`) | `none` |
| `--ai` | AI provider (`none`, `openai`, `claude`, `ollama`) | `none` |
| `--output`, `-o` | Output directory | Current directory |
| `--no-git` | Skip git initialization | `false` |
| `--dry-run` | Preview what would be created | `false` |

## Interactive Mode

When run without flags, the CLI enters an interactive wizard:

```bash
rvr new MyProject
```

The wizard prompts for template, database, modules, security level, and more.

## Non-Interactive Mode

Supply all options via flags to skip the wizard:

```bash
rvr new MyProject \
  --template saas-starter \
  --db postgres \
  --security enterprise \
  --multitenancy schema \
  --devops github \
  --ai claude
```

## Examples

Create a minimal API with SQLite:

```bash
rvr new QuickApi --template api-minimal --db sqlite
```

Create a multi-tenant SaaS application:

```bash
rvr new TenantApp \
  --template saas-starter \
  --multitenancy schema \
  --db postgres \
  --modules "billing,webhooks,export"
```

Create a microservices project with AI support:

```bash
rvr new AIPlatform \
  --template microservices \
  --ai openai \
  --devops github
```

Preview project structure without creating files:

```bash
rvr new MyProject --template saas-starter --dry-run
```

## Generated Structure

```
MyProject/
├── src/
│   ├── MyProject.Domain/
│   ├── MyProject.Application/
│   ├── MyProject.Infrastructure/
│   └── MyProject.Api/
├── tests/
│   ├── MyProject.UnitTests/
│   └── MyProject.IntegrationTests/
├── docker-compose.yml
├── MyProject.sln
└── rivora.json
```
