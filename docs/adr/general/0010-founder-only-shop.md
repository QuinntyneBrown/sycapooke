# ADR-0010: Founder-only antiques and vintage shop

**Date:** 2026-10-31
**Category:** general
**Status:** Accepted
**Deciders:** Quinntyne Brown (product owner)

## Context

Sycapooke inherited a public marketplace model from TuTuMuch. The product owner clarified
that Sycapooke exists for its founder, one older Caribbean woman, to sell her own antiques.
Customers are buyers, not prospective sellers. The owner confirmed private buyer accounts,
the existing 20-year minimum, direct shop payments and founder-handled customer support.

## Decision

- The founder is the sole seller. A trusted operator provisions her management access;
  public registration, email verification, Google sign-in and account updates never grant it.
- Customers retain private accounts, purchase history and account-erasure rights. They have
  no public profiles, inventory or sales pages. Only the founder has a public story and collection.
- Keep jewelry, clothing, housewares and electronics, all at least 20 years old. "Antiques
  and vintage treasures" does not introduce a 100-year minimum.
- Customer payments go through the shop's payment provider to the founder's shop account.
  There is no platform commission, seller onboarding or completion-based payout release.
  Provider settlement timing remains the provider's responsibility.
- The founder handles shipping, cancellations, customer problems and full refunds through
  protected management access. There is no separate marketplace arbitrator or administrator
  role for resolving these orders. Existing order windows and safeguards remain.
- Public marketing invites shopping and introduces the founder; it does not recruit sellers.
- Order journeys by customer and founder value, independently of implementation dependencies.

This supersedes the public-selling and mandatory journey/rule parity portions of
[ADR-0009](0009-cloned-from-tutumuch.md), the seller-recruitment wording of
[ADR-0005](0005-marketing-wording.md), and the separate-administrator and payout-hold portions
of [ADR orders/0007](../orders/0007-order-problems-administrator-flag-private-photos-and-two-outcomes.md).
The vintage rule of [ADR-0006](0006-vintage-only.md), Heirloom design system and existing
deferred-feature decisions remain in force.

## Options Considered

1. **Founder-only shop with private buyer accounts (chosen):** fits the owner's business,
   preserves secure purchase tracking and removes marketplace onboarding and settlement logic.
2. **Public marketplace:** conflicts with the sole-seller purpose and adds unnecessary operations.
3. **Guest checkout:** removes account creation but changes order access and recovery; the owner
   chose to retain private buyer accounts.

## Consequences

The requirements and journeys now describe the target shop. Existing requirement IDs are
preserved; `L2-031` (seller payouts) is retired and must never be reused. Changed journeys
require fresh effort estimates rather than retaining the previous overall total.

This change updates the specs, journeys and the conflicting agent instructions only.
Application code, tests, mocks, designs, estimates, implementation prompts and existing media
still need separate alignment work; their current behavior is not evidence of compliance
with these revised requirements. Historical ADRs remain available as decision history.

## References

- [High-level requirements](../../specs/L1.md)
- [Detailed requirements](../../specs/L2.md)
- [User journeys](../../journeys.md)
