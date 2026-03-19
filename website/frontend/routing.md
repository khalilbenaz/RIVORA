# Routing

Le frontend RIVORA utilise React Router 7 pour gerer la navigation entre les 28 pages de l'application. Le routing distingue les routes publiques, les routes authentifiees et les routes administrateur.

## Configuration du router

```tsx
// App.tsx
import { BrowserRouter, Routes, Route } from 'react-router-dom';
import { lazy, Suspense } from 'react';
import { ProtectedRoute } from './components/ProtectedRoute';
import { AdminLayout } from './components/layout/AdminLayout';
import { ClientLayout } from './components/layout/ClientLayout';
import { LoadingSpinner } from './components/ui/LoadingSpinner';

// Lazy loading des pages
const Landing = lazy(() => import('./pages/Landing'));
const Login = lazy(() => import('./pages/Login'));
const Register = lazy(() => import('./pages/Register'));
const Dashboard = lazy(() => import('./pages/Dashboard'));
const Chat = lazy(() => import('./pages/Chat'));
const FlowBuilder = lazy(() => import('./pages/FlowBuilder'));
const ProjectWizard = lazy(() => import('./pages/ProjectWizard'));
const EntityGenerator = lazy(() => import('./pages/EntityGenerator'));
const Kanban = lazy(() => import('./pages/Kanban'));
const Analytics = lazy(() => import('./pages/Analytics'));
const Webhooks = lazy(() => import('./pages/Webhooks'));
const AuditLogs = lazy(() => import('./pages/AuditLogs'));
const Settings = lazy(() => import('./pages/Settings'));
const Tenants = lazy(() => import('./pages/Tenants'));
const Users = lazy(() => import('./pages/Users'));
const Roles = lazy(() => import('./pages/Roles'));
const NotFound = lazy(() => import('./pages/NotFound'));

export default function App() {
  return (
    <BrowserRouter>
      <Suspense fallback={<LoadingSpinner />}>
        <Routes>
          {/* Routes publiques */}
          <Route path="/" element={<Landing />} />
          <Route path="/login" element={<Login />} />
          <Route path="/register" element={<Register />} />
          <Route path="/oauth/callback" element={<OAuthCallback />} />

          {/* Routes authentifiees - Layout client */}
          <Route element={<ProtectedRoute />}>
            <Route element={<ClientLayout />}>
              <Route path="/dashboard" element={<Dashboard />} />
              <Route path="/chat" element={<Chat />} />
              <Route path="/flow-builder" element={<FlowBuilder />} />
              <Route path="/project-wizard" element={<ProjectWizard />} />
              <Route path="/entity-generator" element={<EntityGenerator />} />
              <Route path="/kanban" element={<Kanban />} />
              <Route path="/analytics" element={<Analytics />} />
              <Route path="/webhooks" element={<Webhooks />} />
              <Route path="/settings" element={<Settings />} />
            </Route>
          </Route>

          {/* Routes admin */}
          <Route element={<ProtectedRoute requiredRoles={['Admin']} />}>
            <Route element={<AdminLayout />}>
              <Route path="/admin/tenants" element={<Tenants />} />
              <Route path="/admin/users" element={<Users />} />
              <Route path="/admin/roles" element={<Roles />} />
              <Route path="/admin/audit-logs" element={<AuditLogs />} />
            </Route>
          </Route>

          {/* 404 */}
          <Route path="*" element={<NotFound />} />
        </Routes>
      </Suspense>
    </BrowserRouter>
  );
}
```

## Routes publiques vs authentifiees

| Type | Acces | Layout | Exemples |
|------|-------|--------|----------|
| Public | Tout le monde | Aucun | `/`, `/login`, `/register` |
| Authentifie | Utilisateurs connectes | `ClientLayout` | `/dashboard`, `/chat` |
| Admin | Role `Admin` requis | `AdminLayout` | `/admin/tenants`, `/admin/users` |

## Layouts

### ClientLayout

Le layout client inclut la barre laterale et la barre de navigation :

