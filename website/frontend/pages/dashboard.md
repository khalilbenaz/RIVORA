---
title: Dashboard
description: Tableau de bord administrateur avec statistiques et logs
---

# Dashboard

Le dashboard est la page d'accueil de l'espace admin. Il affiche les metriques cles et les logs d'audit recents.

**Fichier source** : `frontend/src/pages/Dashboard.tsx`
**Route** : `/admin/dashboard`

## Stat Cards

Le dashboard affiche 4 cartes de statistiques grace au composant `StatCard` :

```tsx
<StatCard label={t('dashboard.users')} value={users?.length ?? 0} icon={<Users size={20} />} />
<StatCard label={t('dashboard.products')} value={products?.length ?? 0} icon={<Package size={20} />} />
<StatCard label={t('dashboard.tenants')} value={tenants?.length ?? 0} icon={<Building2 size={20} />} />
<StatCard label={t('dashboard.auditLogs')} value={logs?.length ?? 0} icon={<ScrollText size={20} />} />
```

## Chargement des donnees avec `useApi`

Chaque source de donnees utilise le hook `useApi` pour le fetching :

```tsx
const { data: users, loading: lu } = useApi<User[]>(() => usersApi.getAll());
const { data: products, loading: lp } = useApi<Product[]>(() => productsApi.getAll());
const { data: tenants, loading: lt } = useApi<Tenant[]>(() => tenantsApi.getAll());
const { data: logs, loading: ll } = useApi<AuditLog[]>(() => auditApi.getAll());
```

Un `<Spinner />` global s'affiche tant que tous les appels ne sont pas termines.

## Table des Audit Logs

Les 10 derniers logs sont affiches dans un tableau avec les colonnes :

| Colonne | Contenu |
|---------|---------|
| Date | `executionDate` formate en `fr-FR` |
| Method | Badge info avec `httpMethod` |
| URL | Texte tronque avec tooltip |
| Status | Badge success (< 400) ou danger (>= 400) |
| Duration | `executionTime` en ms, tabular-nums |

## Traductions

Toutes les chaines sont externalisees via `useTranslation()`. Les cles utilisees sont sous le namespace `dashboard.*` (ex: `dashboard.title`, `dashboard.users`, etc.).

## Composants utilises

- [`StatCard`](/frontend/components/stat-card) -- cartes de metriques
- [`Badge`](/frontend/components/badge) -- badges colores pour methode HTTP et status
- `Spinner` -- indicateur de chargement
