# Fintech Payment

Application de paiement fintech demonstrant les modules securite et billing de RIVORA.

## Modules utilises

- **RVR.Framework.Security** : JWT, RBAC, API Keys, Rate Limiting
- **RVR.Framework.Billing** : Stripe integration
- **RVR.Framework.MultiTenancy** : Isolation par merchant
- **RVR.Framework.Export** : Rapports PDF/CSV

## Entites

| Entite | Description |
|--------|-------------|
| Merchant | Compte marchand |
| PaymentTransaction | Transaction de paiement |
| Settlement | Reglement vers le marchand |

## Endpoints

| Methode | Route | Description |
|---------|-------|-------------|
| POST | /api/payments | Creer un paiement |
| GET | /api/payments/:id | Detail d'un paiement |
| POST | /api/payments/:id/refund | Rembourser |
| GET | /api/merchants | Lister les marchands |
| GET | /api/merchants/:id/transactions | Transactions d'un marchand |

## Demarrage

```bash
cd samples/fintech-payment
dotnet run
```

API disponible sur `http://localhost:5000`.

## Architecture

```
samples/fintech-payment/
├── Controllers/        # MerchantsController, PaymentsController
├── Domain/            # Merchant, PaymentTransaction
├── Services/          # PaymentService
└── Program.cs         # Configuration
```
