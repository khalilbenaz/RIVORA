---
title: Webhooks
description: Gestion des webhooks sortants, entrants et builder de regles
---

# Webhooks

La page Webhooks est organisee en 3 onglets pour gerer les webhooks sortants, entrants et les regles automatisees.

**Fichier source** : `frontend/src/pages/WebhooksPage.tsx`
**Route** : `/admin/webhooks`

## Onglets

```tsx
type TabId = 'outgoing' | 'incoming' | 'builder';
```

## Onglet Outgoing

Gestion des souscriptions webhook sortantes.

### Tableau

| Colonne | Contenu |
|---------|---------|
| Event Type | Badge info |
| Callback URL | Code inline tronque |
| Active | Toggle switch |
| Last Triggered | Date formatee ou "-" |
| Actions | Bouton suppression |

### Formulaire de creation

3 champs : Event Type (texte), Callback URL (url), Secret (optionnel).

## Onglet Incoming

### Configurations

Table des sources de webhooks entrants :
- Name, Source (badge), Algorithm (code), Active (badge), Actions

Formulaire : Name, Source, Signature Header, Secret, Algorithm (HMAC-SHA256/SHA1/SHA512).

### Delivery Logs

Table des logs de livraison expansible :
- Time, Source, Event Type, Status (badge), Signature validation (check/alert)
- **Vue expandue** : headers JSON, payload JSON (pretty-print), erreur eventuelle
- Bouton **Replay** pour rejouer un webhook

### Validation de signature

```tsx
{log.signatureValid ? (
  <span className="text-emerald-600"><Check /> Valid</span>
) : (
  <span className="text-red-500"><AlertCircle /> Invalid</span>
)}
```

## Onglet Builder

Le builder permet de creer des regles d'automatisation webhook.

### Structure d'une regle

```typescript
interface WebhookRule {
  id: string;
  name: string;
  triggerEvent: string;    // ex: 'product.created'
  conditions: WebhookCondition[];
  targetUrl: string;
  method: string;          // POST, PUT, PATCH
  headers: Record<string, string>;
  payloadTemplate: string; // avec variables {{event.*}}
  isActive: boolean;
}
```

### Conditions

Chaque condition a : `field`, `operator` (equals, contains, gt, lt, exists), `value`.

### Evenements disponibles

```typescript
const TRIGGER_EVENTS = [
  'product.created', 'product.updated', 'product.deleted',
  'user.created', 'user.updated', 'user.deleted',
  'tenant.created', 'tenant.updated',
  'order.placed', 'order.updated', 'order.cancelled',
];
```

### Payload template

Le template supporte des variables Mustache-style :

```json
{
  "event": "{{event.type}}",
  "id": "{{event.id}}",
  "data": {{event.data}}
}
```

### Actions par regle

- **Edit** : rouvre le formulaire pre-rempli
- **Test** : execute la regle avec resultat JSON affiche
- **Delete** : suppression avec confirmation

## Composants utilises

- [`Badge`](/frontend/components/badge) -- badges de statut et type
- [`TableSkeleton`](/frontend/components/table-skeleton) -- squelettes pendant le chargement
- `useApi` hook pour le fetching avec `refetch`
