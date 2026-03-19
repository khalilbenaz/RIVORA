---
title: NotificationBell
description: Cloche de notifications avec integration SignalR et compteur
---

# NotificationBell

Composant de cloche de notifications avec badge de compteur, dropdown et indicateur de connexion SignalR.

**Fichier source** : `frontend/src/components/NotificationBell.tsx`

## Fonctionnalites

- Badge rouge avec compteur de messages non lus
- Pastille de connexion (vert = connecte, rouge = deconnecte)
- Dropdown avec liste de notifications
- Bouton "Mark all as read"
- Fermeture au clic exterieur

## Integration SignalR

Le composant utilise le hook `useSignalR` pour recevoir les notifications en temps reel :

```tsx
const { notifications, connected, unreadCount, markAsRead, markAllAsRead } = useSignalR();
```

### Interface Notification

```typescript
interface Notification {
  id: string;
  type: string;
  data: unknown;
  timestamp: Date;
  read: boolean;
}
```

## Indicateur de connexion

Une petite pastille en bas a droite de la cloche indique l'etat de la connexion :

```tsx
<span className={`absolute bottom-1 right-1 h-2 w-2 rounded-full border border-white ${
  connected ? 'bg-green-500' : 'bg-red-500'
}`} />
```

## Dropdown

Le dropdown affiche :
- **En-tete** : titre "Notifications" + bouton "Mark all as read" (visible si unreadCount > 0)
- **Liste** : chaque notification avec type (badge), temps relatif, et contenu
- **Etat vide** : message "No notifications yet"

### Format temps relatif

```tsx
function formatRelativeTime(date: Date): string {
  const diffSec = Math.floor(diffMs / 1000);
  if (diffSec < 60) return `${diffSec}s`;
  if (diffMin < 60) return `${diffMin}m`;
  if (diffHour < 24) return `${diffHour}h`;
  return `${diffDay}d`;
}
```

## Fermeture automatique

Le dropdown se ferme au clic exterieur grace a un `useEffect` avec `mousedown` listener :

```tsx
useEffect(() => {
  function handleClick(e: MouseEvent) {
    if (ref.current && !ref.current.contains(e.target as Node)) {
      setOpen(false);
    }
  }
  document.addEventListener('mousedown', handleClick);
  return () => document.removeEventListener('mousedown', handleClick);
}, []);
```

## Ou il est utilise

Le composant est place dans le header du `Layout.tsx`, visible sur toutes les pages admin.

## Accessibilite

Le bouton de la cloche a un `aria-label` traduit via i18next : `notifications.title`.
