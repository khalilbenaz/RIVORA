---
title: Badge
description: Composant de badge colore avec 5 variantes
---

# Badge

Un composant compact pour afficher des labels, statuts ou categories avec un style colore.

**Fichier source** : `frontend/src/components/Badge.tsx`

## Props

| Prop | Type | Default | Description |
|------|------|---------|-------------|
| `children` | `React.ReactNode` | requis | Contenu du badge |
| `variant` | `'success' \| 'danger' \| 'warning' \| 'info' \| 'neutral'` | `'neutral'` | Variante de couleur |

## Variantes

| Variante | Background | Texte | Cas d'usage |
|----------|-----------|-------|-------------|
| `success` | emerald-100 | emerald-700 | Statut actif, HTTP 2xx |
| `danger` | red-100 | red-700 | Erreur, HTTP 4xx/5xx |
| `warning` | amber-100 | amber-700 | Avertissement, en attente |
| `info` | blue-100 | blue-700 | Information, methode HTTP |
| `neutral` | slate-100 | slate-600 | Default, labels generiques |

## Implementation

```tsx
const styles = {
  success: 'bg-emerald-100 text-emerald-700',
  danger: 'bg-red-100 text-red-700',
  warning: 'bg-amber-100 text-amber-700',
  info: 'bg-blue-100 text-blue-700',
  neutral: 'bg-slate-100 text-slate-600',
};

export default function Badge({ children, variant = 'neutral' }: Props) {
  return (
    <span className={`inline-flex items-center rounded-full px-2 py-0.5 text-xs font-semibold ${styles[variant]}`}>
      {children}
    </span>
  );
}
```

## Exemples d'utilisation

```tsx
// Methode HTTP
<Badge variant="info">GET</Badge>

// Code de statut reussi
<Badge variant="success">200</Badge>

// Code de statut en erreur
<Badge variant="danger">500</Badge>

// Statut de webhook
<Badge variant={webhook.isActive ? 'success' : 'neutral'}>
  {webhook.isActive ? 'Active' : 'Inactive'}
</Badge>

// Statut conditionnel de log
<Badge variant={(log.httpStatusCode ?? 0) >= 400 ? 'danger' : 'success'}>
  {log.httpStatusCode}
</Badge>
```

## Tests

Voir `frontend/src/components/__tests__/Badge.test.tsx` pour les tests unitaires Vitest.
