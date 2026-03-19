---
title: Flow Builder
description: Editeur visuel de workflows avec canvas, pipeline et historique
---

# Flow Builder

Le Flow Builder est un editeur visuel de workflows permettant de creer des automatisations en drag & drop.

**Fichiers sources** :
- `frontend/src/pages/FlowEditorPage.tsx` -- page principale
- `frontend/src/pages/flows/FlowCanvas.tsx` -- mode canvas visuel
- `frontend/src/pages/flows/FlowPipeline.tsx` -- mode pipeline liste
- `frontend/src/pages/flows/FlowRunHistory.tsx` -- historique des executions
- `frontend/src/pages/flows/nodeTypes.ts` -- configuration des 8 types de noeuds

**Route** : `/admin/flows/:id` ou `/admin/flows/new`

## Toolbar

La barre d'outils en haut contient :
- Bouton retour vers la liste des flows
- Champ editable pour le nom du flow
- Toggle **Visual** / **Pipeline** pour changer de mode
- Toggle **Active** / **Inactive** pour activer le flow
- Bouton **Save** (simulation d'appel API)
- Bouton **Run** (execution manuelle)
- Bouton **History** (affiche/masque le panneau lateral)

## Les 8 types de noeuds

| Type | Icone | Couleur | Champs de configuration |
|------|-------|---------|------------------------|
| `trigger` | Zap | amber | event (select), filter |
| `condition` | GitBranch | violet | field, operator (select), value |
| `action` | Play | emerald | action (select), target, data |
| `transform` | ArrowRightLeft | blue | mapping (JSON textarea) |
| `delay` | Clock | slate | duration (seconds) |
| `webhook` | Globe | cyan | url, method (select), headers, body |
| `email` | Mail | pink | to, subject, template (select), body |
| `log` | FileText | gray | message, level (select) |

## Mode Canvas

Le canvas est un espace 2D avec grille de points ou les noeuds sont positionnes librement.

### Fonctionnalites

- **Drag & drop** : glisser un noeud depuis la palette a gauche ou cliquer pour ajouter
- **Deplacement** : cliquer-glisser un noeud pour le repositionner
- **Connexions** : cliquer sur le connecteur de sortie (droite) d'un noeud, puis sur l'entree (gauche) d'un autre
- **Suppression de connexion** : cliquer sur le trait SVG de liaison
- **Zoom** : controles +/- (25% a 200%)
- **Configuration** : panneau droit affichant les champs du noeud selectionne

### Connexions SVG

Les connexions sont tracees en courbes de Bezier cubiques :

```tsx
const path = `M ${x1},${y1} C ${x1+80},${y1} ${x2-80},${y2} ${x2},${y2}`;
```

## Mode Pipeline

Le mode pipeline affiche les noeuds sous forme de liste ordonnee, plus adapte pour des workflows lineaires.

## Historique des executions

Le panneau `FlowRunHistory` affiche les dernieres executions avec :
- Statut (completed, failed)
- Date de debut et de fin
- Detail step-by-step : pour chaque noeud, le statut, la duree et la sortie
- Message d'erreur si applicable

```tsx
interface FlowRun {
  id: string;
  flowId: string;
  status: 'completed' | 'failed';
  startedAt: string;
  completedAt?: string;
  error?: string;
  steps: {
    nodeId: string;
    nodeLabel: string;
    status: 'completed' | 'failed' | 'skipped';
    duration?: number;
    output?: string;
    error?: string;
  }[];
}
```

## Etat local

L'etat du flow est gere localement via `useState`. Les modifications sont propagees vers le haut via le callback `onChange`.
