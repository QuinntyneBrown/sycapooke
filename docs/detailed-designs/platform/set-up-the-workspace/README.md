# Set up the workspace

## Overview

Sycapooke is the founder's shop for antiques and vintage treasures jewelry, clothing, housewares and electronics. Before any
feature can be built, a developer needs a repository that builds, lints and tests on the
first day. This feature, part of the `platform` subsystem, creates that starting point:
the backend solution, the frontend workspace, the empty application shell, the test
harnesses and one command that verifies all of it. It is the prerequisite for every other
journey in `docs/journeys.md`.

Terms used below:

- **workspace** — the Angular monorepo under `src/frontend` with one application project and one library per area, as `AGENTS.md` mandates
- **solution** — the .NET solution under `src/backend` with the six `Sycapooke.*` projects
- **verify command** — the single command that restores, builds, lints and tests both stacks and exits non-zero on any failure (`L2-071`)
- **empty shell** — the application frame of [responsive-accessible-shell](../responsive-accessible-shell/README.md) in its guest state, with a placeholder home page and no feature screens
- **architecture test** — a unit test that inspects the compiled assemblies for forbidden registrations (`L2-060`)

The feature builds no product behaviour. The shell components it ships are the subset of
[responsive-accessible-shell](../responsive-accessible-shell/README.md) that the frame
needs (skip link, guest top bar, footer, toast region); the rest of that design, the
accounts subsystem's `AuthState`, HTTP interceptors beyond the base URL provider, EF
Core, the outbox, Redis, Docker, dependency scanning (`L2-052`, `L2-053`), visual
snapshots and axe runs over every route are deferred to their own features.

## Description

This design targets the founder-only shop under [ADR general/0010](../../../adr/general/0010-founder-only-shop.md). Existing descriptions identify reusable parts, not evidence that revised acceptance criteria pass. The configured founder alone manages inventory, fulfilment and support; customers have private buying accounts.

### Backend solution (`src/backend/Sycapooke.sln`)

Target framework `net10.0` (.NET 10 LTS SDK, pinned in `global.json`), nullable and
implicit usings on, warnings as errors.

- **`Sycapooke.Domain`** — class library; no package references. Empty apart from the
  `Sycapooke.Domain` namespace marker.
- **`Sycapooke.Application`** — class library referencing `Domain`. Empty apart from the
  namespace marker; MediatR and FluentValidation arrive with the first command.
- **`Sycapooke.Infrastructure`** — class library referencing `Application`. Empty apart
  from the namespace marker; EF Core arrives with the first entity.
- **`Sycapooke.Api`** — ASP.NET Core Web API referencing `Application` and
  `Infrastructure`. `Program.cs` registers controllers and maps them; `partial class
  Program` exposes the entry point to `WebApplicationFactory`. **`HealthController`**
  answers `GET /api/health` with `200` and the body `{ "status": "ok" }`; it touches no
  dependency, so it succeeds with no database (`L2-071` criterion 4). Tests run it in-process through `WebApplicationFactory`; no test uses SQL Server, LocalDB or a connection string ([ADR-0004](../../../adr/platform/0004-health-check-without-database.md)). The liveness and
  readiness endpoints of `L2-070` belong to
  [report-errors-and-observe](../report-errors-and-observe/README.md).
- **`Sycapooke.Worker`** — worker service referencing `Application` and `Infrastructure`,
  with an empty `Worker` hosted service.
- **`Sycapooke.Migrator`** — console app referencing `Infrastructure`; prints that no
  migrations exist yet and exits 0.
- **`Directory.Build.props`** — shared `TargetFramework`, `Nullable`, `ImplicitUsings`,
  `TreatWarningsAsErrors` and the `Sycapooke` root namespace.
- **`.editorconfig`** at `src/backend` — the `dotnet format` rules the verify command
  checks with `--verify-no-changes`.

### Backend tests (`tests/`)

One xUnit project per backend project, named `tests/Sycapooke.<Project>.Tests`, each
referencing its project:

- **`Sycapooke.Api.Tests`** — `HealthEndpointTests` uses `WebApplicationFactory<Program>`
  to call `GET /api/health` and asserts `200`, `application/json`, and that the body
  contains neither a version number, the machine name nor the text `ConnectionString`.
  **`StatelessApiTests`** (NetArchTest) loads `Sycapooke.Api` and `Sycapooke.Application`
  and fails when any type depends on the `Microsoft.Extensions.Caching.Memory` or
  `Microsoft.AspNetCore.Session` namespaces (`L2-060` criterion 1). A deliberate
  `IMemoryCache` dependency was used to confirm the test fails. It is the one architecture test of this feature.
