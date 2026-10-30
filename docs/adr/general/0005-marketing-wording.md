# ADR-0005: Marketing Wording: Treasures with a Story

**Date:** 2026-10-26 (rewritten 2026-10-30 for Sycapooke)
**Category:** general
**Status:** Accepted
**Deciders:** Quinntyne Brown (product owner)

## Context

The marketing page (`/welcome`) tells shoppers and sellers what Sycapooke is: a peer-to-peer
marketplace made only for vintage jewelry, clothing, housewares and electronics, where every
piece is at least 20 years old and listed by a real person with a real story. The vintage rule
(`L1-003`, `L2-011` to `L2-013`) accepts listings in those four categories only, every listing
carries the one accepted size value `Vintage`, and the API rejects anything else. Copy that
promised new goods, reproductions, "retro-style" items or a general second-hand marketplace
would send visitors to a catalog and a sell form that refuse them.

The audience is families downsizing a lifetime of treasures, and the people who will love them
next. Many sellers are older and selling for the first time; the copy has to respect that.

## Decision

The marketing copy says plainly that everything here is vintage and that every piece has a
story: "A home for the things that mattered.", "Every piece is at least 20 years old.",
"Downsizing? Let your treasures find their next home.", "Three steps. That's it.", the founder's
story under "sy·ca·pooke", and the closing "Treasures with a story." with "New treasures arrive
daily, every one at least 20 years old." The voice is warm and respectful: it says "treasures"
and "story", never "junk", "clutter", "used goods" or "fast fashion", and never children's copy
of any kind. It names the four categories and the 20-year rule; it never promises new,
reproduction or "vintage-style" goods. The vintage rule, the domain, the API and the sell page
do not change.

The advertising placements shown on the Sycapooke landing page (sponsored slots between the
listings) are **not** a product feature. They are deferred and do not appear in the copy, the
specs, the designs or the mocks.

## Options Considered

### Option 1: Say everything is vintage and every piece has a story (chosen)
- **Pros:** True against the vintage rule; one clear promise; matches the brand and the founder's story.
- **Cons:** Shoppers looking for new or reproduction goods learn quickly that Sycapooke is not for them.

### Option 2: Neutral "second-hand" or "pre-loved" wording
- **Pros:** Sounds broader.
- **Cons:** Implies the sell form accepts anything used; sellers hit rejections on the 20-year rule.

### Option 3: Carry the landing page's advertising placements into the product
- **Pros:** A revenue line.
- **Cons:** No requirement, design or seller agreement for it; it would dilute "listed by a real person". Deferred.

## Consequences

### Positive
- The page is accurate against `L2-011` to `L2-013` and `L2-078`.
- The tone matches the founder's voice and the audience.

### Negative
- None for the product; the copy is narrower than a general vintage-and-antiques marketplace.

### Risks
- The copy names jewelry, clothing, housewares and electronics. The accepted categories live in
  `L2-011`; a change to them must re-check the page copy, the hero eyebrow and the meta
  description. Mitigation: the copy names kinds of things and the 20-year rule, never a count of
  subcategories.

## References

- [marketing-page design](../../detailed-designs/platform/marketing-page/README.md)
- [ADR-0006](0006-vintage-only.md)
- `docs/specs/L2.md` (`L2-011` to `L2-013`, `L2-078`, `L2-079`, `L2-083`)
