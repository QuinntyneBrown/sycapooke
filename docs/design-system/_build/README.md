# Heirloom logo and icon build

The logo, app icon, favicon and link-preview image are generated, never hand-edited.

1. `python make_logo_svgs.py <repo root> <path to Fraunces-Italic[SOFT,WONK,opsz,wght].ttf>`
   writes `../assets/logo.svg`, `logo-dark.svg` and `app-icon.svg` with the Fraunces italic 600
   glyphs (opsz 24) converted to paths, so the files need no font. The variable TTF is the
   upstream release at <https://github.com/undercasetype/Fraunces> (SIL OFL 1.1).
2. `node render_assets.mjs <repo root>` (needs `playwright` installed next to it) renders
   `../assets/logo.png`, `logo-dark.png`, `app-icon.png`, the application's `public/icon-512.png`,
   `apple-touch-icon.png`, three favicon sizes and `public/media/sycapooke-share.jpg`
   (1200 × 630: logo, tagline and the hero photo).
3. Pack the favicon with Pillow:
   `python -c "from PIL import Image; i=[Image.open(f'docs/design-system/assets/_icon-{s}.png') for s in (16,32,64)]; i[2].save('src/frontend/projects/sycapooke/public/favicon.ico', sizes=[(16,16),(32,32),(64,64)], append_images=i[:2])"`
   then delete the `_icon-*.png` files.
