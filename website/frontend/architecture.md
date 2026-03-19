# Architecture Frontend

Ce guide decrit l'organisation du code frontend React de RIVORA, les conventions de nommage et les patterns utilises.

## Structure des dossiers

```
src/
  api/
    axiosInstance.ts       # Instance Axios configuree (interceptors, base URL)
    authApi.ts             # Endpoints authentification
    productsApi.ts         # Endpoints produits
    tenantsApi.ts          # Endpoints multi-tenancy
    webhooksApi.ts         # Endpoints webhooks
    ...
  components/
    ui/                    # Composants UI generiques
      Badge.tsx
      Button.tsx
      Card.tsx
      Pagination.tsx
      StatCard.tsx
      TableSkeleton.tsx
    layout/                # Composants de mise en page
      Sidebar.tsx
      Navbar.tsx
      Footer.tsx
      AdminLayout.tsx
      ClientLayout.tsx
    charts/                # Composants graphiques SVG
      LineChart.tsx
      BarChart.tsx
      PieChart.tsx
    NotificationBell.tsx   # Notifications temps reel
  hooks/
    useAuth.ts             # Hook d'authentification
    useTheme.ts            # Hook de theme (dark mode)
    useSignalR.ts          # Hook de connexion SignalR
    useDebounce.ts         # Hook de debounce
    usePagination.ts       # Hook de pagination
    useI18n.ts             # Hook d'internationalisation
  pages/
    Landing.tsx            # Page d'accueil publique
    Login.tsx              # Connexion
    Register.tsx           # Inscription
    Dashboard.tsx          # Tableau de bord
    Chat.tsx               # Chat IA
    FlowBuilder.tsx        # Editeur de workflows
    ProjectWizard.tsx      # Assistant de creation
    EntityGenerator.tsx    # Generateur d'entites
    Kanban.tsx             # Tableau Kanban
    Analytics.tsx          # Graphiques
    Webhooks.tsx           # Gestion webhooks
    AuditLogs.tsx          # Logs d'audit
    Settings.tsx           # Parametres
    Tenants.tsx            # Gestion tenants
    Users.tsx              # Gestion utilisateurs
    Roles.tsx              # Gestion roles
    NotFound.tsx           # Page 404
    ...
  store/
    authStore.ts           # Store Zustand pour l'authentification
    themeStore.ts          # Store pour le theme
    notificationStore.ts   # Store pour les notifications
    tenantStore.ts         # Store pour le tenant actif
    i18nStore.ts           # Store pour la langue
  types/
    auth.ts                # Types User, LoginRequest, etc.
    api.ts                 # Types ApiResponse, PagedResult, etc.
    entities.ts            # Types des entites metier
    webhooks.ts            # Types Webhook, WebhookEvent
  utils/
    formatters.ts          # Formatage dates, nombres, devises
    validators.ts          # Validation email, mot de passe, etc.
    constants.ts           # Constantes de l'application
    cn.ts                  # Utilitaire de merge de classes CSS
  i18n/
    fr.json                # Traductions francaises
    en.json                # Traductions anglaises
    index.ts               # Configuration i18n
  App.tsx                  # Composant racine
  main.tsx                 # Point d'entree
```

## Conventions de nommage

| Element | Convention | Exemple |
|---------|-----------|---------|
| Composants | PascalCase | `StatCard.tsx` |
| Hooks | camelCase avec prefix `use` | `useAuth.ts` |
| Stores | camelCase avec suffix `Store` | `authStore.ts` |
| Types | PascalCase | `LoginRequest` |
| Utilitaires | camelCase | `formatDate.ts` |
| Fichiers de test | `.test.tsx` | `Badge.test.tsx` |
| Constantes | UPPER_SNAKE_CASE | `API_BASE_URL` |

## Pattern des composants

### Composant fonctionnel avec TypeScript