- **`Sycapooke.Application.Tests`**, **`Sycapooke.Domain.Tests`**,
  **`Sycapooke.Infrastructure.Tests`**, **`Sycapooke.Worker.Tests`**,
  **`Sycapooke.Migrator.Tests`** — each holds one smoke test that the project's marker
  type loads, so every test project runs in CI from day one.

### Frontend workspace (`src/frontend`)

Angular 22 workspace (Angular CLI 22) on Node 22 LTS, standalone components, signals,
`npm ci`. `angular.json` registers the application and eight libraries; `tsconfig.json`
maps `@sycapooke/<library>` to each `projects/<library>/src/public-api.ts`.

- **`projects/sycapooke/`** — the application. `src/main.ts`, `src/index.html`,
  `src/styles.css` (imports the tokens and component styles of `@sycapooke/components`),
  `src/app/app.config.ts` (router with the feature routes, `provideApi()` from
  `@sycapooke/api`), `src/app/app.routes.ts` (`''` renders `HomePlaceholderPage`; each feature library's empty routes are attached with `loadChildren` under a provisional path that the feature's own design confirms),
  **`AppComponent`** (renders `sy-skip-link`, the shell for the active route and
  `sy-toast-region`), **`AppShellComponent`** (`sy-top-bar`, `<main id="main"
  tabindex="-1"><router-outlet>`, `sy-footer`) and **`HomePlaceholderPage`** (an `h1`
  "Treasures with a story." and one paragraph). Auth, founder-management and bare shells are
  `<TO SUPPLY>` by the features that first use them.
- **`projects/components/`** (`@sycapooke/components`, prefix `tl`) — `src/styles/tokens.css`
  and `src/styles/components.css` copied from `docs/design-system/tokens/tokens.css` and
  `docs/design-system/assets/components.css`, with the `L2-061` breakpoint values.
  Components, each with `.ts`, `.html` and `.css`: **`sy-skip-link`**, **`sy-logo`**,
  **`sy-top-bar`** (guest state only: menu button with `aria-expanded="false"`, logo,
  primary navigation, search form, "Sell" and "Sign in" links), **`sy-footer`**
  (Marketplace rules, Privacy, Terms, Cookie settings, "© {year} Sycapooke" from `Date`; Cookie
  settings links to `/privacy#cookie-settings` until the cookie-settings dialog exists),
  **`sy-toast-region`** (`role="status"`, `aria-live="polite"`, empty until
  `ToastService` is implemented). Exported from `public-api.ts`.
- **`projects/api/`** (`@sycapooke/api`) — `provideApi(baseUrl)` registers `HttpClient` (fetch
  backend) and the `API_BASE_URL` injection token; no services yet.
- **`projects/accounts|catalog|listings|orders|reviews|privacy/`** — each exports
  `routes: Routes = []` from `src/lib/<area>.routes.ts` through `public-api.ts`, plus
  `ng-package.json`, `package.json`, `README.md` and `tsconfig.lib.json`.
- **Unit tests** — the workspace has no Vitest or Karma target: frontend behaviour is
  covered by Playwright, per the `AGENTS.md` testing rules, and the CLI's spec files were
  removed.
- **Lint and format** — `eslint.config.js` with `angular-eslint` (TypeScript and
  template rules, including the accessibility rules); `.prettierrc` with
  `prettier --check` in the lint script.
- **Scripts** in `src/frontend/package.json`: `build:all` builds every library then the
  application; `lint` runs Prettier and ESLint; `test:e2e` runs Playwright; **`verify`**
  is the verify command and runs, in order, `dotnet restore`, `dotnet format
  --verify-no-changes`, `dotnet build --no-restore -warnaserror`, `dotnet test
  --no-build` on `../backend/Sycapooke.sln`, then `lint`, `build:all` and `test:e2e`. It
  assumes `npm ci` has run once. Any non-zero exit stops the chain. Each step is also a
  script of its own (`backend:verify`, `lint`, `build:all`, `test:e2e`).

### End-to-end tests (`src/frontend/e2e`)

Playwright, Chromium only. Per the `AGENTS.md` testing rules the backend never runs: `webServer` serves the production build of `projects/sycapooke` (`dist/sycapooke/browser`) as static files on port 4200 with SPA fallback, and every test installs a catch-all `page.route('**/api/**')` fixture so no request can reach `Sycapooke API`.

- **`e2e/pages/shell.page.ts`** — page object with locators for the skip link,
  `main#main`, the top bar, the footer links and a `goto()`.
- **`e2e/tests/shell.spec.ts`** — header `// Traces to: L2-071, L2-065`; at 320 × 640
  asserts the skip link is the first focusable element, `main#main` exists, the guest
  top bar shows "Sign in", the four footer links are present and
  `document.documentElement.scrollWidth <= window.innerWidth` (`L2-071` criterion 5).
  It also asserts that no request left the browser for `/api/**` unfulfilled by a fixture.

