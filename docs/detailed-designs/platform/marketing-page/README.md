# Marketing page

## Overview

Sycapooke is a marketplace made only for vintage: jewelry, clothing, housewares and
electronics, every piece at least 20 years old and listed by a real person with a real story.
This feature, part of the `platform` subsystem, adds a public marketing page at `/welcome`
that sells Sycapooke to shoppers and sellers: a hero on paper with the hero photo, four feature
bands (shoppers on paper, sellers on terracotta, how it works on paper and the founder's story
on cream) and a closing terracotta band. Anyone can open
it, signed in or not. It refines `L1-020` through `L2-078`, `L2-079` and `L2-083`; `L2-080`
to `L2-082` (the former video dialog) are retired and never reused.

Terms used below:

- **band** — one flat, full-width colour from the landing page: paper (the hero, For shoppers,
  How it works), cream under a line rule (Our story), or terracotta with cream text and ink
  buttons (For sellers and the closing call to action). The terracotta band keeps its colours
  in both themes. No gradient, no shapes
  ([design system](../../../design-system/components/feature-band.html))
- **hero photo** — `/media/sycapooke-hero.jpg`, the photograph in the hero's media slot
- **founder photo** — `/media/sycapooke-founder.jpg`, shown beside the founder's quote in
  the Our story band
- **share image** — `/media/sycapooke-share.jpg`, the 1200×630 link-preview image

The page calls no Sycapooke API endpoint; the shell's `GET /api/auth/me` decides the seller
target. Nothing in the vintage rule, the catalog, sell, accounts or the API changes. The copy
says every piece is at least 20 years old and promises nothing newer
([ADR general/0005](../../../adr/general/0005-marketing-wording.md),
[ADR general/0006](../../../adr/general/0006-vintage-only.md)).

## Description

### Library, route and shell ([ADR platform/0011](../../../adr/platform/0011-marketing-feature-library.md))

- `src/frontend/projects/marketing/` (alias `@sycapooke/marketing`), built by `build:libs`,
  exports `routes`: `{ path: '', loadComponent: WelcomePage, title: 'Treasures with a story ·
  Sycapooke', data: { shell: 'marketing' } }`.
- `app.routes.ts` mounts it lazily at `welcome`, before the catalog's empty-path routes.
- `AppShellComponent` gains the `marketing` shell: same payout banner, top bar and footer, but
  `<main id="main" class="marketing" tabindex="-1">` without `.container.page`, so bands are
  full width.

### Components (`@sycapooke/components`)

| Component | Inputs and outputs | Notes |
|---|---|---|
| `sy-marketing-hero` | `eyebrow`, `lede`, `headingId`; slots `[title]`, `[actions]`, `[media]` | The h1 band on paper; the media slot holds the hero photo (4:5, 2rem corners, no frame) |
| `sy-feature-band` | `eyebrow`, `heading`, `headingId`, `lede`, `points`, `tone: 'default' \| 'sell' \| 'story'`, `stacked`; slots `[actions]`, body | Section labelled by its h2; `sell` is the terracotta band (cream text, `syButton="inverse"` actions), `story` the cream band under a line rule; the closing call to action is `sell` + `stacked` with a lede |
| `sy-steps` | `steps: { title, text }[]` | Ordered list, h3 titles |
| `sy-quote` | `cite`, `photo`, `photoAlt`; body | `<blockquote>` with a `<figcaption>` attribution and the founder photo; the photo is not decorative and carries its own alt text |

### Feature (`@sycapooke/marketing`)

