# API Reference

Base URL : `http://localhost:5220`

## Authentication

All endpoints requiring authentication expect a `Bearer` token in the `Authorization` header:

```
Authorization: Bearer <jwt-token>
```

Rate-limited endpoints use the `strict` policy (default: 5 requests per 10 seconds).

---

## Auth Controller

Route prefix: `/api/auth`

### POST `/api/auth/login`

Authenticates a user and returns a JWT token pair.

- **Auth required**: No
- **Rate limiting**: `strict`

**Request body:**

```json
{
  "userName": "admin",
  "password": "Admin@123"
}
```

**Response `200 OK`:**

```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "refreshToken": "dGhpcyBpcyBhIHJlZnJlc2g...",
  "expiresAt": "2026-03-17T14:30:00Z",
  "user": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "userName": "admin",
    "email": "admin@rivora.com",
    "roles": ["Admin"]
  }
}
```

**Errors:**
- `401 Unauthorized` -- Invalid credentials
- `400 Bad Request` -- Validation failure

---

### POST `/api/auth/refresh`

Refreshes an access token using a valid refresh token.

- **Auth required**: No

**Request body:**

```json
"dGhpcyBpcyBhIHJlZnJlc2g..."
```

**Response `200 OK`:**

```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "refreshToken": "bmV3IHJlZnJlc2ggdG9rZW4...",
  "expiresAt": "2026-03-17T15:00:00Z"
}
```

**Errors:**
- `401 Unauthorized` -- Invalid or expired refresh token

---

### POST `/api/auth/logout`

Revokes the refresh token and logs the user out.

- **Auth required**: Yes (Bearer token)

**Request body:**

```json
"dGhpcyBpcyBhIHJlZnJlc2g..."
```

**Response `200 OK`:**

```json
{
  "message": "Deconnexion reussie."
}
```

---

## Init Controller

Route prefix: `/api/init`

### POST `/api/init/first-admin`

Creates the first administrator account. Only works when no users exist in the system.

- **Auth required**: No

**Request body:**

```json
{
  "userName": "admin",
  "email": "admin@rivora-framework.com",
  "password": "Admin@123",
  "firstName": "Admin",
  "lastName": "System"
}
```

**Response `200 OK`:**

```json
{
  "user": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "userName": "admin",
    "email": "admin@rivora-framework.com",
    "firstName": "Admin",
    "lastName": "System"
  },
  "message": "Premier utilisateur administrateur cree avec succes. Vous pouvez maintenant vous connecter via /api/auth/login"
}
```

**Errors:**
- `400 Bad Request` -- System already initialized or validation failure

---

### GET `/api/init/status`

Checks whether the system requires initialization.

- **Auth required**: No

**Response `200 OK`:**

```json
{
  "needsInitialization": true,
  "userCount": 0,
  "message": "Le systeme necessite une initialisation."
}
```

---

## Products Controller

Route prefix: `/api/products`

All endpoints require authentication unless specified otherwise.

### GET `/api/products`

Returns all products.

- **Auth required**: No (AllowAnonymous)

**Response `200 OK`:**

```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "name": "Widget Pro",
    "description": "A premium widget",
    "price": 29.99,
    "isActive": true
  }
]
```

---

### GET `/api/products/active`

Returns only active products.

- **Auth required**: Yes

**Response `200 OK`:**

```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "name": "Widget Pro",
    "price": 29.99,
    "isActive": true
  }
]
```

---

### GET `/api/products/{id}`

Returns a single product by ID.

- **Auth required**: Yes

**Response `200 OK`:**

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "Widget Pro",
  "description": "A premium widget",
  "price": 29.99,
  "isActive": true
}
```

**Errors:**
- `404 Not Found` -- Product does not exist

---

### GET `/api/products/search/{name}`

Searches products by name.

- **Auth required**: Yes

**Response `200 OK`:**

```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "name": "Widget Pro",
    "price": 29.99
  }
]
```

**Errors:**
- `400 Bad Request` -- Empty search term

---

### POST `/api/products`

Creates a new product.

- **Auth required**: Yes

**Request body:**

```json
{
  "name": "Widget Pro",
  "description": "A premium widget",
  "price": 29.99,
  "isActive": true
}
```

**Response `201 Created`:**

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "Widget Pro",
  "description": "A premium widget",
  "price": 29.99,
  "isActive": true
}
```

**Errors:**
- `400 Bad Request` -- Validation failure

---

### PUT `/api/products/{id}`

Updates an existing product.

- **Auth required**: Yes

**Request body:**

