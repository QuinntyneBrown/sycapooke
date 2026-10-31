# User Journeys

Sycapooke is the founder's shop: one older Caribbean woman sells her own antiques and vintage treasures, every piece at least 20 years old. Customers browse and buy through private accounts. They cannot sell or create public profiles. Payments go directly to the founder's shop account, and she handles shipping and customer support ([ADR general/0010](adr/general/0010-founder-only-shop.md)).

Ordered by value added to customers and the founder, highest first, rather than implementation dependencies. Workspace setup and secure founder access are prerequisites even though their direct business value ranks below shopping and fulfilment. Requirement IDs are stable and are not priority numbers.

Existing effort figures are retained only for unchanged journeys. Changed and new journeys need re-estimation; there is no overall total while those estimates are incomplete. These figures do not update `docs/estimates/` or imply that the existing implementation meets the revised requirements.

| Priority | Journey | Audience | Outcome | Requirements | Effort (days) |
| --- | --- | --- | --- | --- | --- |
| 1 | Browse the founder's collection | Visitor or customer | Discover the founder's available treasures in the four categories without signing in | L2-022 | 3 |
| 2 | View a treasure's details, photos and story | Visitor or customer | Understand condition, price, shipping and the piece's story, with a link to the founder's collection | L2-025, L2-026 | Needs re-estimation |
| 3 | Checkout and pay | Verified customer | Securely purchase one treasure from the founder, with payment to her shop and protection against duplicate sales | L2-027 to L2-030 | Needs re-estimation |
| 4 | Founder creates and publishes a listing | Founder | Save a draft and publish an eligible treasure with its story, condition, price and shipping cost | L2-003, L2-011 to L2-013, L2-015, L2-016, L2-018, L2-019 | Needs re-estimation |
| 5 | Founder uploads and orders listing photos | Founder | Add clear photos and choose the cover so customers can assess the treasure | L2-017 | Needs re-estimation |
| 6 | Founder ships an order | Founder | See the delivery address, add tracking and keep the customer informed | L2-032, L2-033 | Needs re-estimation |
| 7 | Search and filter the collection | Visitor or customer | Find treasures by keyword, category, brand, condition and price | L2-023, L2-024 | 5 |
| 8 | Founder manages inventory | Founder | See drafts, available, reserved and sold treasures in one protected place | L2-021 | Needs re-estimation |
| 9 | Founder edits or withdraws a listing | Founder | Keep her inventory accurate while protecting reserved and sold treasures | L2-020 | Needs re-estimation |
| 10 | Track purchases and manage sales | Customer or founder | Customers privately track their purchases; the founder manages the shop's sales | L2-037 | Needs re-estimation |
| 11 | Receive account and order emails | Customer or founder | Receive the account, purchase, sale and support updates relevant to each party | L2-042 | Needs re-estimation |
| 12 | Resolve an order problem with the founder | Customer and founder | The customer reports a problem and the founder handles support, issuing a full refund when appropriate | L2-036 | Needs re-estimation |
| 13 | Cancel an order and receive a refund | Customer and founder | Request or approve cancellation before shipping, with a full refund on cancellation | L2-035 | Needs re-estimation |
| 14 | Confirm receipt and complete an order | Customer | Confirm safe arrival, or let the order complete after its delivery window if no problem remains | L2-034 | 3 |
| 15 | Learn about the founder and her collection | Visitor or customer | Meet the older Caribbean woman behind the shop, learn her story and explore her treasures; the rating summary says "No reviews yet" | L2-040, L2-041, L2-078, L2-079, L2-083, L2-089 to L2-091 | Needs re-estimation |
| 16 | Create, verify and manage a private buyer account | Customer | Join to buy and manage private account details without receiving selling rights or a public profile | L2-001, L2-002, L2-004 | Needs re-estimation |
| 17 | Sign in, recover access and sign out | Customer or founder | Securely access the account, recover a forgotten password and revoke sessions | L2-006 to L2-009 | Needs re-estimation |
| 18 | Founder securely accesses shop management | Founder and trusted operator | The operator provisions the single founder account; only she accesses inventory, fulfilment and support management | L2-003, L2-010 | Needs re-estimation |
| 19 | Manage notification preferences | Customer or founder | Opt in to shop marketing and unsubscribe while retaining essential account and order messages | L2-043 | Needs re-estimation |
| 20 | Manage cookie consent | Visitor or customer | Accept or reject optional cookies and change the choice later | L2-056 | 3 |
| 21 | Close a customer account | Customer | Close an eligible private account, revoke access and erase personal data under the retention rules | L2-005 | Needs re-estimation |
| 22 | Sign in with Google | Customer or founder | Join or sign in securely without changing buyer or founder permissions | L2-088 | Needs re-estimation |
| 23 | Set up the developer workspace | Developer | Clone the repository and run the single verification command to build and test the backend and frontend | L2-071 | 3 |
