---
title: State Management
description: Stores Zustand (auth, theme, toast) et creation de nouveaux stores
---

# State Management

RIVORA utilise **Zustand** pour la gestion d'etat global. 3 stores sont definis.

## authStore

**Fichier** : `frontend/src/store/authStore.ts`

Gere l'authentification utilisateur avec persistance localStorage.

```typescript
interface AuthState {
  token: string | null;
  user: User | null;
  isAuthenticated: boolean;
  login: (token: string, user: User) => void;
  logout: () => void;
  loadFromStorage: () => void;
}
```

### Utilisation

```tsx
import { useAuthStore } from '../store/authStore';

// Lire l'etat
const user = useAuthStore((s) => s.user);
const isAuth = useAuthStore((s) => s.isAuthenticated);

// Actions
const { login, logout } = useAuthStore();
login('jwt_token', { id: '1', name: 'Alice', ... });
logout(); // supprime token + user du localStorage
```

### Persistance

- `login()` : sauvegarde `rvr_token` et `rvr_user` dans localStorage
- `logout()` : supprime les deux cles
- `loadFromStorage()` : restaure l'etat depuis localStorage au demarrage

## themeStore

**Fichier** : `frontend/src/store/themeStore.ts`

Gere le theme (light, dark, system) avec application sur `<html>`.

```typescript
type Theme = 'light' | 'dark' | 'system';

interface ThemeState {
  theme: Theme;
  setTheme: (theme: Theme) => void;
  resolvedTheme: 'light' | 'dark';
}
```

### Fonctionnement

1. `setTheme()` sauvegarde dans `rvr_theme` (localStorage)
2. Appelle `applyTheme()` qui toggle la classe `dark` sur `document.documentElement`
3. Si `system`, detecte la preference via `prefers-color-scheme: dark`

```tsx
const { theme, setTheme, resolvedTheme } = useThemeStore();
setTheme('dark');    // force le dark mode
setTheme('system');  // suit la preference systeme
```

## toastStore

**Fichier** : `frontend/src/store/toastStore.ts`

Gere les notifications toast avec auto-dismiss.

```typescript
interface Toast {
  id: string;
  message: string;
  type: 'success' | 'error' | 'warning' | 'info';
  duration?: number;  // default: 5000ms
}

interface ToastState {
  toasts: Toast[];
  addToast: (toast: Omit<Toast, 'id'>) => void;
  removeToast: (id: string) => void;
}
```

### Utilisation

```tsx
const { addToast } = useToastStore();

addToast({ message: 'Sauvegarde reussie', type: 'success' });
addToast({ message: 'Erreur serveur', type: 'error', duration: 8000 });
```

Les toasts sont supprimes automatiquement apres `duration` ms (default 5000).

## Creer un nouveau store

```typescript
import { create } from 'zustand';

interface MyState {
  count: number;
  increment: () => void;
  reset: () => void;
}

export const useMyStore = create<MyState>((set) => ({
  count: 0,
  increment: () => set((s) => ({ count: s.count + 1 })),
  reset: () => set({ count: 0 }),
}));
```

### Conventions

- Fichier dans `src/store/` avec suffixe `Store.ts`
- Export nomme du hook (`useXxxStore`)
- Utiliser des selectors pour eviter les re-renders : `useMyStore((s) => s.count)`
