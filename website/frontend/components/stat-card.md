---
title: StatCard
description: Carte de statistique avec valeur, label, icone et variante
---

# StatCard

Composant de carte pour afficher une metrique avec label, valeur, icone et style conditionnel.

**Fichier source** : `frontend/src/components/StatCard.tsx`

## Props

| Prop | Type | Default | Description |
|------|------|---------|-------------|
| `label` | `string` | requis | Libelle en haut (uppercase, tracking-wider) |
| `value` | `string \| number` | requis | Valeur principale (grand, bold) |
| `detail` | `string` | - | Texte secondaire sous la valeur |
| `icon` | `ReactNode` | - | Icone Lucide affichee a droite du label |
| `variant` | `'default' \| 'success' \| 'danger' \| 'warning'` | `'default'` | Style de la carte |

## Variantes

| Variante | Bordure | Background |
|----------|---------|------------|
| `default` | slate-200 | blanc |
| `success` | emerald-200 | emerald-50/50 |
| `danger` | red-200 | red-50/50 |
| `warning` | amber-200 | amber-50/50 |

## Exemple d'utilisation

```tsx
import StatCard from '../components/StatCard';
import { Users, Package, Building2, ScrollText } from 'lucide-react';

// Dashboard - 4 cartes en grille
<div className="grid grid-cols-1 gap-4 sm:grid-cols-2 xl:grid-cols-4">
  <StatCard
    label="Utilisateurs"
    value={users?.length ?? 0}
    icon={<Users size={20} />}
  />
  <StatCard
    label="Produits"
    value={products?.length ?? 0}
    icon={<Package size={20} />}
  />
  <StatCard
    label="Tenants"
    value={tenants?.length ?? 0}
    icon={<Building2 size={20} />}
    variant="success"
  />
  <StatCard
    label="Erreurs"
    value={42}
    detail="+12 cette semaine"
    icon={<ScrollText size={20} />}
    variant="danger"
  />
</div>
```

## Style

La carte utilise un design arrondi (`rounded-xl`) avec bordure, ombre et effet hover :

```tsx
<div className={`rounded-xl border bg-white p-5 shadow-sm transition-shadow hover:shadow-md ${variants[variant]}`}>
```

La valeur est affichee en `text-3xl font-bold tabular-nums` pour un alignement numerique propre.

## Tests

Voir `frontend/src/components/__tests__/StatCard.test.tsx`.
