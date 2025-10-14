# RVR SaaS Starter

A complete B2B SaaS multi-tenant application built with .NET 9, Blazor Server, and PostgreSQL.

## Features

- **Multi-tenancy**: Full tenant isolation with database-per-tenant support
- **Identity & Authentication**: JWT-based auth with 2FA support
- **RBAC**: Role-based access control (Admin, User, Manager)
- **Feature Flags**: Toggle beta features per tenant
- **Audit Trail**: Complete audit logging of all actions
- **Background Jobs**: Hangfire for email notifications and scheduled tasks
- **Modern UI**: Blazor WebAssembly with MudBlazor components

## Architecture

```
┌─────────────────┐     ┌─────────────────┐
│   Blazor WASM   │────▶│   API (REST)    │
│   (Port 5001)   │     │   (Port 5000)   │
└─────────────────┘     └────────┬────────┘
                                 │
         ┌───────────────────────┼───────────────────────┐
         │                       │                       │
         ▼                       ▼                       ▼
┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐
│   PostgreSQL    │     │     Redis       │     │   Hangfire      │
│   (Database)    │     │    (Cache)      │     │   Dashboard     │
└─────────────────┘     └─────────────────┘     └─────────────────┘
```

## Quick Start

### Prerequisites
- .NET 9 SDK
- Docker & Docker Compose
- Node.js (optional, for development)

### Using Docker Compose (Recommended)

```bash
cd samples/saas-starter
docker-compose up -d
```

Access the application:
- **Blazor UI**: http://localhost:5001
- **API Swagger**: http://localhost:5000/swagger
- **Hangfire Dashboard**: http://localhost:5000/hangfire

Default credentials:
- Email: `admin@demo.com`
- Password: `Admin123!`

### Local Development

1. Start infrastructure:
```bash
docker-compose up -d postgres redis
```

2. Run API:
```bash
cd src/RVR.SaaS.Starter.Api
dotnet run
```

3. Run Blazor:
```bash
cd src/RVR.SaaS.Starter.Blazor
dotnet run
```

## Project Structure

```
saas-starter/
├── src/
│   ├── RVR.SaaS.Starter.Api/          # REST API
│   ├── RVR.SaaS.Starter.Blazor/       # Blazor WASM UI
│   ├── RVR.SaaS.Starter.Application/  # Application layer (CQRS)
│   ├── RVR.SaaS.Starter.Domain/       # Domain entities
│   ├── RVR.SaaS.Starter.Infrastructure/ # EF Core, Repositories, Jobs
│   └── RVR.SaaS.Starter.Identity/     # JWT Auth, 2FA
├── tests/
│   └── RVR.SaaS.Starter.Tests/
├── docker-compose.yml
└── README.md
```

## API Endpoints

| Endpoint | Method | Auth | Description |
|----------|--------|------|-------------|
| /api/tenants | GET | No | List all tenants |
| /api/tenants | POST | No | Create tenant |
| /api/users/register | POST | No | Register user |
| /api/users/login | POST | No | Login |
| /api/products | GET | Yes | List products |
| /api/products | POST | Yes | Create product |
| /api/orders | GET | Yes | List orders |
| /api/orders | POST | Yes | Create order |
| /api/featureflags | GET | Yes | List feature flags |
| /api/audit | GET | Admin | Audit logs |

## Feature Flags

Enable/disable features per tenant:

```csharp
if (FeatureFlags.IsEnabled("BetaFeatures"))
{
    // Show beta features
}
```

Default flags:
- `BetaFeatures`: Access to beta features
- `AdvancedAnalytics`: Advanced analytics dashboard
- `ApiAccess`: API access for tenant

## Security

- **JWT Tokens**: 60-minute expiration
- **2FA**: TOTP-based (Google Authenticator compatible)
- **Password Policy**: Min 8 chars, uppercase, lowercase, number
- **Account Lockout**: After 5 failed attempts
- **Audit Logging**: All CRUD operations logged

## Background Jobs

Hangfire handles:
- Daily summary emails
- Order confirmation emails
- Password reset emails
- Welcome emails

Access dashboard at `/hangfire` (admin only).

## Testing

```bash
dotnet test
```

## License

MIT License - See LICENSE file for details.
