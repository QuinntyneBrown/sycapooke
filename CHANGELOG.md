# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- HTTP hardening: security headers on every API response, an exception handler that returns problem details, and per-address rate limits with `Retry-After` for sign-in, registration and password reset.
- Shipping addresses are sealed at rest with AES-256-GCM under a rotating key ring (`Privacy__FieldEncryptionKeys`).
- The privacy page, terms, shop policies (`/rules`) and server error page; CI audits npm and NuGet dependencies.
- Founder story: `GET`/`PUT /api/shop/story`, the `/shop/story` page, and a public page that resolves only the founder (everyone else is a uniform 404). The portrait upload is still to come.
- Customer closure: `POST /api/account/closure`, the Close account dialog in Settings, immediate session revocation, and a worker job that erases personal data, leaving orders with "Former customer".
- A purchase confirmation email to the customer when payment succeeds.
- The founder's support queue at `/shop/support` (`GET /api/shop/support`) listing customers' problems and cancellation requests; problems resolve at `/shop/problems/{reference}` with `RefundBuyer` or `CloseWithoutRefund`, and a `Pending` refund is retried by the worker until the provider confirms it. Purchases and sales show the refund state.
- Founder access ([ADR platform/0018](docs/adr/platform/0018-founder-access-and-shop-contracts.md)): the `ShopIdentity` singleton, `Shop:FounderMemberId`, `IFounderAccess`, `Sycapooke.Migrator --provision-founder`, and a server-derived `isFounder` on `/api/auth/me`; "Sell an item" shows only to the founder and the guest "Start selling" button is gone.
- Private accounts and sessions: `GET`/`PATCH /api/account`, `POST /api/account/password`, `POST /api/auth/sign-out-everywhere`, password reset by emailed one-hour link, and a sign-in lockout of 15 minutes after 5 failures with an emailed notice.
- Account pages: `/account`, `/account/profile`, `/account/settings` with Change password and Sign out everywhere dialogs, `/forgot-password`, `/reset-password`, the welcome-back screen at `/sign-in?expired=1`, the locked sign-in state, `/403` and the not-found page; `signedInGuard` and `founderGuard`; the `sy-subnav` component.
- The demo seeder creates one founder who owns every demo listing, plus customers, and binds the founder.

- Founder-only inventory: drafts, publishing, photo uploads, edits, withdrawals and inventory require founder access (403 `FOUNDER_REQUIRED` otherwise); the catalog, counts, brands and listing detail show only the founder's listings; `/shop`, `/sell` and inventory routes are guarded.
- `sy-founder-summary` replaces the seller summary on the listing detail page.

### Removed

- Review emails: the category, the preference and the Reviews row on the Emails page.

- Payouts, fees and the administrator: the payout and webhook endpoints, the payout release job, the payouts page and banner, and `/api/admin/problems`. Sales, shipping and cancellation answers are founder-only (403 `FOUNDER_REQUIRED`) and the founder's pages moved to `/shop/sales/*`. Historical payout tables are kept.

- The fee preview and `sy-earnings` from the sell form, `feePercent` from the selling options, and the payout prompt from publishing.

### Added (earlier)

- `https://www.sycapooke.com/` and `https://sycapooke.com/` serve the marketing application at their root: Namecheap points the apex at the VM's static IP and `www` at the apex, the `sycapooke-marketing` image also serves its build with base href `/` on port 8080, and `deploy/edge/Caddyfile` sends both hosts there; `https://app.sycapooke.com/` serves the full application, and the marketing build's links open it there ([ADR platform/0017](docs/adr/platform/0017-sycapooke-com-shows-the-marketing-site.md)).

## [0.3.0] — 2026-10-30

### Added

- `app.sycapooke.com` serves the full application from its own `sycapooke-app` image, and `www.sycapooke.com` the marketing site from its own `sycapooke-marketing` image, behind an `edge` Caddy service (the official image with `deploy/edge/Caddyfile`) that terminates TLS for the apex, `www` and `app` hosts, redirects the apex to `www` and routes by host ([ADR platform/0015](docs/adr/platform/0015-separate-marketing-and-application-deployments.md), `L2-084` to `L2-087`).
- Two deploy workflows, `deploy-marketing.yml` and `deploy-app.yml`, each started by a green `CI` run for a push to `main` or by hand with a commit SHA, each with its own concurrency group, image tag (`MARKETING_TAG`, `APP_TAG`) and smoke test; the app workflow alone runs the migrator and restarts `api`, `worker` and `app`.
- `provideAppOrigin` / `APP_ORIGIN` / `externalHref` in `@sycapooke/components`: the marketing build (`environment.marketing.ts`, `appOrigin: 'https://app.sycapooke.com'`) renders the top bar, nav menu, footer and welcome-page calls to action as absolute links into the application, and its search sends the visitor to `https://app.sycapooke.com/?q=…`; the application build keeps router links.
- `DeploymentPipelineTests` covers both workflows, the compose stack and the three Caddyfiles.

