---
title: Analytics Dashboard
description: Tableau de bord analytique avec graphiques SVG et metriques
---

# Analytics Dashboard

Le dashboard Analytics affiche des metriques et graphiques generes a partir de donnees mock deterministes.

**Fichier source** : `frontend/src/pages/AnalyticsPage.tsx`
**Route** : `/admin/analytics`

## Stat Cards

4 cartes de statistiques en haut de page :

| Metrique | Icone | Couleur |
|----------|-------|---------|
| Revenue | DollarSign | emerald |
| New Users | Users | blue |
| Active Sessions | Activity | violet |
| API Calls | Zap | amber |

Chaque carte affiche la valeur, un badge de tendance (`TrendBadge`) avec pourcentage, et la mention "vs last month".

```tsx
<TrendBadge trend="up" pct={12.5} />
// Affiche une fleche verte montante avec "12.5%"
```

## Graphiques SVG

3 types de graphiques sont utilises, tous en SVG pur sans dependance externe.

### BarChart

- Barres verticales avec valeur au-dessus
- Lignes de grille horizontales en pointilles
- Labels en bas de chaque barre
- Utilise pour : API calls par jour (7 barres)

### LineChart

- Courbe avec aire remplie semi-transparente
- Points (dots) sur chaque valeur
- Labels espaces automatiquement (every Nth)
- Utilise pour : Users over time (30 points)

### DonutChart

- Anneau SVG avec arcs colores
- Total affiche au centre
- Legende en dessous avec pourcentages
- Utilise pour : Traffic by source (5 segments)

## Table des endpoints

Une table affiche les endpoints API les plus sollicites :

| Colonne | Description |
|---------|-------------|
| Endpoint | Path en font mono |
| Method | Badge bleu (GET, POST, etc.) |
| Requests | Nombre formate |
| Avg (ms) | Temps moyen de reponse |

## Format des donnees

Les graphiques attendent des donnees dans ce format :

```typescript
// BarChart & LineChart
interface ChartDataPoint {
  label: string;
  value: number;
}

// DonutChart
interface DonutDataPoint {
  label: string;
  value: number;
  color: string; // hex color
}
```

## Donnees mock deterministes

Les donnees sont generees par une fonction `seededRandom(42)` pour garantir des valeurs stables entre les rendus :

```tsx
const data = useMemo(() => generateMockData(), []);
```
