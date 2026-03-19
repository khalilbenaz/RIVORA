# Products

Route prefix: `/api/products`

## GET `/api/products`

Returns all products. This endpoint is publicly accessible.

- **Auth required**: No

```bash
curl http://localhost:5220/api/products
```

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

## GET `/api/products/active`

Returns only active products.

- **Auth required**: Yes

```bash
curl http://localhost:5220/api/products/active \
  -H "Authorization: Bearer <token>"
```

---

## GET `/api/products/{id}`

Returns a single product by ID.

- **Auth required**: Yes

```bash
curl http://localhost:5220/api/products/3fa85f64-5717-4562-b3fc-2c963f66afa6 \
  -H "Authorization: Bearer <token>"
```

**Errors:**
- `404 Not Found` -- Product does not exist

---

## GET `/api/products/search/{name}`

Search products by name.

- **Auth required**: Yes

```bash
curl http://localhost:5220/api/products/search/Widget \
  -H "Authorization: Bearer <token>"
```

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

## POST `/api/products`

Create a new product.

- **Auth required**: Yes

```bash
curl -X POST http://localhost:5220/api/products \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Widget Pro",
    "description": "A premium widget",
    "price": 29.99,
    "isActive": true
  }'
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

**DTO fields:**
- `name` (string, required): Product name
- `description` (string, optional): Product description
- `price` (decimal, required): Price, must be >= 0
- `isActive` (boolean, optional): Active status, defaults to `true`

**Errors:**
- `400 Bad Request` -- Validation failure

---

## PUT `/api/products/{id}`

Update an existing product.

- **Auth required**: Yes

```bash
curl -X PUT http://localhost:5220/api/products/3fa85f64-5717-4562-b3fc-2c963f66afa6 \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Widget Pro v2",
    "description": "An updated premium widget",
    "price": 39.99,
    "isActive": true
  }'
```

**Response `200 OK`:** Returns the updated product.

**Errors:**
- `400 Bad Request` -- Validation failure
- `404 Not Found` -- Product does not exist

---

## DELETE `/api/products/{id}`

Delete a product.

- **Auth required**: Yes

```bash
curl -X DELETE http://localhost:5220/api/products/3fa85f64-5717-4562-b3fc-2c963f66afa6 \
  -H "Authorization: Bearer <token>"
```

**Response:** `204 No Content`

**Errors:**
- `404 Not Found` -- Product does not exist
