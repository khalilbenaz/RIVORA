---
title: Charts
description: 3 composants de graphiques SVG (BarChart, LineChart, DonutChart)
---

# Charts

RIVORA inclut 3 composants de graphiques SVG purs, sans dependance externe.

**Fichiers sources** :
- `frontend/src/components/charts/BarChart.tsx`
- `frontend/src/components/charts/LineChart.tsx`
- `frontend/src/components/charts/DonutChart.tsx`

## BarChart

Graphique a barres verticales.

### Props

| Prop | Type | Default | Description |
|------|------|---------|-------------|
| `data` | `{ label: string; value: number }[]` | requis | Donnees a afficher |
| `height` | `number` | `220` | Hauteur du SVG en pixels |
| `color` | `string` | `'#3b82f6'` | Couleur des barres (hex) |

### Fonctionnalites

- Barres arrondies (`rx={4}`) avec opacite 0.85
- Lignes de grille horizontales en pointilles (0%, 25%, 50%, 75%, 100%)
- Valeur affichee au-dessus de chaque barre
- Label sous chaque barre
- Responsive via `width="100%"` et `viewBox`

### Exemple

```tsx
<BarChart
  data={[
    { label: 'Mon', value: 3200 },
    { label: 'Tue', value: 5100 },
    { label: 'Wed', value: 4700 },
  ]}
  height={240}
  color="#3b82f6"
/>
```

## LineChart

Graphique en courbe avec aire remplie.

### Props

| Prop | Type | Default | Description |
|------|------|---------|-------------|
| `data` | `{ label: string; value: number }[]` | requis | Points de donnees |
| `height` | `number` | `220` | Hauteur du SVG |
| `color` | `string` | `'#8b5cf6'` | Couleur de la courbe |

### Fonctionnalites

- Polyline connectant tous les points
- Aire remplie sous la courbe (opacite 8%)
- Points (cercles) sur chaque valeur
- Labels espaces automatiquement (every Nth pour eviter le chevauchement)
- Grille horizontale en pointilles

### Exemple

```tsx
<LineChart
  data={usersOverTime}  // 30 points { label: 'D1', value: 150 }
  height={240}
  color="#8b5cf6"
/>
```

## DonutChart

Graphique en anneau (donut) avec legende.

### Props

| Prop | Type | Default | Description |
|------|------|---------|-------------|
| `data` | `{ label: string; value: number; color: string }[]` | requis | Segments |
| `size` | `number` | `180` | Taille du SVG (largeur = hauteur) |

### Fonctionnalites

- Arcs SVG calcules mathematiquement (sin/cos)
- Rayon interieur = 60% du rayon exterieur
- Total affiche au centre du donut
- Legende avec pastilles de couleur et pourcentages
- Hover : opacite 80%

### Exemple

```tsx
<DonutChart
  data={[
    { label: 'Direct', value: 1500, color: '#3b82f6' },
    { label: 'Organic', value: 1200, color: '#8b5cf6' },
    { label: 'Referral', value: 600, color: '#f59e0b' },
  ]}
  size={200}
/>
```

## Integration

Ces composants sont utilises dans la page [Analytics](/frontend/pages/analytics).
