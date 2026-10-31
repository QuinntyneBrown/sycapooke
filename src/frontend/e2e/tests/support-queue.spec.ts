// Acceptance Test
// Traces to: L2-036
// Description: The founder's support queue lists customers' problems and cancellation requests,
// newest first, filters to the open ones, and links each to the page that handles it.

import { expect, ida, problem, sale, signInAs, test } from '../fixtures';

const item = sale()['item'] as object;

const queue = {
  openCount: 2,
  items: [
    {
      reference: 'P-0912',
      kind: 'Problem',
      label: 'Problem: not as described',
      item: { ...item, title: 'Primary-colour mixing bowl set of four' },
      total: '84.92',
      customer: 'Tessa Bell',
      at: '2026-10-30T10:00:00Z',
      status: 'Open',
      link: '/shop/problems/P-0912',
    },
    {
      reference: 'C-0044',
      kind: 'CancellationRequest',
      label: 'Cancellation request',
      item: { ...item, title: 'Teal drip-glaze ceramic vase' },
      total: '79.48',
      customer: 'Pia Reyes',
      at: '2026-10-29T10:00:00Z',
      status: 'Open',
      link: '/shop/sales/o-44',
    },
    {
      reference: 'P-0907',
      kind: 'Problem',
      label: 'Problem: not as described',
      item: { ...item, title: 'Floral shirtwaist day dress' },
      total: '94.72',
      customer: 'Tessa Bell',
      at: '2026-10-24T10:00:00Z',
      status: 'Refunded',
      link: '/shop/problems/P-0907',
    },
  ],
};

test.describe('support queue', () => {
  test.beforeEach(async ({ page }) => {
    await signInAs(page, ida);
    await page.route('**/photos/**', (route) => route.fulfill({ contentType: 'image/png', body: Buffer.alloc(0) }));
  });

  test('lists requests, filters to open and links to the handling page', async ({ page, unfixturedApiCalls }) => {
    await page.route('**/api/shop/support', (route) => route.fulfill({ json: queue }));
    await page.goto('/shop/support');

    const main = page.locator('main');
    await expect(main.getByRole('heading', { level: 1 })).toHaveText('Support');
    await expect(main.locator('.subtitle')).toHaveText('2 open');
    await expect(main.locator('.order-list li')).toHaveCount(3);
    await expect(main.locator('.order-list li').first()).toContainText(
      'P-0912 · Problem: not as described · Tessa Bell',
    );

    await main.getByRole('button', { name: 'Open' }).click();
    await expect(main.locator('.order-list li')).toHaveCount(2);
    await expect(main.getByRole('button', { name: 'Open' })).toHaveAttribute('aria-current', 'true');

    await expect(main.getByRole('link', { name: /Teal drip-glaze/ })).toHaveAttribute('href', '/shop/sales/o-44');
    expect(unfixturedApiCalls).toEqual([]);
  });

  test('an empty queue says there are no requests', async ({ page }) => {
    await page.route('**/api/shop/support', (route) => route.fulfill({ json: { items: [], openCount: 0 } }));
    await page.goto('/shop/support');

    await expect(page.locator('main')).toContainText('No requests');
  });

  test('a failed load shows the reference and tries again', async ({ page }) => {
    let calls = 0;
    await page.route('**/api/shop/support', (route) => {
      calls += 1;
      return calls === 1
        ? route.fulfill(problem(500, 'INTERNAL', 'Something went wrong'))
        : route.fulfill({ json: queue });
    });
    await page.goto('/shop/support');

    await expect(page.locator('main')).toContainText('Couldn’t load support requests');
    await expect(page.locator('main')).toContainText('Ref 5c1d-0e7a');
    await page.getByRole('button', { name: 'Try again' }).click();
    await expect(page.locator('main .order-list li')).toHaveCount(3);
  });
});
