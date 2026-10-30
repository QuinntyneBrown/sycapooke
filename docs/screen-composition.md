# Screen composition

Sources: `docs/mocks/` (every page, dialog and notification in every state, see
[`docs/mocks/index.html`](mocks/index.html)) and the component catalogue in
[`docs/design-system/README.md`](design-system/README.md), which the `components` library
(`src/frontend/projects/components/`, alias `@sycapooke/components`) implements one to one.

"Library" names the `sy-*` components, directives and services of `@sycapooke/components` that a
screen is composed of. "Local UI" is markup a feature library builds itself because no
catalogue component covers it; it is listed here so that catalogue change control can pick
it up. Mock-only chrome (`mock-bar`, `mock-note`, the theme toggle) is excluded.

The catalogue holds the shell components (skip link, top bar, footer, toast, sheet, button,
link, form field) and the components create-listing added (alert, center state, choice,
dialog with `DialogService`, earnings, error summary, fixed size, icon, input group, item row,
segmented control, size tag, spinner) and the components browse-catalog added (empty
state, listing card with listing grid, load more) and the components search-and-filter-catalog
added (chip, `sy-sheet` with `SheetService`, the `search` icon) and the components
manage-notification-preferences added (switch, and `ToastService` behind `sy-toast-region`) and
the components manage-cookie-consent added (consent banner, the hint of the switch) and the components the
marketing page added (marketing hero, feature band, steps, quote) and the
components sign-in-with-google added (provider button with its "or" divider). Feature screens not built yet are inventoried below
with their states and marked **not yet composed**; each is composed when its journey is
implemented, and the catalogue grows in the same change.

## Shell

