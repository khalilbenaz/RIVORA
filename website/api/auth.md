# Authentication

Route prefix: `/api/auth`

## POST `/api/auth/login`

Authenticate a user and receive a JWT token pair.

- **Auth required**: No
- **Rate limiting**: `strict`

**Request:**

```bash
curl -X POST http://localhost:5220/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "userName": "admin",
    "password": "Admin@123"
  }'
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

## POST `/api/auth/refresh`

Refresh an access token using a valid refresh token.

- **Auth required**: No

**Request:**

```bash
curl -X POST http://localhost:5220/api/auth/refresh \
  -H "Content-Type: application/json" \
  -d '"dGhpcyBpcyBhIHJlZnJlc2g..."'
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

## POST `/api/auth/logout`

Revoke the refresh token and log the user out.

- **Auth required**: Yes (Bearer token)

**Request:**

```bash
curl -X POST http://localhost:5220/api/auth/logout \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIs..." \
  -H "Content-Type: application/json" \
  -d '"dGhpcyBpcyBhIHJlZnJlc2g..."'
```

**Response `200 OK`:**

```json
{
  "message": "Logged out successfully."
}
```
