---
title: Tests E2E
description: Configuration Playwright et tests end-to-end
---

# Tests E2E

Les tests end-to-end utilisent **Playwright** pour tester l'application dans un vrai navigateur.

## Configuration

**Fichier** : `frontend/playwright.config.ts`

```typescript
import { defineConfig } from '@playwright/test';

export default defineConfig({
  testDir: './e2e',
  fullyParallel: true,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,
  workers: process.env.CI ? 1 : undefined,
  reporter: 'html',
  use: {
    baseURL: 'http://localhost:3000',
    trace: 'on-first-retry',
    screenshot: 'only-on-failure',
  },
  webServer: {
    command: 'npm run dev',
    url: 'http://localhost:3000',
    reuseExistingServer: !process.env.CI,
    timeout: 30000,
  },
});
```

### Points cles

- **Parallelisme** : tests en parallele, sauf en CI (1 worker)
- **Retries** : 2 retries en CI, 0 en local
- **Traces** : capturees au premier retry pour le debug
- **Screenshots** : captures d'ecran uniquement en cas d'echec
- **Web Server** : lance `npm run dev` automatiquement

## Structure des tests

Les fichiers de test sont dans `frontend/e2e/` :

| Fichier | Description |
|---------|-------------|
| `landing.spec.ts` | Tests de la landing page |
| `auth.spec.ts` | Tests d'authentification |
| `admin.spec.ts` | Tests du panneau admin |

## Exemple : tests de la landing page

```typescript
import { test, expect } from '@playwright/test';

test.describe('Landing Page', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/');
  });

  test('displays the RIVORA brand in the navbar', async ({ page }) => {
    const brand = page.locator('nav').getByText('RIVORA');
    await expect(brand).toBeVisible();
  });

  test('displays Features section with 6 feature cards', async ({ page }) => {
    const featuresSection = page.locator('#features');
    await expect(featuresSection).toBeVisible();
    const featureCards = featuresSection.locator('.rounded-xl');
    await expect(featureCards).toHaveCount(6);
  });

  test('displays Pricing section with 3 plan cards', async ({ page }) => {
    const pricingSection = page.locator('#pricing');
    for (const name of ['Starter', 'Pro', 'Enterprise']) {
      await expect(pricingSection.getByText(name, { exact: true })).toBeVisible();
    }
  });
});
```

## Lancer les tests E2E

```bash
# Lancer tous les tests E2E
npx playwright test

# Mode UI interactif
npx playwright test --ui

# Un seul fichier
npx playwright test e2e/landing.spec.ts

# Avec un navigateur specifique
npx playwright test --project=chromium

# Voir le rapport HTML
npx playwright show-report
```

## Bonnes pratiques

- Utiliser `page.goto('/')` dans `beforeEach` pour chaque suite
- Preferer `getByText`, `getByRole`, `getByLabel` aux selecteurs CSS
- Utiliser `test.describe` pour grouper les tests par page/feature
- En CI, utiliser `forbidOnly: true` pour empecher les `.only` accidentels
