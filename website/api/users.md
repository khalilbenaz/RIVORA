# Users

Route prefix: `/api/users`

All endpoints require authentication.

## GET `/api/users`

Returns all users.

```bash
curl http://localhost:5220/api/users \
  -H "Authorization: Bearer <token>"
```

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

## GET `/api/users/{id}`

Returns a single user by ID.

```bash
curl http://localhost:5220/api/users/3fa85f64-5717-4562-b3fc-2c963f66afa6 \
  -H "Authorization: Bearer <token>"
```

**Errors:**
- `404 Not Found` -- User does not exist

---

## GET `/api/users/by-username/{userName}`

Returns a user by username.

```bash
curl http://localhost:5220/api/users/by-username/admin \
  -H "Authorization: Bearer <token>"
```

**Errors:**
- `404 Not Found` -- User not found

---

## POST `/api/users`

Create a new user.

```bash
curl -X POST http://localhost:5220/api/users \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{
    "userName": "john.doe",
    "email": "john@example.com",
    "password": "SecureP@ss123",
    "firstName": "John",
    "lastName": "Doe"
  }'
```

**Response `201 Created`:** Returns the created user object.

**Validation rules:**
- `userName`: Required, unique, 3-50 characters
- `email`: Required, unique, valid email format
- `password`: Required, minimum 8 characters, must contain uppercase, lowercase, digit, and special character
- `firstName`: Required, 1-100 characters
- `lastName`: Required, 1-100 characters

**Errors:**
- `400 Bad Request` -- Validation failure or duplicate username/email

---

## PUT `/api/users/{id}`

Update an existing user.

```bash
curl -X PUT http://localhost:5220/api/users/3fa85f64-5717-4562-b3fc-2c963f66afa6 \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{
    "firstName": "John",
    "lastName": "Smith",
    "phoneNumber": "+14155552671"
  }'
```

**Response `200 OK`:** Returns the updated user object.

**Errors:**
- `400 Bad Request` -- Validation failure
- `404 Not Found` -- User does not exist

---

## DELETE `/api/users/{id}`

Delete a user.

```bash
curl -X DELETE http://localhost:5220/api/users/3fa85f64-5717-4562-b3fc-2c963f66afa6 \
  -H "Authorization: Bearer <token>"
```

**Response:** `204 No Content`

**Errors:**
- `404 Not Found` -- User does not exist