```tsx
// components/layout/ClientLayout.tsx
import { Outlet } from 'react-router-dom';
import { Sidebar } from './Sidebar';
import { Navbar } from './Navbar';

export const ClientLayout = () => {
  return (
    <div className="flex h-screen bg-background">
      <Sidebar />
      <div className="flex flex-1 flex-col overflow-hidden">
        <Navbar />
        <main className="flex-1 overflow-y-auto p-6">
          <Outlet />
        </main>
      </div>
    </div>
  );
};
```

### AdminLayout

Le layout admin ajoute un indicateur et des liens supplementaires :

```tsx
// components/layout/AdminLayout.tsx
import { Outlet } from 'react-router-dom';
import { AdminSidebar } from './AdminSidebar';
import { Navbar } from './Navbar';

export const AdminLayout = () => {
  return (
    <div className="flex h-screen bg-background">
      <AdminSidebar />
      <div className="flex flex-1 flex-col overflow-hidden">
        <Navbar showAdminBadge />
        <main className="flex-1 overflow-y-auto p-6">
          <div className="mx-auto max-w-7xl">
            <Outlet />
          </div>
        </main>
      </div>
    </div>
  );
};
```

## Route Guards

### ProtectedRoute

Le composant `ProtectedRoute` verifie l'authentification et les roles :

```tsx
import { Navigate, Outlet, useLocation } from 'react-router-dom';
import { useAuthStore } from '../../store/authStore';

interface ProtectedRouteProps {
  requiredRoles?: string[];
}

export const ProtectedRoute: FC<ProtectedRouteProps> = ({ requiredRoles }) => {
  const { isAuthenticated, user } = useAuthStore();
  const location = useLocation();

  if (!isAuthenticated) {
    // Sauvegarder la route pour rediriger apres login
    return <Navigate to="/login" state={{ from: location }} replace />;
  }

  if (requiredRoles && user) {
    const hasRequiredRole = requiredRoles.some((role) =>
      user.roles.includes(role)
    );
    if (!hasRequiredRole) {
      return <Navigate to="/unauthorized" replace />;
    }
  }

  return <Outlet />;
};
```

### Redirection post-login

Apres une connexion reussie, redirigez vers la page d'origine :

```tsx
// pages/Login.tsx (extrait)
const location = useLocation();
const navigate = useNavigate();

const handleLogin = async (credentials: LoginRequest) => {
  await useAuthStore.getState().login(credentials);
  const from = (location.state as any)?.from?.pathname || '/dashboard';
  navigate(from, { replace: true });
};
```

## Navigation programmatique

```tsx
import { useNavigate } from 'react-router-dom';

const MyComponent = () => {
  const navigate = useNavigate();

  const handleClick = () => {
    navigate('/dashboard');
  };

  const handleBack = () => {
    navigate(-1);
  };

  const handleReplace = () => {
    navigate('/login', { replace: true });
  };
};
```

## Lazy loading et code splitting

Chaque page est chargee a la demande grace a `React.lazy()`. Cela reduit la taille du bundle initial :

```tsx
// Avant : tout est charge au demarrage
import Dashboard from './pages/Dashboard';

// Apres : charge uniquement quand la route est visitee
const Dashboard = lazy(() => import('./pages/Dashboard'));
```

Le `Suspense` wrapper affiche un spinner pendant le chargement :

```tsx
<Suspense fallback={<LoadingSpinner />}>
  <Routes>...</Routes>
</Suspense>
```

## Bonnes pratiques

- **Lazy loading** : Toujours utiliser `lazy()` pour les pages
- **Route state** : Sauvegarder `location.state` pour les redirections post-login
- **Error boundaries** : Encapsuler les routes dans des error boundaries
- **Breadcrumbs** : Utiliser `useMatches()` pour generer des fil d'Ariane
- **Scroll restoration** : Ajouter `<ScrollRestoration />` dans le router

## Etape suivante

- [Authentification](/frontend/authentication) pour le flux JWT
- [Architecture](/frontend/architecture) pour la structure du code
- [Etat global (Zustand)](/frontend/state-management) pour la gestion d'etat
