# E-commerce

Le template **E-commerce** genere une plateforme de vente en ligne complete avec gestion de catalogue, panier, commandes et paiement.

## Creation

```bash
rvr new MaShop --template ecommerce
cd MaShop
```

## Modules inclus

| Module | Role |
|--------|------|
| Core | Entites de base, repositories, specifications |
| Security | Authentification JWT, autorisation |
| Billing | Integration Stripe pour le paiement |
| Export | Export PDF des factures, Excel du catalogue |
| Jobs | Taches planifiees (nettoyage panier, relances) |
| Caching | Cache Redis pour le catalogue |
| Webhooks | Notifications de commande |

## Entites metier

```csharp
public class Product : AggregateRoot<Guid>
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public Money Price { get; private set; }
    public string Sku { get; private set; }
    public int StockQuantity { get; private set; }
    public Guid CategoryId { get; private set; }
    public bool IsActive { get; private set; }

    public void DecreaseStock(int quantity)
    {
        if (StockQuantity < quantity)
            throw new InsufficientStockException(Id, quantity, StockQuantity);
        StockQuantity -= quantity;
        AddDomainEvent(new StockDecreasedEvent(Id, quantity, StockQuantity));
    }
}

public class Order : AggregateRoot<Guid>
{
    public Guid CustomerId { get; private set; }
    public OrderStatus Status { get; private set; }
    public Address ShippingAddress { get; private set; }
    public Money TotalAmount { get; private set; }
    private readonly List<OrderLine> _lines = new();
    public IReadOnlyList<OrderLine> Lines => _lines.AsReadOnly();

    public void AddLine(Guid productId, string productName, Money unitPrice, int quantity)
    {
        _lines.Add(new OrderLine(productId, productName, unitPrice, quantity));
        RecalculateTotal();
        AddDomainEvent(new OrderLineAddedEvent(Id, productId, quantity));
    }
}

public class Cart : AggregateRoot<Guid>
{
    public Guid? CustomerId { get; private set; }
    public string SessionId { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    private readonly List<CartItem> _items = new();
    public IReadOnlyList<CartItem> Items => _items.AsReadOnly();
}

public record Address(
    string Street,
    string City,
    string PostalCode,
    string Country
) : ValueObject;
```

## API Endpoints

| Methode | Endpoint | Description |
|---------|----------|-------------|
| GET | `/api/v1/products` | Catalogue avec pagination et filtres |
| GET | `/api/v1/products/:id` | Detail produit |
| GET | `/api/v1/categories` | Categories |
| POST | `/api/v1/cart/items` | Ajouter au panier |
| DELETE | `/api/v1/cart/items/:id` | Retirer du panier |
| GET | `/api/v1/cart` | Contenu du panier |
| POST | `/api/v1/orders` | Creer une commande |
| GET | `/api/v1/orders` | Historique des commandes |
| POST | `/api/v1/orders/:id/pay` | Payer une commande (Stripe) |
| GET | `/api/v1/orders/:id/invoice` | Telecharger la facture PDF |

## Configuration

```json
{
  "Ecommerce": {
    "Cart": {
      "ExpirationMinutes": 1440,
      "MaxItems": 50
    },
    "Catalog": {
      "CacheDurationSeconds": 300,
      "DefaultPageSize": 20
    }
  },
  "Stripe": {
    "SecretKey": "sk_test_...",
    "PublishableKey": "pk_test_...",
    "WebhookSecret": "whsec_..."
  }
}
```

## Demarrage

```bash
# 1. Lancer l'infrastructure
docker compose up -d

# 2. Appliquer les migrations
rvr migrate apply

# 3. Seeder le catalogue de demo
rvr seed --profile demo

# 4. Lancer l'API
dotnet run --project src/MaShop.Api
```

Le seeder `demo` cree 50 produits, 5 categories et un utilisateur de test.

## Architecture

```
MaShop/
  src/
    MaShop.Domain/
      Products/         # Product, Category, Sku (Value Object)
      Orders/           # Order, OrderLine, OrderStatus
      Carts/            # Cart, CartItem
      Customers/        # Customer, Address (Value Object)
    MaShop.Application/
      Products/
        Queries/        # GetProducts, GetProductById, SearchProducts
      Orders/
        Commands/       # CreateOrder, PayOrder, CancelOrder
        Queries/        # GetOrders, GetOrderById
      Carts/
        Commands/       # AddToCart, RemoveFromCart, ClearCart
    MaShop.Infrastructure/
      Persistence/      # DbContext, Configurations EF Core
      Stripe/           # PaymentService
      Cache/            # CatalogCacheService
    MaShop.Api/
      Controllers/      # ProductController, OrderController, CartController
```

## Taches planifiees

Le template inclut des jobs Hangfire preconfigures :

| Job | Frequence | Description |
|-----|-----------|-------------|
| CleanExpiredCarts | Toutes les heures | Supprime les paniers expires |
| SendAbandonedCartEmails | Toutes les 6h | Relance les paniers abandonnes |
| SyncStripePayments | Toutes les 15min | Synchronise les statuts de paiement |
| GenerateAnalyticsReport | Chaque jour a 2h | Genere le rapport de ventes |

## Etape suivante

- [SaaS Starter](/samples/saas-starter) pour un template multi-tenant
- [AI RAG App](/samples/ai-rag) pour une application IA
- [Export PDF/Excel](/guide/export) pour la generation de documents
