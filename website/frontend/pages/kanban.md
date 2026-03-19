---
title: Kanban Board
description: Tableau kanban avec drag & drop, priorites et labels
---

# Kanban Board

Le tableau Kanban permet de gerer des taches par colonnes avec drag & drop.

**Fichier source** : `frontend/src/pages/KanbanPage.tsx`
**Route** : `/admin/kanban`

## Colonnes

5 colonnes sont definies avec des couleurs distinctes :

| Colonne | Status | Couleur |
|---------|--------|---------|
| Backlog | `backlog` | slate |
| To Do | `todo` | blue |
| In Progress | `in_progress` | amber |
| Review | `review` | purple |
| Done | `done` | green |

Chaque colonne affiche un compteur de taches et un bouton "Add Task" en bas.

## Carte de tache (TaskCard)

Chaque carte affiche :
- **Labels** : badges colores (hash deterministe pour la couleur)
- **Titre** : texte en gras
- **Description** : texte tronque a 2 lignes
- **Priorite** : badge colore (urgent/high/medium/low)
- **Date d'echeance** : avec icone calendrier, rouge si en retard
- **Assignee** : avatar avec initiales

### Priorites

```typescript
const PRIORITY_CONFIG: Record<TaskPriority, { bg: string; text: string; label: string }> = {
  urgent: { bg: 'bg-red-500/20', text: 'text-red-400', label: 'Urgent' },
  high:   { bg: 'bg-orange-500/20', text: 'text-orange-400', label: 'High' },
  medium: { bg: 'bg-blue-500/20', text: 'text-blue-400', label: 'Medium' },
  low:    { bg: 'bg-slate-500/20', text: 'text-slate-400', label: 'Low' },
};
```

## Drag & Drop

Le drag & drop utilise l'API native HTML5 :

1. `onDragStart` : stocke la tache en cours dans une ref
2. `onDragOver` : surligne la colonne cible en bleu
3. `onDrop` : deplace la tache vers la nouvelle colonne

```tsx
const handleDrop = (e: React.DragEvent, status: TaskStatus) => {
  e.preventDefault();
  const task = dragRef.current;
  if (!task || task.status === status) return;
  setTasks(prev => prev.map(t =>
    t.id === task.id ? { ...t, status, order: colTasks.length } : t
  ));
};
```

## Ajout de tache (InlineAddForm)

Un formulaire inline apparait dans la colonne pour ajouter une tache :
- Champ titre (requis)
- Description (optionnel)
- Select de priorite
- Boutons Cancel / Add

## Panneau de detail (DetailPanel)

Cliquer sur une carte ouvre un panneau lateral a droite (396px) avec :
- Titre editable
- Description textarea
- Select de priorite
- Champ assignee
- Champ date d'echeance
- Labels (comma-separated)
- Boutons Delete / Cancel / Save

Le panneau se ferme en cliquant sur le fond sombre (overlay).

## Structure de donnees

```typescript
interface KanbanTask {
  id: string;
  title: string;
  description?: string;
  status: TaskStatus;
  priority: TaskPriority;
  assignee?: string;
  labels: string[];
  dueDate?: string;
  createdAt: string;
  order: number;
}

type TaskStatus = 'backlog' | 'todo' | 'in_progress' | 'review' | 'done';
type TaskPriority = 'urgent' | 'high' | 'medium' | 'low';
```
