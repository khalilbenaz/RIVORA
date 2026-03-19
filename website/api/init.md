# Initialization

Route prefix: `/api/init`

These endpoints handle first-time system setup. They are only active when no administrator account exists.

## POST `/api/init/first-admin`

Create the first administrator account. This endpoint only works when the system has zero users.

- **Auth required**: No

```bash
curl -X POST http://localhost:5220/api/init/first-admin \
  -H "Content-Type: application/json" \
  -d '{
    "userName": "admin",
    "email": "admin@rivora-framework.com",
    "password": "Admin@123",
    "firstName": "Admin",
    "lastName": "System"
  }'
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
  "message": "First admin user created successfully. You can now log in via /api/auth/login"
}
```

**DTO fields:**
- `userName` (string, required): Admin username
- `email` (string, required): Admin email address
- `password` (string, required): Must meet password complexity requirements
- `firstName` (string, required): First name
- `lastName` (string, required): Last name

**Errors:**
- `400 Bad Request` -- System already initialized or validation failure

---

## GET `/api/init/status`

Check whether the system requires initialization.

- **Auth required**: No

```bash
curl http://localhost:5220/api/init/status
```

**Response `200 OK` (needs initialization):**

```json
{
  "needsInitialization": true,
  "userCount": 0,
  "message": "The system requires initialization."
}
```

**Response `200 OK` (already initialized):**

```json
{
  "needsInitialization": false,
  "userCount": 5,
  "message": "The system is already initialized."
}
```

---

## Typical Setup Flow

1. Check if the system needs initialization:

```bash
curl http://localhost:5220/api/init/status
```

2. If `needsInitialization` is `true`, create the first admin:

```bash
curl -X POST http://localhost:5220/api/init/first-admin \
  -H "Content-Type: application/json" \
  -d '{
    "userName": "admin",
    "email": "admin@example.com",
    "password": "Admin@123",
    "firstName": "Admin",
    "lastName": "System"
  }'
```

3. Log in with the new admin credentials:

```bash
curl -X POST http://localhost:5220/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "userName": "admin",
    "password": "Admin@123"
  }'
```
