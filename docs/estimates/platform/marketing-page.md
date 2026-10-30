# Estimate — Marketing page

## Basis

The hours assume an intermediate Angular/TypeScript developer who does not yet know the
Sycapooke codebase. They cover implementation, automated tests and review fixes, and exclude
requirements, design, mock and photo work. They assume the app shell and `AuthState` exist.
The page, components and tests are built; the hero, founder and share photos are committed.
[Detailed design](../../detailed-designs/platform/marketing-page/README.md)

## Full design

| Work item | Hours |
|-----------|------:|
| `marketing` library skeleton, lazy route, `marketing` shell, title and meta description | 4 |
| `sy-marketing-hero` with the hero photo media slot, showcase tokens | 6 |
| `sy-feature-band`, `sy-steps` and the page copy with CTA targets | 5 |
| `sy-quote` and the Our story band with the founder photo | 2 |
| Footer link, hero, founder and share images with `ATTRIBUTION.md` | 1 |
| Playwright page object and acceptance tests | 8 |
| Visual check at phone and desktop, light and dark, and fixes | 4 |
| **Total** | **30** |

## Minimal scope

The minimal version drops `sy-quote` (the Our story band is plain text with the founder
photo) and the share image. Minimal total: **26** hours.

## Partial descoping

| Descope | What is lost | Hours saved |
|---------|--------------|------------:|
| `sy-quote` component | Plain-text quote without the attribution layout | 2 |
| Share image and Open Graph tags | Link previews show no image | 1 |
| Founder photo | Our story band is text only | 1 |
