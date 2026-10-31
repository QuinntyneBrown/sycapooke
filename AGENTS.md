# Agent instructions for Sycapooke

Sycapooke is a peer-to-peer marketplace made only for vintage: jewelry, clothing, housewares and
electronics, every piece at least 20 years old and listed by a real person with a real story
([ADR general/0006](docs/adr/general/0006-vintage-only.md)). Anyone can join, set up a profile
and sell the treasures they are downsizing; Sycapooke's owner sells on it as an ordinary member
too. Sycapooke began as a clone of TuTuMuch, which began as a clone of 12L
([ADR general/0009](docs/adr/general/0009-cloned-from-tutumuch.md)), so every journey, screen,
layout, component (including the `sy` selector prefix) and rule mirrors them; only the domain
words, the vintage rule and the look differ. The look is the **Heirloom design system**,
extracted from the Sycapooke landing page: `docs/design-system/tokens/tokens.css` is Heirloom's
token file (Paper, Cream, Ink, Terracotta, Forest, Mustard; Fraunces and Source Sans 3) plus a
short block of Sycapooke layout tokens that alias 12L-only names onto Heirloom tokens. Change
colour, type, shape or motion in Heirloom's tokens, never in components. The voice is warm and
respectful: "treasures" and "story", never "junk", "clutter" or "fast fashion", and never
children's copy. The product is defined, before code, by `docs/journeys.md`, `docs/specs/`
(L1 and L2 requirements), `docs/mocks/` (generated HTML mocks), `docs/detailed-designs/` and
`docs/estimates/`. Architecture decisions live in `docs/adr/`.

## Out of scope at this time (CRITICAL)

The following are **deferred and must not be looked at**: do not write specs, user
journeys, mocks, detailed designs, estimates, prompts, code or tests for them, and do
not re-add references to them in other documents. The decision is recorded in
[ADR-0001](docs/adr/general/0001-defer-moderation-reviews-and-data-export.md).

- **Moderation**: reporting listings or members, the moderation queue, removing
  listings by an administrator, member suspension and reinstatement, the administrator
  audit log, and administrator second factor (TOTP) sign-in.
- **Leave review**: writing reviews after an order, review publishing rules and review
  replies. The public rating summary and profile reviews section stay and show
  "No reviews yet".
- **Export personal data**: the JSON data export and its email. Account closure and
  erasure stay.

- **Load and scale tests**: no load-test seeder, k6 or other load scripts, performance
  environment, multi-instance docker-compose, `Sycapooke.LoadTestSeeder`,
  `Sycapooke.ScaleTests` or latency-under-load requirement. Statelessness (`L2-060`) is
  checked by an architecture test only. See
  [ADR-0002](docs/adr/general/0002-no-load-or-scale-tests.md).
- **Frontend performance testing**: no Lighthouse or Lighthouse CI, no bundle-size or
  gzip budget script, no lazy-chunk check, no `web-vitals` reporter, no telemetry
  endpoint for Core Web Vitals and no frontend performance requirement. Lazy routes
  stay as a workspace convention, not as a measured budget. See
  [ADR-0003](docs/adr/general/0003-no-frontend-performance-testing.md).
- **Mascot, print collateral and advertising placements**: no mascot, no `docs/print/`, and
  no sponsored slots in the catalog. See [ADR general/0009](docs/adr/general/0009-cloned-from-tutumuch.md)
  and [ADR general/0005](docs/adr/general/0005-marketing-wording.md). The pitch video is
  back in scope ([ADR platform/0014](docs/adr/platform/0014-pitch-video.md), `L2-089` to
  `L2-091`): the hero photo of `/welcome` is its poster and nothing plays until pressed.

The removed requirement numbers `L2-038`, `L2-039`, `L2-044` to `L2-047`, `L2-055`,
`L2-057`, `L2-058`, `L2-014` and `L2-080` to `L2-082` are retired; never reuse them.
`L1-012` no longer exists.

Administrators still resolve order problems (`L2-036`) through the admin shell, guarded
by the `Administrator` role check of `L2-010`. That is the only administrator feature.

## Testing rules (CRITICAL)

- **Backend tests never touch a real database.** Unit, integration and architecture tests
  under `tests/Sycapooke.*.Tests/` use mocks, fakes or an in-memory substitute (the EF Core
  in-memory or SQLite in-memory provider). No SQL Server, LocalDB, Testcontainers, Docker or
  connection string to a live database in any test, fixture or CI job. The same applies to
  Redis and every other external system: fake the port.
