# Authentification

Le frontend RIVORA utilise un flux JWT complet avec refresh token automatique, support 2FA et OAuth2.

## Flux d'authentification

```
1. Login (email + password)
       |
       v
2. API retourne accessToken + refreshToken
       |
       v
3. Zustand stocke les tokens (persist localStorage)
       |
       v
4. Axios interceptor ajoute Authorization header
       |
       v
5. Token expire -> interceptor refresh automatique
       |
       v
6. Refresh echoue -> redirect vers /login
```

## Store d'authentification

Le store Zustand gere l'etat d'authentification :

```tsx
// store/authStore.ts
import { create } from 'zustand';
import { persist } from 'zustand/middleware';
import { authApi } from '../api/authApi';
import type { User, AuthTokens, LoginRequest } from '../types/auth';

interface AuthState {
  user: User | null;
  accessToken: string | null;
  refreshToken: string | null;
  tenantId: string | null;
  isAuthenticated: boolean;
  isLoading: boolean;

  login: (credentials: LoginRequest) => Promise<void>;
  logout: () => void;
  refreshSession: () => Promise<boolean>;
  setTenant: (tenantId: string) => void;
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set, get) => ({
      user: null,
      accessToken: null,
      refreshToken: null,
      tenantId: null,
      isAuthenticated: false,
      isLoading: false,

      login: async (credentials) => {
        set({ isLoading: true });
        try {
          const { data } = await authApi.login(credentials);
          set({
            user: data.user,
            accessToken: data.accessToken,
            refreshToken: data.refreshToken,
            isAuthenticated: true,
            isLoading: false,
          });
        } catch (error) {
          set({ isLoading: false });
          throw error;
        }
      },

      logout: () => {
        set({
          user: null,
          accessToken: null,
          refreshToken: null,
          tenantId: null,
          isAuthenticated: false,
        });
      },

      refreshSession: async () => {
        const { refreshToken } = get();
        if (!refreshToken) return false;
        try {
          const { data } = await authApi.refresh(refreshToken);
          set({
            accessToken: data.accessToken,
            refreshToken: data.refreshToken,
          });
          return true;
        } catch {
          return false;
        }
      },

      setTenant: (tenantId) => set({ tenantId }),
    }),
    {
      name: 'auth-storage',
      partialize: (state) => ({
        accessToken: state.accessToken,
        refreshToken: state.refreshToken,
        user: state.user,
        tenantId: state.tenantId,
      }),
    }
  )
);
```

## API d'authentification

```tsx
// api/authApi.ts
import api from './axiosInstance';
import type { LoginRequest, AuthTokens, User } from '../types/auth';
import type { ApiResponse } from '../types/api';

export const authApi = {
  login: (credentials: LoginRequest) =>
    api.post<ApiResponse<AuthTokens & { user: User }>>('/auth/login', credentials),

  register: (data: { email: string; password: string; firstName: string; lastName: string }) =>
    api.post<ApiResponse<{ userId: string }>>('/auth/register', data),

  refresh: (refreshToken: string) =>
    api.post<ApiResponse<AuthTokens>>('/auth/refresh', { refreshToken }),

  logout: () =>
    api.post('/auth/logout'),

  me: () =>
    api.get<ApiResponse<User>>('/auth/me'),

  verify2FA: (code: string) =>
    api.post<ApiResponse<AuthTokens>>('/auth/2fa/verify', { code }),

  enable2FA: () =>
    api.post<ApiResponse<{ qrCodeUri: string; secret: string }>>('/auth/2fa/enable'),
};
```

## Intercepteurs Axios

### Ajout automatique du token

```tsx
api.interceptors.request.use((config) => {
  const token = useAuthStore.getState().accessToken;
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});
```

### Refresh automatique

Lorsqu'une requete retourne 401, l'intercepteur tente automatiquement un refresh :

```tsx
let isRefreshing = false;
let failedQueue: Array<{ resolve: Function; reject: Function }> = [];

const processQueue = (error: any, token: string | null = null) => {
  failedQueue.forEach(({ resolve, reject }) => {
    if (error) reject(error);
    else resolve(token);
  });
  failedQueue = [];
};

api.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;

    if (error.response?.status === 401 && !originalRequest._retry) {
      if (isRefreshing) {
        return new Promise((resolve, reject) => {
          failedQueue.push({ resolve, reject });
        }).then((token) => {
          originalRequest.headers.Authorization = `Bearer ${token}`;
          return api(originalRequest);
        });
      }

      originalRequest._retry = true;
      isRefreshing = true;

      try {
        const success = await useAuthStore.getState().refreshSession();
        if (success) {
          const newToken = useAuthStore.getState().accessToken;
          processQueue(null, newToken);
          originalRequest.headers.Authorization = `Bearer ${newToken}`;
          return api(originalRequest);
        }
      } catch (refreshError) {
        processQueue(refreshError, null);
        useAuthStore.getState().logout();
        window.location.href = '/login';
      } finally {
        isRefreshing = false;
      }
    }

    return Promise.reject(error);
  }
);
```

## Routes protegees

Composant `ProtectedRoute` pour proteger les pages :

```tsx
// components/ProtectedRoute.tsx
import { Navigate, Outlet } from 'react-router-dom';
import { useAuthStore } from '../store/authStore';

interface ProtectedRouteProps {
  requiredRoles?: string[];
  redirectTo?: string;
}

export const ProtectedRoute: FC<ProtectedRouteProps> = ({
  requiredRoles,
  redirectTo = '/login',
}) => {
  const { isAuthenticated, user } = useAuthStore();

  if (!isAuthenticated) {
    return <Navigate to={redirectTo} replace />;
  }

  if (requiredRoles && user) {
    const hasRole = requiredRoles.some((role) => user.roles.includes(role));
    if (!hasRole) {
      return <Navigate to="/unauthorized" replace />;
    }
  }

  return <Outlet />;
};
```

## OAuth2

Le frontend supporte la connexion via des fournisseurs OAuth2 :

```tsx
// pages/Login.tsx (extrait)
const handleOAuthLogin = (provider: 'google' | 'github' | 'microsoft') => {
  const redirectUri = encodeURIComponent(window.location.origin + '/oauth/callback');
  window.location.href = `${API_URL}/auth/oauth/${provider}?redirect_uri=${redirectUri}`;
};

// pages/OAuthCallback.tsx
const OAuthCallback = () => {
  const searchParams = new URLSearchParams(window.location.search);
  const code = searchParams.get('code');

  useEffect(() => {
    if (code) {
      authApi.exchangeOAuthCode(code).then(({ data }) => {
        useAuthStore.getState().login(data);
        navigate('/dashboard');
      });
    }
  }, [code]);
};
```

## Bonnes pratiques

- **Ne stockez jamais** les tokens dans des cookies sans le flag `HttpOnly`
- **Utilisez `partialize`** dans Zustand pour ne persister que les tokens, pas l'etat transitoire
- **Queue de refresh** : Evitez les appels de refresh en parallele avec une file d'attente
- **Nettoyage au logout** : Effacez tous les stores et redirigez vers `/login`
- **Timeout** : Configurez un timeout sur les requetes d'authentification (30s max)

## Etape suivante

- [Routing](/frontend/routing) pour les routes protegees
- [Etat global (Zustand)](/frontend/state-management) pour la gestion d'etat
- [i18n](/frontend/i18n) pour l'internationalisation
