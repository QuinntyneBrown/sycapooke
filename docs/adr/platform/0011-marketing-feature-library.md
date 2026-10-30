# ADR-0011: A Marketing Feature Library and a Marketing Shell

**Date:** 2026-10-26 (trimmed 2026-10-30: the pitch video is dropped, [ADR general/0009](../general/0009-cloned-from-tutumuch.md))
**Category:** platform
**Status:** Accepted
**Deciders:** Quinntyne Brown (product owner)

## Context

The public marketing page (`/welcome`, `L1-020`) sells Sycapooke to shoppers and sellers with
short copy, a hero photograph and the founder's story. It belongs to no existing feature area,
and the app project may not own feature screens (`AGENTS.md`, Frontend workspace layout). Its
full-bleed bands cannot sit in the app shell's `main.container.page`. Its images must be served
by the static host.

## Decision

- Add the `marketing` library: `src/frontend/projects/marketing/`, alias `@sycapooke/marketing`,
  registered in `angular.json`, `tsconfig.json` and `build:libs`. It exports its lazy routes and
  holds `WelcomePage`. The app mounts it at `welcome`.
- Reusable pieces go to `@sycapooke/components`: `sy-marketing-hero`, `sy-feature-band`,
  `sy-steps` and `sy-quote`.
- Add a `marketing` shell value: routes with `data.shell = 'marketing'` get the same top bar
  and footer but `<main id="main" class="marketing">` without the container.
- Page images live in `src/frontend/projects/sycapooke/public/media/` (served at `/media/`):
  the hero photo `sycapooke-hero.jpg`, the founder photo `sycapooke-founder.jpg` and the
  1200×630 share image `sycapooke-share.jpg`, with their sources and licences in
  `public/media/ATTRIBUTION.md`.

## Options Considered

### Option 1: New `marketing` library and shell value (chosen)
- **Pros:** Follows the one-library-per-area rule; the app stays shell-only; bands are full width.
- **Cons:** One more library to build.

### Option 2: Put the page in `catalog`
- **Pros:** No new library.
- **Cons:** Mixes a static sales page into the catalog area.

## Consequences

### Positive
- The page is lazy-loaded and calls no Sycapooke API endpoint of its own.
- No video, no dialog, no media build step: three photographs and copy.

### Negative
- `public/media/` adds committed images; keep them optimised (JPEG, under 300 KB each).

### Risks
- Photographs change with the brand. Mitigation: `ATTRIBUTION.md` records source and licence so
  a swap is a file replacement.

## References

- [marketing-page design](../../detailed-designs/platform/marketing-page/README.md)
- [ADR general/0005](../general/0005-marketing-wording.md)
