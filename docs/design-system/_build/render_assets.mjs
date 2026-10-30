// Render Heirloom logo/icon/share images from the SVGs. Run with: node render_assets.mjs <repoRoot>
import { chromium } from 'playwright';
import { readFileSync, writeFileSync, mkdirSync } from 'node:fs';
import { join } from 'node:path';

const root = process.argv[2];
const ds = join(root, 'docs', 'design-system', 'assets');
const pub = join(root, 'src', 'frontend', 'projects', 'sycapooke', 'public');
const fonts = join(pub, 'fonts');
const media = join(pub, 'media');
const svg = (name) => readFileSync(join(ds, name), 'utf8');

const browser = await chromium.launch();
async function shot(html, width, height, out, { scale = 1, type = 'png', omitBackground = false, quality } = {}) {
  const page = await browser.newPage({ viewport: { width, height }, deviceScaleFactor: scale });
  await page.setContent(html, { waitUntil: 'load' });
  await page.evaluate(() => document.fonts.ready);
  await page.screenshot({ path: out, type, omitBackground, quality, fullPage: false });
  await page.close();
  console.log('wrote', out);
}
const plain = (inner, bg = 'transparent') => `<!doctype html><html><body style="margin:0;background:${bg};display:grid;place-items:center;width:100vw;height:100vh">${inner}</body></html>`;

// logo.png / logo-dark.png at 2x of 520x112 → 1040x224
await shot(plain(svg('logo.svg').replace('width="520" height="112"', 'width="1040" height="224"')), 1040, 224, join(ds, 'logo.png'), { omitBackground: true });
await shot(plain(svg('logo-dark.svg').replace('width="520" height="112"', 'width="1040" height="224"'), '#2b2118'), 1040, 224, join(ds, 'logo-dark.png'));
// app icon 512, 180 (apple touch), 64 + 32 + 16 for the favicon
for (const [size, out] of [
  [512, join(ds, 'app-icon.png')],
  [512, join(pub, 'icon-512.png')],
  [180, join(pub, 'apple-touch-icon.png')],
  [64, join(ds, '_icon-64.png')],
  [32, join(ds, '_icon-32.png')],
  [16, join(ds, '_icon-16.png')],
]) {
  await shot(plain(svg('app-icon.svg').replace('width="512" height="512"', `width="${size}" height="${size}"`)), size, size, out, { omitBackground: true });
}

// share image 1200x630: paper background, hero photo on the right, logo + tagline on the left
const fontCss = `
@font-face{font-family:'Fraunces';font-style:italic;font-weight:100 900;src:url('file:///${fonts.replace(/\\/g, '/')}/fraunces-italic-latin.woff2') format('woff2')}
@font-face{font-family:'Source Sans 3';font-style:normal;font-weight:200 900;src:url('file:///${fonts.replace(/\\/g, '/')}/source-sans-3-latin.woff2') format('woff2')}`;
const hero = `file:///${media.replace(/\\/g, '/')}/sycapooke-hero.jpg`;
const share = `<!doctype html><html><head><style>${fontCss}
body{margin:0;width:1200px;height:630px;background:#f6f0e4;font-family:'Source Sans 3',sans-serif;color:#2b2118;display:flex;overflow:hidden}
.text{flex:1;padding:72px 64px;display:flex;flex-direction:column;justify-content:center;gap:28px}
.photo{width:520px;height:630px;object-fit:cover;border-left:6px solid #a04228}
h1{font-family:'Fraunces',serif;font-style:italic;font-weight:600;font-size:64px;line-height:1.05;margin:0;letter-spacing:-0.02em}
p{font-size:28px;line-height:1.35;margin:0;color:#665748;max-width:520px}
.logo svg{height:96px;width:auto}
.rule{width:72px;height:6px;background:#c9962e;border-radius:3px}
</style></head><body>
<div class="text"><div class="logo">${svg('logo.svg')}</div><div class="rule"></div>
<h1>Treasures with a story.</h1>
<p>The marketplace made only for vintage. Jewelry, clothing, housewares and electronics, every piece at least 20 years old.</p></div>
<img class="photo" src="${hero}"></body></html>`;
writeFileSync('share.html', share);
{
  const page = await browser.newPage({ viewport: { width: 1200, height: 630 } });
  await page.goto('file:///' + process.cwd().replaceAll(String.fromCharCode(92), '/') + '/share.html', { waitUntil: 'load' });
  await page.evaluate(() => document.fonts.ready);
  await page.screenshot({ path: join(media, 'sycapooke-share.jpg'), type: 'jpeg', quality: 86 });
  await page.close();
  console.log('wrote share');
}
await browser.close();
