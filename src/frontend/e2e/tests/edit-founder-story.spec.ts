// Acceptance Test
// Traces to: L2-003, L2-041
// Description: The founder edits her public story at /shop/story: display name, a story of up to
// 500 characters and an optional city and region. Server errors show inline; a customer is sent to the 403.

import { expect, ida, marcus, problem, signInAs, test } from '../fixtures';

const story = {
  displayName: 'ida_campbell',
  story: 'Treasures I have kept.',
  city: 'Kingston',
  region: 'Jamaica',
};

test.describe('edit founder story', () => {
  test('loads the story, counts characters and saves', async ({ page, unfixturedApiCalls }) => {
    await signInAs(page, ida);
    let sent: unknown;
    await page.route('**/api/shop/story', async (route) => {
      if (route.request().method() === 'PUT') {
        sent = route.request().postDataJSON();
        return route.fulfill({ json: sent });
      }
      return route.fulfill({ json: story });
    });
    await page.goto('/shop/story');

    await expect(page.getByRole('heading', { level: 1 })).toHaveText('Story');
    await expect(page.getByLabel('Display name')).toHaveValue('ida_campbell');
    await expect(page.locator('.field__counter')).toHaveText('22 / 500');

    await page.getByLabel('Story').fill('A new story.');
    await expect(page.locator('.field__counter')).toHaveText('12 / 500');
    await page.getByRole('button', { name: 'Save story' }).click();

    await expect(page.getByRole('status').filter({ hasText: 'Story saved' })).toBeVisible();
    expect(sent).toEqual({ ...story, story: 'A new story.' });
    expect(unfixturedApiCalls).toEqual([]);
  });

  test('shows a taken name inline', async ({ page }) => {
    await signInAs(page, ida);
    await page.route('**/api/shop/story', (route) =>
      route.request().method() === 'PUT'
        ? route.fulfill(
            problem(422, 'VALIDATION_FAILED', 'Check the highlighted fields', {
              displayName: { code: 'TAKEN', message: 'That display name is taken' },
            }),
          )
        : route.fulfill({ json: story }),
    );
    await page.goto('/shop/story');
    await page.getByRole('button', { name: 'Save story' }).click();

    await expect(page.locator('#display-name-error, .field__error').first()).toContainText(
      'That display name is taken',
    );
  });

  test('a customer opening it sees the management 403', async ({ page }) => {
    await signInAs(page, marcus);
    await page.goto('/shop/story');

    await expect(page).toHaveURL('/403?reason=management');
  });
});
