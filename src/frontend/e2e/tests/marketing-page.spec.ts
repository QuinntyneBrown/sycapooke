// Acceptance Test
// Traces to: L2-078, L2-079, L2-080, L2-081, L2-082, L2-083
// Description: The public marketing page at /welcome: sections and CTA targets for visitors and
// members, the footer link, heading order, title, meta description and link-preview tags. The share image comes from the static build.

import { expect, marcus, serveCatalog, signInAs, test } from '../fixtures';
import { ShellPage } from '../pages/shell.page';
import { WelcomePage } from '../pages/welcome.page';

test.describe('marketing page', () => {
  test('a visitor sees the sections and Start selling goes to /join', async ({ page, unfixturedApiCalls }) => {
    const welcome = new WelcomePage(page);
    await welcome.goto();

    await expect(welcome.heading).toHaveText('A home for the things that mattered.');
    for (const name of [
      'Every piece is at least 20 years old.',
      'Downsizing? Let your treasures find their next home.',
      'Three steps. That’s it.',
      'sy·ca·pooke',
      'Treasures with a story.',
    ]) {
      await expect(welcome.main.getByRole('heading', { level: 2, name })).toBeVisible();
    }
    await expect(welcome.main.getByRole('heading', { level: 3 })).toHaveText(['List it', 'Sell it', 'Get paid']);
    await expect(welcome.main).not.toContainText(/\bgirls?\b|\bkids?\b/i);
    const sell = welcome.sellLinks();
    await expect(sell).not.toHaveCount(0);
    for (const link of await sell.all()) await expect(link).toHaveAttribute('href', '/join');
    expect(unfixturedApiCalls).toEqual([]);
  });

  test('a signed-in member gets Start selling to /sell', async ({ page }) => {
    await signInAs(page, marcus);
    const welcome = new WelcomePage(page);
    await welcome.goto();

    await expect(welcome.heading).toBeVisible();
    await expect(welcome.sellLinks().first()).toHaveAttribute('href', '/sell');
    for (const link of await welcome.sellLinks().all()) await expect(link).toHaveAttribute('href', '/sell');
  });

  test('Shop the catalog goes to the catalog', async ({ page }) => {
    await serveCatalog(page, 0);
    const welcome = new WelcomePage(page);
    await welcome.goto();

    await welcome.link('Shop the catalog').click();
    await expect(page).toHaveURL(/\/$/);
    await expect(page.getByRole('heading', { level: 1 })).toHaveText('Find something with a story.');
  });

  test('title, meta description, heading order and no horizontal scroll at 320px', async ({ page }) => {
    await page.setViewportSize({ width: 320, height: 640 });
    const welcome = new WelcomePage(page);
    await welcome.goto();

    await expect(page).toHaveTitle('Treasures with a story · Sycapooke');
    await expect(page.locator('meta[name="description"]')).toHaveAttribute('content', /made only for vintage/i);
    const levels = await welcome.headingLevels();
    expect(levels[0]).toBe(1);
    expect(levels.filter((level) => level === 1)).toHaveLength(1);
    levels.forEach((level, i) => i > 0 && expect(level - levels[i - 1]).toBeLessThanOrEqual(1));
    expect(await new ShellPage(page).hasHorizontalScroll()).toBe(false);
  });

  test('the footer links to the marketing page', async ({ page }) => {
    await serveCatalog(page, 0);
    const shell = new ShellPage(page);
    await shell.goto();

    await shell.footerLink('About Sycapooke').click();
    await expect(page).toHaveURL(/\/welcome$/);
    await expect(new WelcomePage(page).heading).toBeVisible();
  });

  test('the HTML a link preview fetches carries the share tags and the image exists', async ({ page }) => {
    const html = await (await page.request.get('/welcome')).text();
    const meta = (key: string) => html.match(new RegExp(`<meta (?:property|name)="${key}" content="([^"]*)"`))?.[1];
    expect(meta('og:title')).toBe('Sycapooke · Treasures with a story');
    expect(meta('og:type')).toBe('website');
    expect(meta('og:image')).toBe('https://www.sycapooke.com/media/sycapooke-share.jpg');
    expect(meta('og:image:width')).toBe('1200');
    expect(meta('og:image:height')).toBe('630');
    expect(meta('twitter:card')).toBe('summary_large_image');
    expect(meta('twitter:image')).toBe('https://www.sycapooke.com/media/sycapooke-share.jpg');
    expect(meta('og:description')).toMatch(/made only for vintage/);
    const image = await page.request.get('/media/sycapooke-share.jpg');
    expect(image.status()).toBe(200);
    expect(image.headers()['content-type']).toBe('image/jpeg');
  });
});
