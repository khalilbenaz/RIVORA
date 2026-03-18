# RVR.Fintech.Payment - Exemple de traitement de paiements

Cet exemple montre comment utiliser les modules **RIVORA Framework** pour construire une API de traitement de paiements fintech.

## Modules RIVORA utilises

| Module | Usage |
|--------|-------|
| **RVR.Framework.Idempotency** | Garantit qu'un paiement n'est traite qu'une seule fois, meme si la requete est rejouee |
| **RVR.Framework.ApiKeys** | Authentification des marchands via cles API |
| **RVR.Framework.Webhooks** | Notification des marchands lors des changements de statut de paiement |
| **RVR.Framework.Billing** | Suivi de la facturation et des soldes marchands |

## Architecture

```
POST /api/payments  (cle API + cle d'idempotence)
    |
    v
[Validation ApiKey] --> [Verification Idempotency] --> [PaymentService]
    |                                                        |
    v                                                        v
[Retour cache si doublon]                          [Traitement du paiement]
                                                             |
                                                             v
                                                   [Webhook: payment.completed]
                                                   [Mise a jour du solde marchand]
```

## Demarrage rapide

```bash
# Depuis la racine du framework
cd samples/fintech-payment
dotnet run
```

L'API demarre sur `https://localhost:5001`. Swagger est disponible a `/swagger`.

## Endpoints

### Marchands

| Methode | Route | Description |
|---------|-------|-------------|
| POST | `/api/merchants` | Enregistrer un nouveau marchand |
| POST | `/api/merchants/{id}/api-keys` | Generer une cle API |
| GET | `/api/merchants/{id}/balance` | Consulter le solde |

### Paiements

| Methode | Route | En-tetes requis | Description |
|---------|-------|-----------------|-------------|
| POST | `/api/payments` | `X-Api-Key`, `X-Idempotency-Key` | Creer un paiement |
| POST | `/api/payments/{id}/refund` | `X-Api-Key` | Rembourser un paiement |
| GET | `/api/payments/{id}` | `X-Api-Key` | Consulter le statut |
| GET | `/api/payments` | `X-Api-Key` | Lister les paiements du marchand |

## Patterns demontres

### Idempotence
Chaque requete de paiement necessite un en-tete `X-Idempotency-Key`. Si la meme cle est reutilisee, le systeme retourne le resultat original sans retraiter le paiement.

### Cles API
Les marchands s'authentifient via l'en-tete `X-Api-Key`. Les cles sont hashees et validees par le module `RVR.Framework.ApiKeys`.

### Webhooks
Quand un paiement change de statut (complete, echoue, rembourse), un webhook est envoye a l'URL configuree du marchand.

### Facturation
Les frais de transaction sont calcules et le solde du marchand est mis a jour automatiquement.

## Configuration

Voir `appsettings.json` pour les parametres de configuration. Le stockage utilise SQLite pour simplifier l'execution locale.
