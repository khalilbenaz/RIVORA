# Tenants

Route prefix: `/api/tenants`

All endpoints require authentication. Tenant operations are restricted to users with the `Admin` role.

## GET `/api/tenants`

Returns all tenants.

```bash
curl http://localhost:5220/api/tenants \
  -H "Authorization: Bearer <token>"
```

**Response `200 OK`:**

```json
[
  {
    "id": "tenant-abc",
    "name": "Acme Corp",
    "subdomain": "acme",
    "plan": "professional",
    "isActive": true,
    "createdAt": "2026-01-15T10:00:00Z"
  }
]
```

---

## GET `/api/tenants/{id}`

Returns a single tenant by ID.

```bash
curl http://localhost:5220/api/tenants/tenant-abc \
  -H "Authorization: Bearer <token>"
```

**Errors:**
- `404 Not Found` -- Tenant does not exist

---

## POST `/api/tenants`

Create a new tenant.

```bash
curl -X POST http://localhost:5220/api/tenants \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{
    "id": "tenant-xyz",
    "name": "New Corp",
    "subdomain": "newcorp",
    "plan": "starter",
    "connectionString": "Host=localhost;Database=tenant_xyz;..."
  }'
```

**Response `201 Created`:**

```json
{
  "id": "tenant-xyz",
  "name": "New Corp",
  "subdomain": "newcorp",
  "plan": "starter",
  "isActive": true,
  "createdAt": "2026-03-19T12:00:00Z"
}
```

**DTO fields:**
- `id` (string, required): Unique tenant identifier
- `name` (string, required): Display name
- `subdomain` (string, optional): Subdomain for tenant resolution
- `plan` (string, optional): Subscription plan name
- `connectionString` (string, optional): Tenant-specific database connection (for database-per-tenant isolation)

**Errors:**
- `400 Bad Request` -- Validation failure or duplicate ID/subdomain

---

## PUT `/api/tenants/{id}`

Update an existing tenant.

```bash
curl -X PUT http://localhost:5220/api/tenants/tenant-abc \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Acme Corporation",
    "plan": "enterprise",
    "isActive": true
  }'
```

**Response `200 OK`:** Returns the updated tenant.

**Errors:**
- `400 Bad Request` -- Validation failure
- `404 Not Found` -- Tenant does not exist

---

## DELETE `/api/tenants/{id}`

Delete a tenant. This deactivates the tenant and its associated data.

```bash
curl -X DELETE http://localhost:5220/api/tenants/tenant-xyz \
  -H "Authorization: Bearer <token>"
```

**Response:** `204 No Content`

**Errors:**
- `404 Not Found` -- Tenant does not exist

---

## Tenant Resolution

The API resolves tenants from requests using three strategies (in priority order):

1. **Header**: `X-Tenant-Id: tenant-abc`
2. **Query string**: `?tenant=tenant-abc`
3. **Subdomain**: `tenant-abc.app.example.com`

For authenticated requests, the tenant middleware verifies that the resolved tenant matches the `TenantId` claim in the JWT. Mismatches return `403 Forbidden`.
