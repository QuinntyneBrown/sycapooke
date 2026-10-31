// Acceptance Test
// Traces to: L2-054, L2-061
// Description: The privacy page, the terms, the shop policies and the server error page are plain pages
// that need no sign-in and call no API.

import { expect, test } from '../fixtures';

test.describe('static pages', () => {
  test('privacy lists what the shop keeps and opens cookie settings', async ({ page, unfixturedApiCalls }) => {
    await page.goto('/privacy');

    await expect(page.getByRole('heading', { level: 1 })).toHaveText('Privacy');
    await expect(page.getByText('Shipping addresses are encrypted at rest.')).toBeVisible();
    await page.getByRole('link', { name: 'Cookie settings' }).first().click();
    await expect(page.getByRole('dialog')).toBeVisible();
    expect(unfixturedApiCalls.filter((url) => !url.includes('/api/auth/'))).toEqual([]);
  });

  test('the terms are a numbered list that links to the policies and privacy', async ({ page }) => {
    await page.goto('/terms');

    await expect(page.getByRole('heading', { level: 1 })).toHaveText('Terms of service');
    await expect(page.locator('.terms-list li')).toHaveCount(6);
    await page.getByRole('link', { name: 'shop policies', exact: true }).click();
    await expect(page).toHaveURL('/rules');
  });

  test('the shop policies list the four categories and six policies', async ({ page }) => {
    await page.goto('/rules');

    await expect(page.getByRole('heading', { level: 1 })).toHaveText('Shop policies');
    await expect(page.locator('tbody tr')).toHaveCount(4);
    await expect(page.locator('.rules-list li')).toHaveCount(6);
    await expect(page.getByText('Paid straight to the shop')).toBeVisible();
  });

  test('the error page offers the way back to the shop', async ({ page }) => {
    await page.goto('/error');

    await expect(page.getByRole('heading', { level: 1 })).toHaveText('Something went wrong');
    await expect(page.getByRole('link', { name: 'Back to shop' })).toHaveAttribute('href', '/');
  });
});
