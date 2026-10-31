// Acceptance Test
// Traces to: L2-004, L2-008, L2-010
// Description: A signed-in customer sees only their own private account, renames themself, changes
// their password and signs out everywhere. Visitors are sent to sign in; the founder sees the shop
// management tabs, a customer the account tabs.

import { expect, ida, marcus, problem, signInAs, test } from '../fixtures';

const account = {
  displayName: 'marcus_12',
  email: 'marcus.bell@example.com',
  emailVerified: true,
  memberSince: '2025-03-04',
};

test.describe('private account', () => {
  test('a visitor is sent to sign in and returns afterwards', async ({ page }) => {
    await page.route('**/api/auth/me', (route) => route.fulfill({ status: 401, json: {} }));
    await page.route('**/api/auth/refresh', (route) => route.fulfill({ status: 401, json: {} }));
    await page.goto('/account/settings');

    await expect(page).toHaveURL(/\/sign-in\?returnUrl=%2Faccount%2Fsettings/);
  });

  test('the overview greets the customer and shows the account tabs', async ({ page }) => {
    await signInAs(page, marcus);
    await page.route('**/api/account', (route) => route.fulfill({ json: account }));
    await page.goto('/account');

    await expect(page.getByRole('heading', { level: 1 })).toHaveText('Hi, marcus_12');
    await expect(page.getByText('Customer since Mar 2025. Only you see your account.')).toBeVisible();
    const tabs = page.getByRole('navigation', { name: 'Account' });
    await expect(tabs.getByRole('link')).toHaveText(['Overview', 'Purchases', 'Emails', 'Settings']);
    await expect(tabs.getByRole('link', { name: 'Overview' })).toHaveAttribute('aria-current', 'page');
  });

  test('the founder sees shop management tabs instead', async ({ page }) => {
    await signInAs(page, ida);
    await page.route('**/api/account', (route) => route.fulfill({ json: { ...account, displayName: 'MissIda' } }));
    await page.goto('/account');

    await expect(page.getByRole('navigation', { name: 'Shop management' })).toBeVisible();
    await expect(page.getByRole('navigation', { name: 'Account' })).toHaveCount(0);
  });

  test('renaming saves the new display name', async ({ page }) => {
    await signInAs(page, marcus);
    let sent: unknown;
    await page.route('**/api/account', async (route) => {
      if (route.request().method() === 'PATCH') {
        sent = route.request().postDataJSON();
        await route.fulfill({ json: { ...account, displayName: 'attic_treasures' } });
      } else {
        await route.fulfill({ json: account });
      }
    });
    await page.goto('/account/profile');

    await page.getByLabel('Display name').fill('attic_treasures');
    await page.getByRole('button', { name: 'Save details' }).click();

    await expect(page.getByText('Details saved')).toBeVisible();
    expect(sent).toEqual({ displayName: 'attic_treasures' });
  });

  test('a taken name shows against the field', async ({ page }) => {
    await signInAs(page, marcus);
    await page.route('**/api/account', async (route) => {
      if (route.request().method() === 'PATCH') {
        await route.fulfill(
          problem(422, 'VALIDATION_FAILED', 'Check the highlighted fields', {
            displayName: { code: 'DISPLAY_NAME_TAKEN', message: 'That name is taken' },
          }),
        );
      } else {
        await route.fulfill({ json: account });
      }
    });
    await page.goto('/account/profile');

    await page.getByLabel('Display name').fill('taken_name');
    await page.getByRole('button', { name: 'Save details' }).click();

    await expect(page.getByText('That name is taken')).toBeVisible();
  });
});

test.describe('settings', () => {
  test('changing the password closes the dialog and confirms', async ({ page }) => {
    await signInAs(page, marcus);
    let sent: unknown;
    await page.route('**/api/account/password', async (route) => {
      sent = route.request().postDataJSON();
      await route.fulfill({ status: 204 });
    });
    await page.goto('/account/settings');

    await page.getByRole('button', { name: 'Change', exact: true }).click();
    const dialog = page.getByRole('dialog', { name: 'Change password' });
    await dialog.getByLabel('Current password').fill('the old passphrase');
    await dialog.getByLabel('New password').fill('a brand new passphrase');
    await dialog.getByRole('button', { name: 'Change password' }).click();

    await expect(dialog).toBeHidden();
    await expect(page.getByText('Other devices have been signed out.')).toBeVisible();
    expect(sent).toEqual({ currentPassword: 'the old passphrase', newPassword: 'a brand new passphrase' });
  });

  test('a wrong current password shows against its field', async ({ page }) => {
    await signInAs(page, marcus);
    await page.route('**/api/account/password', (route) =>
      route.fulfill(
        problem(422, 'VALIDATION_FAILED', 'Check the highlighted fields', {
          currentPassword: { code: 'PASSWORD_INCORRECT', message: 'That’s not your current password' },
        }),
      ),
    );
    await page.goto('/account/settings');

    await page.getByRole('button', { name: 'Change', exact: true }).click();
    const dialog = page.getByRole('dialog', { name: 'Change password' });
    await dialog.getByLabel('Current password').fill('wrong wrong wrong');
    await dialog.getByLabel('New password').fill('a brand new passphrase');
    await dialog.getByRole('button', { name: 'Change password' }).click();

    await expect(dialog.getByText('That’s not your current password')).toBeVisible();
  });

  test('a short new password is refused before any request', async ({ page, unfixturedApiCalls }) => {
    await signInAs(page, marcus);
    await page.goto('/account/settings');

    await page.getByRole('button', { name: 'Change', exact: true }).click();
    const dialog = page.getByRole('dialog', { name: 'Change password' });
    await dialog.getByLabel('Current password').fill('the old passphrase');
    await dialog.getByLabel('New password').fill('short');
    await dialog.getByRole('button', { name: 'Change password' }).click();

    await expect(dialog.getByText('Password must be at least 12 characters')).toBeVisible();
    expect(unfixturedApiCalls).toEqual([]);
  });

  test('sign out everywhere ends this session too', async ({ page }) => {
    await signInAs(page, marcus);
    await page.route('**/api/auth/sign-out-everywhere', (route) => route.fulfill({ status: 204 }));
    await page.goto('/account/settings');

    await page.getByRole('button', { name: 'Sign out everywhere' }).click();
    await page
      .getByRole('dialog', { name: 'Sign out everywhere?' })
      .getByRole('button', { name: 'Sign out everywhere' })
      .click();

    await expect(page).toHaveURL('/sign-in');
  });

  test('a failed sign out offers Try again', async ({ page }) => {
    await signInAs(page, marcus);
    await page.route('**/api/auth/sign-out-everywhere', (route) =>
      route.fulfill(problem(500, 'SERVER_ERROR', 'Something went wrong')),
    );
    await page.goto('/account/settings');

    await page.getByRole('button', { name: 'Sign out everywhere' }).click();
    const dialog = page.getByRole('dialog', { name: 'Sign out everywhere?' });
    await dialog.getByRole('button', { name: 'Sign out everywhere' }).click();

    await expect(dialog.getByText('Couldn’t sign you out')).toBeVisible();
    await expect(dialog.getByRole('button', { name: 'Try again' })).toBeVisible();
  });
});
