---
title: Landing Page
description: Page publique de presentation de RIVORA
---

# Landing Page

La landing page publique de RIVORA est accessible a la racine `/`. Elle est rendue par le composant `LandingPage.tsx`.

**Fichier source** : `frontend/src/pages/landing/LandingPage.tsx`

## Sections

| Section | Ancre | Description |
|---------|-------|-------------|
| Navbar | sticky top | Logo RIVORA, liens Features/Pricing/Temoignages, Connexion + CTA |
| Hero | - | Titre gradient, sous-titre, 2 boutons CTA, badges tech |
| Features | `#features` | Grille 3 colonnes, 6 cartes avec icone Lucide |
| Code preview | - | Split layout : texte + terminal faux CLI |
| Pricing | `#pricing` | 3 plans (Starter, Pro, Enterprise) |
| Temoignages | `#testimonials` | 3 cartes avec etoiles, citation, nom/role |
| CTA final | - | Bandeau gradient bleu-violet |
| Footer | - | 4 colonnes : Produit, Developpeurs, Legal, branding |

## Donnees des features

Les 6 features sont definies dans un tableau statique en haut du fichier :

```tsx
const features = [
  { icon: Shield, title: 'Securite avancee', desc: '...' },
  { icon: Building2, title: 'Multi-tenancy', desc: '...' },
  { icon: Database, title: 'Multi-database', desc: '...' },
  { icon: Zap, title: 'Performance', desc: '...' },
  { icon: BarChart3, title: 'Observabilite', desc: '...' },
  { icon: Lock, title: 'GDPR & Privacy', desc: '...' },
];
```

## Personnalisation des plans tarifaires

Les plans sont dans le tableau `pricing`. Chaque plan a un flag `highlighted` pour le style "Populaire" :

```tsx
const pricing = [
  { name: 'Starter', price: 'Gratuit', period: 'open source',
    features: ['Clean Architecture', 'Auth JWT', ...], highlighted: false },
  { name: 'Pro', price: '49EUR', period: '/mois',
    features: ['Tout Starter +', 'Multi-tenancy', ...], highlighted: true },
  { name: 'Enterprise', price: 'Sur mesure', period: '',
    features: ['Tout Pro +', 'SSO (SAML/OIDC)', ...], highlighted: false },
];
```

## Personnalisation du hero

Le hero affiche un badge de version, un titre avec gradient et deux CTA. Pour modifier le titre :

```tsx
<h1 className="text-5xl font-extrabold ...">
  Le framework .NET pour{' '}
  <span className="bg-gradient-to-r from-blue-600 to-violet-600 bg-clip-text text-transparent">
    applications SaaS
  </span>
</h1>
```

## Navigation

- `Connexion` pointe vers `/app/login`
- `Essai gratuit` et les CTA pointent vers `/app/register`
- Le lien GitHub ouvre `https://github.com/khalilbenaz/RIVORA`

## Stack technique

- React Router `<Link>` pour la navigation interne
- Icones Lucide React (Shield, Zap, Building2, etc.)
- Tailwind CSS avec classes utilitaires (backdrop-blur, gradient, etc.)
