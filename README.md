<p align="center">
  <picture>
    <source media="(prefers-color-scheme: dark)" srcset="docs/design-system/assets/logo-dark.svg">
    <img src="docs/design-system/assets/logo.svg" alt="Sycapooke" width="240">
  </picture>
</p>

# Sycapooke

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![Frontend: Angular](https://img.shields.io/badge/Frontend-Angular-dd0031.svg)](https://angular.dev)
[![Backend: .NET](https://img.shields.io/badge/Backend-.NET-512bd4.svg)](https://dotnet.microsoft.com)
[![PRs Welcome](https://img.shields.io/badge/PRs-welcome-brightgreen.svg)](CONTRIBUTING.md)
[![Contributor Covenant](https://img.shields.io/badge/Contributor%20Covenant-2.1-4baaaa.svg)](CODE_OF_CONDUCT.md)

**Treasures with a story.** Sycapooke is a peer-to-peer marketplace made only for **vintage**: jewelry, clothing, housewares and electronics. Every piece is at least 20 years old and listed by a real person with a real story. It is for families downsizing a lifetime of treasures, and for the people who'll love them next.

Sycapooke is a responsive web application built with [Angular](https://angular.dev) and [ASP.NET Core](https://dotnet.microsoft.com). It began as a clone of TuTuMuch (itself a clone of 12L), so every journey and screen mirrors them; only the domain words, the vintage rule and the look differ ([ADR general/0009](docs/adr/general/0009-cloned-from-tutumuch.md)).

> [!NOTE]
> Sycapooke is in active development. Every journey in `docs/journeys.md` is built end to end; payment, payout, carrier, email and analytics providers are still placeholders. See [Project status](#project-status).

## Contents

- [Overview](#overview)
- [Project status](#project-status)
- [Getting started](#getting-started)
  - [Load demo data](#load-demo-data)
- [Configuration](#configuration)
- [Testing](#testing)
- [Deployment](#deployment)
- [API reference](#api-reference)
- [Architecture](#architecture)
- [Repository structure](#repository-structure)
- [Documentation](#documentation)
- [Contributing](#contributing)
- [Security](#security)
- [Support](#support)
- [License](#license)

## Overview

General marketplaces bury vintage among everything else. On Sycapooke every listing is vintage, so shoppers browse only pieces with at least 20 years behind them, listed by the people who kept them. The API enforces the vintage lock (every listing's size is `Vintage`, set by the server from its category) whatever the client sends ([ADR general/0006](docs/adr/general/0006-vintage-only.md)). Sycapooke's owner also sells on the marketplace as a regular member, with the same rules as everyone else.

| Category (page)              | Subcategories                                                                                       | Accepted size | Condition   |
| ---------------------------- | --------------------------------------------------------------------------------------------------- | ------------- | ----------- |
| Jewelry (`/jewelry`)         | Necklaces, Brooches and pins, Earrings, Bracelets, Rings, Watches                                   | Vintage       | New or used |
| Clothing (`/clothing`)       | Dresses, Coats and jackets, Knitwear, Shirts and blouses, Skirts and trousers, Hats and accessories | Vintage       | New or used |
| Housewares (`/housewares`)   | Tableware, Glassware, Ceramics and pottery, Lighting, Linens, Decor                                 | Vintage       | New or used |
| Electronics (`/electronics`) | Radios, Cameras, Audio, Telephones, Clocks, Typewriters and office                                  | Vintage       | New or used |

The look is the **Heirloom** design system, extracted from the Sycapooke landing page: paper and cream surfaces, ink text, terracotta buttons, forest bands and mustard highlights, set in Fraunces and Source Sans 3 ([docs/design-system](docs/design-system/README.md)).

## Project status

| Area          | Status    | What works today                                                                                                                                                     |
| ------------- | --------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Accounts      | Available | Join, email verification, sign in (with a password or with Google) and sign out with session cookies and rotating refresh tokens                                     |
| Listings      | Available | Sell page with drafts, publishing, photo upload and processing, and a daily publish limit                                                                            |
| Catalog       | Available | Home and four category pages (`/jewelry`, `/clothing`, `/housewares`, `/electronics`), paged newest first, search and filters, and the listing detail page at `/items/{reference}` |
| Orders        | Available | Checkout, shipping, receipt confirmation, payouts and order problems                                                                                                 |
| Privacy       | Partial   | Cookie consent; account closure and erasure are planned                                                                                                              |
| Reviews       | Available | Public seller profile; ratings show "No reviews yet"                                                                                                                 |
| Notifications | Available | Transactional email through an outbox, email preferences and one-click unsubscribe                                                                                   |
| Marketing     | Available | Public page at `/welcome` ("Treasures with a story") for shoppers and sellers, with the founder's story and link-preview tags; linked as "About Sycapooke" in the footer |

Overall progress and effort are tracked in [docs/estimates](docs/estimates/README.md). Moderation, writing reviews and personal data export are deferred by [ADR-0001](docs/adr/general/0001-defer-moderation-reviews-and-data-export.md).

## Getting started

### Prerequisites

- [Git](https://git-scm.com)
- [.NET SDK 10.0.402 or later](https://dotnet.microsoft.com/download) (pinned in `src/backend/global.json`)
- [Node.js 22 LTS](https://nodejs.org) and npm
- [SQL Server Express LocalDB](https://learn.microsoft.com/sql/database-engine/configure-windows/sql-server-express-localdb) or another SQL Server instance, to run the API locally

The automated tests don't need a database. They fake every external system.

### Clone and verify

```bash
git clone https://github.com/QuinntyneBrown/sycapooke.git
cd sycapooke/src/frontend
npm ci
npx playwright install chromium
npm run verify
```

`npm run verify` is the same command CI runs. It restores, format-checks, builds and tests the backend, lints the frontend with Prettier and ESLint, builds every Angular library and the application, and runs the Playwright tests. It stops at the first failure.

### Run locally

1. Set the connection string. The API and the migrator read `ConnectionStrings__Sycapooke`.

   ```powershell
   $env:ConnectionStrings__Sycapooke = 'Server=(localdb)\MSSQLLocalDB;Database=Sycapooke;Trusted_Connection=True;TrustServerCertificate=True'
   ```

2. Apply the database migrations.

   ```powershell
   dotnet run --project src/backend/Sycapooke.Migrator
   ```

3. Start the API on `http://localhost:5045`.

   ```powershell
   dotnet run --project src/backend/Sycapooke.Api --launch-profile http
   ```

4. In a second terminal, start the Angular app.

   ```powershell
   cd src/frontend
   npm start
   ```

5. Open [http://localhost:4200](http://localhost:4200). The development server proxies `/api` and `/photos` to the API.

To check that the API is up, send `GET http://localhost:5045/api/health`. It returns `200`.

> [!IMPORTANT]
> **Windows on Arm:** LocalDB ships only as x64, so an arm64 process can't load its client and fails with SQL error 193. Build with your usual SDK, then run the built assemblies with the x64 host:
>
> ```powershell
> dotnet build src/backend/Sycapooke.sln
> & "C:\Program Files\dotnet\x64\dotnet.exe" src/backend/Sycapooke.Migrator/bin/Debug/net10.0/Sycapooke.Migrator.dll
> & "C:\Program Files\dotnet\x64\dotnet.exe" src/backend/Sycapooke.Api/bin/Debug/net10.0/Sycapooke.Api.dll --urls http://localhost:5045
> ```
>
> This requires the x64 .NET 10 runtime under `C:\Program Files\dotnet\x64`.

### Load demo data

`Sycapooke.Cli` resets a local database and fills it with 8 demo members and 48 listings with
real photos of vintage pieces, 12 in each category, so `/`, the four category pages
and the listing pages look real. It is for your own machine only; see
[its README](src/backend/Sycapooke.Cli/README.md) for the demo accounts and password.

```bash
# from src/backend, with SQL Server LocalDB (or any SQL Server you own)
export ConnectionStrings__Sycapooke="Server=(localdb)\MSSQLLocalDB;Database=Sycapooke;Trusted_Connection=True;TrustServerCertificate=True"
export DOTNET_ENVIRONMENT=Development   # reset refuses to run otherwise, unless --force
dotnet build Sycapooke.Cli
dotnet Sycapooke.Cli/bin/Debug/net10.0/Sycapooke.Cli.dll seed --reset
```

Start the API with the same `ConnectionStrings__Sycapooke` so it reads the seeded database.
On Windows on Arm, LocalDB is x64 only: build with the normal SDK, then run the built DLL
with the x64 host, `"C:\Program Files\dotnet\x64\dotnet.exe" Sycapooke.Cli\bin\Debug\net10.0\Sycapooke.Cli.dll seed --reset`
(the arm64 host fails with error 193). Run the API the same way.

## Configuration

The API reads standard ASP.NET Core configuration: `appsettings.json`, `appsettings.{Environment}.json` and environment variables.

| Setting                        | Environment variable           | Description                                                                                                  |
| ------------------------------ | ------------------------------ | ------------------------------------------------------------------------------------------------------------ |
| `ConnectionStrings:Sycapooke`  | `ConnectionStrings__Sycapooke` | SQL Server connection string. Required to run the API and the migrator.                                      |
| `Auth:SigningKey`              | `Auth__SigningKey`             | Key that signs access tokens. The development value in `appsettings.Development.json` is for local use only. |
| `App:BaseUrl`                  | `App__BaseUrl`                 | Public URL of the frontend, used in links such as email verification.                                        |
| `Selling:PlatformFeePercent`   | `Selling__PlatformFeePercent`  | Fee shown to sellers when they price a piece.                                                                |
| `Selling:PublishesPerDay`      | `Selling__PublishesPerDay`     | Maximum number of listings a member can publish per day.                                                     |
| `Selling:SuggestedBrands`      | `Selling__SuggestedBrands__0`… | Vintage makers offered on the sell page (Trifari, Pyrex, Kodak, Seiko...). Sellers may type any brand.       |

## Testing

Run each check from `src/frontend`.

| Command                  | Runs                                                 |
| ------------------------ | ---------------------------------------------------- |
| `npm run verify`         | Every check below, in order                          |
| `npm run backend:verify` | Backend restore, format check, build and xUnit tests |
| `npm run lint`           | Prettier check and ESLint                            |
| `npm run build:all`      | Every library, then the application                  |
| `npm run test:e2e`       | Playwright (Chromium) against the production build   |

Backend tests use in-memory substitutes and never connect to a real database. Playwright tests serve the API through route fixtures and never start the backend. See [AGENTS.md](AGENTS.md#testing-rules-critical) for the full rules.

## Deployment

Every push to `main` deploys to `https://www.sycapooke.com` (the marketing page) and `https://uat.sycapooke.com` (the full application, on the same API and database) once CI's `npm run verify` passes: the Deploy workflow builds the four images (`sycapooke-api`, `sycapooke-worker`, `sycapooke-migrator`, `sycapooke-web`), pushes them to GitHub Container Registry, migrates the database, restarts the Docker Compose stack on one small Azure Linux VM and checks `/api/health` and the UAT host. Run the Deploy workflow by hand with an older commit SHA to roll back. See [docs/deployment.md](docs/deployment.md) and ADR platform/0012.

## API reference

Endpoints are served under `/api`, except processed photos under `/photos`. Endpoints marked _member_ need a signed-in session; listing and photo upload endpoints also need a verified email.

| Method | Path                                | Description                                            |
| ------ | ----------------------------------- | ------------------------------------------------------ |
| `GET`  | `/api/health`                       | Liveness check                                         |
| `POST` | `/api/auth/register`                | Create an account                                      |
| `POST` | `/api/auth/verify-email`            | Confirm an email address                               |
| `POST` | `/api/auth/verification-email`      | Resend the verification email                          |
| `POST` | `/api/auth/sign-in`                 | Start a session                                        |
| `POST` | `/api/auth/refresh`                 | Rotate the refresh token                               |
| `POST` | `/api/auth/sign-out`                | End the session                                        |
| `GET`  | `/api/auth/me`                      | Current member _(member)_                              |
| `GET`  | `/api/auth/providers`               | Which third-party sign-ins are configured              |
| `GET`  | `/api/auth/google/start`            | Leave for Google's consent page (`returnUrl`)          |
| `GET`  | `/api/auth/google/callback`         | Back from Google: sign in, link or start a join        |
| `GET`  | `/api/auth/google/pending`          | The email of a pending Google join                     |
| `POST` | `/api/auth/google/finish`           | Finish a Google join: display name, age, terms         |
| `GET`  | `/api/categories`                   | The four categories, their subcategories and the accepted size (`Vintage`) |
| `GET`  | `/api/selling/options`              | Vintage makers, conditions and fee for the sell page   |
| `POST` | `/api/listings`                     | Create a draft listing _(member)_                      |
| `PUT`  | `/api/listings/{reference}`         | Update a draft _(member)_                              |
| `POST` | `/api/listings/{reference}/publish` | Publish a draft _(member)_                             |
| `GET`  | `/api/me/listings/{reference}`      | One of your own listings _(member)_                    |
| `POST` | `/api/photo-uploads`                | Upload a listing photo _(member)_                      |
| `GET`  | `/api/photo-uploads/{id}`           | Photo upload status _(member)_                         |
| `GET`  | `/api/catalog/listings`             | Active and reserved listings, newest first, paged      |
| `GET`  | `/api/catalog/listings/{reference}` | Listing detail with photos, seller and viewer relation |
| `GET`  | `/photos/{hash}/{file}`             | Processed photo files                                  |

For example, `GET /api/catalog/listings?category=jewelry&subcategory=brooches-and-pins&brand=Trifari&sort=price-asc` lists Trifari brooches from the cheapest up, and a `POST /api/listings` body with `category: "Electronics"`, `subcategory: "Radios"` and `size: "Modern"` is refused with `422 SIZE_NOT_ACCEPTED`, because the server sets every listing's size to `Vintage`.

Detailed contracts and acceptance criteria are in [docs/specs/L2.md](docs/specs/L2.md).

## Architecture

| Layer          | Technology                                   | Responsibility                                                          |
| -------------- | -------------------------------------------- | ----------------------------------------------------------------------- |
| Frontend       | Angular 22, standalone components, signals   | Single-page app: one application shell and one library per feature area |
| API            | ASP.NET Core 10 Web API                      | Controllers, authorization policies, middleware                         |
| Application    | MediatR                                      | Commands, queries, validators, DTOs and ports                           |
| Domain         | C#                                           | Entities, value objects, listing state machine and the vintage lock     |
| Infrastructure | Entity Framework Core, SQL Server, SkiaSharp | Persistence, repositories, outbox and photo processing                  |
| Worker         | .NET hosted service                          | Background jobs                                                         |

Architecture decisions are recorded in [docs/adr](docs/adr).

## Repository structure

```text
sycapooke/
├── .github/                 CI and deploy workflows, issue and pull request templates
├── deploy/                  Production stack: Docker Compose, Dockerfiles, Caddyfile, cloud-init
├── docs/                    Journeys, specs, mocks, Heirloom design system, designs, estimates, ADRs
├── src/
│   ├── backend/             .NET solution: Api, Application, Domain, Infrastructure, Worker, Migrator, Cli
│   └── frontend/            Angular workspace
│       ├── e2e/             Playwright tests and fixtures
│       └── projects/        sycapooke app (public/media/ photos, public/fonts/ Fraunces and Source Sans 3);
│                            api, components, accounts, catalog, listings, orders, reviews,
│                            marketing and privacy libraries
└── tests/                   xUnit test project per backend project
```

## Documentation

| Document                                       | Contents                                        |
| ---------------------------------------------- | ----------------------------------------------- |
| [docs/journeys.md](docs/journeys.md)           | User journeys and their effort                  |
| [docs/specs](docs/specs)                       | L1 and L2 requirements with acceptance criteria |
| [docs/mocks](docs/mocks)                       | HTML mocks for every screen and state           |
| [docs/design-system](docs/design-system)       | Heirloom: tokens, foundations and components    |
| [docs/screen-composition.md](docs/screen-composition.md) | Which library components each screen uses |
| [docs/detailed-designs](docs/detailed-designs) | Per-feature designs and diagrams                |
| [docs/estimates](docs/estimates/README.md)     | Effort, progress and roadmap                    |
| [docs/deployment.md](docs/deployment.md)       | Production host, deploys and rollback           |
| [AGENTS.md](AGENTS.md)                         | Working agreement for AI coding agents          |
| [CHANGELOG.md](CHANGELOG.md)                   | Notable changes                                 |

## Contributing

Contributions are welcome. Read [CONTRIBUTING.md](CONTRIBUTING.md) before you open an issue or a pull request, and run `npm run verify` before every commit.

This project has adopted the [Contributor Covenant](CODE_OF_CONDUCT.md). By participating, you're expected to uphold it.

## Security

Don't report security vulnerabilities through public issues. See [SECURITY.md](SECURITY.md) for how to report them privately.

## Support

See [SUPPORT.md](SUPPORT.md) for how to file issues and get help. For anything about the marketplace itself, write to hello@sycapooke.com.

## License

Sycapooke is licensed under the [MIT License](LICENSE). The demo photos in
`src/backend/Sycapooke.Cli/SeedPhotos/` and the page images in
`src/frontend/projects/sycapooke/public/media/` are not: they keep their own licences, with authors
and sources in [SeedPhotos/ATTRIBUTION.md](src/backend/Sycapooke.Cli/SeedPhotos/ATTRIBUTION.md) and
[media/ATTRIBUTION.md](src/frontend/projects/sycapooke/public/media/ATTRIBUTION.md). The Fraunces and
Source Sans 3 fonts in `public/fonts/` are under the SIL Open Font License.