- **Playwright tests never run the backend.** Every e2e test in `src/frontend/e2e/` serves
  the `Sycapooke API` through `page.route` fixtures or an equivalent fake. No test starts, waits
  for, or calls `Sycapooke.Api`, `Sycapooke.Worker` or a database. The frontend under test is
  the built Angular application only.
- These rules are absolute. Do not add an opt-in flag, an "integration" tag or a separate
  CI lane that relaxes them.

## Folder structure (MANDATORY)

This layout is required. Every file you create must land in one of the folders below;
never invent a parallel folder, rename one of these, or put code outside `src/` and
`tests/`. If a design seems to need a folder that is not listed, add it to this section
first and record the change as an ADR.

```
sycapooke/
├── AGENTS.md
├── README.md
├── .github/workflows/          ci.yml runs `npm run verify` on ubuntu-latest; deploy-marketing.yml and deploy-app.yml each deploy green pushes to main (ADR platform/0015)
├── deploy/                      production stack on one VM (ADR platform/0012, 0015)
│   ├── edge/                    Caddyfile for the `edge` service (official Caddy image): TLS, /welcome/ → marketing and / → app on the Azure host, sycapooke.com and www → marketing at / (ADR platform/0016, 0017)
│   ├── marketing/               Dockerfile + Caddyfile of the `sycapooke-marketing` image (/welcome/ on the Azure host, / on sycapooke.com and www)
│   ├── app/                     Dockerfile + Caddyfile of the `sycapooke-app` image (/ on the Azure host; proxies /api and /photos)
│   ├── backend.Dockerfile       api, worker and migrator from one restore
│   ├── docker-compose.yml       edge, marketing, app, api, worker, migrator (MARKETING_TAG, APP_TAG)
│   └── cloud-init.yaml          the VM's first-boot script
├── docs/
│   ├── journeys.md              user journeys with effort
│   ├── deployment.md            production host, the two deploy workflows, rollback
│   ├── specs/                   L1.md and L2.md requirements
│   ├── mocks/                   generated HTML mocks (_build/ holds the Python sources; _build/video/ builds the pitch video)
│   ├── design-system/           Heirloom: tokens, foundations, components and patterns extracted from the mocks
│   ├── detailed-designs/        {subsystem}/{feature}/README.md + diagrams/
│   ├── estimates/               README.md + one file per subsystem
│   ├── prompts/                 ready-to-run implementation prompts, NN-journey.md in build order
│   └── adr/                     {category}/NNNN-title.md
├── src/
│   ├── backend/                 .NET solution, namespace prefix Sycapooke
│   │   ├── Sycapooke.Api/         ASP.NET Core Web API: controllers, policies, middleware
│   │   ├── Sycapooke.Application/ MediatR commands, queries, validators, DTOs, ports
│   │   ├── Sycapooke.Domain/      entities, value objects, state machines, domain events
│   │   ├── Sycapooke.Infrastructure/ EF Core DbContext, repositories, outbox, adapters
│   │   ├── Sycapooke.Worker/      hosted background jobs
│   │   ├── Sycapooke.Migrator/    applies EF Core migrations
│   │   └── Sycapooke.Cli/         developer tool: `reset` and `seed` a local demo install
│   │       └── SeedPhotos/      {category}/ demo product photos + ATTRIBUTION.md
│   └── frontend/                Angular workspace (see below)
│       ├── angular.json
│       ├── package.json
│       ├── tsconfig.json        path alias @sycapooke/<library> per library
│       ├── eslint.config.js
│       ├── e2e/                 Playwright: pages/, tests/, fixtures.ts, serve.mjs (static build server)
│       └── projects/
│           ├── sycapooke/         the application (projectType "application")
│           │   ├── src/app/     app.config.ts, app.routes.ts, shells only
│           │   └── public/
│           │       ├── media/   hero, founder and share images, the pitch video (mp4, webm) and its captions (vtt) + ATTRIBUTION.md (ADR platform/0011, 0014)
│           │       └── fonts/   Fraunces and Source Sans 3 (woff2) + their SIL OFL licences
│           ├── api/             library: typed HTTP services, DTOs, auth state, interceptors
│           ├── components/      library: Heirloom design-system components and styles (tokens.css)
│           ├── accounts/        library: feature area
│           ├── catalog/         library: feature area
│           ├── listings/        library: feature area
│           ├── orders/          library: feature area (checkout, payouts, admin problems)
│           ├── reviews/         library: feature area (public profile only)
│           ├── marketing/       library: feature area (the /welcome page)
│           └── privacy/         library: feature area
└── tests/
    └── Sycapooke.*.Tests/         unit, integration and architecture tests per backend project
                                 (including Sycapooke.Cli.Tests)
```

