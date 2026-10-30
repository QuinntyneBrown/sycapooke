# ADR-0006: Vintage Only: Four Categories and the 20-Year Rule

**Date:** 2026-10-26 (rewritten 2026-10-30 for Sycapooke)
**Category:** general
**Status:** Accepted (the seller confirmation is superseded by [ADR-0008](0008-remove-the-for-girls-confirmation.md))
**Deciders:** Quinntyne Brown (product owner)

## Context

Sycapooke began as a clone of TuTuMuch ([ADR-0009](0009-cloned-from-tutumuch.md)), which carried
one accepted size value per category, enforced by the API and the `CK_Listings_Size` check
constraint (`L1-003`, `L2-011` to `L2-013`). That lock was an *audience* lock: a fixed value
that says what every listing is, set by the server from the category, with no size input on the
sell form. Sycapooke is a marketplace made only for vintage: every piece is at least 20 years
old and listed by a real person with a real story. It needs the same kind of lock, but for a
different promise, and a catalogue of vintage categories instead of children's goods.

## Decision

Sycapooke accepts **vintage jewelry, clothing, housewares and electronics**, every piece at
least 20 years old. The catalogue (`CategoryCatalog`, still code, not configuration) is four
categories with 24 subcategories:

| Category (route) | Subcategories |
|---|---|
| Jewelry (`/jewelry`) | Necklaces, Brooches and pins, Earrings, Bracelets, Rings, Watches |
| Clothing (`/clothing`) | Dresses, Coats and jackets, Knitwear, Shirts and blouses, Skirts and trousers, Hats and accessories |
| Housewares (`/housewares`) | Tableware, Glassware, Ceramics and pottery, Lighting, Linens, Decor |
| Electronics (`/electronics`) | Radios, Cameras, Audio, Telephones, Clocks, Typewriters and office |

The size lock keeps its mechanism and becomes a **vintage lock**: `CategoryCatalog.Vintage`
(`Vintage`, label "Vintage") is the one accepted value for every category, the server sets it
from the category whatever the client sends, the sell form shows "Size: Vintage" as read-only
text with no size input, every card and detail page keeps its size tag (now reading "Vintage"),
and `CK_Listings_Size` keeps its shape with the new value. The 20-year rule is a listing rule the
seller agrees to on the marketplace rules page and shows in the listing itself ("c.1960s" in the
title or description); it is not a separate stored attribute. Suggested brands are vintage
makers (Trifari, Coro, Monet, Pendleton, Levi's, Pyrex, Fire-King, Lenox, Wedgwood, Fiesta,
Zenith, Kodak, Polaroid, Sony, Seiko). Category and subcategory slugs in URLs are the
lower-cased names with spaces turned into hyphens (`brooches-and-pins`, `typewriters-and-office`).

## Options Considered

### Option 1: Four categories under one vintage lock (chosen)
- **Pros:** Matches the marketing promise; no schema change beyond the lock's value; the lock,
  its tests and the sell form keep their shape; every listing is plainly vintage.
- **Cons:** "Vintage" tells a shopper nothing about fit or era; a decade or a garment size waits
  for a later attribute.

### Option 2: Store the year or decade as a validated attribute now
- **Pros:** The 20-year rule becomes machine-checked and filterable.
- **Cons:** A new attribute, a migration, form and filter work, and sellers often know only
  "my mother's, 1960s". Too much for this change; deferred.

### Option 3: Keep a general second-hand catalogue
- **Pros:** No rule to enforce.
- **Cons:** The marketing page promises "only vintage"; the catalogue would not.

## Consequences

### Positive
- The catalogue, sell form, seed data, mocks and marketing copy agree.
- No new persisted shape; the one accepted value changes from the inherited value to `Vintage`.

### Negative
- Seed data, seed photos, mocks, design-system examples and tests all change.

### Risks
- Sellers may list reproductions or recent items. Mitigation: the rules page, the listing
  convention ("c.1950s") and the copy rule of [ADR-0005](0005-marketing-wording.md).

## Open questions

- Does a decade or year attribute (and a filter) come later? **Deferred**, not built here.
- Do garment sizes return for Clothing as a stored attribute? **Deferred**; the title carries it
  ("Camel wool overcoat, size M").

## References

- `docs/specs/L1.md` (`L1-003`), `docs/specs/L2.md` (`L2-011` to `L2-013`, `L2-022` to `L2-024`,
  `L2-075`)
- `src/backend/Sycapooke.Domain/Listings/CategoryCatalog.cs`
- [create-listing design](../../detailed-designs/listings/create-listing/README.md)
- [ADR-0009](0009-cloned-from-tutumuch.md)
