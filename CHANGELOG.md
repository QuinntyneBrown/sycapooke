# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- `https://sycapooke.com/` shows the marketing site and `www.sycapooke.com` redirects to it: Namecheap points the apex at the VM's static IP and `deploy/edge/Caddyfile` has site blocks for both hosts; other paths redirect to the application on the Azure DNS name ([ADR platform/0017](docs/adr/platform/0017-sycapooke-com-shows-the-marketing-site.md)).

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
