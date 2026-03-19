# Modules

RIVORA Framework est compose de modules independants enregistrables via `IRvrModule`.

| Module | Package | Description |
|--------|---------|-------------|
| [Core](/modules/core) | `RVR.Framework.Core` | Abstractions, Result pattern, pagination |
| [Security](/modules/security) | `RVR.Framework.Security` | JWT, BCrypt, 2FA, Rate Limiting |
| [Caching](/modules/caching) | `RVR.Framework.Caching` | Cache L1 (Memory) + L2 (Redis) |
| [Jobs](/modules/jobs) | `RVR.Framework.Jobs.*` | Background jobs (Hangfire, Quartz) |
| [Export](/modules/export) | `RVR.Framework.Export` | PDF, Excel, CSV |
| [Webhooks](/modules/webhooks) | `RVR.Framework.Webhooks` | Publish/Subscribe HMAC-SHA256 |
| [GraphQL](/modules/graphql) | `RVR.Framework.GraphQL` | HotChocolate gateway |
| [Billing](/modules/billing) | `RVR.Framework.Billing` | Facturation SaaS |
| [SMS](/modules/sms) | `RVR.Framework.Sms` | Multi-provider SMS (Twilio, Vonage, OVH, Azure) |
| [AI Guardrails](/modules/guardrails) | `RVR.Framework.AI.Guardrails` | Securite et validation LLM |
| [AI Agents](/modules/agents) | `RVR.Framework.AI.Agents` | Orchestration multi-agents |
| [Plugin System](/modules/plugins) | `RVR.Framework.Plugins` | Decouverte et gestion de plugins NuGet |
