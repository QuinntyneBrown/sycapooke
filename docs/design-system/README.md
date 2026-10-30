# Heirloom — the Sycapooke design system

The look is taken from the Sycapooke landing page on [Magic Patterns](https://project-vintage-marketplace-landing-page-451.magicpatterns.app) (Oct 2026); the layout, component set and states come from [docs/mocks](../mocks/README.md). Open [index.html](index.html) locally; press `t` to toggle dark mode.

Sycapooke is the marketplace made only for vintage: jewelry, clothing, housewares and electronics, every piece at
least 20 years old and listed by a real person with a real story. "Treasures with a story." Heirloom is its look:
ink on paper, terracotta for action, flat terracotta, forest and ink bands, a mustard rule, Fraunces 600 headings
over Source Sans 3, flat pill buttons that only change colour on hover, and 0.375rem / 1rem / 2rem corners. Sycapooke keeps the 12L marketplace layout and component
set it inherited through its clone origin (ADR general/0009); only the domain words, the vintage rule and the look differ.

## Palette

| Name | Hex | Token | Role |
|---|---|---|---|
| Paper | `#F6F0E4` | `--palette-paper-100` | Page canvas (`--color-bg-canvas`) |
| Cream | `#FBF7EF` | `--palette-paper-50` | Surfaces, hover fills, text on the terracotta, forest and ink bands (`--color-bg-surface`, `--color-fg-on-accent`, `--color-fg-on-terracotta|forest|ink`) |
| Ink | `#2B2118` | `--palette-ink-800` | Text, the inverse toast, dark-theme surfaces (`--color-fg-default`) |
| Muted | `#665748` | `--palette-ink-500` | Secondary text (`--color-fg-muted`) |
| Line | `#E0D4BE` | `--palette-paper-300` | Borders and dividers (`--color-border-default`) |
| Terracotta | `#A04228` | `--palette-terracotta-600` | Primary: the tag logo, primary buttons, the sell and closing bands, focus ring (`--color-accent`, `--color-band-terracotta`) |
| Terracotta hover | `#85341E` | `--palette-terracotta-700` | Button hover; terracotta text and links, 7.3:1 on paper (`--color-accent-hover`, `--color-fg-link`) |
| Forest | `#2F4A3A` | `--palette-forest-600` | Secondary: the forest card, the inverse button's hover, eyebrows and stat labels (`--color-band-forest`; `--color-fg-forest` for forest text) |
| Mustard | `#C9962E` | `--color-rule-mustard` | The quote rule; a fill only. Mustard text uses 700, dark-theme links use 300 |

Each name is one step of a full scale in `tokens.css` (paper 25–400, ink 200–990, terracotta 50–950, forest
50–950, mustard 50–950). Heirloom's older pop names stay as aliases: pink → terracotta, grape → forest,
sun → mustard, and the cream and sun primitives point at paper and mustard; nothing draws with the pops. Dark mode is an ink canvas with
cream text, a lighter terracotta accent and mustard links; the terracotta, forest and ink bands keep their colours in both themes.

## Typefaces

- **Fraunces** (`--font-family-display`): display and headings; the wordmark and the tag's "S" in italic 600 (`--font-family-logo`).
- **Source Sans 3** (`--font-family-sans`): body, labels and controls.
- Both are self-hosted from `public/fonts/` under the SIL Open Font License; the stacks fall back to Georgia and the system sans.

## Logo

A terracotta price tag (`--color-logo-tag`) with a cream dashed inner line (`--color-logo-tag-line`), a paper hole
(`--color-logo-tag-hole`), an ink string (`--color-logo-string`) and a Fraunces italic "S" (`--color-logo-tag-letter`),
beside the Fraunces italic wordmark "Sycapooke" (`--color-logo-wordmark`, `--font-family-logo`). The tag keeps its
colours in both themes; the string and wordmark follow the text colour, or cream on the showcase. `sy-logo` draws it;
`assets/logo.svg` / `logo.png` (and the `-dark` pair) are the exports, and the app icon and favicon are the tag alone.

## Foundations

| Page | Covers |
|---|---|
| [Color](foundations/color.html) | The named colours, the paper, ink, terracotta, forest and mustard ramps, roles, status, the flat bands and their on-colours, both themes, contrast pairs |
| [Typography](foundations/typography.html) | Fraunces 600 headings (tracking-tight h1) and Source Sans 3 body, the italic wordmark, the landing-page sizes, roles |
| [Spacing](foundations/spacing.html) | 4px scale, inside/between rules |
| [Layout](foundations/layout.html) | L2-061 breakpoints, grid, container, page templates |
| [Elevation](foundations/elevation.html) | The one shadow, z-order |
| [Shape](foundations/shape.html) | Three radii and the pill |
| [Motion](foundations/motion.html) | 150 ms colour hovers, the 300 ms photo zoom, spinners, reduced motion |
| [Iconography](foundations/iconography.html) | Inline SVG set and sizes |
| [Theming](foundations/theming.html) | Token tiers, brand palette layer, light/dark, adding a theme |
| [Responsive](foundations/responsive.html) | Per-component rules and testing matrix |
| [Accessibility](foundations/accessibility.html) | WCAG 2.2 AA commitments |
| [Content](foundations/content.html) | Voice, formulas, microcopy |

## Components

| Component | Variants | States | Source mocks |
|---|---|---|---|
| [Skip link](components/skip-link.html) | one | hidden, focused | every page |
| [Top bar](components/top-bar.html) | guest (Sign in link, Start selling pill), member (Sell pill, avatar), admin, bare | default, hover, current | catalog/guest, catalog/default, problem-detail, maintenance |
| [Footer](components/footer.html) | one (ink; brand column and three link columns) | default, hover | catalog |
| [Consent banner](components/consent-banner.html) | one | shown until a choice, then gone | notifications/cookie-consent/info |
| [Toast](components/toast.html) | info, success, warning, danger; optional action; `ToastService` | entering, leaving, paused | notifications/toast |
| [Sheet](components/sheet.html) | navigation, filter | open | dialogs/nav-menu, dialogs/filters |
| [Button](components/button.html) | primary, secondary, inverse, ghost, link, danger, icon | default, hover, focus, active, disabled, loading | catalog/guest, nav-menu, toast |
| [Provider button](components/provider-button.html) | google; with the "or" divider | default, hover, focus, active, busy, disabled | sign-in/default, sign-in/google-busy, register/default |
| [Link](components/link.html) | inline, nav, footer | default, hover, focus, current | catalog, rules |
| [Form field](components/form-field.html) | input, textarea, select | default, hover, focus, invalid, disabled, read only | sign-in, sell |
| [Alert](components/alert.html) | info, success, warning, danger; banner without title; projected actions; dismissible | static, polite, assertive | sign-in/invalid, sell/draft-saved, verify-email-prompt, payout-banner |
| [Center state](components/center-state.html) | blocked, limited, error, success; icon or status code | static | sell/unverified, sell/limited, sell/error, sell/success, listing-detail/unavailable, listing-detail/error |
| [Chip](components/chip.html) | default, selected, with remove button | default, hover, focus | catalog/filtered, catalog/no-results |
| [Filter chips](components/filter-chips.html) | one pressed toggle chip per value, each with a count | default, hover, focus, pressed | my-listings/default, my-listings/filtered |
| [Choice](components/choice.html) | plain, with links | unticked, ticked, invalid | register, sell |
| [Dialog](components/dialog.html) | default, with top alert; info or danger icon tone | default, busy, sent, limited | dialogs/verify-email-prompt, dialogs/cancel-order |
| [Earnings](components/earnings.html) | one | unknown, known | sell/filled |
| [Error summary](components/error-summary.html) | one | shown and focused | sell/invalid, register/invalid |
| [Fixed size](components/fixed-size.html) | Vintage | absent, shown | sell/filled |
| [Icon](components/icon.html) | 19 glyphs (search added by search-and-filter-catalog, flag by resolve-order-problems) | static | every page |
| [Input group](components/input-group.html) | leading addon | default, invalid | sell price and shipping |
| [Item row](components/item-row.html) | with or without thumbnail | default, truncated | sell/success |
| [Empty state](components/empty-state.html) | default, error (compact); dashed line box | static | catalog/empty, catalog/error |
| [Listing card](components/listing-card.html) | active, reserved, sold; grid of 1, 2 or 4 columns | default, hover (photo zoom), focus, loading skeleton | catalog/default, catalog/loading, catalog/category |
| [Load more](components/load-more.html) | one | default, busy, hidden when all shown | catalog/default |
| [Gallery](components/gallery.html) | one photo, several photos | selected thumbnail, swiped | listing-detail/default |
| [Buy bar](components/buy-bar.html) | Buy now, Edit listing | sticky below 992 px, inline from 992 px | listing-detail/default, listing-detail/guest, listing-detail/own |
| [Seller summary](components/seller-summary.html) | established, new seller | static | listing-detail/default, listing-detail/new-seller |
| [Rating summary](components/rating-summary.html) | rated; "No reviews yet" | static | profile/default, profile/own, profile/empty |
| [Order summary](components/order-summary.html) | priced, not yet priced | static | checkout/default, checkout/new-address |
| [Reservation timer](components/reservation-timer.html) | info, warning, danger | counting, last minute, ended | checkout/default, reservation-timer/warning, reservation-timer/danger |
| [Order timeline](components/order-timeline.html) | paid, shipped, delivered | done, current, to come | sale-detail/shipped, order-detail/default, order-detail/paid |
| [Tracking link](components/tracking-link.html) | USPS, UPS, FedEx, DHL | link states | sale-detail/shipped, order-detail/default |
| [Photo uploader](components/photo-uploader.html) | listing (cover, reorder), evidence (`[cover]="false"`, report-problem) | empty, uploading, filled, rejected, invalid, full | sell/default, sell/filled, sell/invalid |
| [Segmented control](components/segmented.html) | one | empty, chosen, invalid | sell condition |
| [Size tag](components/size-tag.html) | Vintage (the category-pill style); default, large | static | sell, catalog |
| [Spinner](components/spinner.html) | with message | running | verify-email/loading |
| [Stat](components/stat.html) | in a `.stats` stack; compact | static, loading skeleton | payouts/default, payouts/loading |
| [Switch](components/switch.html) | on, off, locked on; hidden label or label with hint | off, on, focus, disabled | notification-settings/default, notification-settings/submitting |
| [Quote](components/quote.html) | default, small | static | order-detail/cancelled, sale-detail/cancel-requested, respond-cancellation |
| [Radio group](components/radio-group.html) | one | nothing chosen, chosen, invalid | cancel-order/default, cancel-order/invalid |
| [Thread](components/thread.html) | one | static, with photos | order-detail/problem, sale-detail/problem, problem-detail/default |
| [Facts](components/facts.html) | one | static | problem-detail/default, problem-detail/resolved |
| [Status](components/status.html) | draft, active, reserved, sold, removed, info (plain pills) | static | problem-detail/default, problem-detail/resolved |
| [Marketing hero](components/marketing-hero.html) | one (on paper, with the photo) | static | welcome |
| [Feature band](components/feature-band.html) | default (paper), sell (terracotta), story (cream), stacked, closing | static | welcome |
| [Steps](components/steps.html) | one (ringed numbers) | static | welcome |

## Patterns

| Pattern | Covers |
|---|---|
| [Navigation](patterns/navigation.html) | Skip link, top bar, sheet, subnav and footer together |

## Tokens

- `tokens/tokens.css` — source of truth (a copy of `src/frontend/projects/components/src/styles/tokens.css`): Heirloom's
  base tokens, then the "Heirloom brand palette" block (paper, ink, terracotta, forest and mustard scales, the logo
  tokens and the semantic overrides for both themes), then the "Sycapooke layout tokens" block (the 12L layout names
  such as `--color-brand`, `--photo-bg-*` and the stone and signal ramps, each an alias, plus the band tokens
  `--color-band-terracotta|forest|ink` and their on-colours; `--chamfer: 0px` because every Heirloom corner is a radius).
- `tokens/tokens.json` — DTCG export generated by `tokens_to_json.py`; don't edit it by hand.
- `tokens/contrast-pairs.json` — Heirloom's pairs plus Sycapooke's (toast status icons, terracotta links on a cream well, cream on the terracotta, forest and ink bands, the forest label, the terracotta price); checked in both themes.
- `assets/logo.svg` / `logo.png` and `logo-dark.svg` / `logo-dark.png` (the tag and wordmark on paper and on ink),
  `app-icon.png` / `app-icon.svg` and `favicon.ico` (the tag alone).
- `assets/components.css` — component stylesheet extracted from the mocks' `ui.css`.

## Drift found in the mocks

| Mock | Issue | Resolution |
|---|---|---|
| `assets/tokens.css` | Heirloom's `--layout-breakpoint-*` tokens (40, 48, 64, 80, 96rem) and its grid switches at 48rem and 64rem differ from the `L2-061` breakpoints (36, 48, 62, 75rem) that `ui.css` uses in its media queries | Documented on the Layout page; media queries keep `L2-061`. The token values are Heirloom's and are not changed here |
| `assets/ui.css` | Size tag, photo stamps and the confirmation mark were cut with a 45° clip-path and drawn in the old dark and light primitives | Size tag is an accent pill, stamps are pills on surface or inverse, the mark is round; all through semantic tokens |
| `assets/ui.css` | Square buttons, chips, badges, switches, avatars, status marks and photos | Pills for controls, `--radius-lg` photos and tiles |
| `assets/ui.css` | Admin tag used white text on the brand colour, which fails in dark mode | `--color-fg-on-accent` on `--color-accent` |
| `assets/ui.css` | The logo was an inline SVG of the old wordmark | The Heirloom tag-and-wordmark logo (`assets/logo.png`, drawn by `sy-logo` in the app) inside the same link |

## Provenance and what was removed

The look is taken from the Magic Patterns landing page and nothing else. On 2026-10-30 every design element that
could not be traced to it was removed from the mocks, this design system and the components library:

| Removed | Why |
|---|---|
| Decorative shapes (`sy-shapes`, blobs, the dotted band, the drift animation, `--color-shape-*`, `--duration-drift`) | Not on the landing page; the bands are flat |
| The press lip and hover lift on primary buttons (`--shadow-press`, `--color-accent-press`, the translate on press) | Landing-page buttons change colour only (`transition-colors duration-150`) |
| The forest gradient showcase (`--gradient-showcase`, `--color-showcase-*`, `--color-fg-showcase*`) | The hero sits on paper; the closing band is flat terracotta |
| Borders, shadows and the hover lift on listing cards | The landing-page card is bare: a cream photo well and text |
| The leading dot and capitals on badges, status pills, eyebrows and overlines | The landing page sets everything in sentence case |
| Shadow levels 2 to 4 | One `shadow-sm` exists; the levels now alias `--shadow-1` |
| Rounded display and `ui-rounded` type stacks, weights above 700 | Fraunces 600 and Source Sans 3 only |
| The 1px outlined secondary button, the accent-filled size tag, the tinted photo wells | Replaced by the 2px ink pill, the cream category pill and cream wells |

## Verification

- `python3 .claude/skills/extracting-design-systems/scripts/check_contrast.py docs/design-system/tokens/tokens.css`: every pair checked in light and dark; see the latest run.
- `python3 .claude/skills/extracting-design-systems/scripts/tokens_to_json.py docs/design-system/tokens/tokens.css` regenerates `tokens.json`.
- `python3 .claude/skills/extracting-design-systems/scripts/check_design_system.py docs/design-system`: see the latest run (0 errors, 0 warnings expected).
