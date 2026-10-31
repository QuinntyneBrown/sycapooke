# Sycapooke detailed designs

Sycapooke is the shop of one older Caribbean woman, its founder and sole seller. Customers browse and buy antiques and vintage treasures through private accounts. They cannot sell or publish profiles. The four categories and 20-year minimum remain.

These 33 feature designs refine the current [L1](../specs/L1.md) and [L2](../specs/L2.md) requirements under [ADR general/0010](../adr/general/0010-founder-only-shop.md). Each feature has Overview, Description, Requirements and Diagrams sections. Requirements tables reproduce current L2 text and its owning L1. Feature navigation follows [journey value](../journeys.md), not implementation dependency order.

## Architecture baseline

| Boundary | Target responsibility |
| --- | --- |
| Sycapooke Web | Angular application shell and lazy feature libraries; customers see private buying navigation, the founder sees shop management |
| Sycapooke API | ASP.NET Core controllers, server authorization, application commands and domain rules |
| Sycapooke Worker | Durable email dispatch, reservations, shipment deadlines, completion, refund retries, draft cleanup and customer erasure |
| Sycapooke database | EF Core account, singleton shop identity, inventory, order, payment, refund and outbox records |
| Distributed cache | Shared session and rate-limit state; no process-local authoritative state |
| Photo storage | Public processed listing and founder images; private originals and authenticated support evidence |
| External ports | Hosted payment and original-payment refunds, email, Google identity and carrier events; tests substitute every port |

The application owns shells and routes. Feature libraries call HTTP only through `@sycapooke/api` and compose `@sycapooke/components`. Heirloom tokens own appearance. The `reviews` library retains its name but presents only the founder story and collection. The `notifications` design subsystem uses the existing accounts and API libraries; it does not add a frontend library.

## Shared design decisions

- **Founder access:** one `Shop:FounderMemberId` agrees with the persisted singleton `ShopIdentity`. A trusted operator provisions it through the proposed migrator operation. Public registration, verification, Google linking and account changes grant no management capability. The API and application handlers enforce the same boundary.
- **Navigation:** `CurrentMemberDto` and frontend `CurrentMember` gain server-derived `isFounder`. Navigation uses it; the server independently authorizes every request. Existing `CanSell` is only an internal compatibility policy name.
- **Privacy:** only the founder has a public story at `/members/{displayName}`. Customer and unknown profile requests share 404. Customers access their own orders; the founder sees shipping addresses only within the existing retention window.
- **Money:** signed provider callbacks confirm payment to the shop merchant account. Completion creates no financial transfer. Full refunds use the original payment and a persisted idempotency key. Temporary failures remain pending for retry.
- **Support:** the founder handles `/shop/problems/:reference` and `/api/shop/problems/{reference}`. Existing administrator-facing support contracts are replaced. Open problems pause completion; provider settlement continues independently.
- **Errors and concurrency:** protected anonymous requests return 401; customer attempts at founder actions return 403; another customer's private resource returns indistinguishable 404. Existing problem details, version conflicts and idempotency contracts remain.
- **Verification:** backend tests use in-memory substitutes and fake ports. Browser tests use the built Angular application with intercepted APIs. No test starts a backend, real database or other external service. Statelessness is checked by architecture assertions only.

The detailed access decision is recorded in [ADR platform/0018](../adr/platform/0018-founder-access-and-shop-contracts.md). Concrete provider selections remain `<TO SUPPLY>` where existing decisions defer them.

## Subsystems

| Subsystem | Responsibility | Features |
| --- | --- | --- |
| [catalog](catalog/README.md) | Public browsing, search and treasure detail for the founder collection. | 3 |
| [orders](orders/README.md) | Private purchases, direct shop payments, founder fulfilment and customer support. | 6 |
| [listings](listings/README.md) | Founder-only drafts, photographs, publication and inventory changes. | 4 |
| [notifications](notifications/README.md) | Account, customer order and founder shop messages with optional marketing. | 2 |
| [reviews](reviews/README.md) | The founder public story, collection and empty review summary. | 1 |
| [platform](platform/README.md) | Responsive accessibility, security, operations, static marketing and local developer tooling. | 10 |
| [accounts](accounts/README.md) | Private buyer accounts, authentication and trusted founder access. | 5 |
| [privacy](privacy/README.md) | Private account and address protection, erasure boundaries and cookie choice. | 2 |

## Implementation boundary

These documents define the target design. Existing class names identify reusable code; proposed additions and contract changes are called out in each Description. Current code, mocks, screen composition, media and tests still require alignment before compliance can be claimed. The [estimate tables](../estimates/README.md) separate implemented behavior with test evidence from remaining target work. Existing deferred scope remains unchanged.
