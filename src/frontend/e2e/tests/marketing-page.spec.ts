// Acceptance Test
// Traces to: L2-078, L2-079, L2-083, L2-089, L2-090, L2-091
// Description: The public marketing page at /welcome: sections and CTA targets for visitors and
// members, the footer link, heading order, title, meta description and link-preview tags, and the
// pitch video: poster only until pressed, keyboard-operable player with captions off by default,
// and a plain error with Try again when the video cannot load. The share image comes from the
// static build.

import { expect, marcus, serveCatalog, servePitchVideo, signInAs, test } from '../fixtures';
import { ShellPage } from '../pages/shell.page';
import { WelcomePage } from '../pages/welcome.page';

test.describe('marketing page', () => {
  test('a visitor sees the sections and Start selling goes to /join', async ({ page, unfixturedApiCalls }) => {
    const welcome = new WelcomePage(page);
    await welcome.goto();

    await expect(welcome.heading).toHaveText('A home for the things that mattered.');
    await expect(welcome.poster).toBeVisible();
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
    const meta = (key: string) =>
      html.match(new RegExp(`<meta\\s+(?:property|name)="${key}"\\s+content="([^"]*)"`))?.[1];
    expect(meta('og:title')).toBe('Sycapooke · Treasures with a story');
    expect(meta('og:type')).toBe('website');
    expect(meta('og:image')).toBe('https://sycapooke.canadacentral.cloudapp.azure.com/media/sycapooke-share.jpg');
    expect(meta('og:image:width')).toBe('1200');
    expect(meta('og:image:height')).toBe('630');
    expect(meta('twitter:card')).toBe('summary_large_image');
    expect(meta('twitter:image')).toBe('https://sycapooke.canadacentral.cloudapp.azure.com/media/sycapooke-share.jpg');
    expect(meta('og:description')).toMatch(/made only for vintage/);
    const image = await page.request.get('/media/sycapooke-share.jpg');
    expect(image.status()).toBe(200);
    expect(image.headers()['content-type']).toBe('image/jpeg');
  });

  test.describe('pitch video', () => {
    test.beforeEach(async ({ page }) => {
      await servePitchVideo(page);
    });

    test('only the poster is on the page until it is pressed', async ({ page }) => {
      const welcome = new WelcomePage(page);
      await welcome.goto();

      await expect(welcome.poster).toBeVisible();
      await expect(welcome.poster).toHaveAccessibleName('Play');
      await expect(welcome.poster.locator('img')).toHaveAttribute('alt', '');
      await expect(welcome.poster).toContainText('0:29');
      await expect(page.locator('video')).toHaveCount(0);
      await expect(welcome.dialog).toHaveCount(0);
    });

    test('the keyboard opens the dialog, plays and pauses, and Escape returns focus to the poster', async ({
      page,
    }) => {
      const welcome = new WelcomePage(page);
      await welcome.goto();

      await welcome.poster.focus();
      await page.keyboard.press('Enter');
      await expect(welcome.dialog).toBeVisible();
      await expect(welcome.video).toHaveAttribute('aria-label', 'Sycapooke video');
      await expect(welcome.playPause).toBeVisible();

      // Autoplay may be blocked; the Play/Pause pill reflects whichever state the video is in.
      await welcome.playPause.focus();
      const pausedBefore = await welcome.isPaused();
      await page.keyboard.press('Enter');
      await expect.poll(() => welcome.isPaused()).toBe(!pausedBefore);
      await expect(welcome.playPause).toHaveText(pausedBefore ? 'Pause' : 'Play');
      await page.keyboard.press('Space');
      await expect.poll(() => welcome.isPaused()).toBe(pausedBefore);
      await expect(welcome.playPause).toHaveText(pausedBefore ? 'Play' : 'Pause');

      await page.keyboard.press('Escape');
      await expect(welcome.dialog).toHaveCount(0);
      await expect(page.locator('video')).toHaveCount(0);
      await expect(welcome.poster).toBeFocused();
    });

    test('captions are off until Captions turns them on, and the dialog is named by a hidden heading', async ({
      page,
    }) => {
      const welcome = new WelcomePage(page);
      await welcome.goto();
      await welcome.poster.click();

      await expect(welcome.dialog).toBeVisible();
      const heading = welcome.dialog.getByRole('heading', { name: 'Sycapooke video' });
      await expect(heading).toHaveClass(/visually-hidden/);
      await expect(welcome.captions).toHaveAttribute('aria-pressed', 'false');
      const trackMode = () => welcome.video.evaluate((video: HTMLVideoElement) => video.textTracks[0]?.mode);
      await expect.poll(trackMode).toBe('hidden');

      await welcome.captions.click();
      await expect(welcome.captions).toHaveAttribute('aria-pressed', 'true');
      await expect.poll(trackMode).toBe('showing');

      await welcome.captions.click();
      await expect(welcome.captions).toHaveAttribute('aria-pressed', 'false');
      await expect.poll(trackMode).toBe('hidden');
    });

    test('when the video cannot load the dialog says so with Try again, and the page still works', async ({ page }) => {
      await page.route('**/media/sycapooke-pitch.{mp4,webm}', (route) => route.fulfill({ status: 404 }));
      await serveCatalog(page, 0);
      const welcome = new WelcomePage(page);
      await welcome.goto();
      await welcome.poster.click();

      await expect(welcome.dialog).toBeVisible();
      await expect(welcome.videoError).toContainText('The video didn’t load');
      await expect(welcome.videoError).toContainText('Check your connection and try again.');
      await expect(welcome.videoError.getByRole('button', { name: 'Try again' })).toBeVisible();
      await expect(welcome.playPause).toHaveCount(0);

      await welcome.dialog.getByRole('button', { name: 'Close' }).click();
      await expect(welcome.dialog).toHaveCount(0);
      await welcome.link('Shop the catalog').click();
      await expect(page).toHaveURL(/\/$/);
    });
  });
});
