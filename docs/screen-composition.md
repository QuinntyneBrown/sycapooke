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
dialog with `DialogService`, error summary, fixed size, icon, input group, item row,
segmented control, size tag, spinner) and the components browse-catalog added (empty
state, listing card with listing grid, load more) and the components search-and-filter-catalog
added (chip, `sy-sheet` with `SheetService`, the `search` icon) and the components
manage-notification-preferences added (switch, and `ToastService` behind `sy-toast-region`) and
the components manage-cookie-consent added (consent banner, the hint of the switch) and the components the
marketing page added (marketing hero, feature band, steps, quote) and the
components sign-in-with-google added (provider button with its "or" divider). Feature screens not built yet are inventoried below
with their states and marked **not yet composed**; each is composed when its journey is
implemented, and the catalogue grows in the same change.

Sycapooke is a founder-only shop ([ADR general/0010](adr/general/0010-founder-only-shop.md)):
one founder is the sole seller, customers have private buyer accounts, and there are no
commissions, payouts, fees or administrator role. So there is no admin shell, no payouts
page and no payout banner; the founder's pages (shop, inventory, sales, support, story) use the
app shell with a founder sub-navigation. The `sy-earnings` component is retired with seller
payouts and no screen uses it; `sy-founder-summary` is the founder summary (avatar, name, "No
reviews yet", completed sales). Entries marked **not yet built** are composed from existing
catalogue components but their journey is not implemented.

## Shell

| Part | Library components | Local UI |
|---|---|---|
| App shell (`shell: 'app'`) | `sy-skip-link`, `sy-top-bar` (guest: category links, search and Sign in only; customer: search and the initials avatar to `/account`; founder: search, an "Inventory" link, primary "Sell an item" and the initials avatar to `/shop`; four category links inline from 1280px, menu button and nav-menu sheet below; no Sell, Join or admin link for visitors or customers), `sy-footer` (Shop categories; About Sycapooke, Shop policies, Privacy, Terms, Cookie settings; its `cookieSettings` opens `CookieSettingsDialogService`), `sy-toast-region`, `@sycapooke/privacy` `sy-cookie-consent` in the root component | `<main id="main" tabindex="-1">` with the router outlet, in `projects/sycapooke`. The account and shop sub-navigation (`.subnav` of `.tab` links, `aria-current="page"`; no catalogue component yet): customer "Account" tabs Overview, Purchases, Emails, Settings; founder "Shop management" tabs Shop, Inventory, Sales, Support, Story, Emails, Settings. `sy-top-bar`, `sy-nav-menu` and `sy-footer` read `APP_ORIGIN` (`provideAppOrigin`, `externalHref`): empty in the application build, so their links are router links; `https://sycapooke.canadacentral.cloudapp.azure.com` in the marketing build, so the same links are absolute anchors into the application (ADR platform/0016). Render separate plain-anchor and RouterLink branches; never combine a disabled RouterLink with an external href |
| Auth shell (`shell: 'auth'`) | `sy-skip-link`, `sy-logo` (lg), `sy-toast-region` | `.auth-layout` with `<main id="main" class="auth-card" tabindex="-1">` and the router outlet, in `projects/sycapooke`; chosen by route `data.shell = 'auth'` |
| Marketing shell (`shell: 'marketing'`) | `sy-skip-link`, `sy-top-bar` (guest, customer), `sy-footer` (with "About Sycapooke" to `/welcome`), `sy-toast-region` | `<main id="main" class="marketing" tabindex="-1">` with the router outlet and no container, in `projects/sycapooke`; chosen by route `data.shell = 'marketing'` ([ADR platform/0011](adr/platform/0011-marketing-feature-library.md)). In the marketing build (`/welcome/` on the Azure host) `APP_ORIGIN` is `https://sycapooke.canadacentral.cloudapp.azure.com`, so the top bar's Sign in, the nav-menu category links, the footer links (except "About Sycapooke", which stays local) and the welcome page's calls to action are absolute links into the application, and the top-bar search goes to `https://sycapooke.canadacentral.cloudapp.azure.com/?q=…` ([ADR platform/0015](adr/platform/0015-separate-marketing-and-application-deployments.md)) |
| Bare shell (`shell: 'bare'`) | `sy-skip-link`, `sy-top-bar` (bare variant), `sy-toast-region` | Not yet composed |
| Nav-menu sheet (`dialogs/nav-menu`: default, guest, founder) | `sy-sheet`, `sy-button`, `sy-link` | The four category links (Jewelry, Clothing, Housewares, Electronics) at `/jewelry`, `/clothing`, `/housewares`, `/electronics`; the top bar shows them inline only from 1280px (`--layout-breakpoint-xl`) and the menu button opens this sheet below that. Below the categories the small links depend on who is signed in: customer (default) Your account, Purchases, Shop policies, Settings and Sign out; guest About Sycapooke, Shop policies and a primary Sign in; founder (state `founder`) Inventory, Sell an item, Sales, Support, Story, Shop and Sign out. Composed as `sy-nav-menu` in `@sycapooke/components` (opened by `sy-top-bar` through `SheetService`) with the four category links; the customer, guest and founder links, Sign out and the row chevrons of the mock are not composed yet |
| Toast region (`notifications/toast`: info, success, warning, danger, with-action, stacked) | `sy-toast-region`, `sy-toast`, `ToastService` | None |
| System banner (`notifications/system-banner`: info, success, warning, danger) | `sy-system-banner` (not in the catalogue yet) | Not yet composed |

## Shared local components

| Item | Used by | Notes |
|---|---|---|
| Logo (inline SVG) | top bar, footer, sheet, auth shell | Lives in `@sycapooke/components` as `sy-logo`; sizes sm, md, lg |

## Pages

### Account — `/account` (`pages/account`: default, loading, empty, error, unverified)

Customer overview: "Hi, {name}", "Customer since {date}. Only you see your account." and two linked
counts (On the way, To confirm). It has no selling, sales, listing or public-profile actions.

App shell with the account subnav. `@sycapooke/accounts` `AccountPage` (`src/lib/account/`), guarded by `signedInGuard`.

- Library: `sy-subnav` (shell), `sy-empty-state` (error), `syButton`, `sy-icon`; the kit's `settings-list` rows
- Local: the greeting and two link rows. The On the way and To confirm counts join when the orders overview is composed

### Account details — `/account/profile` (`pages/edit-profile`: default, invalid, submitting, success, loading, error)

Customer account details: `@sycapooke/accounts` `ProfilePage` (`src/lib/account/`), guarded by `signedInGuard`. A private display name only: no photo, bio, location or
public page. The founder's public details are edited on Edit story.

- Library: `sy-error-summary` (invalid), `sy-form-field` (Display name with help "3–30 letters,
  numbers, _ or -. Private: only you see it."; Email, read-only, help "Change your sign-in details
  under Settings."), `sy-alert` (success "Details saved", `live="polite"`), `syButton` (primary
  "Save details" with `busy`), `sy-empty-state` (error tone "Couldn’t load your details" with the
  reference and "Try again"). The single field shows its error inline, so the error summary is not used
- Local: the page header, the `.page--form` wrapper and the loading skeleton

### Shop — `/shop` (`pages/shop`: default, loading, empty, error)

Founder overview, built as `@sycapooke/listings` `ShopPage` (`src/lib/shop/`) behind `founderGuard`; so far it shows the Active listings figure and the Inventory row, and Sales, Support and Story join with their pages. App shell with the founder sub-navigation (Shop tab). Only
the verified founder reaches it; anyone else gets the Forbidden page (state `management`).

- Library: `sy-stat` (Active listings, To ship by {date}, Open problem; each inside a linked
  `.panel` to inventory, sales or support), `syButton` (primary "Sell an item" with icon `plus` in
  the header; "Open inventory", "Open sales", "Open support", "Edit story"; primary "Try again"),
  `sy-empty-state` (empty: icon `tag`, "Nothing for sale yet" with primary "Sell an item", and the
  header button hidden; error tone "Couldn’t load the shop" with the reference and "Try again"),
  `sy-icon` (`plus`, `refresh`)
- Local: the page header with the subtitle "{founder}’s collection", the `.grid--auto` row of linked
  stat panels and its 3 skeleton panels, and the `.settings-list` of four link rows (Inventory,
  Sales, Support, Your story)

### Edit story — `/shop/story` (`pages/edit-story`: default, invalid, submitting, success, loading, error)

Founder only; App shell, Story tab (built in accounts: `story/`). Edits what the public founder page shows:
portrait, display name, story (up to 500 characters) and city; there is no street address.

- Library: `sy-error-summary` (invalid), `sy-form-field` (Display name; Story textarea with counter
  "{n} / 500"; City and State or region, both optional, in a `.form-row`), `syButton` ("Change
  portrait" with icon `camera`, secondary; primary "Save story" with `busy`; ghost "View public
  page" to the founder page), `sy-alert` (success "Story saved" with the "View public page" link
  projected as `actions`, `live="polite"`), `sy-empty-state` (error tone "Couldn’t load your story"
  with the reference and "Try again"), `sy-icon` (`camera`, `refresh`)
- Local: the page header "Story", the portrait row (`avatar--xl` with the help "JPEG, PNG or WebP,
  up to 10 MB. Location data is removed."), the "Public location" `fieldset` with its help, and the
  loading skeleton (portrait, field, 9rem textarea)

### Forgot password — `/forgot-password` (`pages/forgot-password`: default, invalid, submitting, success, error)

Auth shell. `@sycapooke/accounts` `ForgotPasswordPage` (`src/lib/forgot-password/`).

- Library: `sy-form-field`, `sy-alert` (danger), `sy-icon` (mail), `syButton` (primary, link)
- Local: none

### Email settings — `/account/emails` (`pages/notification-settings`: default, founder, submitting, success, loading, error)

App shell. `@sycapooke/accounts` `NotificationSettingsPage` (`src/lib/notification-settings/`), built by
manage-notification-preferences. The page has no Reviews rows: the built page still carries a
"Review emails" row that is removed. The account sub-navigation is the shell's (customer or
founder tabs).

- Library: `sy-switch` (customer, 3 rows: "Account security emails, always on" and "Purchase emails,
  always on" checked and disabled; "Marketing emails", off until opted in, disabled while saving;
  founder state, 4 rows all checked and disabled: "Account security", "Sales", "Ship-by reminders"
  and "Support"), `syButton` (primary "Save" with `busy` for customers only, none for the founder;
  primary "Try again"), `sy-alert` (success "Email settings saved", `live="polite"`),
  `sy-empty-state` (error tone "Couldn’t load email settings" with the reference), `sy-icon`
  (`refresh`), `ToastService` (danger "Something went wrong" with the reference and "Try again" when
  a save fails; warning when the settings changed elsewhere)
- Local: the page header with its subtitle ("Security and purchase emails are always on." or the
  founder's "Security, sales, ship-by and support emails are always on."), the `.settings-list`
  rows with title and description, the `.form-actions` row, and the loading skeleton rows

### Unsubscribe — `/emails/unsubscribe?token=…` (not mocked: working, unsubscribed, invalid, error)

Auth shell. `@sycapooke/accounts` `UnsubscribePage` (`src/lib/unsubscribe/`), built by
manage-notification-preferences; the mocks have no screen for it, so it follows the verify-email
layout.

- Library: `sy-spinner` (working), `syButton` ("Back to shop", "Manage your emails", "Try again"),
  `sy-icon` (`refresh`)
- Local: the `h1` and lead of each state and the `.stack` of actions

### Founder page — `/members/{displayName}` (`pages/profile`: default, own, empty, loading, error, unavailable, long)

Built by view-member-profile in `@sycapooke/reviews` (`src/lib/member-profile/`) and reshaped by
ADR general/0010: it is the founder's one public page (portrait, story, city, completed sales,
items for sale). Customers have no public profile: a customer display name, a closed account or an
unknown name returns the same unavailable page. It shows no email, phone, street address, customer
identity or purchase history. The Reviews section keeps "No reviews yet" while review authoring is
deferred (ADR reviews/0001). The own state (the founder viewing it) adds "Edit story".

- Library: `sy-rating-summary` (header; "No reviews yet"), `sy-stat` (Completed sales, Member
  since), `sy-listing-grid` (For sale cards; loading skeleton), `sy-load-more` (long:
  "Showing {n} of {total}", 24 per page), `sy-empty-state` (empty: `compact`, icon `tag`, "Nothing
  for sale right now", "New pieces from the collection are added often. Check back soon."),
  `sy-alert` (danger "Couldn’t load more items"), `sy-center-state` (unavailable: code "404", "This
  page isn’t available", "Back to shop"; error: reference, "Try again" with icon `refresh` and "Back
  to shop"), `syButton` (own: "Edit story" with icon `edit`, linking to `/shop/story`), `sy-icon`
  (`map-pin`, `refresh`)
- Local: `.profile-head` with the `avatar--xl` portrait, `h1`, city and story; the `.stats` row;
  the For sale and Reviews `.section-head` blocks; the header loading skeleton

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

Auth shell. `@sycapooke/accounts` `ResetPasswordPage` (`src/lib/reset-password/`), reading `token` from the query string.

- Library: `sy-form-field`, `sy-alert` (danger), `syButton`
- Local: none

### Session expired — `/sign-in?expired=1` (`pages/session-expired`: default)

Auth shell. A state of `SignInPage` (`src/lib/sign-in/`): "Welcome back" until the member chooses Sign in.

- Library: `syButton`
- Local: none

### Settings — `/account/settings` (`pages/settings`: default, founder, loading, error)

A short `.settings-list`: Email, Password ("Change" opens Change password), Sessions ("Sign out
everywhere"), Cookies ("Cookie settings") and, for customers only, Close account (ghost danger
button opening Close account). The founder state has no Close account row.

App shell with the account subnav. `@sycapooke/accounts` `SettingsPage` (`src/lib/settings/`), guarded by `signedInGuard`.
Built so far: Email, Password and Sessions. Cookies and Close account join in Phases 4 and 5.

- Library: `sy-alert` (success "Password changed"), `syButton`, `DialogService` opening `ChangePasswordDialog` and `SignOutEverywhereDialog`; the kit's `settings-list`
- Local: none

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
`subcategory`, `brand`, `condition`, `min`, `max`, `sort`). No state offers selling: the empty
state has no "Sell an item" action.

- Library: `sy-listing-grid` (cards and the 8-card loading skeleton) with `sy-listing-card`
  (no seller shown: the founder sells everything),
  `sy-size-tag` (lg, category header), `sy-load-more`, `sy-empty-state` (empty: icon `tag`,
  "Nothing on the shelf right now", "The founder is choosing her next treasures. Please check back
  soon.", no action; no-results: icon `search`, "No items match", "Clear
  search and filters"; error: tone error, icon `refresh`, "Try again"), `sy-alert` (warning
  banner with icon `wifi-off` for offline; danger "Couldn’t load more items" with a
  `syButton="link"` Try again when a next page fails), `sy-chip` (one per active filter value),
  `syButton` (Filters toggle, "Clear all" link button, primary "Search" in the hero), `sy-choice` and `sy-input-group` (inside
  the filter form), `SheetService` (opens the Filters sheet below 992px), `sy-icon`
- Local: the hero (`.hero`, h1 "Treasures with a *story.*", lead "Jewelry, clothing, housewares and electronics, every piece at least 20 years old, from the founder’s own collection." and the hero search well), the page header
  (category name with size tag; eyebrow "Search" with “{q}”; "Shop" for other filters), the
  `.catalog-layout` grid, the `.toolbar` with the polite item count and its text skeleton, the
  `.sort` select, `FilterForm` (`sy-filter-form`, fieldsets Category, subcategories, Brand with
  "All {n} brands", Condition, Price) and `FiltersSidebar` (`sy-filters-sidebar`,
  `<aside class="filters" aria-label="Filters">`). The offline banner renders at the top of
  `main`, not above the top bar as in the mock, until `sy-system-banner` exists. A failed next
  page shows an inline danger alert instead of the toast in the design, until `ToastService`
  exists (responsive-accessible-shell). The subcategory chips under a category header are not
  built; the sidebar's subcategory group covers the same filter.

### Listing detail — `/items/{id}` (`pages/listing-detail`: default, guest, zero-sales, reserved, sold, own, loading, error, unavailable)

Built by view-listing-detail in `@sycapooke/catalog` (`src/lib/listing-detail/`) for every state; the
`new-seller` state is now `zero-sales`. In the own state (the founder, with the founder navigation)
edit-and-withdraw-listing adds "Withdraw", which opens the Withdraw listing dialog
(`src/lib/withdraw-listing/`, ADR listings/0003). Every listing is the founder's, so the page
shows one founder block and no seller choice.

- Library: `sy-gallery` (main photo track, dots, thumbnails), `sy-size-tag` (lg), `sy-buy-bar`
  (default and guest: "Buy now" as `syButton="primary"` lg; own: "Edit listing" with icon
  `edit` and secondary lg "Withdraw" through `DialogService`; absent for Reserved and Sold), `sy-alert` (reserved: warning, icon `clock`, "In
  someone’s checkout"), `sy-founder-summary` (the founder summary: avatar, name linking to the
  founder page, "Founder of Sycapooke · {n} completed sales", "No reviews yet" and the link "Read her
  story and see her collection"; zero-sales shows "0 completed sales" and no "New seller" label),
  `sy-listing-grid` (sold:
  "More treasures"), `sy-center-state` (unavailable: code "404" and "Shop
  similar items"; error: reference, "Try again" with icon `refresh` and "Back to shop"),
  `syButton`, `sy-icon`
- Local: the breadcrumb, the info column (`.photo__stamp` Reserved or Sold, `.eyebrow` brand,
  `h1.detail__title`, `.detail__price` line, `.flaws`, description, the optional story
  paragraph, `dl.facts` with the relative `time`), the two-column `.detail` layout and the loading skeleton

### Checkout — `/checkout/{id}` (`pages/checkout`: default, new-address, invalid, non-us, submitting, declined, expired, conflict, loading, error)

Built by checkout-and-pay in `@sycapooke/orders` (`src/lib/checkout/`) for every state; `{id}` is
the listing reference. A refused start for the founder's own, a sold or a missing listing goes
back to `/items/{id}`, and a visitor goes to sign in with checkout as the return URL.

- Library: `sy-reservation-timer` (under the title; its phase drives the banners),
  `sy-alert` (banner warning "Less than a minute left to pay…" and danger "Your reservation
  ended…" with "Start again", both icon `clock`; non-us, declined, total changed and payment
  failure alerts in the form), `sy-error-summary` (invalid), `sy-form-field` (Full name,
  Street address, Apartment, suite (optional), City, State, ZIP code with `inputmode="numeric"`,
  Country), `sy-order-summary`, `syButton` (primary lg block "Pay $164.40", or "Continue" before
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

- Library: `sy-item-row` ("Paid $164.40 · Visa ending 4242"), `syButton` ("View order",
  "Keep shopping"), `sy-center-state` (error: reference, "Try again" and "Go to purchases"),
  `sy-icon` (`check`, `refresh`)
- Local: the `.confirm` block (mark, `.eyebrow` order reference, heading), the
  `ol.next-steps` list and the loading center state with the large spinner

### Order (buyer) — `/orders/{id}` (`pages/order-detail`: default, paid, cancel-requested, cancel-declined, delivered, completed, cancelled, refunded, problem, founder-replied, closed-no-refund, refund-pending, loading, error)

Built by ship-order in `@sycapooke/orders` (`src/lib/order-detail/`) for paid ("Waiting to ship"),
default ("On its way"), delivered, cancelled, loading and error. complete-order adds "I received
it" (default and delivered), the automatic completion note (delivered) and the completed state.
cancel-order adds "Ask to cancel" (paid), cancel-requested, cancel-declined and the founder's note
on cancelled. resolve-order-problems adds "Report a problem" (default and delivered, while
`canReportProblem`), the problem state and the refunded state; the founder resolves problems
herself (ADR general/0010), which adds founder-replied, closed-no-refund and refund-pending. There
is no payout or hold wording. The mock shows "Expected {date}." under the tracking link of the
default state; the API has no expected date, so it is not built.

- Library: `sy-order-timeline`, `sy-tracking-link` (once shipped), `sy-alert` (info "Refunded in
  full" with the amount and card when cancelled), `sy-quote` (the founder's note, cited with her
  name, when cancelled; her explanation, cited "{founder} · {day}", when refunded or closed without
  a refund), `sy-order-summary` (Item, Shipping, Sales tax, Total; no visible heading; labelled "Order
  summary"), `sy-center-state` (error, with `sy-button` "Try again" and "Back to purchases"),
  `sy-button` (primary "I received it" when Shipped or Delivered, opening Confirm receipt through
  `DialogService`; secondary "Ask to cancel" when Paid and never asked, opening Ask to cancel;
  ghost "Report a problem" beside "I received it", opening Report a problem; primary link "Shop
  similar items" to the catalog when cancelled; "Back to purchases" when refunded),
  `sy-alert` (warning, icon `flag`, "Problem open · {reason}" when a problem is open, with
  "{founder} has replied. Completion stays paused until she resolves the problem." after her reply;
  warning, icon `clock`, "Refund pending" while the payment provider has not confirmed the refund;
  info, icon `flag`, "Problem closed without a refund" with the timeline at Completed; success
  "Refunded in full" / "{founder} refunded {total} to {card}." once refunded), `sy-thread` (the
  claim with photos, captioned "You", and the founder's reply)
- Local: the breadcrumb, eyebrow and heading block, the ship-by note ("{founder} ships by {date}.
  If not, you’re refunded automatically."), the request note (`.order__request`: "You asked
  {founder} to cancel. She hasn’t answered yet." or "{founder} declined to cancel. She still ships
  by {date}."), the completion note ("Completes automatically on {date} unless you report a
  problem."), `OrderPartyComponent` (founder avatar, "Sold by", link to the founder page) projected into the summary,
  and the two-column loading skeleton. "Shop similar items" opens the catalog home, because the
  order does not carry the item's category.

### Purchases — `/account/purchases` (`pages/purchases`: default, loading, empty, error)

App shell. `@sycapooke/orders` `OrderHistoryPage` (`src/lib/order-history/`, route data `list: 'purchases'`),
built by order-history. The account sub-navigation is the shell's customer tabs.

- Library: `sy-item-row` (title, "{date}" with thumbnail; no seller, the founder is always the seller), `sy-status` (info; active
  when Completed; removed when Cancelled or Refunded), `sy-empty-state` (icon `bag`, "Nothing
  bought yet" with primary `sy-button` "Browse the catalog"; error tone, icon `refresh`, "Couldn’t
  load purchases" with the reference and primary "Try again" with icon `refresh`)
- Local: the page header with the counts subtitle ("{n} on the way · {n} to confirm"), the `.order-list` rows linking to `/orders/{id}`
  with the price, and the three skeleton rows

### Sale (founder) — `/shop/sales/{id}` (`pages/sale-detail`: default, shipped, cancel-requested, problem, completed, address-hidden, former-customer, cancelled, loading, error)

Founder only; App shell, Sales tab. Built by ship-order in `@sycapooke/orders` (`src/lib/sale-detail/`)
for default (Paid), shipped, cancelled, loading and error. cancel-order adds "Cancel order",
cancel-requested and "Relist item"; resolve-order-problems adds problem; order-history adds
address-hidden (`sy-icon` `eye-off`, "Removed 30 days after the order ended."). The completed state
shows only the timeline: payment already reached the shop account, so there is no payout panel (the
seller-payouts journey is retired).

- Library: `sy-button` (primary "Add tracking" with icon `truck`, opening the Add tracking
  dialog through `DialogService`; ghost "Cancel order" beside it, opening Cancel order; primary
  "Respond" when cancel-requested, opening Respond to cancellation; primary "Relist item" when
  cancelled and the listing can be relisted, opening Relist item), `sy-quote` (the customer's
  message, cited with the customer's name, when cancel-requested), `sy-order-timeline` and
  `sy-tracking-link` (once shipped), `sy-alert` (info "Cancelled · Customer refunded in full" with
  "The item stays off the catalog until you relist it."), `sy-order-summary` (Item, Shipping, Sales
  tax, Total; no visible heading), `sy-center-state` (error,
  with "Try again" and "Back to sales"), and for problem `sy-alert` (warning, icon `flag`,
  "Problem reported · {reason}" / "Completion is paused until you resolve it." with primary
  "Resolve the problem" projected as `actions`, linking to `/shop/problems/{reference}`),
  `sy-thread`, `sy-form-field` (textarea "Your reply", help "Visible to the customer.") and
  `sy-button` ("Send reply" with busy, "Try again" after a failure) until the founder has replied
- Local: the breadcrumb, eyebrow and heading block, "Ship by {date}." lead, the "Ship to"
  address block with its caption "Visible until 30 days after the order ends.", `OrderPartyComponent` ("Customer": avatar and
  name, no profile link, since customers have no public page) projected into the summary, and the
  loading skeleton

### Sales — `/shop/sales` (`pages/sales`: default, loading, empty, error)

Founder only; App shell, Sales tab. The same `OrderHistoryPage` with route data `list: 'sales'`, built by order-history.

- Library: `sy-item-row` ("{customer} · ship by {date} · {date}" while Paid, else "{customer} ·
  {date}"), `sy-status`, `sy-empty-state` (icon `tag`, "No sales yet", "Orders show up here with what to do next." with primary "Sell an item";
  error tone "Couldn’t load sales" with the reference and "Try again")
- Local: the page header with "{n} to ship", the `.order-list` rows linking to `/shop/sales/{id}`, and
  the skeleton rows

### Support queue — `/shop/support` (`pages/support`: default, filtered, loading, empty, error)

Founder only; App shell, Support tab (built in orders: `support/`). Customer problems and cancellation
requests, newest first; the founder handles them herself (no separate administrator).

- Library: `sy-item-row` ("{reference} · {kind} · {customer} · {when}" with the item thumbnail,
  for example "P-0912 · Problem: not as described · …" or "C-0044 · Cancellation request · …"),
  `sy-status` (Open, Refunded), `syButton` (the All and Open filter links, `size="sm"`, primary when
  current, `aria-current`; primary "Try again" with icon `refresh`), `sy-empty-state` (empty: icon
  `check-circle`, "No requests", "When a customer reports a problem or asks to cancel, it shows up
  here."; error tone "Couldn’t load support requests" with the reference)
- Local: the page header with "{n} open", the `.cluster` filter group ("Filter support requests"),
  the `.order-list` rows linking to `/shop/problems/{reference}` (problems) or `/shop/sales/{id}`
  (cancellation requests) with the price, and the 4 skeleton rows

### Edit listing — `/items/{id}/edit` (`pages/edit-listing`: default, loading, error, invalid, submitting, success, locked, changed, sold, withdrawn)

App shell (founder). `@sycapooke/listings` `EditListingPage` (`src/lib/edit-listing/`), built by
edit-and-withdraw-listing. The fields are the sell form's, shared through the local
`ListingFormComponent` (`src/lib/listing-form/`) (ADR listings/0003). A draft opens the sell form
at `/sell/{reference}` instead. There is no earnings or fee panel: payment goes straight to the
shop account.

- Library: `sy-status` (the listing status beside the heading), `sy-error-summary`,
  `sy-photo-uploader`, `sy-form-field`, `sy-fixed-size`, `sy-segmented`, `sy-input-group`,
  `sy-alert` (success "Changes saved"
  with "View listing"; warning "This listing changed since you opened it" with sm "Reload" and icon
  `refresh`; danger "Couldn’t save your changes" with the reference), `syButton` (ghost "Cancel"
  link, primary "Save changes" with busy), `sy-center-state` (error: icon `refresh`, "Couldn’t
  load this listing", reference, "Try again" and "My listings"; locked: icon `clock`, "This item
  is in someone’s checkout", "Back to listing"; sold: icon `check`, "This item has sold", "Your
  sales" and "My listings"; withdrawn: icon `trash`, "This listing was withdrawn", "My
  listings"), `sy-icon`
- Local: the page header (listing title link, heading, status), `ListingFormComponent` (the
  photos, item, price and confirmation sections), the sticky split actions row and the loading
  skeleton (3 photo tiles, 4 fields)

### Inventory — `/account/listings` (`pages/my-listings`: default, filtered, loading, empty, error)

The founder's inventory (heading "My listings", Inventory tab). App shell. `@sycapooke/listings` `MyListingsPage` (`src/lib/my-listings/`), built by manage-my-listings.
The status is kept in the URL query (`?status=sold`). There is no payout banner. The founder
sub-navigation is the shell's.

- Library: `ToastService` (info "Drafts older than 90 days are deleted", "We’ll email you a
  week before.", once per visit when the founder has drafts), `sy-button` (primary "Sell an item"
  with icon `plus`), `sy-filter-chips` ("Filter by
  status": All, Draft, Active, Reserved, Sold, Removed with counts), `sy-item-row` (title or
  "Untitled draft · {category}", thumbnail, with `sy-status` projected under the title in the
  tone of the listing status), `sy-icon` (`image` placeholder for a listing without photo),
  `sy-empty-state` (icon `tag`, "Nothing listed yet", "List a brooch, a teacup or anything with a story to tell." with primary "Sell an item"; error tone, icon
  `refresh`, "Couldn’t load your listings" with the reference and primary "Try again")
- Local: the page header with "{n} listings · {n} active", the `.order-list` rows linking to
  `/sell/{reference}` (Draft), `/shop/sales/{orderId}` (Sold), `/items/{reference}` (Active) or
  `/items/{reference}/edit` (Removed) with the price
  ("$60" or "—") and the date ("Today", "Yesterday", "Sat 24 Oct"), and the four skeleton rows

### Sell an item — `/sell` (`pages/sell`: default, filled, invalid, submitting, draft-saved, success, limited, error)

App shell (founder only; a customer or visitor gets the Forbidden page). `@sycapooke/listings` `SellPage` at `/sell` and `/sell/{reference}` (continue a draft).
The Photos section comes first; each chosen file
uploads at once and a save sends the photo IDs in order. The sticky actions bar replaces the footer
under the form. There is no earnings preview, no payouts line and no `unverified` state: the founder
is not blocked by email verification.

- Library: `sy-photo-uploader` (photos with cover, uploading and rejected tiles, add tile, help and error), `sy-error-summary` (invalid), `sy-alert` (draft-saved success, save or publish failure), `sy-form-field` (category, type, title with counter, brand, flaws, "Description and story" with counter, price, shipping), `sy-input-group` (`$` addon), `sy-fixed-size` with `sy-size-tag` (after a category is chosen), `sy-segmented` (condition), `sy-button` (Save draft, Publish with busy), `sy-center-state` (success "It’s live", limited "That’s 20 listings today", error) with `sy-item-row` (success panel) and `sy-icon`
- Local: `.page-header` with the h1, `.sell-form` sections (Photos, Item, Price: `.form-section`, `.form-row`), the upload state of each tile and its client size check, the brand `<datalist>`, the sticky `.form-actions` bar and the success panel wrapper with its link to the inventory

### Forbidden — `/403` (`pages/forbidden`: default, management)

Built as `@sycapooke/accounts` `ForbiddenPage` (`src/lib/errors/`), reached at `/403` or `/403?reason=management`; `founderGuard` redirects a customer there. The default says "It belongs to another customer’s account."; the `management` state
is a signed-in customer opening a founder-only route (shop, inventory, sales, support, story):
"Shop management is only for the shop’s founder." Both go back to the customer's account.

- Library: `sy-center-state` (code "403", "You can’t open this", the reason line, primary
  `syButton` "Go to your account")
- Local: none

### Maintenance — `(any)` (`pages/maintenance`: default)

- Library: not yet composed
- Local: not yet composed

### Not found — `/*` (`pages/not-found`: default)

`@sycapooke/accounts` `NotFoundPage` (`src/lib/errors/`), the `**` route of the app.

- Library: `sy-center-state` (code "404", "Page not found"), `syButton`
- Local: none

### Offline — `(any)` (`pages/offline`: default)

- Library: not yet composed
- Local: not yet composed

### Marketing page — `/welcome` (`pages/welcome`: default, signed-in)

Built in `@sycapooke/marketing` (`src/lib/welcome/`) by the marketing page and rewritten for the
founder-only shop (ADR general/0010): it invites shopping and introduces the founder and does not
recruit sellers. Calls no Sycapooke API endpoint; every call to action goes to the catalog or to
the Our story band, so the signed-in state differs only in the shell (no selling or management
links). Title "Treasures with a story · Sycapooke" (route `title`); the meta description and
link-preview tags are static in `index.html` and must follow the same shop wording (not shown by the
mocks). The copy says everything is vintage, every piece at least 20 years old, and never promises
new or reproduction goods ([ADR general/0005](adr/general/0005-marketing-wording.md)).

- Library: `sy-marketing-hero` (on paper, no decoration: eyebrow "Jewelry · Clothing · Housewares · Electronics",
  h1 "A home for the things that *mattered.*", lede "Antiques and vintage treasures from one
  Caribbean woman’s own collection. Every piece is at least 20 years old.", actions primary lg
  "Shop the collection" to `/` and secondary lg "Meet the founder" to `#our-story`, media slot with `sy-video-poster`: the hero photo `/media/sycapooke-hero.jpg` as one
  button named "Play" with the "Play · 0:29" pill, opening the Pitch video dialog through `DialogService`),
  `sy-feature-band` (For shoppers "Every piece is at least 20 years old." on paper with the
  points Four focused categories, From the founder’s own collection and Honest condition, and "Browse the
  collection"; From my collection "Treasures I have kept, ready for their next home.", `tone="sell"`
  (terracotta, cream text, inverse buttons), with the points Chosen by hand and Described with care, and
  "See what is on the shelf"; How it works "Three steps. That’s
  it.", `stacked`; Our story "sy·ca·pooke" with `id="our-story"`, `tone="story"` (cream under a line rule), `stacked`;
  the closing call to action "Treasures with a *story.*", lede "Explore her collection. Every piece
  is at least 20 years old.", `tone="sell"`, `stacked`, with the inverse "Browse the collection"),
  `sy-steps` (Find a treasure, Pay securely, Receive its story), `sy-quote` (the founder's
  quote, currently placeholder copy awaiting her approval and stating no age, family or biography,
  attributed "Founder of Sycapooke", with the founder photo `/media/sycapooke-founder.jpg`),
  `syButton` (primary "Shop the collection", "Browse the collection" and inverse "See what is on the shelf", all to `/`)
- Local: `WelcomePage` (copy, the two photographs), `PitchVideoDialog`
  (see Dialogs).

### Privacy — `/privacy` (`pages/privacy`: default)

- Library: not yet composed
- Local: not yet composed

### Problem detail — `/shop/problems/{reference}` (`pages/problem-detail`: default, replied, resolved, closed-no-refund, refund-pending, refund-failed, loading, error)

Built by resolve-order-problems in `@sycapooke/orders` (`src/lib/problem-detail/`) as a founder page
in the app shell (Support tab); there is no admin shell and no payout hold. The founder replies and
decides herself.

- Library: `sy-thread` (customer claim with private photos captioned "{customer} · customer · {day}"
  and the note that only the founder and the customer see them; her reply captioned "{founder} ·
  you · {day}"), `sy-form-field` (textarea "Your reply", help "Visible to the customer.", default
  state only) with `sy-button` ("Send reply"), `sy-item-row` (the item in the side panel),
  `sy-facts` (Status as `sy-status`, Total, Delivered, Completion: "Paused", "Refunded", "Completed"
  or "Refund pending"), `sy-button` (primary "Resolve", opening Resolve problem through
  `DialogService`, while open), `sy-alert` (success "Refunded in full" / "{total} refunded to
  {customer}, who has your explanation."; info "Closed without a refund" when closed; warning, icon
  `clock`, "Refund pending" while the provider has not confirmed; danger "The refund failed" with primary
  "Retry refund" (icon `refresh`) as `actions`, reopening Resolve problem in its failed state),
  `sy-empty-state` (error "Couldn’t load this problem" with the reference and "Try again")
- Local: the breadcrumb (Support), the `.page-header` with the reason as heading and "Order problem {reference} · opened
  {day}", the sunken `aside` panel with its eyebrow, and the
  two-column loading skeleton

### Shop policies — `/rules` (`pages/rules`: default)

Renamed from Marketplace rules; the footer link reads "Shop policies". Static, guest layout. Not yet built.

- Library: `sy-size-tag` (the size column)
- Local: the narrow page header ("Short, because it only has to cover one shop."), the accepted
  categories `.table` (Category, Includes, Size) and the `.rules-list` of six policies (Vintage
  only, Honest condition, Ships in 5 business days, Paid straight to the shop, Problems handled by
  the founder, US only)

### Server error — `/error` (`pages/server-error`: default)

- Library: not yet composed
- Local: not yet composed

### Terms — `/terms` (`pages/terms`: default)

- Library: not yet composed
- Local: not yet composed


## Dialogs

| Dialog (mock folder: states) | Overlay surface | Library components | Local UI |
|---|---|---|---|
| Change password (`dialogs/change-password`: default, invalid, busy, failed) | `DialogService` | `sy-dialog`, `sy-form-field`, `sy-alert` (danger, top), `syButton` (`ChangePasswordDialog`) | none |
| Close account (`dialogs/close-account`: default, invalid, busy, google, google-expired, blocked) | `DialogService` | `sy-dialog` (icon `trash`, danger; description "Your data is erased within 30 days. This can’t be undone."), `sy-form-field` (password "Enter your password to confirm", with the error), `sy-alert` (warning "Finish your open orders first" for blocked), `sy-button` (Cancel, disabled while busy; danger "Close account" with busy, never the default focus; google: danger "Continue with Google" with icon `external`; blocked: primary "Close"), `sy-status` (blocked list) | Not yet built. The google state (a Google-only account has no password) replaces the password field with a line asking for a fresh Google sign-in; the blocked state lists the open orders as links with their status; customers only, the founder account cannot be closed here |
| Sign out everywhere (`dialogs/sign-out-everywhere`: default, busy, failed) | `DialogService` | `sy-dialog`, `sy-alert` (danger, top), `syButton` (`SignOutEverywhereDialog`) | none |
| Verify email prompt (`dialogs/verify-email-prompt`: default, busy, sent, limited) | `DialogService` | `sy-dialog` (mail icon, description "Open the link we sent to {email} to buy."), `sy-alert` (sent success, limited warning) in the top slot, `sy-button` (Not now, Resend link with busy, Done) | `VerifyEmailPromptComponent` in `@sycapooke/listings`, opened from Buy now for an unverified customer (listing detail); it moves to a shared place when checkout also opens it |
| Filters (`dialogs/filters`: default, invalid, busy) | `SheetService` | `sy-sheet` (`side`, heading "Filters", close "Close filters"), `sy-error-summary` (heading "Check the price range"), `sy-choice`, `sy-input-group`, `sy-button` (Clear; primary "Show {n} items" with `aria-busy`, disabled when invalid) | `FiltersSheet` and `FilterForm` in `@sycapooke/catalog` |
| Menu (`dialogs/nav-menu`: default, guest, founder) | `SheetService` | `sy-sheet` (heading "Menu", close "Close menu"), the four category links on `.menu-sheet__links`, then the small links for the customer, guest or founder (see Shell) | `sy-nav-menu` in `@sycapooke/components`, opened by `sy-top-bar`; the small links, Sign out and row chevrons are not composed yet |
| Add tracking (`dialogs/add-tracking`: default, invalid, busy, failed) | `DialogService` | `sy-dialog` (description "We’ll email {customer} the tracking link."), `sy-form-field` (Carrier select, focused on open; Tracking number with the inline error), `sy-alert` (danger "That didn’t go through" in the top slot), `sy-button` (Cancel, disabled while busy; primary "Mark as shipped" with busy, "Try again" after a failure) | `AddTrackingDialog` in `@sycapooke/orders` |
| Cancel order (`dialogs/cancel-order`: default, invalid, busy, failed) | `DialogService` | `sy-dialog` (icon `alert`, `tone="danger"`, title "Cancel this sale?", description "{customer} is refunded {total} in full. The item stays off the catalog until you relist it."), `sy-radio-group` (legend "Reason", error "Choose a reason") with four `sy-choice` radios (Item damaged, No longer have it, Listed by mistake, Customer asked me to cancel; first focused on open), `sy-form-field` (textarea "Note to the customer", optional), `sy-alert` (danger "That didn’t go through" in the top slot), `sy-button` ("Keep sale", disabled while busy; danger "Cancel sale" with busy, "Try again" after a failure) | `CancelOrderDialog` in `@sycapooke/orders` |
| Confirm receipt (`dialogs/confirm-receipt`: default, busy, failed) | `DialogService` | `sy-dialog` (`size="sm"`, title "Got it and all good?", description "This completes the order. You can’t report a problem afterwards."), `sy-item-row` (item thumbnail, title, brand and size), `sy-alert` (danger "That didn’t go through" in the top slot), `sy-button` ("Not yet", disabled while busy; primary "Yes, complete order" with busy, "Try again" after a failure) | `ConfirmReceiptDialog` in `@sycapooke/orders` |
| Relist item (`dialogs/relist`: default, busy) | `DialogService` | `sy-dialog` (`size="sm"`, description "It goes back in the catalog with its original photos, price and description."), `sy-item-row` (thumbnail, title, "{price} · {shipping} shipping"), `sy-alert` (danger "That didn’t go through" in the top slot after a failure), `sy-button` ("Not now"; primary "Relist", focused, with busy) | `RelistDialog` in `@sycapooke/orders`, with "Edit first" (`syButton` link to `/items/{id}/edit`, closing the dialog) added by edit-and-withdraw-listing |
| Report a problem (`dialogs/report-problem`: default, invalid, busy, failed) | `DialogService` | `sy-dialog` (description "Open until {deadline}. {founder} will reply, and the order won’t complete while she looks into it."), `sy-error-summary` (invalid, in the top slot), `sy-segmented` (legend "What’s wrong?": Not received, Not as described, Wrong size, Counterfeit), `sy-form-field` (textarea "Describe it"), `sy-photo-uploader` (`[cover]="false"`, max 4, "Add photo"), `sy-alert` (danger "That didn’t go through" / "Your photos and description are saved here. Try again."), `sy-button` (Cancel, disabled while busy; primary "Send to the founder" with busy, "Try again" after a failure) | `ReportProblemDialog` in `@sycapooke/orders`; the "Photos (up to 4)" field label |
| Ask to cancel (`dialogs/request-cancellation`: default, busy, failed) | `DialogService` | `sy-dialog` (description "If {founder} accepts, you’re refunded in full. Until then the order stays as is."), `sy-form-field` (textarea "Message to the founder", optional, focused on open), `sy-alert` (danger "That didn’t go through" in the top slot; the message is kept), `sy-button` ("Keep order", disabled while busy; primary "Send request" with busy, "Try again" after a failure) | `RequestCancellationDialog` in `@sycapooke/orders` |
| Respond to cancellation (`dialogs/respond-cancellation`: default, busy, failed) | `DialogService` | `sy-dialog` (heading "{customer} asked to cancel", description "Accepting refunds {total} in full. Declining keeps the order, and you still ship by {date}."), `sy-quote` (the customer's message), `sy-alert` (danger "That didn’t go through" in the top slot), `sy-button` ("Decline"; primary "Accept and refund", focused; the chosen one shows busy and the other is disabled) | `RespondCancellationDialog` in `@sycapooke/orders` |
| Withdraw listing (`dialogs/withdraw-listing`: default, busy, failed) | `DialogService` | `sy-dialog` (`size="sm"`, icon `trash`, tone danger, title "Withdraw “{title}”?", description "It leaves the shop right away. This can’t be undone."), `sy-alert` (danger "Couldn’t withdraw" / "Your listing is still live." in the top slot after a failure), `sy-button` ("Keep listing", focused, disabled while busy; danger "Withdraw" with busy, "Try again" after a failure) | `WithdrawListingDialog` in `@sycapooke/catalog` (ADR listings/0003); on success the page goes to `/account/listings` |
| Cookie settings (`dialogs/cookie-settings`: default, analytics-on) | `DialogService` through `CookieSettingsDialogService` | `sy-dialog` (size `sm`, heading "Cookie settings"), `sy-switch` with `hint` "Strictly necessary" ("Sign-in and checkout. Always on.", checked and disabled) and "Analytics" ("Helps us see which pages are slow.", `autofocus`), `sy-button` (Cancel; primary Save) | `CookieSettingsDialog` in `@sycapooke/privacy` |
| Pitch video (`dialogs/pitch-video`: default, loading, error, captions) | `DialogService` | `sy-dialog` (size `lg`, heading "Sycapooke video" visually hidden), `sy-video-player` (ink 16:9 frame with the native `video`: MP4 then WebM sources at `/media/sycapooke-pitch.*`, English captions track, poster `/media/sycapooke-hero.jpg`; spinner overlay with "Loading video…" while loading; "The video didn’t load" / "Check your connection and try again." with Try again on error; `sy-button` primary Play/Pause and secondary Captions with `aria-pressed`, off by default) | `PitchVideoDialog` in `@sycapooke/marketing`; Escape closes, stops the video and returns focus to the poster |
| Resolve problem (`dialogs/resolve-problem`: default, invalid, busy, failed) | `DialogService` | `sy-dialog` (heading "Resolve problem {reference}"), `sy-radio-group` (legend "Decision") with `sy-choice` "Refund in full" ("{total} back to {customer}") and "Close without refund (order completes)" ("No refund; the order becomes Completed"), `sy-form-field` (textarea "Your explanation", help "Shown to the customer.", error "Add an explanation. The customer sees it."), `sy-alert` (danger "The refund didn’t go through" in the top slot for failed: nothing refunded, explanation kept), `sy-button` (Cancel, disabled while busy; primary "Send decision", busy "Sending", "Try again" after a failure) | `ResolveProblemDialog` in `@sycapooke/orders`; opened by the founder from Problem detail, and by "Retry refund" in its failed state |

## Notifications

| Notification (mock folder: states) | Surface | Library components | Local UI |
|---|---|---|---|
| Account banner (`notifications/account-banner`: info, success, warning) | banner above the top bar | not yet composed | not yet composed |
| Reservation timer (`notifications/reservation-timer`: info, warning, danger, success) | timer under the checkout title; warning and danger banners at the top of checkout | `sy-reservation-timer`, `sy-alert` (banner, icon `clock`) | None; success is the confirmation page until `ToastService` exists |
| Toasts (`notifications/toast`: info, success, warning, danger, with-action, stacked) | toast region | `sy-toast-region`, `sy-toast`, `ToastService` | None |
| Cookie consent (`notifications/cookie-consent`: info, success) | fixed bottom panel, `@sycapooke/privacy` `CookieConsentComponent` in the root component | `sy-consent-banner` (info); `ToastService` success toast "Cookie choice saved" (success) | None |
| System notices (`notifications/system-banner`: info, success, warning, danger) | banner above the top bar | not yet composed | not yet composed |

## Summary of local UI to build

1. `main#main` with the router outlet and the home placeholder (`projects/sycapooke`).
2. The auth layout of the auth shell (`projects/sycapooke`).
3. Heading blocks of the auth pages and the sell page header, form sections and sticky actions bar.
4. The founder's pages (Shop, Edit story, Support queue, Problem detail and Sale): the linked stat panels, the `.settings-list` link rows, the support filter group, the portrait row and "Public location" fieldset, and the account and shop sub-navigation (`.subnav` of `.tab` links), for catalogue change control.
5. Everything marked "not yet composed" is composed, and its library components added to the catalogue, when its journey is implemented.
