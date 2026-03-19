# Installation du Frontend

Ce guide couvre l'installation et la configuration du frontend React de RIVORA.

## Prerequis

| Outil | Version minimale | Verification |
|-------|-----------------|--------------|
| Node.js | 20+ | `node --version` |
| npm | 10+ | `npm --version` |
| Git | 2.40+ | `git --version` |

## Installation

### 1. Cloner le projet

```bash
git clone https://github.com/khalilbenaz/RIVORA.git
cd RIVORA/src/frontend
```

### 2. Installer les dependances

```bash
npm install
```

### 3. Configurer l'environnement

Creez un fichier `.env.local` a la racine du dossier frontend :

```env
# URL de l'API backend
VITE_API_URL=http://localhost:5220

# URL du hub SignalR
VITE_SIGNALR_URL=http://localhost:5220/hubs

# Mode de l'application
VITE_APP_MODE=development

# Activer les devtools
VITE_ENABLE_DEVTOOLS=true
```

### 4. Lancer le serveur de developpement

```bash
npm run dev
```

Le frontend est accessible sur `http://localhost:5173`.

## Configuration du proxy

En developpement, Vite est configure pour proxyfier les requetes API vers le backend :

```typescript
// vite.config.ts
import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: {
      '/api': {
        target: 'http://localhost:5220',
        changeOrigin: true,
        secure: false,
      },
      '/hubs': {
        target: 'http://localhost:5220',
        changeOrigin: true,
        ws: true,
      },
    },
  },
});
```

## Demarrer avec le backend

Pour un environnement de developpement complet, lancez le backend et le frontend en parallele :

```bash
# Terminal 1 - Infrastructure
docker compose -f docker-compose.dev.yml up -d

# Terminal 2 - Backend API
cd src/api/RVR.Framework.Api
dotnet run

# Terminal 3 - Frontend
cd src/frontend
npm run dev
```

## Build de production

### Build standard

```bash
npm run build
```

Les fichiers sont generes dans `dist/`. Servez-les avec n'importe quel serveur HTTP statique.

### Preview du build

```bash
npm run preview
```

### Deploiement Docker

Le frontend peut etre deploye dans un conteneur Nginx :

```dockerfile
# Dockerfile
FROM node:20-alpine AS build
WORKDIR /app
COPY package*.json ./
RUN npm ci
COPY . .
RUN npm run build

FROM nginx:alpine
COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
```

Configuration Nginx pour le SPA :

```nginx
# nginx.conf
server {
    listen 80;
    root /usr/share/nginx/html;
    index index.html;

    # SPA routing - toutes les routes vers index.html
    location / {
        try_files $uri $uri/ /index.html;
    }

    # Proxy vers l'API backend
    location /api/ {
        proxy_pass http://api:5220;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
    }

    # WebSocket pour SignalR
    location /hubs/ {
        proxy_pass http://api:5220;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "upgrade";
    }

    # Cache des assets statiques
    location ~* \.(js|css|png|jpg|jpeg|gif|ico|svg|woff2?)$ {
        expires 1y;
        add_header Cache-Control "public, immutable";
    }
}
```

### Docker Compose

```yaml
services:
  frontend:
    build:
      context: ./src/frontend
      dockerfile: Dockerfile
    ports:
      - "3001:80"
    depends_on:
      - api
    environment:
      - VITE_API_URL=http://api:5220
```

## Problemes courants

### CORS en developpement

Si vous obtenez des erreurs CORS, verifiez que le backend autorise l'origine du frontend :

```csharp
// Dans Program.cs du backend
builder.Services.AddCors(options =>
{
    options.AddPolicy("Development", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});
```

### Port deja utilise

```bash
# Trouver le processus utilisant le port 5173
lsof -i :5173

# Ou changer le port dans vite.config.ts
server: { port: 3001 }
```

### Erreurs TypeScript

```bash
# Verifier les types
npx tsc --noEmit

# Regenerer les types depuis l'API
npm run generate-types
```

## Etape suivante

- [Architecture](/frontend/architecture) pour comprendre la structure du code
- [Authentification](/frontend/authentication) pour le flux JWT
- [Routing](/frontend/routing) pour la navigation