### Continuous integration (`.github/workflows/ci.yml`)

Runs on every pull request and push to `main` on `ubuntu-latest`: checks out, installs
Node 22 and the .NET 10 SDK, runs `npm ci` and `npx playwright install --with-deps
chromium` in `src/frontend`, then `npm run verify`. A failing step fails the workflow
(`L2-071` criteria 2 and 3).

### Configuration values

| Setting | Value | Source |
|---------|-------|--------|
| .NET SDK | 10.0.x (`global.json`, `rollForward: latestFeature`) | [ADR-0001](../../../adr/platform/0001-runtime-versions.md) |
| Node | 22 LTS (`engines` in `package.json`) | ADR-0001 (platform) |
| Angular | 22.x | ADR-0001 (platform) |
| API base URL | `environment.apiBaseUrl`: `/api` in production, `http://localhost:5045/api` in development (the API's launch profile) | `projects/sycapooke/src/environments` |
| Playwright browser | Chromium | `AGENTS.md` |

### Implementation evidence

Completed behavior evidence: `sequence-verify` in `src/frontend/package.json` and `tests/Sycapooke.Api.Tests/StatelessApiTests.cs`. 1 of 1 target behaviors are counted. Remaining target behavior and acceptance checks are not counted merely because older marketplace tests pass.

Effort in the matching estimate is the full target feature effort in developer-days, not remaining work. Completion counts whole verified behaviors and rounds to the nearest ten percent. Source and existing acceptance tests supply the evidence; this documentation refresh does not execute new target tests.

### Existing workspace and target alignment

The current workspace already contains application shells and shared overlay services. This feature records the verification and dependency baseline rather than claiming those projects are still empty. The founder-only navigation and account contracts require later implementation against the revised mocks and composition. Security and accessibility verification remains owned by the corresponding platform designs.

## Requirements

| L2 ID | Refines (L1) | Requirement |
| --- | --- | --- |
| `L2-053` | `L1-013` | CI shall scan .NET and npm dependencies and run static analysis on every pull request. A high or critical vulnerability with a fix available shall fail the build. |
| `L2-060` | `L1-015` | The API shall be stateless so service processes can be replaced without losing account, order or reservation state. Sessions, rate limits and reservations shall work across instances. |
| `L2-061` | `L1-016` | Every page and dialog shall render without horizontal scrolling from 320px wide upwards. The layout shall adapt at these breakpoints: extra small (<576px), small (>=576px), medium (>=768px), large (>=992px) and extra large (>=1200px). |
| `L2-064` | `L1-017` | Every page and dialog shall conform to WCAG 2.2 level AA. Automated accessibility checks shall run in CI on every page and fail the build on any violation. |
| `L2-065` | `L1-017` | Every function shall be usable with a keyboard alone, with a visible focus indicator and a logical focus order. Dialogs shall trap focus and return it on close. Every photo shall have alt text, every form field a visible label, and errors shall be announced to screen readers. |
| `L2-071` | `L1-019` | One command, run from a fresh clone, shall restore, build, lint and test the backend solution (`src/backend`) and the frontend workspace (`src/frontend`), and shall fail when any step fails. The API shall answer `GET /api/health` with HTTP 200 without a database or any other external dependency. The frontend command shall build every library and the application. The same command shall pass on Windows and on Linux CI. Statelessness is checked by the architecture test of L2-060; breakpoints follow L2-061; the shell's accessibility follows L2-064 and L2-065; dependency scanning (L2-053) is not part of this command. |

## Diagrams

### Containers

The developer's machine and the CI runner hold the same two containers, `Sycapooke Web` and
`Sycapooke API`, plus the test projects. No database, cache or external system is needed to
verify the workspace.

![C4 container view for setting up the workspace](diagrams/c4-container.png)

### Components

The verify command fans out to the backend solution and the frontend workspace; the
application depends on the feature libraries, which depend on `api` and `components`.

![C4 component view for setting up the workspace](diagrams/c4-component.png)

### Behaviour — verify the workspace

The verify command runs each step in order and stops at the first failure. CI runs the
same command on Linux.

![Sequence diagram for the verify command](diagrams/sequence-verify.png)

### System context

The system context view makes the feature boundary and its dependencies explicit. The structure represents the target responsibilities described in this feature.

![System context for set up the workspace](diagrams/c4-context.png)

### Class structure

The class structure view makes the feature boundary and its dependencies explicit. The structure represents the target responsibilities described in this feature.

![Class structure for set up the workspace](diagrams/class-structure.png)
