---
title: Entity Generator
description: Designer visuel d'entites avec generation de code multi-couche
---

# Entity Generator

Le Entity Generator est un outil visuel pour designer des entites et generer automatiquement le code C# et TypeScript associe.

**Fichiers sources** :
- `frontend/src/pages/generator/EntityDesigner.tsx` -- composant principal
- `frontend/src/pages/generator/codeGenerator.ts` -- generateurs de code
- `frontend/src/pages/generator/types.ts` -- types TypeScript

**Route** : `/admin/generator`

## Layout en 3 panneaux

| Panneau | Largeur | Contenu |
|---------|---------|---------|
| Gauche | 224px | Liste des entites, bouton ajouter |
| Centre | flexible | Formulaire d'edition de l'entite selectionnee |
| Droit | 420px | Preview du code genere avec onglets |
| Bas (optionnel) | 100% | Diagramme ER en SVG |

## 8 types de champs

Les types disponibles pour les champs d'entite :

```typescript
type FieldType = 'string' | 'int' | 'decimal' | 'bool'
  | 'DateTime' | 'Guid' | 'enum' | 'relation';
```

### Champs speciaux

- **enum** : un champ supplementaire apparait pour saisir les valeurs (comma-separated)
- **relation** : deux selects apparaissent pour choisir l'entite cible et le type de relation (`one-to-one`, `one-to-many`, `many-to-many`)

## Formulaire d'edition

Chaque entite expose les proprietes suivantes :

| Propriete | Description |
|-----------|-------------|
| `name` | Nom de l'entite (auto-genere : pluriel, prefix API) |
| `pluralName` | Nom pluriel (auto-calcule) |
| `description` | Description optionnelle |
| `apiPrefix` | Prefix de route API (auto-calcule) |
| `hasAudit` | Ajoute CreatedAt, UpdatedAt |
| `hasSoftDelete` | Ajoute IsDeleted |
| `hasTenantId` | Ajoute TenantId |

La table de champs inclut des colonnes : Name, Type, Required, Searchable, Filterable, Show in List, Show in Form.

## Generateurs de code

Le fichier `codeGenerator.ts` genere 8 fichiers de code pour chaque entite :

1. **Domain Entity** (`Entity.cs`) -- classe C# avec proprietes
2. **DTO** (`EntityDto.cs`) -- Data Transfer Object
3. **Application Service** (`EntityService.cs`)
4. **EF Configuration** (`EntityConfiguration.cs`)
5. **API Controller** (`EntityController.cs`)
6. **TypeScript Interface** (`entity.ts`)
7. **React Form Component** (`EntityForm.tsx`)
8. **React List Component** (`EntityList.tsx`)

Les onglets dans le panneau droit affichent chaque fichier genere avec coloration syntaxique et bouton "Copy".

## Diagramme ER (SVG)

Le bouton "Show ER Diagram" affiche un diagramme entite-relation en SVG :

- Chaque entite est representee par un rectangle avec en-tete bleu et liste de champs
- Les relations sont tracees en courbes de Bezier avec fleches
- Les labels de relation (1:N, etc.) sont affiches au milieu des courbes

## Telechargement

Le bouton "Generate All" telecharge chaque fichier individuellement via `Blob` + `URL.createObjectURL`.

## Preview UML

Un composant `EntityCard` affiche une preview style UML de l'entite selectionnee avec les champs, les types, et les annotations (Audit, SoftDelete, Tenant).