- **`WelcomePage`** — static copy, composed top to bottom:
  1. `sy-marketing-hero` (paper) — eyebrow "Jewelry · Clothing · Housewares · Electronics", h1
     "A home for the things that *mattered.*", lede "Sycapooke is the marketplace made only
     for vintage — for families downsizing a lifetime of treasures, and the people who’ll love
     them next.", primary action "Shop the catalog" → `/`, secondary "Start selling" →
     `sellHref`; the hero photo in the media slot.
  2. `sy-feature-band` For shoppers (paper) — heading "Every piece is at least 20 years old.",
     points Four focused categories / Listed by a real person / Honest condition; action
     "Browse the catalog" → `/`.
  3. `sy-feature-band` For sellers (`tone="sell"`, terracotta) — heading "Downsizing? Let your
     treasures find their next home.", points Big buttons, clear steps / A real person to call
     1-800-555-0123; inverse action "List your first item" → `sellHref`.
  4. `sy-feature-band` How it works (`stacked`, paper) — heading "Three steps. That’s it." with
     `sy-steps` List it / Sell it / Get paid.
  5. `sy-feature-band` Our story (`tone="story"`, `stacked`, cream) — heading "sy·ca·pooke"
     with `sy-quote`: “It’s the made-up word I whispered to my son when he was a baby. When it
     came time to clear out our family home, I realized every teacup and brooch deserved that
     same word — and someone who’d see it too.” — Founder, age 70, grandmother of five; and the
     founder photo.
  6. `sy-feature-band` closing (`tone="sell"`, `stacked`, terracotta) — h2 "Treasures with a
     *story.*", lede "New treasures arrive daily, every one at least 20 years old.", inverse
     action "Shop the catalog" → `/`.

  `sellHref = computed(() => auth.member() ? '/sell' : '/join')` (`L2-078`).
- **Share tags** — crawlers that build link previews do not run scripts, so the meta
  description ("The marketplace made only for vintage: jewelry, clothing, housewares and
  electronics, every piece at least 20 years old and listed by a real person with a real
  story."), Open Graph (`og:title` "Sycapooke · Treasures with a story", `og:description`,
  `og:type`, `og:site_name`, `og:image` with type, width, height and alt, `og:locale`) and
  Twitter card (`summary_large_image`) tags are static in `projects/sycapooke/src/index.html`.
  The marketing build (served at `https://www.sycapooke.com/`) uses `index.marketing.html`
  (angular.json `index`), which adds `og:url`, the canonical link and the title "Treasures
  with a story · Sycapooke". `og:image` is the absolute
  `https://www.sycapooke.com/media/sycapooke-share.jpg`.
- **Footer** — `sy-footer` carries the "About Sycapooke" link to `/welcome` on every page
  (`L2-079`).

### Media

The hero, founder and share images live in `src/frontend/projects/sycapooke/public/media/`
(served at `/media/`) with their licences in `ATTRIBUTION.md`, and are copied to
`docs/mocks/assets/media/`. Fraunces and Source Sans 3 are self-hosted from `public/fonts/`.
Nothing on the page autoplays and nothing is loaded from a third party. Advertising
placements from the landing page are not a product feature (deferred).

### Tests (`src/frontend/e2e/`)

`tests/marketing-page.spec.ts` with `pages/welcome.page.ts`, served by the static build.
Covers `L2-078`, `L2-079` and `L2-083`: the six sections and their headings in order, the CTA
targets for a visitor and for a member, the founder quote and both photos with alt text,
reduced motion, the footer link, the title, the meta description and the link-preview tags.

## Requirements

| L2 ID | Refines (L1) | Requirement |
|-------|--------------|-------------|
| `L2-078` | `L1-020` | Page sections and CTA targets for visitors and members; copy for vintage only |
| `L2-079` | `L1-020` | Footer "About Sycapooke" link |
| `L2-080` | `L1-020` | Retired (video dialog); never reused |
| `L2-081` | `L1-020` | Retired (video captions); never reused |
| `L2-082` | `L1-020` | Retired (video error state); never reused |
| `L2-083` | `L1-020` | Flat bands with no decoration, contrast on them, heading order, no decorative motion, title, meta description and link-preview tags |

Related requirements applied without restating them: `L2-061` to `L2-065` (responsive,
accessible shell), `L2-010` (no role needed), `L2-011` to `L2-013` (the vintage rule the
copy must stay true to).

## Diagrams

The PlantUML sources are in `diagrams/`, each with its rendered PNG. Re-render after an edit
with
`python -B -I .claude/skills/writing-software-design-documents/scripts/render_puml.py docs/detailed-designs/platform/marketing-page/diagrams`.

### Components

![C4 component view of the marketing page](diagrams/c4-component.png)

## Open questions

- Deferred: SEO beyond the title, meta description and link-preview tags, analytics, A/B tests,
  newsletter sign-up, making `/welcome` the home page, translations and advertising placements.
