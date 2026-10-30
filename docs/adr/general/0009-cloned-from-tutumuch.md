# ADR-0009: Sycapooke Is a Clone of TuTuMuch

**Date:** 2026-10-30
**Category:** general
**Status:** Accepted
**Deciders:** Quinntyne Brown (product owner)

## Context

Sycapooke is a peer-to-peer marketplace made only for vintage jewelry, clothing, housewares and
electronics, for families downsizing a lifetime of treasures and the people who will love them
next. TuTuMuch, a marketplace for children's goods that itself began as a clone of 12L, already
had every journey Sycapooke needs: open membership, a locked catalogue, listings with photos,
checkout, shipping, payouts, problems, notifications, privacy, a public profile, a marketing
page, a single `verify` command, continuous deployment to one small VM and a UAT host. Only the
domain words, the catalogue rule and the look differ.

## Decision

Sycapooke is a **fresh clone** of the TuTuMuch repository, copied into a new repository with no
shared git history, then rebranded and re-skinned. Every journey, screen, layout, component,
rule and ADR mirrors TuTuMuch (and through it 12L); this ADR records what changed.

**Renamed.** TuTuMuch → Sycapooke throughout: the .NET solution and projects (`Sycapooke.*`),
the Angular application project (`projects/sycapooke/`) and path aliases (`@sycapooke/*`), the
component selector prefix (`tl` → `sy`, so every component is `sy-*`), the design-system name
(Giddy → **Heirloom**), the domain (`sycapooke.com`, `uat.sycapooke.com`, `hello@sycapooke.com`),
cookies (`sy_*`), order references (`SY-`), configuration keys (`ConnectionStrings__Sycapooke`),
image names (`sycapooke-api`, `sycapooke-web`, ...) and the GitHub repository
(`QuinntyneBrown/sycapooke`).

**Re-skinned.** The look is the Heirloom design system, extracted from the Sycapooke landing page
built in Magic Patterns (October 2026): Paper, Cream, Ink, Muted, Line, Terracotta, Forest and
Mustard; Fraunces for display and the italic wordmark, Source Sans 3 for body, both self-hosted
under the SIL Open Font License; pill buttons and rounded cards; a terracotta price-tag logo. The
Heirloom palette block and the Sycapooke layout tokens in `tokens.css` replace the Giddy palette;
12L-only token names alias Heirloom tokens so the layout did not change
([design system](../../design-system/README.md)).

**Re-ruled.** The catalogue's audience lock became the vintage lock: four categories, 24
subcategories, one accepted value `Vintage`, every piece at least 20 years old
([ADR-0006](0006-vintage-only.md)). The marketing copy follows
([ADR-0005](0005-marketing-wording.md)).

**Dropped.** The 30-second pitch video and its dialog, poster, player, captions, media files and
build script (`L2-080` to `L2-082` retired, never reused); the mascot; and all print collateral
(`docs/print/` and its ADR, general/0007, deleted). The marketing page gained hero and founder
photographs and an Our story band with the founder's quote instead.

**Deferred.** The advertising placements on the landing page are not a product feature.

## Options Considered

### Option 1: Fresh clone, rebrand in place (chosen)
- **Pros:** Every journey, test and deployment path arrives working; one rename pass; the two
  products diverge freely from here.
- **Cons:** No shared history: a fix in TuTuMuch does not flow to Sycapooke, and vice versa.

### Option 2: Fork with shared history and merge upstream changes
- **Pros:** Fixes flow from TuTuMuch.
- **Cons:** Every upstream change carries children's copy, categories and the Giddy look that
  must be undone on each merge; the products are different businesses with different owners of
  their copy. The merge cost outweighs the fixes.

### Option 3: Extract a shared marketplace core
- **Pros:** One codebase for both.
- **Cons:** A platform project neither product needs yet; deferred until a third marketplace
  appears.

## Consequences

### Positive
- Sycapooke starts complete: 20 journeys, specs, mocks, designs, estimates, tests and deployment.
- The lineage is recorded once, here; other documents name TuTuMuch only as the origin.

### Negative
- Children's wording can survive in prose; the final sweep of the rebrand checked every document
  outside the build folders, and new work must not reintroduce it.

### Risks
- A reader may assume a design choice was made for Sycapooke when it was inherited. Mitigation:
  ADRs dated before 2026-10-30 are TuTuMuch decisions carried over, as their dates show.

## References

- [ADR-0005](0005-marketing-wording.md), [ADR-0006](0006-vintage-only.md), [ADR-0008](0008-remove-the-for-girls-confirmation.md)
- [ADR platform/0011](../platform/0011-marketing-feature-library.md)
- `CHANGELOG.md` (0.1.0)
