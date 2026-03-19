# API Reference

The RIVORA REST API provides endpoints for authentication, user management, products, tenants, webhooks, health checks, and system initialization.

## Base URL

```
http://localhost:5220
```

## Authentication

All protected endpoints require a JWT Bearer token in the `Authorization` header:

```
Authorization: Bearer <jwt-token>
```

Obtain a token via [`POST /api/auth/login`](/api/auth#post-apiauthlogin).

## Rate Limiting

Rate-limited endpoints use the `strict` policy: **5 requests per 10 seconds** per client. When exceeded, the API returns `429 Too Many Requests` with a `Retry-After` header.

## Multi-Tenancy Headers

Include a tenant identifier via one of:

- **Header**: `X-Tenant-Id: tenant-abc`
- **Query string**: `?tenant=tenant-abc`
- **Subdomain**: `tenant-abc.app.example.com`

## Error Format

All errors follow a consistent structure:

```json
{
  "message": "Description of the error"
}
```

## Status Codes

| Code | Meaning |
|------|---------|
| `200` | Success |
| `201` | Created |
| `202` | Accepted |
| `204` | No Content |
| `400` | Bad Request (validation errors) |
| `401` | Unauthorized (missing/invalid token) |
| `403` | Forbidden (insufficient permissions) |
| `404` | Not Found |
| `429` | Too Many Requests |
| `500` | Internal Server Error |

## Endpoints

| Section | Description |
|---------|-------------|
| [Authentication](/api/auth) | Login, token refresh, logout |
| [Users](/api/users) | User CRUD operations |
| [Products](/api/products) | Product CRUD and search |
| [Tenants](/api/tenants) | Tenant management |
| [Webhooks](/api/webhooks) | Webhook subscriptions and receivers |
| [Health](/api/health) | Health check and readiness probes |
| [Initialization](/api/init) | First-admin setup and status |

## Interactive Documentation

- **Swagger UI**: `http://localhost:5220/swagger`
- **ReDoc**: `http://localhost:5220/api-docs`
- **OpenAPI spec**: `http://localhost:5220/swagger/v1/swagger.json`
- **GraphQL Playground**: `http://localhost:5220/graphql/ui`
