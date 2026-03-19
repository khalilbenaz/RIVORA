import { test, expect } from '@playwright/test';

test.describe('Admin Login Page', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/admin/login');
  });

  test('renders the admin login page', async ({ page }) => {
    await expect(page.locator('h1')).toBeVisible();
    await expect(page.locator('form')).toBeVisible();
  });

  test('has username and password fields', async ({ page }) => {
    const usernameInput = page.locator('#userName');
    await expect(usernameInput).toBeVisible();
    await expect(usernameInput).toHaveAttribute('type', 'text');

    const passwordInput = page.locator('#password');
    await expect(passwordInput).toBeVisible();
    await expect(passwordInput).toHaveAttribute('type', 'password');
  });

  test('displays RIVORA branding', async ({ page }) => {
    const branding = page.locator('h1');
    await expect(branding).toContainText('RIVORA');
  });

  test('has a submit button', async ({ page }) => {
    const submitButton = page.locator('button[type="submit"]');
    await expect(submitButton).toBeVisible();
    await expect(submitButton).toBeEnabled();
  });

  test('username field is focused by default', async ({ page }) => {
    const usernameInput = page.locator('#userName');
    await expect(usernameInput).toBeFocused();
  });
});
