---
title: TableSkeleton
description: Squelette anime de tableau pour les etats de chargement
---

# TableSkeleton

Composant de squelette anime imitant un tableau en cours de chargement.

**Fichier source** : `frontend/src/components/TableSkeleton.tsx`

## Props

| Prop | Type | Default | Description |
|------|------|---------|-------------|
| `columns` | `number` | requis | Nombre de colonnes a afficher |
| `rows` | `number` | `5` | Nombre de lignes de squelette |

## Implementation

```tsx
export default function TableSkeleton({ columns, rows = 5 }: Props) {
  return (
    <div className="overflow-hidden rounded-xl border border-slate-200 bg-white shadow-sm">
      <table className="w-full">
        <thead className="bg-slate-50">
          <tr>
            {Array.from({ length: columns }).map((_, i) => (
              <th key={i} className="px-4 py-3">
                <div className="h-3 w-20 animate-pulse rounded bg-slate-200" />
              </th>
            ))}
          </tr>
        </thead>
        <tbody>
          {Array.from({ length: rows }).map((_, row) => (
            <tr key={row} className="border-t border-slate-100">
              {Array.from({ length: columns }).map((_, col) => (
                <td key={col} className="px-4 py-3">
                  <div className="h-4 animate-pulse rounded bg-slate-100"
                       style={{ width: `${60 + Math.random() * 30}%` }} />
                </td>
              ))}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
```

## Animation

Chaque cellule contient un `div` avec `animate-pulse` de Tailwind qui cree un effet de pulsation. La largeur varie entre 60% et 90% pour un aspect naturel.

## Exemples d'utilisation

```tsx
// Chargement de la table des webhooks
if (loading) return <TableSkeleton columns={5} />;

// Chargement avec un nombre specifique de lignes
if (loading) return <TableSkeleton columns={3} rows={8} />;
```

## Ou il est utilise

- `WebhooksPage.tsx` -- onglets Outgoing, Incoming et Builder
- Toute page utilisant `useApi` avec un tableau

## Tests

Voir `frontend/src/components/__tests__/TableSkeleton.test.tsx`.
