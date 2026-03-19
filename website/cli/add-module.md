# rvr add-module

Add a RIVORA module to your project. This installs NuGet packages, generates configuration, and scaffolds required files.

## Usage

```bash
rvr add-module <module-name> [options]
```

## Options

| Flag | Description | Default |
|------|-------------|---------|
| `--dry-run` | Preview changes without applying | `false` |
| `--skip-config` | Skip configuration file updates | `false` |
| `--force` | Overwrite existing module files | `false` |

## Available Modules

| Module | Description |
|--------|-------------|
| `Core` | Base framework (included by default) |
| `Security` | Authentication, authorization, JWT, RBAC |
| `Caching` | Redis-based distributed caching |
| `Jobs` | Background job processing with Hangfire |
| `Export` | PDF and Excel export capabilities |
| `Webhooks` | Outgoing and incoming webhook support |
| `GraphQL` | HotChocolate GraphQL server |
| `Billing` | Stripe-based SaaS billing |
| `SMS` | SMS notifications via Twilio |
| `Localization` | Multi-language support |
| `Audit` | Audit logging with UI dashboard |
| `Plugins` | Plugin system for extensibility |
| `EventSourcing` | Event sourcing with projections |
| `Saga` | Saga / Process Manager pattern |
| `Privacy` | GDPR compliance tools |
| `Identity` | Identity.Pro with advanced auth flows |
| `Multitenancy` | Multi-tenancy and SaaS isolation |
| `AI` | AI integration and NaturalQuery |
| `Guardrails` | AI guardrails and safety filters |
| `Agents` | AI agent orchestration |
| `Client` | Typed API client generation |

## Examples

Add the Webhooks module:

```bash
rvr add-module Webhooks
```

Add the Billing module with dry-run preview:

```bash
rvr add-module Billing --dry-run
```

Add multiple modules:

```bash
rvr add-module Caching
rvr add-module Export
rvr add-module Webhooks
```

The command updates your `rivora.json`, installs the required NuGet packages, and adds default configuration to `appsettings.json`.