```tsx
import { type FC } from 'react';

interface StatCardProps {
  title: string;
  value: number | string;
  icon: React.ReactNode;
  trend?: {
    value: number;
    direction: 'up' | 'down';
  };
  className?: string;
}

export const StatCard: FC<StatCardProps> = ({
  title,
  value,
  icon,
  trend,
  className,
}) => {
  return (
    <div className={cn('rounded-xl border bg-card p-6 shadow-sm', className)}>
      <div className="flex items-center justify-between">
        <p className="text-sm text-muted-foreground">{title}</p>
        {icon}
      </div>
      <p className="mt-2 text-2xl font-bold">{value}</p>
      {trend && (
        <p className={cn(
          'mt-1 text-xs',
          trend.direction === 'up' ? 'text-green-500' : 'text-red-500'
        )}>
          {trend.direction === 'up' ? '+' : ''}{trend.value}%
        </p>
      )}
    </div>
  );
};
```

### Custom hook

```tsx
import { useState, useEffect, useCallback } from 'react';

export function usePagination<T>(
  fetchFn: (page: number, pageSize: number) => Promise<PagedResult<T>>,
  pageSize = 10
) {
  const [data, setData] = useState<T[]>([]);
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(0);
  const [loading, setLoading] = useState(false);

  const fetch = useCallback(async () => {
    setLoading(true);
    try {
      const result = await fetchFn(page, pageSize);
      setData(result.items);
      setTotalPages(result.totalPages);
    } finally {
      setLoading(false);
    }
  }, [page, pageSize, fetchFn]);

  useEffect(() => { fetch(); }, [fetch]);

  return { data, page, totalPages, loading, setPage, refresh: fetch };
}
```

### Store Zustand

```tsx
import { create } from 'zustand';
import { persist } from 'zustand/middleware';

interface ThemeState {
  mode: 'light' | 'dark' | 'system';
  setMode: (mode: 'light' | 'dark' | 'system') => void;
  resolvedMode: () => 'light' | 'dark';
}

export const useThemeStore = create<ThemeState>()(
  persist(
    (set, get) => ({
      mode: 'system',
      setMode: (mode) => set({ mode }),
      resolvedMode: () => {
        const { mode } = get();
        if (mode !== 'system') return mode;
        return window.matchMedia('(prefers-color-scheme: dark)').matches
          ? 'dark'
          : 'light';
      },
    }),
    { name: 'theme-storage' }
  )
);
```

## Couche API

Toutes les interactions avec le backend passent par des modules API dedies qui utilisent une instance Axios partagee :

```tsx
// api/axiosInstance.ts
import axios from 'axios';
import { useAuthStore } from '../store/authStore';

const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL || '/api',
  timeout: 30000,
  headers: { 'Content-Type': 'application/json' },
});

// Interceptor : ajouter le token JWT
api.interceptors.request.use((config) => {
  const token = useAuthStore.getState().accessToken;
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  // Multi-tenancy
  const tenantId = useAuthStore.getState().tenantId;
  if (tenantId) {
    config.headers['X-Tenant-Id'] = tenantId;
  }
  return config;
});

// Interceptor : refresh token automatique
api.interceptors.response.use(
  (response) => response,
  async (error) => {
    if (error.response?.status === 401) {
      const refreshed = await useAuthStore.getState().refreshSession();
      if (refreshed) {
        return api.request(error.config);
      }
      useAuthStore.getState().logout();
    }
    return Promise.reject(error);
  }
);

export default api;
```

## Types partages

```tsx
// types/api.ts
export interface ApiResponse<T> {
  data: T;
  success: boolean;
  message?: string;
  errors?: Record<string, string[]>;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

// types/auth.ts
export interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  roles: string[];
  tenantId?: string;
}

export interface LoginRequest {
  email: string;
  password: string;
  twoFactorCode?: string;
}

export interface AuthTokens {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
}
```

## Bonnes pratiques

- **Colocation** : Gardez les fichiers lies ensemble (composant + test + types)
- **Barrel exports** : Utilisez des `index.ts` pour simplifier les imports
- **Lazy loading** : Chargez les pages a la demande avec `React.lazy()`
- **Error boundaries** : Encapsulez les sections critiques dans des error boundaries
- **Memoisation** : Utilisez `React.memo`, `useMemo` et `useCallback` quand necessaire
- **Types stricts** : Evitez `any`, preferez des types precis

## Etape suivante

- [Routing](/frontend/routing) pour comprendre la navigation
- [Authentification](/frontend/authentication) pour le flux JWT
- [Etat global (Zustand)](/frontend/state-management) pour la gestion d'etat
