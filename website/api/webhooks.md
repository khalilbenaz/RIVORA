# Webhooks

Route prefix: `/api/webhooks`

All endpoints require authentication.

## Outgoing Webhook Subscriptions

### GET `/api/webhooks/subscriptions`

List all webhook subscriptions.

```bash
curl http://localhost:5220/api/webhooks/subscriptions \
  -H "Authorization: Bearer <token>"
```

**Response `200 OK`:**

```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "url": "https://example.com/hooks/orders",
    "events": ["order.created", "order.updated"],
    "secret": "whsec_...",
    "isActive": true,
    "createdAt": "2026-03-01T10:00:00Z"
  }
]
```

### POST `/api/webhooks/subscriptions`

Create a webhook subscription.

```bash
curl -X POST http://localhost:5220/api/webhooks/subscriptions \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{
    "url": "https://example.com/hooks/orders",
    "events": ["order.created", "order.updated"],
    "secret": "my-signing-secret"
  }'
```

**Response `201 Created`:** Returns the created subscription.

**DTO fields:**
- `url` (string, required): HTTPS endpoint to receive events
- `events` (string[], required): List of event types to subscribe to
- `secret` (string, optional): Signing secret for payload verification

### DELETE `/api/webhooks/subscriptions/{id}`

Remove a webhook subscription.

```bash
curl -X DELETE http://localhost:5220/api/webhooks/subscriptions/3fa85f64-5717-4562-b3fc-2c963f66afa6 \
  -H "Authorization: Bearer <token>"
```

**Response:** `204 No Content`

---

## Incoming Webhook Receiver

### POST `/api/webhooks/receive/{provider}`

Receive incoming webhooks from external providers (Stripe, GitHub, etc.).

```bash
curl -X POST http://localhost:5220/api/webhooks/receive/stripe \
  -H "Stripe-Signature: t=1234567890,v1=..." \
  -H "Content-Type: application/json" \
  -d '{ "type": "invoice.paid", "data": { ... } }'
```

**Response:** `200 OK` on success, `400 Bad Request` on signature validation failure.

---

## Webhook Rules

### GET `/api/webhooks/rules`

List webhook routing rules.

```bash
curl http://localhost:5220/api/webhooks/rules \
  -H "Authorization: Bearer <token>"
```

**Response `200 OK`:**

```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "provider": "stripe",
    "eventType": "invoice.paid",
    "action": "ProcessPayment",
    "isActive": true
  }
]
```

### POST `/api/webhooks/rules`

Create a webhook routing rule.

```bash
curl -X POST http://localhost:5220/api/webhooks/rules \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{
    "provider": "stripe",
    "eventType": "invoice.paid",
    "action": "ProcessPayment"
  }'
```

**Response `201 Created`:** Returns the created rule.

---

## Webhook Payload Format

Outgoing webhooks are signed and delivered with these headers:

```
X-Webhook-Id: evt_abc123
X-Webhook-Timestamp: 1679012345
X-Webhook-Signature: v1=sha256hash...
Content-Type: application/json
```

Verify the signature by computing `HMAC-SHA256` of `{timestamp}.{body}` using the subscription secret.
