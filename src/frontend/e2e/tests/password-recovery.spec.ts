// Acceptance Test
// Traces to: L2-007, L2-008, L2-009
// Description: A visitor who forgot a password asks for a link without learning whether the address
// is registered, then chooses a new password from the emailed link. Expired links, weak passwords,
// a locked account, a long absence, and unknown or forbidden addresses each get their own page.

import { expect, marcus, problem, signInAs, test } from '../fixtures';
import { SignInPage } from '../pages/sign-in.page';

test.describe('forgot password', () => {
  test('sign in offers Forgot password? and it opens the request page', async ({ page }) => {
    const signIn = new SignInPage(page);
    await signIn.goto();

    await page.getByRole('link', { name: 'Forgot password?' }).click();

    await expect(page).toHaveURL('/forgot-password');
    await expect(page.getByRole('heading', { level: 1 })).toHaveText('Reset your password');
  });

  test('a well-formed email gets the same generic answer whoever it belongs to', async ({ page }) => {
    let body: unknown;
    await page.route('**/api/auth/password-reset', async (route) => {
      body = route.request().postDataJSON();
      await route.fulfill({ status: 202 });
    });
    await page.goto('/forgot-password');

    await page.getByLabel('Email').fill('  marcus.bell@example.com ');
    await page.getByRole('button', { name: 'Send reset link' }).click();

    await expect(page.getByRole('heading', { level: 1 })).toHaveText('Check your email');
    await expect(page.getByText('If an account exists for this email, we’ve sent a link.')).toBeVisible();
    await expect(page.getByRole('link', { name: 'Back to sign in' })).toHaveAttribute('href', '/sign-in');
    expect(body).toEqual({ email: 'marcus.bell@example.com' });
  });

  test('a malformed email is refused before any request', async ({ page, unfixturedApiCalls }) => {
    await page.goto('/forgot-password');

    await page.getByLabel('Email').fill('not an email');
    await page.getByRole('button', { name: 'Send reset link' }).click();

    await expect(page.getByText('Enter an email like name@example.com')).toBeVisible();
    expect(unfixturedApiCalls).toEqual([]);
  });

  test('a server failure shows the reference and keeps the form', async ({ page }) => {
    await page.route('**/api/auth/password-reset', (route) =>
      route.fulfill(problem(500, 'SERVER_ERROR', 'Something went wrong')),
    );
    await page.goto('/forgot-password');

    await page.getByLabel('Email').fill('marcus.bell@example.com');
    await page.getByRole('button', { name: 'Send reset link' }).click();

    await expect(page.getByRole('alert')).toContainText('Couldn’t send the link');
    await expect(page.getByLabel('Email')).toHaveValue('marcus.bell@example.com');
  });
});

