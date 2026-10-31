// Acceptance Test
// Traces to: L2-003, L2-010
// Description: Founder-only pages open for the founder, send a visitor to sign in and show a signed-in
// customer the management 403. The shop overview shows the founder's inventory count. The server still
// authorises every call; these guards only steer navigation.

import { expect, ida, marcus, signInAs, test } from '../fixtures';

const founderPaths = [
  '/shop',
  '/sell',
  '/account/listings',
  '/items/L-10001/edit',
  '/shop/sales',
  '/shop/support',
  '/shop/problems/P-1001',
];

const emptyCounts = { all: 0, draft: 0, active: 0, reserved: 0, sold: 0, removed: 0 };

test.describe('founder-only pages', () => {
  for (const path of founderPaths) {
    test(`a visitor to ${path} is sent to sign in`, async ({ page }) => {
      await page.route('**/api/auth/me', (route) => route.fulfill({ status: 401, json: {} }));
      await page.route('**/api/auth/refresh', (route) => route.fulfill({ status: 401, json: {} }));
      await page.goto(path);

      await expect(page).toHaveURL(
        new RegExp(`/sign-in\\?returnUrl=${encodeURIComponent(path).replace(/\./g, '\\.')}`),
      );
    });

    test(`a customer opening ${path} sees the management 403`, async ({ page }) => {
      await signInAs(page, marcus);
      await page.goto(path);

      await expect(page).toHaveURL('/403?reason=management');
      await expect(page.getByText('Shop management is only for the shop’s founder.')).toBeVisible();
    });
  }
});

test.describe('shop overview', () => {
  test('shows the number of active listings and links to inventory', async ({ page }) => {
    await signInAs(page, ida);
    await page.route('**/api/me/listings**', (route) =>
      route.fulfill({ json: { items: [], counts: { ...emptyCounts, all: 20, active: 18, sold: 2 } } }),
    );
    await page.goto('/shop');

    await expect(page.getByRole('heading', { level: 1 })).toHaveText('Shop');
    await expect(page.getByRole('link', { name: /Active listings/ })).toContainText('18');
    await expect(page.getByRole('link', { name: 'Open inventory' })).toHaveAttribute('href', '/account/listings');
    await expect(page.getByRole('link', { name: 'Sell an item' }).first()).toBeVisible();
    await expect(
      page.getByRole('navigation', { name: 'Shop management' }).getByRole('link', { name: 'Shop' }),
    ).toHaveAttribute('aria-current', 'page');
  });

  test('an empty shop invites the first piece', async ({ page }) => {
    await signInAs(page, ida);
    await page.route('**/api/me/listings**', (route) => route.fulfill({ json: { items: [], counts: emptyCounts } }));
    await page.goto('/shop');

    await expect(page.getByRole('heading', { level: 2, name: 'Nothing for sale yet' })).toBeVisible();
  });

  test('a failed load shows the reference and tries again', async ({ page }) => {
    await signInAs(page, ida);
    let calls = 0;
    await page.route('**/api/me/listings**', (route) => {
      calls += 1;
      return calls === 1
        ? route.fulfill({
            status: 500,
            contentType: 'application/problem+json',
            body: JSON.stringify({ title: 'x', status: 500, code: 'SERVER_ERROR', correlationId: '5c1d0e7a9b2f4c3d' }),
          })
        : route.fulfill({ json: { items: [], counts: { ...emptyCounts, all: 1, active: 1 } } });
    });
    await page.goto('/shop');

    await expect(page.getByText('Couldn’t load the shop')).toBeVisible();
    await page.getByRole('button', { name: 'Try again' }).click();
    await expect(page.getByRole('link', { name: /Active listings/ })).toContainText('1');
  });
});
