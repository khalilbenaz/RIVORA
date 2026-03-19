import { test, expect } from '@playwright/test';

test.describe('Landing Page', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/');
  });

  test('displays the RIVORA brand in the navbar', async ({ page }) => {
    const brand = page.locator('nav').getByText('RIVORA');
    await expect(brand).toBeVisible();
  });

  test('displays hero section with headline', async ({ page }) => {
    const hero = page.locator('h1');
    await expect(hero).toBeVisible();
    await expect(hero).toContainText('applications SaaS');
  });

  test('displays Features section with 6 feature cards', async ({ page }) => {
    const featuresSection = page.locator('#features');
    await expect(featuresSection).toBeVisible();

    const featureCards = featuresSection.locator('.rounded-xl');
    await expect(featureCards).toHaveCount(6);
  });

  test('displays Pricing section with 3 plan cards', async ({ page }) => {
    const pricingSection = page.locator('#pricing');
    await expect(pricingSection).toBeVisible();

    const planNames = ['Starter', 'Pro', 'Enterprise'];
    for (const name of planNames) {
      await expect(pricingSection.getByText(name, { exact: true })).toBeVisible();
    }
  });

  test('displays footer with RIVORA branding', async ({ page }) => {
    const footer = page.locator('footer');
    await expect(footer).toBeVisible();
    await expect(footer.getByText('RIVORA')).toBeVisible();
  });

  test('footer contains API Explorer link', async ({ page }) => {
    const footer = page.locator('footer');
    const apiExplorerLink = footer.getByText('API Explorer');
    await expect(apiExplorerLink).toBeVisible();
  });

  test('CTA buttons link to /app/register', async ({ page }) => {
    const registerLinks = page.locator('a[href="/app/register"]');
    const count = await registerLinks.count();
    expect(count).toBeGreaterThanOrEqual(1);
  });

  test('navbar has Connexion and Essai gratuit links', async ({ page }) => {
    const nav = page.locator('nav');
    await expect(nav.getByText('Connexion')).toBeVisible();
    await expect(nav.getByText('Essai gratuit')).toBeVisible();
  });
});
