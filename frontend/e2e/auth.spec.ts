import { test, expect } from '@playwright/test';

test.describe('Admin Login', () => {
  test('renders the admin login form', async ({ page }) => {
    await page.goto('/admin/login');

    await expect(page.locator('h1')).toContainText('RIVORA');
    await expect(page.locator('#userName')).toBeVisible();
    await expect(page.locator('#password')).toBeVisible();
    await expect(page.locator('button[type="submit"]')).toBeVisible();
  });

  test('does not navigate away when submitting empty form', async ({ page }) => {
    await page.goto('/admin/login');

    await page.locator('button[type="submit"]').click();

    // Should still be on the login page (HTML5 validation prevents submission)
    await expect(page).toHaveURL(/\/admin\/login/);
    await expect(page.locator('#userName')).toBeVisible();
  });

  test('shows error message on wrong credentials', async ({ page }) => {
    await page.goto('/admin/login');

    await page.locator('#userName').fill('wronguser');
    await page.locator('#password').fill('wrongpassword');
    await page.locator('button[type="submit"]').click();

    // The API call will fail (no backend), which triggers the error state
    const errorMessage = page.locator('.bg-red-50');
    await expect(errorMessage).toBeVisible({ timeout: 10000 });
  });
});

test.describe('Client Login', () => {
  test('renders the client login page with split-screen layout', async ({ page }) => {
    await page.goto('/app/login');

    // Left side - form
    await expect(page.getByText('Bon retour !')).toBeVisible();
    await expect(page.locator('#userName')).toBeVisible();
    await expect(page.locator('#password')).toBeVisible();
    await expect(page.locator('button[type="submit"]')).toBeVisible();

    // RIVORA branding
    await expect(page.getByText('RIVORA').first()).toBeVisible();
  });

  test('has link to registration page', async ({ page }) => {
    await page.goto('/app/login');

    const registerLink = page.getByText("S'inscrire gratuitement");
    await expect(registerLink).toBeVisible();
    await expect(registerLink).toHaveAttribute('href', '/app/register');
  });
});

test.describe('Client Registration', () => {
  test('renders the registration form with all fields', async ({ page }) => {
    await page.goto('/app/register');

    await expect(page.getByText('Créer un compte')).toBeVisible();

    // Check all form fields exist
    const labels = ['Prénom', 'Nom', "Nom d'utilisateur", 'Email', 'Mot de passe'];
    for (const label of labels) {
      await expect(page.getByText(label, { exact: true }).first()).toBeVisible();
    }

    await expect(page.locator('button[type="submit"]')).toBeVisible();
  });

  test('has link to login page', async ({ page }) => {
    await page.goto('/app/register');

    const loginLink = page.getByText('Se connecter');
    await expect(loginLink).toBeVisible();
    await expect(loginLink).toHaveAttribute('href', '/app/login');
  });
});

test.describe('Auth Redirects', () => {
  test('/admin redirects to /admin/login when not authenticated', async ({ page }) => {
    await page.goto('/admin');

    await expect(page).toHaveURL(/\/admin\/login/);
  });

  test('/app redirects to /app/login when not authenticated', async ({ page }) => {
    await page.goto('/app');

    await expect(page).toHaveURL(/\/app\/login/);
  });
});
