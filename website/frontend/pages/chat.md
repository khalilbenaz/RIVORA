---
title: Chat
description: Messagerie temps reel avec rooms et integration SignalR
---

# Chat Page

La page Chat offre une messagerie temps reel avec des rooms, des messages et une integration SignalR.

**Fichier source** : `frontend/src/pages/ChatPage.tsx`
**Route** : `/admin/chat`

## Layout

Layout en deux panneaux responsive :
- **Gauche (320px)** : liste des rooms avec recherche
- **Droite (flexible)** : fil de messages + zone de saisie

Sur mobile, un seul panneau est visible a la fois avec navigation par bouton retour.

## Rooms

Chaque room affiche :
- Avatar (premiere lettre du nom, gradient bleu-violet)
- Nom de la room
- Dernier message + timestamp relatif
- Badge de messages non lus (max 99+)

```tsx
interface ChatRoom {
  id: string;
  name: string;
  participants: string[];
  lastMessage?: ChatMessage;
  unreadCount: number;
}
```

### Recherche

Un champ de recherche filtre les rooms par nom en temps reel.

## Messages

Les messages s'affichent sous forme de bulles :
- **Messages propres** : alignes a droite, fond bleu
- **Messages recus** : alignes a gauche, fond gris, avec nom de l'expediteur

```tsx
interface ChatMessage {
  id: string;
  roomId: string;
  senderId: string;
  senderName: string;
  content: string;
  createdAt: string;
}
```

## Integration SignalR

La page ecoute les notifications SignalR via `useSignalR()` :

```tsx
const { notifications } = useSignalR();

useEffect(() => {
  const latest = notifications[0];
  if (!latest || latest.type !== 'chat.message') return;
  const incoming = latest.data as ChatMessage;
  // Ajoute le message au chat courant et met a jour la room list
}, [notifications, selectedRoomId]);
```

### Comportement temps reel

1. Un nouveau message recu dans la room active est ajoute a la liste (deduplique par `id`)
2. La room est mise a jour avec le dernier message
3. Si le message concerne une autre room, le compteur `unreadCount` est incremente

## Envoi de messages

- Zone de saisie `<textarea>` avec auto-resize
- Envoi via bouton ou touche `Enter` (sans Shift)
- Indicateur de chargement pendant l'envoi
- Restauration du message en cas d'erreur
- Auto-scroll vers le bas a chaque nouveau message

## API

Les appels API passent par `chatApi` :
- `getRooms()` -- charger les rooms
- `getMessages(roomId)` -- charger les messages d'une room
- `sendMessage(roomId, content)` -- envoyer un message
- `markAsRead(roomId)` -- marquer comme lu

## Etats de chargement

- Squelettes animes pour la liste des rooms
- `MessageSkeleton` avec bulles animees pour le chargement des messages
- Etat vide avec icone `MessageCircle` quand aucun message