| Part | Library components | Local UI |
|---|---|---|
| App shell (`shell: 'app'`) | `sy-skip-link`, `sy-top-bar` (guest, member; four category links inline from 1280px, menu button and nav-menu sheet below), `sy-footer` (its `cookieSettings` opens `CookieSettingsDialogService`), `sy-toast-region`, `@sycapooke/privacy` `sy-cookie-consent` in the root component | `<main id="main" tabindex="-1">` with the router outlet, in `projects/sycapooke` |
| Auth shell (`shell: 'auth'`) | `sy-skip-link`, `sy-logo` (lg), `sy-toast-region` | `.auth-layout` with `<main id="main" class="auth-card" tabindex="-1">` and the router outlet, in `projects/sycapooke`; chosen by route `data.shell = 'auth'` |
| Admin shell (`shell: 'admin'`) | `sy-skip-link`, `sy-top-bar` (`variant="admin"`: logo, "Admin" label, the member's initials), `sy-toast-region` | `<main id="main" class="container page" tabindex="-1">` with the router outlet and no footer, in `projects/sycapooke`; chosen by route `data.shell = 'admin'` |
| Marketing shell (`shell: 'marketing'`) | `sy-skip-link`, `sy-top-bar` (guest, member), `sy-footer` (with "About Sycapooke" to `/welcome`), `sy-toast-region` | `<main id="main" class="marketing" tabindex="-1">` with the router outlet and no container, in `projects/sycapooke`; chosen by route `data.shell = 'marketing'` ([ADR platform/0011](adr/platform/0011-marketing-feature-library.md)) |
| Bare shell (`shell: 'bare'`) | `sy-skip-link`, `sy-top-bar` (bare variant), `sy-toast-region` | Not yet composed |
| Nav-menu sheet (`dialogs/nav-menu`: default, guest) | `sy-sheet`, `sy-button`, `sy-link` | The four category links (Jewelry, Clothing, Housewares, Electronics) at `/jewelry`, `/clothing`, `/housewares`, `/electronics`; the top bar shows them inline only from 1280px (`--layout-breakpoint-xl`) and the menu button opens this sheet below that. Composed as `sy-nav-menu` in `@sycapooke/components` (opened by `sy-top-bar` through `SheetService`) with the four category links; the member links, Sign out and the row chevrons of the mock are not composed yet |
| Toast region (`notifications/toast`: info, success, warning, danger, with-action, stacked) | `sy-toast-region`, `sy-toast`, `ToastService` | None |
| System banner (`notifications/system-banner`: info, success, warning, danger) | `sy-system-banner` (not in the catalogue yet) | Not yet composed |

## Shared local components

| Item | Used by | Notes |
|---|---|---|
| Logo (inline SVG) | top bar, footer, sheet, auth shell | Lives in `@sycapooke/components` as `sy-logo`; sizes sm, md, lg |

## Pages

### Account — `/account` (`pages/account`: default, loading, empty, error, unverified)

- Library: not yet composed
- Local: not yet composed

### Edit profile — `/account/profile` (`pages/edit-profile`: default, invalid, submitting, success, loading, error)

- Library: not yet composed
- Local: not yet composed

### Forgot password — `/forgot-password` (`pages/forgot-password`: default, invalid, submitting, success, error)

- Library: not yet composed
- Local: not yet composed

### Email settings — `/account/emails` (`pages/notification-settings`: default, submitting, success, loading, error)

App shell. `@sycapooke/accounts` `NotificationSettingsPage` (`src/lib/notification-settings/`), built by
manage-notification-preferences. The account sub-navigation of the mocks is composed with the
account overview journey.

- Library: `sy-switch` (4 rows: "Account security emails, always on" and "Order emails, always
  on" checked and disabled; "Review emails" and "Marketing emails", disabled while saving),
  `syButton` (primary "Save" with `busy`; primary "Try again"), `sy-alert` (success "Email
  settings saved", `live="polite"`), `sy-empty-state` (error tone "Couldn’t load email settings"
  with the reference), `sy-icon` (`refresh`), `ToastService` (danger "Something went wrong" with
  the reference and "Try again" when a save fails; warning when the settings changed elsewhere)
- Local: the page header with its subtitle, the `.settings-list` rows with title and
  description, the `.form-actions` row, and the loading skeleton rows

### Unsubscribe — `/emails/unsubscribe?token=…` (not mocked: working, unsubscribed, invalid, error)

Auth shell. `@sycapooke/accounts` `UnsubscribePage` (`src/lib/unsubscribe/`), built by
manage-notification-preferences; the mocks have no screen for it, so it follows the verify-email
layout.

- Library: `sy-spinner` (working), `syButton` ("Back to shop", "Manage your emails", "Try again"),
  `sy-icon` (`refresh`)
- Local: the `h1` and lead of each state and the `.stack` of actions

### Member profile — `/members/{displayName}` (`pages/profile`: default, own, empty, loading, error, unavailable, long)

Built by view-member-profile in `@sycapooke/reviews` (`src/lib/member-profile/`). The Reviews section
shows "No reviews yet" while review authoring is deferred; the own state shows no "Edit
profile" until edit-profile exists, and avatar, location and bio show only when a member has
them (ADR reviews/0001).

- Library: `sy-rating-summary` (header; "No reviews yet" with no reviews), `sy-stat` (Sales,
  Member since), `sy-listing-grid` (For sale cards; loading skeleton), `sy-load-more` (long:
  "Showing {n} of {total}", 24 per page), `sy-empty-state` (empty: `compact`, icon `tag`, "No
  items for sale"), `sy-alert` (danger "Couldn’t load more items"), `sy-center-state`
  (unavailable: code "404", "Back to shop"; error: reference, "Try again" with icon `refresh`
  and "Back to shop"), `syButton`, `sy-icon` (`map-pin`, `refresh`)
- Local: `.profile-head` with the `avatar--xl` initials or photo, `h1` and bio; the `.stats`
  row; the For sale and Reviews `.section-head` blocks; the header loading skeleton

### Join — `/join` (`pages/register`: default, invalid, submitting, success, error)

Auth shell. `@sycapooke/accounts` `JoinPage`. "Continue with Google" sits above the form when
`GET /api/auth/providers` says Google is configured ([ADR accounts/0001](adr/accounts/0001-sign-in-with-google.md), `L2-088`).

- Library: `sy-provider-button` (google, to `/api/auth/google/start`, busy while leaving) and `sy-or-divider`, `sy-form-field` (display name, email, password), `sy-choice` (age, terms), `sy-error-summary` (invalid), `sy-alert` (error with reference; success "Link sent" and warning "Too many requests" after Resend email), `sy-button` (Join Sycapooke primary lg block with busy; Browse while you wait; Resend email link), `sy-icon` (mail)
- Local: heading block with the lead line; the mail icon circle and "We sent a link to …" paragraph of the success state

### Finish joining with Google — `/join/google` (`pages/join-google`: loading, default, invalid, submitting, expired, error)

Auth shell. `@sycapooke/accounts` `GoogleJoinPage`. Reached from the Google callback with a pending
join held server-side for 10 minutes; the page reads the Google email from
`GET /api/auth/google/pending` and finishes with `POST /api/auth/google/finish` (`L2-088`).

- Library: `sy-form-field` (Google email read-only with help "From your Google account. It counts as verified."; display name), `sy-choice` (age, terms), `sy-error-summary` (invalid), `sy-alert` (error with reference), `sy-button` (Join Sycapooke primary lg block with busy; expired: Continue with Google again primary lg block and Back to sign in lg block)
- Local: heading block with the lead line; the form-shaped skeleton of the loading state; "Not you? Back to sign in" line

### Reset password — `/reset-password` (`pages/reset-password`: default, invalid, submitting, success, expired, error)

- Library: not yet composed
- Local: not yet composed

### Session expired — `/sign-in?expired=1` (`pages/session-expired`: default)

- Library: not yet composed
- Local: not yet composed

### Settings — `/account/settings` (`pages/settings`: default, loading, error)

- Library: not yet composed
- Local: not yet composed

### Sign in — `/sign-in` (`pages/sign-in`: default, return, invalid, submitting, locked, error, google-busy, google-cancelled, google-existing, google-failed)

Auth shell. `@sycapooke/accounts` `SignInPage`; `returnUrl` keeps the page the member came from.
When `returnUrl` is a listing (`/items/{id}`) the heading reads "Sign in to buy" with the line
"You’ll go straight back to the item." (state return). "Continue with Google" sits first, above an
"or" divider, when `GET /api/auth/providers` says Google is configured; the `google` query
parameter the callback sends back (`cancelled`, `existing`, `failed` with `ref`) picks the alert
(`L2-088`). The `locked` state and the "Forgot password?" link arrive with account lockout and
password reset.

- Library: `sy-provider-button` (google, to `/api/auth/google/start?returnUrl=…`, busy while leaving) and `sy-or-divider`, `sy-form-field` (email, password), `sy-alert` (invalid: "Email or password is incorrect"; error with reference; google-cancelled info "Google sign-in was cancelled" as status; google-existing warning "An account with this email already exists" with the lock icon; google-failed danger "Couldn’t sign in with Google" with reference), `sy-button` (Sign in primary lg block with busy)
- Local: heading block; "New to Sycapooke? Join" line

### Verify email — `/verify` (`pages/verify-email`: default, loading, expired, used, error)

Auth shell. `@sycapooke/accounts` `VerifyEmailPage` reads `token` from the query string.

- Library: `sy-spinner` (loading), `sy-button` (Start shopping, Sell an item, Send a new link, Sign in, Try again), `sy-alert` (Link sent, Too many requests), `sy-icon` (refresh)
- Local: heading block with the lead line; the reference line of the error state

### Catalog — `/` (`pages/catalog`: default, guest, category, search, filtered, loading, empty, no-results, error, offline)

Built in `@sycapooke/catalog` (`src/lib/catalog/`, `src/lib/filters/`) by browse-catalog (default,
guest, category at `/jewelry`, `/clothing`, `/housewares`, `/electronics`, loading, empty, error, offline)
and search-and-filter-catalog (search, filtered, no-results, the filter sidebar, the filters
toggle, the chips and the sort select). The URL holds the result set (`q`, `category`,
`subcategory`, `brand`, `condition`, `min`, `max`, `sort`).

- Library: `sy-listing-grid` (cards and the 8-card loading skeleton) with `sy-listing-card`,
  `sy-size-tag` (lg, category header), `sy-load-more`, `sy-empty-state` (empty: icon `tag`,
  "Sell an item" as `syButton="primary"`; no-results: icon `search`, "No items match", "Clear
  search and filters"; error: tone error, icon `refresh`, "Try again"), `sy-alert` (warning
  banner with icon `wifi-off` for offline; danger "Couldn’t load more items" with a
  `syButton="link"` Try again when a next page fails), `sy-chip` (one per active filter value),
  `syButton` (Filters toggle, "Clear all" link button), `sy-choice` and `sy-input-group` (inside
  the filter form), `SheetService` (opens the Filters sheet below 992px), `sy-icon`
- Local: the hero (`.hero`, h1 "Treasures with a *story.*" and lead "Jewelry, clothing, housewares and electronics, every piece at least 20 years old and listed by a real person."), the page header
  (category name with size tag; eyebrow "Search" with “{q}”; "Shop" for other filters), the
  `.catalog-layout` grid, the `.toolbar` with the polite item count and its text skeleton, the
  `.sort` select, `FilterForm` (`sy-filter-form`, fieldsets Category, subcategories, Brand with
  "All {n} brands", Condition, Price) and `FiltersSidebar` (`sy-filters-sidebar`,
  `<aside class="filters" aria-label="Filters">`). The offline banner renders at the top of
  `main`, not above the top bar as in the mock, until `sy-system-banner` exists. A failed next
  page shows an inline danger alert instead of the toast in the design, until `ToastService`
  exists (responsive-accessible-shell). The subcategory chips under a category header are not
  built; the sidebar's subcategory group covers the same filter.

### Listing detail — `/items/{id}` (`pages/listing-detail`: default, guest, new-seller, reserved, sold, own, loading, error, unavailable)

Built by view-listing-detail in `@sycapooke/catalog` (`src/lib/listing-detail/`) for every state. In
the own state edit-and-withdraw-listing adds "Withdraw", which opens the Withdraw listing dialog
(`src/lib/withdraw-listing/`, ADR listings/0003). Sellers show no city until members can edit
their profile.

- Library: `sy-gallery` (main photo track, dots, thumbnails), `sy-size-tag` (lg), `sy-buy-bar`
  (default and guest: "Buy now" as `syButton="primary"` lg; own: "Edit listing" with icon
  `edit` and secondary lg "Withdraw" through `DialogService`; absent for Reserved and Sold), `sy-alert` (reserved: warning, icon `clock`, "In
  someone’s checkout"), `sy-seller-summary` (rating or "New seller"), `sy-listing-grid` (sold:
  "More treasures"), `sy-center-state` (unavailable: code "404" and "Shop
  similar items"; error: reference, "Try again" with icon `refresh` and "Back to shop"),
  `syButton`, `sy-icon`
- Local: the breadcrumb, the info column (`.photo__stamp` Reserved or Sold, `.eyebrow` brand,
  `h1.detail__title`, `.detail__price` line, `.flaws`, description, `dl.facts` with the
  relative `time`), the two-column `.detail` layout and the loading skeleton

### Checkout — `/checkout/{id}` (`pages/checkout`: default, new-address, invalid, non-us, submitting, declined, expired, conflict, loading, error)

Built by checkout-and-pay in `@sycapooke/orders` (`src/lib/checkout/`) for every state; `{id}` is
the listing reference. A refused start for the seller's own, a sold or a missing listing goes
back to `/items/{id}`, and a visitor goes to sign in with checkout as the return URL.

- Library: `sy-reservation-timer` (under the title; its phase drives the banners),
  `sy-alert` (banner warning "Less than a minute left to pay…" and danger "Your reservation
  ended…" with "Start again", both icon `clock`; non-us, declined, total changed and payment
  failure alerts in the form), `sy-error-summary` (invalid), `sy-form-field` (Full name,
  Street address, Apartment, suite (optional), City, State, ZIP code with `inputmode="numeric"`,
  Country), `sy-order-summary`, `syButton` (primary lg block "Pay $102.34", or "Continue" before
  a total exists, busy while submitting and disabled at zero; ghost "Cancel and release item"),
  `sy-center-state` (conflict: icon `lock`; expired: icon `clock`; error: reference and "Try
  again" with icon `refresh`), `sy-icon`
- Local: `.page-header`, the `.split` layout, the `.checkout-step` sections, the saved-address
  radios (`.option`, `address.address`), `HostedCardFieldsComponent` (the "Card" field, the
  provider's hosted fields from `PAYMENT_FIELDS` in `@sycapooke/api` and the `.secure-note`), the
  "Confirming with your bank…" status line and the loading skeleton

### Order confirmed — `/orders/{id}/confirmed` (`pages/order-confirmation`: default, loading, error)

Built by checkout-and-pay in `@sycapooke/orders` (`src/lib/order-confirmation/`). The "Payment
confirmed" toast of `notifications/reservation-timer` success waits for `ToastService`
(responsive-accessible-shell); the page itself shows the amount and card.

- Library: `sy-item-row` ("Paid $102.34 · Visa ending 4242"), `syButton` ("View order",
  "Keep shopping"), `sy-center-state` (error: reference, "Try again" and "Go to purchases"),
  `sy-icon` (`check`, `refresh`)
- Local: the `.confirm` block (mark, `.eyebrow` order reference, heading), the
  `ol.next-steps` list and the loading center state with the large spinner

### Order (buyer) — `/orders/{id}` (`pages/order-detail`: default, paid, cancel-requested, cancel-declined, delivered, completed, cancelled, refunded, problem, loading, error)

Built by ship-order in `@sycapooke/orders` (`src/lib/order-detail/`) for paid ("Waiting to ship"),
default ("On its way"), delivered, cancelled, loading and error. complete-order adds "I received
it" (default and delivered), the automatic completion note (delivered) and the completed state.
cancel-order adds "Ask to cancel" (paid), cancel-requested, cancel-declined and the seller's note
on cancelled. resolve-order-problems adds "Report a problem" (default and delivered, while
`canReportProblem`), the problem state and the refunded state. The expected delivery date is not
built: the API has no expected date.

- Library: `sy-order-timeline`, `sy-tracking-link` (once shipped), `sy-alert` (info "Refunded in
  full" with the amount and card when cancelled), `sy-quote` (the seller's note, cited with the
  seller's name, when cancelled), `sy-order-summary` (no visible heading; labelled "Order
  summary"), `sy-center-state` (error, with `sy-button` "Try again" and "Back to purchases"),
  `sy-button` (primary "I received it" when Shipped or Delivered, opening Confirm receipt through
  `DialogService`; secondary "Ask to cancel" when Paid and never asked, opening Ask to cancel;
  ghost "Report a problem" beside "I received it", opening Report a problem; primary link "Shop
  similar items" to the catalog when cancelled; "Back to purchases" when refunded on a problem),
  `sy-alert` (warning, icon `flag`, "Problem open · {reason}" when a problem is open; success
  "Refunded in full" / "Our team reviewed the problem. {total} is on its way back to {card}." when
  refunded on a problem), `sy-thread` (the claim with photos, captioned "You", and the seller's
  reply)
- Local: the breadcrumb, eyebrow and heading block, the ship-by note ("{seller} ships by {date}.
  If not, you’re refunded automatically."), the request note (`.order__request`: "You asked
  {seller} to cancel. They haven’t answered yet." or "{seller} declined to cancel. They still ship
  by {date}."), the completion note ("Completes automatically on {date} unless you report a
  problem."), `OrderPartyComponent` (avatar, "Seller", profile link) projected into the summary,
  and the two-column loading skeleton. "Shop similar items" opens the catalog home, because the
  order does not carry the item's category.

### Purchases — `/account/purchases` (`pages/purchases`: default, loading, empty, error)

App shell. `@sycapooke/orders` `OrderHistoryPage` (`src/lib/order-history/`, route data `list: 'purchases'`),
built by order-history. The account sub-navigation of the mocks is composed with the account
overview journey.

- Library: `sy-item-row` (title, "from {seller} · {date}", thumbnail), `sy-status` (info; active
  when Completed; removed when Cancelled or Refunded), `sy-empty-state` (icon `bag`, "Nothing
  bought yet" with primary `sy-button` "Browse the catalog"; error tone, icon `refresh`, "Couldn’t
  load purchases" with the reference and primary "Try again" with icon `refresh`)
- Local: the page header with the counts subtitle, the `.order-list` rows linking to `/orders/{id}`
  with the price, and the three skeleton rows

### Sale (seller) — `/sales/{id}` (`pages/sale-detail`: default, shipped, cancel-requested, problem, completed, address-hidden, cancelled, loading, error)

Built by ship-order in `@sycapooke/orders` (`src/lib/sale-detail/`) for default (Paid), shipped,
cancelled, loading and error. cancel-order adds "Cancel order", cancel-requested and "Relist
item"; seller-payouts adds completed; resolve-order-problems adds problem; order-history adds
address-hidden (`sy-icon` `eye-off`, "Removed 30 days after the order ended.").

- Library: `sy-button` (primary "Add tracking" with icon `truck`, opening the Add tracking
  dialog through `DialogService`; ghost "Cancel order" beside it, opening Cancel order; primary
  "Respond" when cancel-requested, opening Respond to cancellation; primary "Relist item" when
  cancelled and the listing can be relisted, opening Relist item), `sy-quote` (the buyer's
  message, cited with the buyer's name, when cancel-requested), `sy-order-timeline` and
  `sy-tracking-link` (once shipped), `sy-alert` (info "Cancelled · Buyer refunded in full" with
  "The listing stays off the catalog until you relist it.", or "The listing is back in the
  catalog." once relisted), `sy-order-summary` (no visible heading), `sy-center-state` (error,
  with "Try again" and "Back to sales"), and for problem `sy-alert` (warning, icon `flag`,
  "Problem reported · {reason}" / "Your payout is on hold while our team reviews this."),
  `sy-thread`, `sy-form-field` (textarea "Your reply", help "Visible to the buyer and to Sycapooke.") and
  `sy-button` ("Send reply" with busy, "Try again" after a failure) until the seller has replied
- Local: the breadcrumb, eyebrow and heading block, "Ship by {date}." lead, the "Ship to"
  address block with its caption, `OrderPartyComponent` ("Buyer") projected into the summary, the
  loading skeleton, and (completed, by seller-payouts) the sunken "Payout" panel with its
  `.totals` list ("Item and shipping", "Sycapooke fee (10%)", "Your payout") and "Sent {date} to {bank}." caption

### Sales — `/account/sales` (`pages/sales`: default, loading, empty, error)

App shell. The same `OrderHistoryPage` with route data `list: 'sales'`, built by order-history.

- Library: `sy-item-row` ("to {buyer} · ship by {date} · {date}" while Paid, else "to {buyer} ·
  {date}"), `sy-status`, `sy-empty-state` (icon `tag`, "No sales yet" with primary "Sell an item";
  error tone "Couldn’t load sales" with the reference and "Try again")
- Local: the page header with "{n} to ship", the `.order-list` rows linking to `/sales/{id}`, and
  the skeleton rows

### Edit listing — `/items/{id}/edit` (`pages/edit-listing`: default, loading, error, invalid, submitting, success, locked, changed, sold, withdrawn)

App shell. `@sycapooke/listings` `EditListingPage` (`src/lib/edit-listing/`), built by
edit-and-withdraw-listing. The fields are the sell form's, shared through the local
`ListingFormComponent` (`src/lib/listing-form/`) (ADR listings/0003). A draft opens the sell form
at `/sell/{reference}` instead.

- Library: `sy-status` (the listing status beside the heading), `sy-error-summary`,
  `sy-photo-uploader`, `sy-form-field`, `sy-fixed-size`, `sy-segmented`, `sy-input-group`,
  `sy-earnings`, `sy-alert` (success "Changes saved"
  with "View listing"; warning "This listing changed since you opened it" with sm "Reload" and icon
  `refresh`; danger "Couldn’t save your changes" with the reference), `syButton` (ghost "Cancel"
  link, primary "Save changes" with busy), `sy-center-state` (error: icon `refresh`, "Couldn’t
  load this listing", reference, "Try again" and "Your listings"; locked: icon `clock`, "This item
  is in someone’s checkout", "Back to listing"; sold: icon `check`, "This item has sold", "Your
  sales" and "Your listings"; withdrawn: icon `trash`, "This listing was withdrawn", "Your
  listings"), `sy-icon`
- Local: the page header (listing title link, heading, status), `ListingFormComponent` (the
  photos, item, price and confirmation sections), the sticky split actions row and the loading
  skeleton (3 photo tiles, 4 fields)

### Your listings — `/account/listings` (`pages/my-listings`: default, filtered, loading, empty, error)

App shell. `@sycapooke/listings` `MyListingsPage` (`src/lib/my-listings/`), built by manage-my-listings.
The status is kept in the URL query (`?status=sold`). The payout banner stays in the app shell
(ADR orders/0005, ADR listings/0002). The account sub-navigation of the mocks is composed with
the account overview journey.

- Library: `ToastService` (info "Drafts older than 90 days are deleted", "We’ll email you a
  week before.", once per visit for a seller with drafts), `sy-button` (primary "Sell an item"
  with icon `plus`), `sy-filter-chips` ("Filter by
  status": All, Draft, Active, Reserved, Sold, Removed with counts), `sy-item-row` (title or
  "Untitled draft · {category}", thumbnail, with `sy-status` projected under the title in the
  tone of the listing status), `sy-icon` (`image` placeholder for a listing without photo),
  `sy-empty-state` (icon `tag`, "Nothing listed yet" with primary "Sell an item"; error tone, icon
  `refresh`, "Couldn’t load your listings" with the reference and primary "Try again")
- Local: the page header with "{n} listings · {n} active", the `.order-list` rows linking to
  `/sell/{reference}` (Draft), `/sales/{orderId}` (Sold) or `/items/{reference}` with the price
  ("$60" or "—") and the date ("Today", "Yesterday", "Sat 24 Oct"), and the four skeleton rows

### Payouts — `/account/payouts` (`pages/payouts`: default, not-onboarded, loading, empty, error)

App shell. `@sycapooke/orders` `PayoutsPage` (`src/lib/payouts/`), built by seller-payouts. The account
sub-navigation of the mocks is composed with the account overview journey.

- Library: `sy-stat` ("Paid out", "Pending"), `sy-button` ("Change" and primary "Set up payouts",
  both with icon `external`; "Try again" with icon `refresh`; primary "Sell an item"), `sy-alert`
  (warning "Set up payouts to get paid" with the button projected as `actions`; danger for a
  provider failure), `sy-item-row` (each history row), `sy-empty-state` (empty with icon `tag`;
  error tone with the reference), `sy-icon`
- Local: the page header, the `.stats` row, the payout method `.settings-list` row, the
  `.section-head` ("History" or "Held"), the `.order-list` rows with their thumbnail, amount and
  status caption, and the loading skeleton

### Sell an item — `/sell` (`pages/sell`: default, filled, invalid, submitting, draft-saved, success, unverified, limited, error)

App shell. `@sycapooke/listings` `SellPage` at `/sell` and `/sell/{reference}` (continue a draft).
Visitors are sent to `/sign-in?returnUrl=/sell`. The Photos section comes first; each chosen file
uploads at once and a save sends the photo IDs in order. The footer stays visible under the form.

- Library: `sy-photo-uploader` (photos with cover, uploading and rejected tiles, add tile, help and error), `sy-error-summary` (invalid), `sy-alert` (draft-saved success, save or publish failure), `sy-form-field` (category, type, title with counter, brand, flaws, description with counter, price, shipping), `sy-input-group` (`$` addon), `sy-fixed-size` with `sy-size-tag` (after a category is chosen), `sy-segmented` (condition), `sy-earnings`, `sy-button` (Save draft, Publish with busy), `sy-center-state` (success, unverified, limited, error) with `sy-item-row` (success panel) and `sy-icon`, `DialogService` (opens the verify-email prompt)
- Local: `.page-header` with the h1, `.sell-form` sections (Photos, Item, Price, confirmation: `.form-section`, `.form-row`), the upload state of each tile and its client size check, the brand `<datalist>`, the sticky `.form-actions` bar, the success panel wrapper and the payouts line

### Forbidden — `/403` (`pages/forbidden`: default)

- Library: not yet composed
- Local: not yet composed

### Maintenance — `(any)` (`pages/maintenance`: default)

- Library: not yet composed
- Local: not yet composed

### Not found — `/*` (`pages/not-found`: default)

- Library: not yet composed
- Local: not yet composed

### Offline — `(any)` (`pages/offline`: default)

- Library: not yet composed
- Local: not yet composed

### Marketing page — `/welcome` (`pages/welcome`: default, signed-in)

Built in `@sycapooke/marketing` (`src/lib/welcome/`) by the marketing page. Calls no Sycapooke API
endpoint; the shell's `AuthState` decides the seller target. Title "Treasures with a story · Sycapooke"
(route `title`); the meta description ("The marketplace made only for vintage: jewelry, clothing,
housewares and electronics, every piece at least 20 years old and listed by a real person with a
real story.") and link-preview tags (`og:title` "Sycapooke · Treasures with a story", share image
`/media/sycapooke-share.jpg`) are static in `index.html`. The copy says everything is vintage,
every piece at least 20 years old, and never promises new or reproduction goods
([ADR general/0005](adr/general/0005-marketing-wording.md)).

- Library: `sy-marketing-hero` (on paper, no decoration: eyebrow "Jewelry · Clothing · Housewares · Electronics",
  h1 "A home for the things that *mattered.*", lede "Sycapooke is the marketplace made only for
  vintage — for families downsizing a lifetime of treasures, and the people who’ll love them
  next.", actions, media slot with `sy-video-poster`: the hero photo `/media/sycapooke-hero.jpg` as one
  button named "Play" with the "Play · 0:29" pill, opening the Pitch video dialog through `DialogService`),
  `sy-feature-band` (For shoppers "Every piece is at least 20 years old." on paper with the
  points Four focused categories, Listed by a real person and Honest condition, and "Browse the
  catalog"; For sellers "Downsizing? Let your treasures find their next home.", `tone="sell"`
  (terracotta, cream text, inverse buttons), with the points Big buttons, clear steps and A real
  person to call 1-800-555-0123, and "List your first item"; How it works "Three steps. That’s
  it.", `stacked`; Our story "sy·ca·pooke", `tone="story"` (cream under a line rule), `stacked`;
  the closing call to action "Treasures with a *story.*", lede "New treasures arrive daily, every
  one at least 20 years old.", `tone="sell"`, `stacked`, with the inverse "Shop the catalog"),
  `sy-steps` (List it, Sell it, Get paid), `sy-quote` (the founder's
  quote “It’s the made-up word I whispered to my son when he was a baby. When it came time to
  clear out our family home, I realized every teacup and brooch deserved that same word — and
  someone who’d see it too.”, attributed "Founder, age 70, grandmother of five", with the founder
  photo `/media/sycapooke-founder.jpg`), `syButton` (primary "Shop the catalog" to `/`; "Start
  selling" and "List your first item" to `/join`, or `/sell` for a member)
- Local: `WelcomePage` (copy, CTA targets from `AuthState.member()`, the two photographs), `PitchVideoDialog`
  (see Dialogs).

### Privacy — `/privacy` (`pages/privacy`: default)

- Library: not yet composed
- Local: not yet composed

### Problem detail — `/admin/problems/{id}` (`pages/problem-detail`: default, resolved, loading, error)

Built by resolve-order-problems in `@sycapooke/orders` (`src/lib/problem-detail/`), in the admin shell.

- Library: `sy-thread` (claim with photos captioned "{buyer} · buyer · {day}", reply captioned
  "{seller} · seller · {day}"), `sy-item-row` (the item in the side panel), `sy-facts` (Status as
  `sy-status`, Total, Delivered, Payout "Held", "Not paid" or "Released"), `sy-button` (primary
  "Resolve", opening Resolve problem through `DialogService`, while open), `sy-alert` (success
  "Refunded in full" / "{total} back to the buyer. No payout." or "Released to the seller" when
  resolved), `sy-empty-state` (error "Couldn’t load this problem" with the reference and "Try again")
- Local: the `.page-header` with the reason as heading and "Order problem {reference} · opened
  {day}", the sunken `aside` panel with its eyebrow, the resolution reason line, and the
  two-column loading skeleton

### Marketplace rules — `/rules` (`pages/rules`: default)

- Library: not yet composed
- Local: not yet composed

### Server error — `/error` (`pages/server-error`: default)

- Library: not yet composed
- Local: not yet composed

### Terms — `/terms` (`pages/terms`: default)

- Library: not yet composed
- Local: not yet composed


## Dialogs

| Dialog (mock folder: states) | Overlay surface | Library components | Local UI |
|---|---|---|---|
| Change password (`dialogs/change-password`: default, invalid, busy, failed) | `DialogService` | not yet composed | not yet composed |
| Close account (`dialogs/close-account`: default, invalid, busy, blocked) | `DialogService` | not yet composed | not yet composed |
| Sign out everywhere (`dialogs/sign-out-everywhere`: default, busy, failed) | `DialogService` | not yet composed | not yet composed |
| Verify email prompt (`dialogs/verify-email-prompt`: default, busy, sent, limited) | `DialogService` | `sy-dialog` (mail icon, description with the email), `sy-alert` (sent success, limited warning) in the top slot, `sy-button` (Not now, Resend link with busy, Done) | `VerifyEmailPromptComponent` in `@sycapooke/listings`; it moves to a shared place when checkout also opens it |
| Filters (`dialogs/filters`: default, invalid, busy) | `SheetService` | `sy-sheet` (`side`, heading "Filters", close "Close filters"), `sy-error-summary` (heading "Check the price range"), `sy-choice`, `sy-input-group`, `sy-button` (Clear; primary "Show {n} items" with `aria-busy`, disabled when invalid) | `FiltersSheet` and `FilterForm` in `@sycapooke/catalog` |
| Menu (`dialogs/nav-menu`: default, guest) | `SheetService` | `sy-sheet` (heading "Menu", close "Close menu"), the four category links on `.menu-sheet__links` | `sy-nav-menu` in `@sycapooke/components`, opened by `sy-top-bar`; member links, Sign out and row chevrons not composed yet |
| Add tracking (`dialogs/add-tracking`: default, invalid, busy, failed) | `DialogService` | `sy-dialog` (description "We’ll email {buyer} the tracking link."), `sy-form-field` (Carrier select, focused on open; Tracking number with the inline error), `sy-alert` (danger "That didn’t go through" in the top slot), `sy-button` (Cancel, disabled while busy; primary "Mark as shipped" with busy, "Try again" after a failure) | `AddTrackingDialog` in `@sycapooke/orders` |
| Cancel order (`dialogs/cancel-order`: default, invalid, busy, failed) | `DialogService` | `sy-dialog` (icon `alert`, `tone="danger"`, description "{buyer} is refunded {total} in full. The listing stays off the catalog until you relist."), `sy-radio-group` (legend "Reason", error "Choose a reason") with four `sy-choice` radios (first focused on open), `sy-form-field` (textarea "Note to buyer", optional), `sy-alert` (danger "That didn’t go through" in the top slot), `sy-button` ("Keep sale", disabled while busy; danger "Cancel sale" with busy, "Try again" after a failure) | `CancelOrderDialog` in `@sycapooke/orders` |
| Confirm receipt (`dialogs/confirm-receipt`: default, busy, failed) | `DialogService` | `sy-dialog` (`size="sm"`, description "This completes the order and pays {seller}. You can’t report a problem afterwards."), `sy-item-row` (item thumbnail, title, brand and size), `sy-alert` (danger "That didn’t go through" in the top slot), `sy-button` ("Not yet", disabled while busy; primary "Yes, complete order" with busy, "Try again" after a failure) | `ConfirmReceiptDialog` in `@sycapooke/orders` |
| Relist item (`dialogs/relist`: default, busy) | `DialogService` | `sy-dialog` (`size="sm"`, description "It goes back in the catalog with its original photos, price and description."), `sy-item-row` (thumbnail, title, "{price} · {shipping} shipping"), `sy-alert` (danger "That didn’t go through" in the top slot after a failure), `sy-button` ("Not now"; primary "Relist", focused, with busy) | `RelistDialog` in `@sycapooke/orders`, with "Edit first" (`syButton` link to `/items/{id}/edit`, closing the dialog) added by edit-and-withdraw-listing |
| Report a problem (`dialogs/report-problem`: default, invalid, busy, failed) | `DialogService` | `sy-dialog` (description "Open until {deadline}. We hold the seller’s payout while our team looks into it."), `sy-error-summary` (invalid, in the top slot), `sy-segmented` (legend "What’s wrong?": Not received, Not as described, Wrong size, Counterfeit), `sy-form-field` (textarea "Describe it"), `sy-photo-uploader` (`[cover]="false"`, max 4, "Add photo"), `sy-alert` (danger "That didn’t go through" / "Your photos and description are saved here. Try again."), `sy-button` (Cancel, disabled while busy; primary "Send to Sycapooke" with busy, "Try again" after a failure) | `ReportProblemDialog` in `@sycapooke/orders`; the "Photos (up to 4)" field label |
| Ask to cancel (`dialogs/request-cancellation`: default, busy, failed) | `DialogService` | `sy-dialog` (description "If {seller} accepts, you’re refunded in full. Until then the order stays as is."), `sy-form-field` (textarea "Message to {seller}", optional, focused on open), `sy-alert` (danger "That didn’t go through" in the top slot; the message is kept), `sy-button` ("Keep order", disabled while busy; primary "Send request" with busy, "Try again" after a failure) | `RequestCancellationDialog` in `@sycapooke/orders` |
| Respond to cancellation (`dialogs/respond-cancellation`: default, busy, failed) | `DialogService` | `sy-dialog` (heading "{buyer} asked to cancel", description "Accepting refunds {total} in full. Declining keeps the order — you still ship by {date}."), `sy-quote` (the buyer's message), `sy-alert` (danger "That didn’t go through" in the top slot), `sy-button` ("Decline"; primary "Accept and refund", focused; the chosen one shows busy and the other is disabled) | `RespondCancellationDialog` in `@sycapooke/orders` |
| Withdraw listing (`dialogs/withdraw-listing`: default, busy, failed) | `DialogService` | `sy-dialog` (`size="sm"`, icon `trash`, tone danger, title "Withdraw “{title}”?", description "It leaves the shop right away. This can’t be undone."), `sy-alert` (danger "Couldn’t withdraw" / "Your listing is still live." in the top slot after a failure), `sy-button` ("Keep listing", focused, disabled while busy; danger "Withdraw" with busy, "Try again" after a failure) | `WithdrawListingDialog` in `@sycapooke/catalog` (ADR listings/0003); on success the page goes to `/account/listings` |
| Cookie settings (`dialogs/cookie-settings`: default, analytics-on) | `DialogService` through `CookieSettingsDialogService` | `sy-dialog` (size `sm`, heading "Cookie settings"), `sy-switch` with `hint` "Strictly necessary" ("Sign-in and checkout. Always on.", checked and disabled) and "Analytics" ("Helps us see which pages are slow.", `autofocus`), `sy-button` (Cancel; primary Save) | `CookieSettingsDialog` in `@sycapooke/privacy` |
| Pitch video (`dialogs/pitch-video`: default, loading, error, captions) | `DialogService` | `sy-dialog` (size `lg`, heading "Sycapooke video" visually hidden), `sy-video-player` (ink 16:9 frame with the native `video`: MP4 then WebM sources at `/media/sycapooke-pitch.*`, English captions track, poster `/media/sycapooke-hero.jpg`; spinner overlay with "Loading video…" while loading; "The video didn’t load" / "Check your connection and try again." with Try again on error; `sy-button` primary Play/Pause and secondary Captions with `aria-pressed`, off by default) | `PitchVideoDialog` in `@sycapooke/marketing`; Escape closes, stops the video and returns focus to the poster |
| Resolve problem (`dialogs/resolve-problem`: default, invalid, busy) | `DialogService` | `sy-dialog` (heading "Resolve problem {reference}"), `sy-radio-group` (legend "Decision") with `sy-choice` "Refund buyer in full" ("{total} to {buyer} · no payout") and "Release payout to seller" ("Order completes · {seller} is paid"), `sy-form-field` (textarea "Reason", help "Sent to both members.", error "Add a reason. Both members see it."), `sy-alert` (danger after a failure), `sy-button` (Cancel, disabled while busy; primary "Refund buyer" or "Release payout", busy "Refunding" or "Releasing") | `ResolveProblemDialog` in `@sycapooke/orders` |

## Notifications

| Notification (mock folder: states) | Surface | Library components | Local UI |
|---|---|---|---|
| Account banner (`notifications/account-banner`: info, success, warning) | banner above the top bar | not yet composed | not yet composed |
| Reservation timer (`notifications/reservation-timer`: info, warning, danger, success) | timer under the checkout title; warning and danger banners at the top of checkout | `sy-reservation-timer`, `sy-alert` (banner, icon `clock`) | None; success is the confirmation page until `ToastService` exists |
| Payout banner (`notifications/payout-banner`: info, success, warning, danger) | banner above the top bar, `@sycapooke/orders` `PayoutBannerComponent` in the app shell | `sy-alert` (`banner`, links projected as `actions`; info and success `dismissible`) | None |
| Toasts (`notifications/toast`: info, success, warning, danger, with-action, stacked) | toast region | `sy-toast-region`, `sy-toast`, `ToastService` | None |
| Cookie consent (`notifications/cookie-consent`: info, success) | fixed bottom panel, `@sycapooke/privacy` `CookieConsentComponent` in the root component | `sy-consent-banner` (info); `ToastService` success toast "Cookie choice saved" (success) | None |
| System notices (`notifications/system-banner`: info, success, warning, danger) | banner above the top bar | not yet composed | not yet composed |

## Summary of local UI to build

1. `main#main` with the router outlet and the home placeholder (`projects/sycapooke`).
2. The auth layout of the auth shell (`projects/sycapooke`).
3. Heading blocks of the auth pages and the sell page header, form sections and sticky actions bar.
4. Everything marked "not yet composed" is composed, and its library components added to the catalogue, when its journey is implemented.
