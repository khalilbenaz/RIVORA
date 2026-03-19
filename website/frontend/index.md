# Frontend React

RIVORA inclut un frontend React 19 moderne et complet, concu pour fonctionner avec l'API backend .NET 9. Il fournit 28 pages pretes a l'emploi couvrant l'ensemble des fonctionnalites du framework.

## Stack technique

| Technologie | Version | Role |
|-------------|---------|------|
| React | 19 | Framework UI |
| Vite | 6 | Build tool & dev server |
| TailwindCSS | 4 | Styles utilitaires |
| Zustand | 5 | Gestion d'etat global |
| Axios | 1.7 | Client HTTP |
| React Router | 7 | Routing SPA |
| SignalR | 9 | Communication temps reel |
| Vitest | 2 | Tests unitaires |
| Playwright | 1.48 | Tests E2E |

## Fonctionnalites

- **Authentification** : Login, Register, 2FA, OAuth2, session JWT
- **Multi-tenant** : Switch de tenant, isolation des donnees
- **Internationalisation** : Francais et Anglais avec switch dynamique
- **Dark Mode** : Theme clair/sombre avec detection systeme
- **Temps reel** : Notifications live, chat, mises a jour SignalR
- **28 pages** : Dashboard, Chat, Flow Builder, Kanban, Analytics, Webhooks, etc.

## Demarrage rapide

```bash
cd src/frontend

# Installer les dependances
npm install

# Lancer le serveur de developpement
npm run dev
```

Le frontend demarre sur `http://localhost:5173` et se connecte automatiquement a l'API sur `http://localhost:5220`.

## Structure du projet

```
src/frontend/
  src/
    api/              # Clients API (Axios instances, endpoints)
    components/       # Composants reutilisables (Badge, StatCard, etc.)
    hooks/            # Custom hooks React
    pages/            # Pages de l'application (28 pages)
    store/            # Stores Zustand (auth, theme, notifications)
    types/            # Types TypeScript partages
    utils/            # Fonctions utilitaires
    i18n/             # Fichiers de traduction FR/EN
    App.tsx           # Composant racine et routing
    main.tsx          # Point d'entree
  public/             # Assets statiques
  index.html          # Template HTML
  vite.config.ts      # Configuration Vite
  tailwind.config.js  # Configuration TailwindCSS
  tsconfig.json       # Configuration TypeScript
```

## Pages disponibles

| Page | Route | Description |
|------|-------|-------------|
| Landing | `/` | Page d'accueil publique |
| Login | `/login` | Connexion |
| Register | `/register` | Inscription |
| Dashboard | `/dashboard` | Tableau de bord principal |
| Chat | `/chat` | Chat IA temps reel |
| Flow Builder | `/flow-builder` | Editeur de workflows visuels |
| Project Wizard | `/project-wizard` | Assistant de creation de projet |
| Entity Generator | `/entity-generator` | Generateur d'entites |
| Kanban | `/kanban` | Tableau Kanban |
| Analytics | `/analytics` | Graphiques et metriques |
| Webhooks | `/webhooks` | Gestion des webhooks |
| Audit Logs | `/audit-logs` | Journal d'audit |
| Settings | `/settings` | Parametres utilisateur |
| Tenants | `/tenants` | Gestion des tenants |
| Users | `/users` | Gestion des utilisateurs |
| Roles | `/roles` | Gestion des roles |

## Scripts npm

```bash
npm run dev        # Serveur de developpement (port 5173)
npm run build      # Build de production
npm run preview    # Preview du build de production
npm run lint       # ESLint
npm run test       # Tests unitaires (Vitest)
npm run test:e2e   # Tests E2E (Playwright)
npm run type-check # Verification TypeScript
```

## Etape suivante

- [Installation](/frontend/installation) pour configurer l'environnement de developpement
- [Architecture](/frontend/architecture) pour comprendre la structure du code
- [Authentification](/frontend/authentication) pour le flux JWT
- [Routing](/frontend/routing) pour la navigation
