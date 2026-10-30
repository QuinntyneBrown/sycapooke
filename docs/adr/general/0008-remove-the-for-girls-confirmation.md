# ADR-0008: Remove the Seller Confirmation Checkbox

**Date:** 2026-10-26 (reworded 2026-10-30 for Sycapooke; the file keeps its original name)
**Category:** general
**Status:** Accepted
**Deciders:** Quinntyne Brown (product owner)

## Context

The marketplace Sycapooke was cloned from ([ADR-0009](0009-cloned-from-tutumuch.md)) kept a
seller confirmation checkbox on the sell and edit listing form that attested the item matched
the catalogue's lock (requirement `L2-014`). A seller had to tick it to publish, the API refused
a publish without it (`SIZE_CONFIRMATION_REQUIRED`) and the listing stored the time as
`SizeConfirmedAt`. In practice it was one more checkbox that every seller ticked. The lock
itself (`L2-013`: the size is always the one accepted value, set by the server) already says
what every listing is. For Sycapooke that value is `Vintage` ([ADR-0006](0006-vintage-only.md)),
and the 20-year rule is learned from the marketplace rules page, not from a checkbox.

## Decision

The confirmation stays removed everywhere: no checkbox on the sell and edit listing form, no
`sizeConfirmed` field in the publish and update requests, no `SIZE_CONFIRMATION_REQUIRED`
violation, no `Listing.SizeConfirmedAt` property or column (dropped by the
`RemoveSizeConfirmation` migration). `POST /api/listings/{reference}/publish` takes no body.
`L2-014` is retired and its number is never reused.

This supersedes only the confirmation part of ADR-0006. Everything else there stands: the four
categories, the vintage lock, the `Vintage` size value and `CK_Listings_Size`. Every listing is
still vintage; the marketplace rules page and the copy rule of
[ADR-0005](0005-marketing-wording.md) remain the way sellers learn that.

## Options Considered

### Option 1: No confirmation (chosen)
- **Pros:** A shorter form and publish path; no field, flag, timestamp or violation to maintain.
- **Cons:** No per-listing record that the seller attested the item is at least 20 years old.

### Option 2: Reinstate it as "This piece is at least 20 years old"
- **Pros:** An explicit attestation per listing.
- **Cons:** Friction for every seller with little protection; the vintage rule is the rules page's job.

## Consequences

- Clients that still send `sizeConfirmed` are unaffected: unknown members are ignored.
- Specs, designs, mocks, estimates and tests carry no confirmation step.

## References

- [ADR-0006](0006-vintage-only.md) (confirmation part superseded)
- `docs/specs/L2.md` (`L2-014` retired)