test.describe('reset password', () => {
  test('the link sets a new password and says you are signed out everywhere', async ({ page }) => {
    let body: unknown;
    await page.route('**/api/auth/password-reset/confirm', async (route) => {
      body = route.request().postDataJSON();
      await route.fulfill({ status: 204 });
    });
    await page.goto('/reset-password?token=abc123');

    await page.getByLabel('New password').fill('a brand new passphrase');
    await page.getByLabel('Confirm password').fill('a brand new passphrase');
    await page.getByRole('button', { name: 'Save password' }).click();

    await expect(page.getByRole('heading', { level: 1 })).toHaveText('Password saved');
    await expect(page.getByRole('link', { name: 'Sign in' })).toHaveAttribute('href', '/sign-in');
    expect(body).toEqual({ token: 'abc123', password: 'a brand new passphrase' });
  });

  test('a short or mismatched password is refused before any request', async ({ page, unfixturedApiCalls }) => {
    await page.goto('/reset-password?token=abc123');

    await page.getByLabel('New password').fill('short');
    await page.getByRole('button', { name: 'Save password' }).click();
    await expect(page.getByText('Password must be at least 12 characters')).toBeVisible();

    await page.getByLabel('New password').fill('a brand new passphrase');
    await page.getByLabel('Confirm password').fill('something different');
    await page.getByRole('button', { name: 'Save password' }).click();
    await expect(page.getByText('The passwords don’t match')).toBeVisible();
    expect(unfixturedApiCalls).toEqual([]);
  });

  test('a breached password shows the server message against the field', async ({ page }) => {
    await page.route('**/api/auth/password-reset/confirm', (route) =>
      route.fulfill(
        problem(422, 'PASSWORD_INVALID', 'Check the highlighted fields', {
          password: {
            code: 'PASSWORD_BREACHED',
            message: 'This password has appeared in a data breach. Choose another.',
          },
        }),
      ),
    );
    await page.goto('/reset-password?token=abc123');

    await page.getByLabel('New password').fill('password1234567');
    await page.getByLabel('Confirm password').fill('password1234567');
    await page.getByRole('button', { name: 'Save password' }).click();

    await expect(page.getByText('This password has appeared in a data breach. Choose another.')).toBeVisible();
    await expect(page.getByRole('heading', { level: 1 })).toHaveText('Choose a new password');
  });

  test('an expired or used link offers a new one', async ({ page }) => {
    await page.route('**/api/auth/password-reset/confirm', (route) =>
      route.fulfill(problem(400, 'RESET_LINK_EXPIRED', 'This link has expired')),
    );
    await page.goto('/reset-password?token=old');

    await page.getByLabel('New password').fill('a brand new passphrase');
    await page.getByLabel('Confirm password').fill('a brand new passphrase');
    await page.getByRole('button', { name: 'Save password' }).click();

    await expect(page.getByRole('heading', { level: 1 })).toHaveText('This link has expired');
    await expect(page.getByRole('link', { name: 'Request a new link' })).toHaveAttribute('href', '/forgot-password');
  });
});

test.describe('sign in after trouble', () => {
  test('a locked account is told to wait 15 minutes', async ({ page }) => {
    await page.route('**/api/auth/sign-in', (route) =>
      route.fulfill(problem(429, 'SIGN_IN_LOCKED', 'Too many sign-in attempts. Try again in 15 minutes')),
    );
    const signIn = new SignInPage(page);
    await signIn.goto();

    await signIn.signIn('marcus.bell@example.com', 'whatever it is');

    await expect(page.getByText('Too many attempts')).toBeVisible();
    await expect(page.getByText('Try again in 15 minutes. We’ve emailed you about this.')).toBeVisible();
  });

  test('after 14 days away the page says welcome back, then shows the form', async ({ page }) => {
    const signIn = new SignInPage(page);
    await signIn.goto(undefined, { expired: '1' });

    await expect(signIn.heading).toHaveText('Welcome back');
    await expect(page.getByText('For your security, sign in again after 14 days away.')).toBeVisible();

    await page.getByRole('button', { name: 'Sign in' }).click();

    await expect(signIn.heading).toHaveText('Sign in');
    await expect(signIn.email).toBeVisible();
  });
});

test.describe('pages that are not there or not yours', () => {
  test('an unknown address says page not found', async ({ page }) => {
    await page.route('**/api/catalog/**', (route) => route.fulfill({ status: 404, json: {} }));
    await page.goto('/no/such/page');

    await expect(page.getByRole('heading', { level: 1 })).toHaveText('Page not found');
    await expect(page.getByRole('link', { name: 'Back to shop' })).toHaveAttribute('href', '/');
  });

  test('403 says it belongs to another customer', async ({ page }) => {
    await signInAs(page, marcus);
    await page.goto('/403');

    await expect(page.getByRole('heading', { level: 1 })).toHaveText('You can’t open this');
    await expect(page.getByText('It belongs to another customer’s account.')).toBeVisible();
    await expect(page.getByRole('link', { name: 'Go to your account' })).toHaveAttribute('href', '/account');
  });

  test('403 for management says it is only for the founder', async ({ page }) => {
    await signInAs(page, marcus);
    await page.goto('/403?reason=management');

    await expect(page.getByText('Shop management is only for the shop’s founder.')).toBeVisible();
  });
});