### Changed

- `docs/deployment.md`, the deploy-to-production design and diagrams, `L2-084` to `L2-087` and the Google sign-in docs describe the two hosts and workflows; `App__BaseUrl` and the Google OAuth origin and redirect URI are on `app.sycapooke.com`.

### Removed

- The `uat.sycapooke.com` host, the single `sycapooke-web` image, `deploy/web/`, `deploy.yml`, `IMAGE_TAG` and `npm run build:web` ([ADR platform/0013](docs/adr/platform/0013-uat-host-in-the-production-web-image.md) is superseded).

## [0.2.0] — 2026-10-30

### Added

- The pitch video is back: a 29-second film (MP4 H.264/AAC with a WebM VP9/Opus fallback and English WebVTT captions at `/media/sycapooke-pitch.*`) whose poster is the hero photograph of `/welcome`. One "Play · 0:29" pill opens the Pitch video dialog (`sy-dialog`, heading "Sycapooke video" hidden) with `sy-video-player`: native controls, Play/Pause, Captions off by default, a loading spinner and a "The video didn’t load" state with Try again; nothing plays or sounds before the visitor presses Play (`L2-089` to `L2-091`, [ADR platform/0014](docs/adr/platform/0014-pitch-video.md)).
- `sy-video-poster` and `sy-video-player` in the Heirloom design system and `@sycapooke/components`; the `pitch-video` dialog mocks (default, loading, error, captions).
- `docs/mocks/_build/video/build_video.py` builds the video from Playwright-rendered slides with Kokoro-82M narration (voice `af_heart`) and a ducked Pixabay track, credited in `public/media/ATTRIBUTION.md`.

### Changed

- The hero photograph of `/welcome` is the video poster: a named "Play" button with a decorative image (`L2-083`, `L2-078`).
- `AGENTS.md` keeps the mascot, print collateral and advertising placements out of scope; the video no longer is.

## [0.1.0] — 2026-10-30

### Added

- Sycapooke, a peer-to-peer marketplace made only for vintage jewelry, clothing, housewares and electronics, cloned from TuTuMuch (itself a clone of 12L) into a fresh repository with no shared history ([ADR general/0009](docs/adr/general/0009-cloned-from-tutumuch.md)). Every journey arrives working: membership and sign-in (password and Google), listings with photos, the catalog with search and filters, checkout, shipping, completion, cancellation, payouts, order problems, email notifications and preferences, cookie consent, public profiles, the marketing page, the single `npm run verify` command, continuous deployment to one small VM and the UAT host.
- The Heirloom design system, extracted from the Sycapooke landing page built in Magic Patterns: Paper, Cream, Ink, Muted, Line, Terracotta, Forest and Mustard; Fraunces (display and the italic wordmark) and Source Sans 3 (body), self-hosted; pill buttons, rounded cards and a terracotta price-tag logo. `tokens.css` carries the Heirloom palette and the Sycapooke layout tokens; the design-system pages, mocks and components follow it.
- The vintage catalogue: four categories (Jewelry, Clothing, Housewares, Electronics) with 24 subcategories at `/jewelry`, `/clothing`, `/housewares` and `/electronics`, vintage makers as suggested brands, and the vintage lock: every listing's size is `Vintage`, set by the server from the category ([ADR general/0006](docs/adr/general/0006-vintage-only.md), `L2-011` to `L2-013`).
- The marketing page at `/welcome` rewritten as "Treasures with a story": hero and founder photographs, For shoppers, For sellers, How it works and Our story bands with the founder's quote, link-preview tags and the share image (`L2-078`, `L2-079`, `L2-083`, [ADR general/0005](docs/adr/general/0005-marketing-wording.md)).

### Changed

- Renamed throughout: TuTuMuch → Sycapooke (`Sycapooke.*` projects, `projects/sycapooke/`, `@sycapooke/*` aliases, `ConnectionStrings__Sycapooke`), selector prefix `tl` → `sy`, Giddy → Heirloom, domain `sycapooke.com` (`www`, `uat`), cookies `sy_*`, order references `SY-`, images `sycapooke-*`, repository `QuinntyneBrown/sycapooke`.
- Demo data (`Sycapooke.Cli seed`) and mocks show vintage pieces, vintage makers and sellers downsizing a family home.

### Removed

- The 30-second pitch video, its dialog, poster, player, captions, media files and build script; `L2-080` to `L2-082` are retired and never reused.
- The mascot and all print collateral (`docs/print/` and ADR general/0007).
- The advertising placements shown on the landing page are not a product feature (deferred, ADR general/0005).
