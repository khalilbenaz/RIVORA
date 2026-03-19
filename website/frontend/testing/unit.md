---
title: Tests unitaires
description: Configuration Vitest, patterns de test et couverture
---

# Tests unitaires

Les tests unitaires du frontend utilisent **Vitest** avec **jsdom** et **React Testing Library**.

## Configuration

**Fichier** : `frontend/vitest.config.ts`

```typescript
import { defineConfig } from 'vitest/config';
import react from '@vitejs/plugin-react';

export default defineConfig({
  plugins: [react()],
  test: {
    environment: 'jsdom',
    globals: true,
    setupFiles: './src/test/setup.ts',
    exclude: ['e2e/**', 'node_modules/**'],
  },
});
```

### Points cles

- **Environment** : `jsdom` pour simuler le DOM du navigateur
- **Globals** : `true` pour eviter d'importer `describe`, `it`, `expect`
- **Setup** : `src/test/setup.ts` pour les configurations globales
- **Exclusions** : les tests E2E et node_modules sont exclus

## Fichiers de test

Les tests sont places dans `src/components/__tests__/` :

| Fichier | Composant teste |
|---------|----------------|
| `Badge.test.tsx` | Badge |
| `StatCard.test.tsx` | StatCard |
| `Pagination.test.tsx` | Pagination |
| `Spinner.test.tsx` | Spinner |
| `TableSkeleton.test.tsx` | TableSkeleton |
| `FormField.test.tsx` | FormField |

## Patterns de test

### Rendu basique

```tsx
import { render, screen } from '@testing-library/react';
import Badge from '../Badge';

describe('Badge', () => {
  it('renders children', () => {
    render(<Badge>GET</Badge>);
    expect(screen.getByText('GET')).toBeInTheDocument();
  });

  it('applies variant class', () => {
    render(<Badge variant="success">OK</Badge>);
    const el = screen.getByText('OK');
    expect(el.className).toContain('bg-emerald-100');
  });
});
```

### Test avec interactions

```tsx
import { render, screen, fireEvent } from '@testing-library/react';
import Pagination from '../Pagination';

it('calls onPageChange when clicking next', () => {
  const onPageChange = vi.fn();
  render(<Pagination total={50} page={1} pageSize={10} onPageChange={onPageChange} />);
  fireEvent.click(screen.getByLabelText('Page suivante'));
  expect(onPageChange).toHaveBeenCalledWith(2);
});
```

## Lancer les tests

```bash
# Lancer tous les tests
npm test

# Mode watch
npm test -- --watch

# Avec couverture
npm test -- --coverage

# Un seul fichier
npm test -- Badge.test
```

## Conventions

- Un fichier de test par composant dans `__tests__/`
- Nommage : `ComponentName.test.tsx`
- Utiliser `screen.getByText`, `screen.getByRole`, etc. pour les queries
- Utiliser `vi.fn()` pour les mocks de fonctions