```json
{
  "name": "Widget Pro v2",
  "description": "An updated premium widget",
  "price": 39.99,
  "isActive": true
}
```

**Response `200 OK`:** Returns the updated product.

**Errors:**
- `400 Bad Request` -- Validation failure
- `404 Not Found` -- Product does not exist

---

### DELETE `/api/products/{id}`

Deletes a product.

- **Auth required**: Yes

**Response**: `204 No Content`

**Errors:**
- `404 Not Found` -- Product does not exist

---

## Users Controller

Route prefix: `/api/users`

All endpoints require authentication.

### GET `/api/users`

Returns all users.

- **Auth required**: Yes

**Response `200 OK`:**

```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "userName": "admin",
    "email": "admin@rivora.com",
    "firstName": "Admin",
    "lastName": "System",
    "isActive": true
  }
]
```

---

### GET `/api/users/{id}`

Returns a single user by ID.

- **Auth required**: Yes

**Response `200 OK`:**

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "userName": "admin",
  "email": "admin@rivora.com",
  "firstName": "Admin",
  "lastName": "System",
  "isActive": true
}
```

**Errors:**
- `404 Not Found` -- User does not exist

---

### GET `/api/users/by-username/{userName}`

Returns a user by username.

- **Auth required**: Yes

**Response `200 OK`:** Same as GET by ID.

**Errors:**
- `404 Not Found` -- User not found

---

### POST `/api/users`

Creates a new user.

- **Auth required**: Yes

**Request body:**

```json
{
  "userName": "john.doe",
  "email": "john@example.com",
  "password": "SecureP@ss123",
  "firstName": "John",
  "lastName": "Doe"
}
```

**Response `201 Created`:** Returns the created user.

**Errors:**
- `400 Bad Request` -- Validation failure or duplicate username/email

---

### PUT `/api/users/{id}`

Updates an existing user.

- **Auth required**: Yes

**Request body:**

```json
{
  "firstName": "John",
  "lastName": "Smith",
  "phoneNumber": "+14155552671"
}
```

**Response `200 OK`:** Returns the updated user.

**Errors:**
- `400 Bad Request` -- Validation failure
- `404 Not Found` -- User does not exist

---

### DELETE `/api/users/{id}`

Deletes a user.

- **Auth required**: Yes

**Response**: `204 No Content`

**Errors:**
- `404 Not Found` -- User does not exist

---

## Billing Endpoints (Minimal API)

Route prefix: `/api/billing`

### POST `/api/billing/checkout`

Creates a Stripe checkout session for subscription.

- **Auth required**: Yes

**Request body:**

```json
{
  "tenantId": "tenant-abc",
  "planId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "successUrl": "https://app.example.com/billing/success",
  "cancelUrl": "https://app.example.com/billing/cancel"
}
```

**Response `200 OK`:**

```json
{
  "url": "https://checkout.stripe.com/c/pay/cs_test_..."
}
```

---

### POST `/api/billing/portal`

Creates a Stripe customer portal session for managing subscriptions.

- **Auth required**: Yes

**Request body:**

```json
{
  "tenantId": "tenant-abc",
  "returnUrl": "https://app.example.com/billing"
}
```

**Response `200 OK`:**

```json
{
  "url": "https://billing.stripe.com/p/session/..."
}
```

---

### GET `/api/billing/subscription?tenantId={tenantId}`

Returns the current subscription for a tenant.

- **Auth required**: Yes

**Response `200 OK`:**

```json
{
  "id": "sub_1234",
  "tenantId": "tenant-abc",
  "planId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "status": "active",
  "currentPeriodEnd": "2026-04-17T00:00:00Z"
}
```

**Errors:**
- `404 Not Found` -- No subscription found

---

### GET `/api/billing/invoices?tenantId={tenantId}`

Returns all invoices for a tenant.

- **Auth required**: Yes

**Response `200 OK`:**

```json
[
  {
    "id": "inv_1234",
    "amount": 4999,
    "currency": "usd",
    "status": "paid",
    "createdAt": "2026-03-01T00:00:00Z"
  }
]
```

---

### POST `/api/billing/usage`

Records a usage event for metered billing.

- **Auth required**: Yes

**Request body:**

```json
{
  "tenantId": "tenant-abc",
  "metricName": "api_calls",
  "quantity": 150
}
```

**Response**: `202 Accepted`

---

### POST `/api/billing/webhooks/stripe`

Handles incoming Stripe webhook events. Requires a valid `Stripe-Signature` header.

- **Auth required**: No (validated via Stripe signature)

**Headers:**

```
Stripe-Signature: t=1234567890,v1=...
```

**Response**: `200 OK` on success, `400 Bad Request` on failure.

---

## Health Endpoints

The framework provides configurable health check endpoints via `AddRvrHealthChecks()`.

### GET `/health`

Main health check endpoint. Returns the overall health status of the application including all registered checks (database, Redis, RabbitMQ, AI providers, jobs).

**Response `200 OK`:**

```json
{
  "status": "Healthy",
  "totalDuration": "00:00:00.0234567",
  "entries": {
    "self": { "status": "Healthy" },
    "database": { "status": "Healthy" },
    "redis": { "status": "Healthy" }
  }
}
```

---

### GET `/health/ready`

Readiness probe. Returns only checks tagged with `ready`. Useful for Kubernetes readiness probes.

**Response `200 OK`:**

```json
{
  "status": "Healthy",
  "entries": {
    "database": { "status": "Healthy" },
    "redis": { "status": "Healthy" }
  }
}
```

---

### GET `/health/detailed`

Detailed health check with full diagnostics (enabled via `UseRvrDetailedHealthChecks()`).

---

## GraphQL

### Endpoint: `/graphql`

The GraphQL endpoint supports queries, mutations, and subscriptions using HotChocolate.

**Interactive UI**: `http://localhost:5220/graphql/ui`

