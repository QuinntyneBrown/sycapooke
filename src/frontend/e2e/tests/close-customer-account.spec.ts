// Acceptance Test
// Traces to: L2-005
// Description: A customer closes their account from Settings by confirming with their password. A wrong
// password shows inline, open orders block it and are listed, and the founder has no Close account row.

import { expect, ida, marcus, problem, serveCatalog, signInAs, test } from '../fixtures';

test.describe('close customer account', () => {
  test('a customer closes the account with their password and is sent home', async ({ page, unfixturedApiCalls }) => {
    await signInAs(page, marcus);
    await serveCatalog(page, 0);
    let sent: unknown;
    await page.route('**/api/account/closure', (route) => {
      sent = route.request().postDataJSON();
      return route.fulfill({ status: 204 });
    });
    await page.goto('/account/settings');
    await page.getByRole('button', { name: 'Close account' }).click();

    const dialog = page.getByRole('dialog', { name: 'Close account?' });
    await expect(dialog).toContainText('Your data is erased within 30 days. This can’t be undone.');
    await dialog.getByRole('button', { name: 'Close account' }).click();
    await expect(dialog.getByText('Enter your password to confirm.')).toBeVisible();

    await dialog.getByLabel('Enter your password to confirm').fill('correct horse battery');
    await dialog.getByRole('button', { name: 'Close account' }).click();

    await expect(page).toHaveURL('/');
    expect(sent).toEqual({ password: 'correct horse battery' });
    expect(unfixturedApiCalls).toEqual([]);
  });

  test('a wrong password is shown inline', async ({ page }) => {
    await signInAs(page, marcus);
    await page.route('**/api/account/closure', (route) =>
      route.fulfill(
        problem(422, 'VALIDATION_FAILED', 'Check the highlighted fields', {
          password: { code: 'PASSWORD_INCORRECT', message: 'That’s not your password' },
        }),
      ),
    );
    await page.goto('/account/settings');
    await page.getByRole('button', { name: 'Close account' }).click();
    const dialog = page.getByRole('dialog', { name: 'Close account?' });
    await dialog.getByLabel('Enter your password to confirm').fill('nope');
    await dialog.getByRole('button', { name: 'Close account' }).click();

    await expect(dialog.getByText('That’s not your password')).toBeVisible();
  });

  test('open orders block closing and link to the orders', async ({ page }) => {
    await signInAs(page, marcus);
    await page.route('**/api/account/closure', (route) =>
      route.fulfill(
        problem(409, 'ACCOUNT_HAS_OPEN_ORDERS', 'Finish your open orders first', {
          'o-1': { code: 'Shipped', message: 'SY-48302' },
        }),
      ),
    );
    await page.goto('/account/settings');
    await page.getByRole('button', { name: 'Close account' }).click();
    const dialog = page.getByRole('dialog', { name: 'Close account?' });
    await dialog.getByLabel('Enter your password to confirm').fill('correct horse battery');
    await dialog.getByRole('button', { name: 'Close account' }).click();

    await expect(dialog).toContainText('Finish your open orders first');
    await expect(dialog.getByRole('link', { name: 'SY-48302' })).toHaveAttribute('href', '/orders/o-1');
    await expect(dialog.getByRole('button', { name: 'Close', exact: true }).last()).toBeVisible();
  });

  test('the founder has no Close account row', async ({ page }) => {
    await signInAs(page, ida);
    await page.goto('/account/settings');

    await expect(page.getByRole('heading', { name: 'Settings' })).toBeVisible();
    await expect(page.getByRole('button', { name: 'Close account' })).toHaveCount(0);
  });
});