`Sycapooke.Cli` is a developer tool only: deployments and CI never run it against a database
([ADR platform/0010](docs/adr/platform/0010-sycapooke-cli-for-demo-seeding.md)).

There is no `moderation` library and no `Sycapooke.Moderation` project. There is no
print-collateral folder; the only video under `public/media/` is the pitch video.

## Frontend workspace layout (MANDATORY)

The frontend is an Angular workspace organised like
`C:\projects\promise-hub\frontend\projects`: one application project and one Angular
library per area, all registered in `angular.json`. Follow these rules:

- **One application**, `projects/sycapooke/`, with `src/app/` holding only
  `app.config.ts`, `app.routes.ts`, the shell components and `main.ts`/`index.html`/
  `styles.css`. It owns no feature screens.
- **One library per feature area** (`accounts`, `catalog`, `listings`, `orders`,
  `reviews`, `privacy`, `marketing`), each with `ng-package.json`, `package.json`, `README.md`,
  `tsconfig.lib.json` and `src/public-api.ts`. Code lives under `src/lib/` grouped by
  screen (`src/lib/sign-in/`, `src/lib/checkout/`...). The library exports its lazy
  route table (`export { routes } from './lib/<area>.routes';`) and nothing the app does
  not need.
- **`projects/api/`** is the only place that calls `Sycapooke API`: typed services per area
  (`src/lib/orders/`, `src/lib/listings/`...), DTOs, `provide-api.ts`, base URL,
  interceptors, auth and session state, and shared result types. Feature libraries
  import from it; they never call `HttpClient` directly.
- **`projects/components/`** holds the Heirloom design-system components extracted from
  `docs/design-system/` (one folder per component under `src/lib/`) plus `src/styles/`
  tokens. Feature libraries compose screens from it and build only screen-specific UI
  locally (see `docs/screen-composition.md`).
- **Path aliases** in `tsconfig.json`: `@sycapooke/api`, `@sycapooke/components`, `@sycapooke/accounts`,
  and so on, each mapping to that library's `public-api.ts`. Import across libraries only
  through these aliases, never by relative path.
- **Dependency direction**: `sycapooke` → feature libraries → `api` and `components`.
  Feature libraries do not import each other; shared pieces move to `api` or
  `components`.
- Component selector prefix is `sy` (every component is `sy-*`, every attribute directive
  `sy*`). Standalone components, signals and lazy routes throughout. Formatting by Prettier,
  linting by angular-eslint, `npm run build:all` compiles every library and the application.

## Components library is mandatory (CRITICAL)

- The `sycapooke` application and every feature library MUST be built from
  `@sycapooke/components` (`src/frontend/projects/components/`). Never hand-write markup, CSS,
  icons, overlays or toasts the library already provides. Use the `sy-*` component,
  `DialogService`, `SheetService` and `ToastService`.
- `docs/screen-composition.md` is binding. Before implementing any page, dialog,
  notification or shell part, read its entry and use exactly the components it lists.
  Build locally only the UI it marks as local.
- If the library lacks something the entry needs, STOP. Update
  `docs/design-system/` and the library first. Do not work around it in the app.
- Keep `docs/screen-composition.md` aligned with the mocks and `docs/design-system/`.
- The library is Heirloom: its look comes from `src/styles/tokens.css` (the copy of
  `docs/design-system/tokens/tokens.css`). The logo is `sy-logo` (the terracotta tag and the
  Fraunces italic wordmark, tokens `--color-logo-*` and `--font-family-logo`); never draw
  another one.

## Working in this repository

- Run `npm run verify` in `src/frontend` before every commit; it is the same command CI runs
  (`L2-071`, [ADR-0002](docs/adr/platform/0002-single-verify-command.md)).
- Change mocks in `docs/mocks/_build/screens_*.py`, then run
  `python docs/mocks/_build/build.py` (Python 3.12 or later) and
  `python .claude/skills/writing-html-mocks/scripts/check_mocks.py docs/mocks --write`.
  Never hand-edit the generated HTML, `manifest.json`, `index.html` or the coverage
  matrix in `docs/mocks/README.md`.
- Re-render diagrams after editing a `.puml` with
  `python .claude/skills/writing-software-design-documents/scripts/render_puml.py <dir>`.
- Keep `docs/estimates/` consistent with `docs/detailed-designs/`: one subsystem file per
  design folder, and a README whose totals are the sum of the subsystem tables.
- Record any architectural or scope decision as an ADR under `docs/adr/<category>/`.