**Example query:**

```graphql
query {
  products {
    id
    name
    price
    isActive
  }
}
```

**Example mutation:**

```graphql
mutation {
  createProduct(input: {
    name: "New Widget"
    price: 19.99
    isActive: true
  }) {
    id
    name
  }
}
```

---

## Swagger / OpenAPI

- **Swagger UI**: `http://localhost:5220/swagger`
- **ReDoc**: `http://localhost:5220/api-docs`
- **OpenAPI spec**: `http://localhost:5220/swagger/v1/swagger.json`

---

## SignalR Hubs

### `/hubs/kba`

Real-time notification hub with multi-tenancy support. Requires authentication (Bearer token).

**Client events:**

| Event | Payload | Description |
|-------|---------|-------------|
| `ReceiveNotification` | `(type: string, data: object)` | Receives a real-time notification |

**Connection behavior:**
- On connect: user is automatically added to their tenant group and personal user group (`User_{userId}`)
- On disconnect: user is removed from tenant group

**JavaScript client example:**

```javascript
import * as signalR from "@microsoft/signalr";

const connection = new signalR.HubConnectionBuilder()
  .withUrl("http://localhost:5220/hubs/kba", {
    accessTokenFactory: () => accessToken
  })
  .withAutomaticReconnect()
  .build();

connection.on("ReceiveNotification", (type, data) => {
  console.log(`Notification [${type}]:`, data);
});

await connection.start();
```

**C# client example:**

```csharp
var connection = new HubConnectionBuilder()
    .WithUrl("http://localhost:5220/hubs/kba", options =>
    {
        options.AccessTokenProvider = () => Task.FromResult(accessToken);
    })
    .WithAutomaticReconnect()
    .Build();

connection.On<string, object>("ReceiveNotification", (type, data) =>
{
    Console.WriteLine($"[{type}]: {data}");
});

await connection.StartAsync();
```

---

## Natural Query

### GET `/api/natural-query?entity={Entity}&q={query}`

Queries entities using natural language (French and English supported).

- **Auth required**: Yes

**Example:**

```
GET /api/natural-query?entity=Product&q=produits actifs dont le prix est superieur a 20
```

**Response `200 OK`:**

```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "name": "Widget Pro",
    "price": 29.99,
    "isActive": true
  }
]
```

---

## Multi-Tenancy Headers

All API requests can include a tenant identifier via:

1. **Header**: `X-Tenant-Id: tenant-abc`
2. **Query string**: `?tenant=tenant-abc`
3. **Subdomain**: `tenant-abc.app.example.com`

The tenant middleware validates the tenant against `ITenantStore` and verifies it matches the JWT `TenantId` claim for authenticated requests.

---

## Common Error Response Format

All errors follow a consistent structure:

```json
{
  "message": "Description of the error"
}
```

HTTP status codes:

| Code | Meaning |
|------|---------|
| `200` | Success |
| `201` | Created |
| `202` | Accepted |
| `204` | No Content |
| `400` | Bad Request (validation errors) |
| `401` | Unauthorized (missing/invalid token) |
| `403` | Forbidden (tenant mismatch, insufficient permissions) |
| `404` | Not Found |
| `500` | Internal Server Error |
