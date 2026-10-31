# ADR-0018: Founder identity binding and shop contracts

**Date:** 2026-10-31
**Category:** platform
**Status:** Accepted
**Deciders:** Quinntyne Brown (product owner)

## Context

[ADR general/0010](../general/0010-founder-only-shop.md) replaces public selling with one
founder-owned shop. Verification currently permits selling, public queries resolve every
open member, and administrator authorization controls order support. The revised requirements
require a trusted founder identity, private customers and founder-handled support.

## Decision

### Single trusted founder identity

`Shop:FounderMemberId` holds one immutable member ID. The proposed `ShopOptions`,
`IFounderAccess`, `FounderAccessRequirement` and `FounderProvisioner` implement the boundary.
A trusted operator verifies the existing founder account, then invokes the proposed
`Sycapooke.Migrator --provision-founder` operation with that configuration. Normal migration
and deployment execution does not provision privileges. The developer-only demo CLI never
provisions production.

Provisioning transactionally inserts one `ShopIdentity` row with fixed primary key `Id = 1`
and the founder member ID. Repeating the same binding is a no-op. A second identity is refused.
Protected access requires agreement between configuration and the stored binding, an open
account and verified email. Changing email, display name, Google claims or request fields
does not change identity. Missing or invalid configuration grants no founder access.

Configuration shape is checked at startup. Database-backed identity checks run on protected
operations, so the dependency-free health endpoint remains available without a database.
API, worker and migrator share the binding; controlled restart applies configuration changes.
Operator identity-recovery details remain `<TO SUPPLY>` rather than introducing a public
promotion or founder-replacement flow.

### Public and private contracts

`CurrentMemberDto` and frontend `CurrentMember` gain an additive server-derived `isFounder`
boolean. All session-producing responses use the same derivation. Frontend guards and menus
consume the flag; API policies and application handlers independently check `IFounderAccess`.
The existing internal `CanSell` name remains but no longer means every verified member.
An inherited administrator role grants no shop-management privilege.

Existing public founder URLs and member-read endpoint paths remain, restricted to the configured
founder. Customer, closed and unknown profile requests share `404 MEMBER_UNAVAILABLE`.
New private-account, customer-closure and founder-story contracts are specified in their designs.
Public catalog queries exclude existing non-founder inventory without reassigning its ownership.
Historical orders and financial records are not reassigned to the founder.

Founder support uses `/shop/problems/:reference` and `/api/shop/problems/{reference}`.
`ShopProblemsController` and `ShopProblemsApi` replace administrator-facing support contracts.
The old administrator routes have no compatibility alias that preserves their previous grants.
Resolution outcomes are `RefundBuyer` and `CloseWithoutRefund`, with an explanation.
Historical `ReleaseToSeller` results project as `CloseWithoutRefund`; new requests use the target values.
`PublishListingResult.PayoutOnboardingRequired`, `SellingOptionsDto.FeePercent` and `SaleDto.Payout`
are removed from the target contracts. Purchase and sale details expose pending versus issued refunds.

### Direct payment and refund completion

The configured merchant account receives payment through `IPaymentProvider`. No connected-seller
onboarding, marketplace fee or completion-based transfer participates. Signed webhook confirmation
and existing idempotency rules remain. A pending full refund retains its original-payment
reference and stable refund ID; a retry job reuses that ID. A support refund closes the problem
and applies Refunded only after provider confirmation. A temporary failure preserves the
completion hold. Historical financial data is retained for separate migration planning.

## Options Considered

1. **One configured and persisted identity (chosen):** prevents public promotion and detects
   an accidental second founder binding across restarts.
2. **Verified membership or administrator role:** retains public-selling or independent-arbiter
   semantics that conflict with the revised product.
3. **Email-based founder matching:** ties privilege to a mutable contact field and external
   identity claims rather than a trusted account identity.

## Consequences

The target implementation adds the singleton identity migration and the guarded provisioning
operation. Public signup remains private buyer signup. Existing purchase and inventory contracts
are reused where their semantics remain applicable; founder checks change their authorization.

This ADR and the detailed designs do not change application code, deploy configuration, media
or tests. Existing interfaces and data require a later implementation and compatibility review.
The application does not yet satisfy these contracts solely because the designs describe them.

## References

- [Founder access design](../../detailed-designs/accounts/manage-founder-access/README.md)
- [Private customer account design](../../detailed-designs/accounts/manage-private-account/README.md)
- [Customer closure design](../../detailed-designs/accounts/close-customer-account/README.md)
- [Founder support design](../../detailed-designs/orders/resolve-order-problems/README.md)
- [Direct payment design](../../detailed-designs/orders/checkout-and-pay/README.md)
