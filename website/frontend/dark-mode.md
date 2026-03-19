---
title: Dark Mode
description: Theme toggle, Tailwind dark variant et themeStore
---

# Dark Mode

RIVORA supporte le dark mode avec 3 options : Light, Dark et System (preference OS).

## ThemeToggle

**Fichier** : `frontend/src/components/ThemeToggle.tsx`

Composant de bascule avec 3 boutons iconiques :

```tsx
import { Sun, Moon, Monitor } from 'lucide-react';
import { useThemeStore } from '../store/themeStore';

const options = [
  { value: 'light', icon: Sun, label: 'Light' },
  { value: 'dark', icon: Moon, label: 'Dark' },
  { value: 'system', icon: Monitor, label: 'System' },
];

export default function ThemeToggle() {
  const { theme, setTheme } = useThemeStore();

  return (
    <div className="flex items-center rounded-lg bg-slate-800 p-0.5">
      {options.map(({ value, icon: Icon, label }) => (
        <button
          key={value}
          onClick={() => setTheme(value)}
          aria-label={label}
          className={`rounded-md p-1.5 transition-colors ${
            theme === value
              ? 'bg-slate-600 text-white'
              : 'text-slate-400 hover:text-slate-200'
          }`}
        >
          <Icon size={14} />
        </button>
      ))}
    </div>
  );
}
```

## themeStore

**Fichier** : `frontend/src/store/themeStore.ts`

Le store gere 3 valeurs de theme et applique la classe CSS :

```typescript
type Theme = 'light' | 'dark' | 'system';

function applyTheme(theme: Theme) {
  const resolved = theme === 'system' ? getSystemTheme() : theme;
  document.documentElement.classList.toggle('dark', resolved === 'dark');
  return resolved;
}
```

### Detection systeme

```typescript
function getSystemTheme(): 'light' | 'dark' {
  return window.matchMedia('(prefers-color-scheme: dark)').matches
    ? 'dark' : 'light';
}
```

### Persistance

Le theme choisi est sauvegarde dans `localStorage` sous la cle `rvr_theme`.

## Utilisation de Tailwind dark variant

Tailwind CSS est configure en mode `class`. Les composants utilisent le prefix `dark:` :

```tsx
// Exemple dans FlowCanvas.tsx
<div className="border-slate-200 bg-white dark:border-slate-700 dark:bg-slate-800">

// Exemple dans ChatPage.tsx
<input className="border-slate-200 bg-slate-50 dark:border-slate-600 dark:bg-slate-800 dark:text-slate-100" />
```

### Conventions

- **Backgrounds** : `bg-white` / `dark:bg-slate-800` ou `dark:bg-slate-900`
- **Borders** : `border-slate-200` / `dark:border-slate-700`
- **Text** : `text-slate-900` / `dark:text-slate-100`
- **Hover** : `hover:bg-slate-100` / `dark:hover:bg-slate-700`

## Architecture

```
Utilisateur clique sur ThemeToggle
  --> useThemeStore.setTheme('dark')
    --> localStorage.setItem('rvr_theme', 'dark')
    --> document.documentElement.classList.add('dark')
    --> Tailwind applique les variantes dark:*
```

## Acceder au theme resolu

Pour les cas ou vous avez besoin du theme effectif (pas "system") :

```tsx
const { resolvedTheme } = useThemeStore();
// resolvedTheme est toujours 'light' ou 'dark'
```
