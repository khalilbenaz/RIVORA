---
title: Project Wizard
description: Assistant de creation de projet en 6 etapes avec templates
---

# Project Wizard

Le Project Wizard est un assistant en 6 etapes pour generer un projet RIVORA complet.

**Fichier source** : `frontend/src/pages/projects/ProjectWizard.tsx`
**Route** : `/admin/projects/new`

## Les 6 etapes

| # | Etape | Description |
|---|-------|-------------|
| 1 | Template | Choix d'un template ou projet vierge |
| 2 | Configuration | Nom, namespace, description, base de donnees |
| 3 | Modules | Selection des modules (checkboxes par categorie) |
| 4 | Entities | Ajout et edition d'entites avec leurs champs |
| 5 | Flows | Activation des workflows proposes par le template |
| 6 | Summary | Recapitulatif, arborescence, commande CLI, generation |

## Barre de progression

Une barre de progression avec 6 cercles numerotes affiche l'etape courante. Les etapes completees sont cochees en bleu.

## Templates disponibles

6 templates sont importes depuis `./templates.ts`. Chaque template pre-configure :
- La base de donnees
- Les modules selectionnes
- Les entites avec leurs champs
- Les workflows disponibles

Un bouton "Projet vierge" est aussi disponible avec des defaults minimaux (`jwt`, `health-checks`).

## Configuration (etape 2)

- **Nom du projet** : champ texte requis
- **Namespace** : auto-genere par slugification du nom
- **Description** : textarea optionnel
- **Base de donnees** : grille 4 colonnes (PostgreSQL, SQL Server, MySQL, SQLite)

```tsx
const databaseOptions = [
  { id: 'postgresql', name: 'PostgreSQL', ... },
  { id: 'sqlserver', name: 'SQL Server', ... },
  { id: 'mysql', name: 'MySQL', ... },
  { id: 'sqlite', name: 'SQLite', ... },
];
```

## Modules (etape 3)

Les modules sont organises par categories dans `allModules`. Chaque module a un `id`, un `name` et une `description`. La selection se fait via des checkboxes avec compteur.

## Entities (etape 4)

- Bouton "Add Entity" pour creer une nouvelle entite
- Chaque entite a un nom editable et une table de champs
- Champs : name, type (select parmi `fieldTypes`), required (checkbox)
- Boutons pour ajouter/supprimer des champs

## Summary (etape 6)

L'etape finale affiche :
- **Recap** : nom, namespace, template, base de donnees
- **Modules** : badges des modules selectionnes
- **Entites** : liste avec nombre de champs
- **Arborescence** : structure de fichiers generee par `buildProjectTree()`
- **Commande CLI** : commande `rvr new` copiable
- **Actions** : Generate (API simulee), Copy CLI, Download ZIP

```tsx
const buildCliCommand = () => {
  const parts = [`rvr new "${state.projectName}"`];
  parts.push(`--db ${state.database}`);
  if (state.selectedModules.length) {
    parts.push(`--modules ${state.selectedModules.join(',')}`);
  }
  return parts.join(' \\\n  ');
};
```

## Navigation

- Boutons Precedent / Suivant avec animation de transition (`animate-fade-in-right` / `animate-fade-in-left`)
- Le bouton Suivant est desactive tant que les champs requis ne sont pas remplis
