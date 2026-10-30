# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

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
