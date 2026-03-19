---
title: Pagination
description: Composant de pagination avec navigation precedent/suivant
---

# Pagination

Composant de pagination pour naviguer dans des listes paginables.

**Fichier source** : `frontend/src/components/Pagination.tsx`

## Props

| Prop | Type | Description |
|------|------|-------------|
| `total` | `number` | Nombre total d'elements |
| `page` | `number` | Page courante (commence a 1) |
| `pageSize` | `number` | Nombre d'elements par page |
| `onPageChange` | `(page: number) => void` | Callback lors du changement de page |

## Comportement

- Calcule `totalPages` = `Math.ceil(total / pageSize)` (minimum 1)
- Affiche "Affichage X-Y sur Z resultats" (ou "Aucun resultat" si `total === 0`)
- Bouton "Precedent" desactive sur la page 1
- Bouton "Suivant" desactive sur la derniere page
- Indicateur "Page X / Y" au centre

## Exemple d'utilisation

```tsx
import Pagination from '../components/Pagination';

const [page, setPage] = useState(1);
const pageSize = 10;

<Pagination
  total={items.length}
  page={page}
  pageSize={pageSize}
  onPageChange={setPage}
/>
```

## Implementation

```tsx
export default function Pagination({ total, page, pageSize, onPageChange }: PaginationProps) {
  const totalPages = Math.max(1, Math.ceil(total / pageSize));
  const start = total === 0 ? 0 : (page - 1) * pageSize + 1;
  const end = Math.min(page * pageSize, total);

  return (
    <div className="flex items-center justify-between border-t border-slate-200 bg-white px-4 py-3 text-sm">
      <span className="text-slate-600">
        {total === 0 ? 'Aucun resultat' : `Affichage ${start}--${end} sur ${total} resultats`}
      </span>
      <div className="flex items-center gap-2">
        <button onClick={() => onPageChange(page - 1)} disabled={page <= 1}>
          <ChevronLeft size={14} /> Precedent
        </button>
        <span>Page {page} / {totalPages}</span>
        <button onClick={() => onPageChange(page + 1)} disabled={page >= totalPages}>
          Suivant <ChevronRight size={14} />
        </button>
      </div>
    </div>
  );
}
```

## Accessibilite

Les boutons de navigation ont des `aria-label` descriptifs : "Page precedente" et "Page suivante".

## Tests

Voir `frontend/src/components/__tests__/Pagination.test.tsx`.
