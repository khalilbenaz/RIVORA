# KBA Microservices Demo

A complete DDD-based microservices architecture with gRPC, RabbitMQ, Saga Orchestrator, and YARP API Gateway.

## Architecture

```
                                    ┌─────────────────┐
                                    │   API Gateway   │
                                    │     (YARP)      │
                                    │   Port 5000     │
                                    └────────┬────────┘
                                             │
         ┌───────────────────────────────────┼───────────────────────────────────┐
         │                                   │                                   │
         ▼                                   ▼                                   ▼
┌─────────────────┐              ┌─────────────────┐              ┌─────────────────┐
│   Identity      │              │    Product      │              │     Order       │
│   Service       │              │    Service      │              │    Service      │
│   Port 5001     │              │    Port 5002    │              │    Port 5003    │
│   gRPC + REST   │              │   gRPC + REST   │              │   gRPC + REST   │
└────────┬────────┘              └────────┬────────┘              └────────┬────────┘
         │                                │                                │
         └────────────────────────────────┼────────────────────────────────┘
                                          │
                                          ▼
                                   ┌─────────────┐
                                   │   RabbitMQ  │
                                   │   EventBus  │
                                   │  Port 5672  │
                                   └─────────────┘
                                          │
                                          ▼
                                   ┌─────────────────┐
                                   │  Notification   │
                                   │    Service      │
                                   │   Port 5004     │
                                   └─────────────────┘
```

## Services

| Service | Port | Database | Description |
|---------|------|----------|-------------|
| API Gateway | 5000 | - | YARP Reverse Proxy |
| Identity Service | 5001 | identity_db | JWT Auth, User Management |
| Product Service | 5002 | products_db | Product Catalog, Stock |
| Order Service | 5003 | orders_db | Order Management, Saga |
| Notification Service | 5004 | - | Email, Events Consumer |

## Features

- **DDD Architecture**: Clean separation of Domain, Application, Infrastructure
- **gRPC Communication**: Service-to-service communication via gRPC
- **RabbitMQ EventBus**: Async messaging with MassTransit
- **Saga Orchestrator**: Distributed transaction management for checkout
- **YARP Gateway**: Reverse proxy with routing and transforms
- **Event-Driven**: Integration events for cross-service communication
- **PostgreSQL**: Database per service pattern

## Quick Start

### Prerequisites
- .NET 8 SDK
- Docker & Docker Compose

### Using Docker Compose

```bash
cd samples/microservices-demo
docker-compose up -d
```

Access points:
- **API Gateway**: http://localhost:5000
- **Gateway Swagger**: http://localhost:5000/swagger
- **RabbitMQ Management**: http://localhost:15672 (guest/guest)

### Service Endpoints

| Service | Swagger URL |
|---------|-------------|
| Identity | http://localhost:5001/swagger |
| Products | http://localhost:5002/swagger |
| Orders | http://localhost:5003/swagger |
| Notifications | http://localhost:5004/swagger |

## Saga Checkout Flow

```
1. Order Created Event
         │
         ▼
2. Reserve Stock (gRPC to Product Service)
         │
    ┌────┴────┐
    │         │
    ▼         ▼
3a. Success  3b. Failure
    │         │
    ▼         ▼
4. Process Payment    Cancel Order
    │
    ├────┬────┐
    │    │    │
    ▼    ▼    ▼
5a. OK  5b. Fail
    │    │
    ▼    ▼
6. Complete  Release Stock + Cancel
```

## Project Structure

```
microservices-demo/
├── src/
│   ├── KBA.Microservices.ApiGateway/      # YARP Gateway
│   ├── KBA.Microservices.IdentityService/ # Identity & Auth
│   ├── KBA.Microservices.ProductService/  # Product Catalog
│   ├── KBA.Microservices.OrderService/    # Orders + Saga
│   ├── KBA.Microservices.NotificationService/ # Events
│   └── KBA.Microservices.Shared/
│       ├── Domain/        # Shared domain entities
│       ├── Application/   # Shared application layer
│       ├── Infrastructure/ # EventBus, Repositories
│       └── Grpc/          # Proto definitions
├── docker-compose.yml
└── README.md
```

## API Routes (via Gateway)

| Route | Service | Description |
|-------|---------|-------------|
| /api/identity/* | Identity | Auth endpoints |
| /api/products/* | Product | Product CRUD |
| /api/orders/* | Order | Order management |
| /api/notifications/* | Notification | Notification settings |

## gRPC Services

- **IdentityService**: ValidateToken, GetUserById
- **ProductService**: GetProductById, ReserveStock, ReleaseStock

## License

MIT License
