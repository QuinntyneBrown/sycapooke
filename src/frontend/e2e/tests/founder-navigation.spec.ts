// Acceptance Test
// Traces to: L2-003, L2-010
// Description: The top bar offers "Sell an item" only to the founder. A guest sees only "Sign in"
// and a customer sees the avatar alone. Sycapooke API derives isFounder; the app only shows it.

import { expect, ida, marcus, serveCatalog, signInAs, test } from '../fixtures';

test.describe('Founder navigation', () => {
  test.beforeEach(async ({ page }) => {
    await serveCatalog(page, 0);
  });

  test('a guest sees Sign in and no selling action', async ({ page }) => {
    await page.goto('/');

    const bar = page.getByRole('banner');
    await expect(bar.getByRole('link', { name: 'Sign in' })).toBeVisible();
    await expect(bar.getByRole('link', { name: /sell/i })).toHaveCount(0);
  });

  test('a customer sees the avatar and no selling action', async ({ page }) => {
    await signInAs(page, marcus);
    await page.goto('/');

    const bar = page.getByRole('banner');
    await expect(bar.getByRole('link', { name: 'Your account, marcus_12' })).toBeVisible();
    await expect(bar.getByRole('link', { name: /sell/i })).toHaveCount(0);
  });

  test('the founder sees Sell an item beside the avatar', async ({ page }) => {
    await signInAs(page, ida);
    await page.goto('/');

    const bar = page.getByRole('banner');
    await expect(bar.getByRole('link', { name: 'Sell an item' })).toHaveAttribute('href', '/sell');
    await expect(bar.getByRole('link', { name: 'Your account, miss_ida' })).toBeVisible();
  });
});
