# Modules

RIVORA Framework is composed of independent modules, each registerable via `IRvrModule`.

| Module | Package | Description |
|--------|---------|-------------|
| [Core](/en/modules/core) | `RVR.Framework.Core` | Abstractions, Result pattern, pagination |
| [Security](/en/modules/security) | `RVR.Framework.Security` | JWT, BCrypt, 2FA, Rate Limiting |
| [Caching](/en/modules/caching) | `RVR.Framework.Caching` | L1 (Memory) + L2 (Redis) cache |
| [Jobs](/en/modules/jobs) | `RVR.Framework.Jobs.*` | Background jobs (Hangfire, Quartz) |
| [Export](/en/modules/export) | `RVR.Framework.Export` | PDF, Excel, CSV |
| [Webhooks](/en/modules/webhooks) | `RVR.Framework.Webhooks` | Publish/Subscribe HMAC-SHA256 |
| [GraphQL](/en/modules/graphql) | `RVR.Framework.GraphQL` | HotChocolate gateway |
| [Billing](/en/modules/billing) | `RVR.Framework.Billing` | SaaS billing |
