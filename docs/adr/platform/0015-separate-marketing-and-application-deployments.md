# ADR-0015: Separate Marketing and Application Deployments

**Date:** 2026-10-30
**Category:** platform
**Status:** Accepted
**Deciders:** Quinntyne Brown (product owner)

## Context

[ADR platform/0013](0013-uat-host-in-the-production-web-image.md) put the marketing-only build
and the full application into one `sycapooke-web` image, with Caddy choosing by host between
`www.sycapooke.com` and a `uat.sycapooke.com` host, built and restarted by the single
`deploy.yml`. In practice the coupling hurt: a broken application build, or a failed migration,
held back a one-line copy change on the marketing page, and every marketing tweak rebuilt and
restarted the API, the worker and the whole web container. The owner also decided that the
`uat` host had no future: there is no separate environment behind it
([ADR platform/0012](0012-low-cost-single-vm-continuous-deployment.md)), so it is simply the
production application and should be named as such. The budget is unchanged: one small VM,
no managed hosting.

## Decision

- **Two web images.** `deploy/marketing/Dockerfile` runs `npm run build:marketing` and
  packages `/srv/marketing` with Caddy as `sycapooke-marketing`; `deploy/app/Dockerfile` runs
  `npm run build:all` and packages `/srv/app` with Caddy as `sycapooke-app`, whose Caddyfile
  proxies `/api/*` and `/photos/*` to `api:8080`. Both listen on port 80 inside the stack and
  keep the `Cache-Control: no-cache` rule for `index.html` and SPA routes. `deploy/web/` and
  `npm run build:web` are gone.
- **An edge that routes by host.** The `edge` service is the official `caddy:2-alpine` image
  with `deploy/edge/Caddyfile` mounted read-only. It owns ports 80 and 443 and the
  certificates for every host in `SITE_ADDRESS`, redirects the apex to `www`, sends
  `app.<host>` to `app:80` and everything else to `marketing:80`. It is the only service named in both
  workflows' `up -d` lists, so a changed compose definition of the edge is applied by either.
- **Two workflows, two tags.** `.github/workflows/deploy-marketing.yml` and
  `deploy-app.yml` both start on `workflow_run` of `CI` for a push to `main` and on
  `workflow_dispatch` with a full SHA. Each has its own concurrency group
  (`deploy-marketing`, `deploy-app`). The marketing workflow builds one image, sets
  `MARKETING_TAG` in `/opt/sycapooke/.env` and runs `docker compose up -d --no-deps marketing
  edge`; the app workflow builds `api`, `worker`, `migrator` and `app`, sets `APP_TAG`, runs the
  migrator and then `up -d --no-deps api worker app edge`. Neither writes the other's tag.
  `IMAGE_TAG` no longer exists. Both rewrite `SITE_ADDRESS` as the apex, `www` and `app` hosts.
- **Hosts.** `www.sycapooke.com` is the marketing site; `app.sycapooke.com` is the full
  application. The `uat` host is retired, its DNS record can go, and `App__BaseUrl` and the
  Google OAuth client move to the `app` host.
- **An application origin for cross-host links.** The marketing build gets
  `appOrigin: 'https://app.sycapooke.com'` from `environment.marketing.ts` (an `angular.json`
  file replacement) through `provideAppOrigin` / `APP_ORIGIN` / `externalHref` in
  `@sycapooke/components`. The top bar, nav menu, footer and welcome-page calls to action
  render absolute links to the application in the marketing build and router links in the
  application; "About Sycapooke" stays local on both; the marketing top-bar search sends the
  visitor to `https://app.sycapooke.com/?q=…`. In the application build the origin is empty.
- **Tests.** `DeploymentPipelineTests` reads both workflows, the compose file and the three
  Caddyfiles and asserts the gate, the per-unit tags and services, the separate concurrency
  groups, the migrate-before-restart order, the smoke tests, the memory limits and the host
  routing (`L2-084` to `L2-087`).

## Options Considered

### Option 1: Keep one web image and one workflow (ADR platform/0013)
- **Pros:** No change; one build, one tag, one smoke test.
- **Cons:** Marketing and application cannot move independently; a red application build or a
  failed migration blocks a copy change; every deploy restarts everything. The `uat` name
  misdescribes what is production.

### Option 2: Two images, two workflows and an edge on the same VM (chosen)
- **Pros:** Each site deploys, fails and rolls back on its own; a marketing deploy takes a
  minute and never touches the API or database; no new Azure resource, secret or cost; the
  stack stays one compose file on one VM; the official Caddy image for the edge means the
  router is never rebuilt.
- **Cons:** Two workflows and two tags to understand; three Caddy processes; the marketing
  build must carry the application origin for its links.

### Option 3: Azure Static Web Apps for the marketing site
- **Pros:** Managed hosting and a global CDN for the public page; the VM would serve only
  the application.
- **Cons:** Azure credentials in GitHub, a second hosting model and a second DNS target to
  operate; the marketing page is tiny and already behind Caddy's compression; diverges from
  ADR platform/0012 for little gain.

### Option 4: A second VM for the marketing site
- **Pros:** Full isolation of the two sites, including from a VM outage.
- **Cons:** Doubles the VM cost and the moving parts for a page that is a few static files;
  ruled out by ADR platform/0012.

## Consequences

### Positive
- The marketing site and the application ship on their own schedules, from their own images,
  and roll back on their own; a failure in one leaves the other serving.
- `app.sycapooke.com` names the production application honestly; nothing suggests a sandbox.
- Monthly cost is unchanged: no new Azure resource, secret or workflow credential.

### Negative
- Every push to `main` runs two workflows and two image builds; the application build is
  still the slow one.
- Links from the marketing site into the application are a cross-host navigation, not a
  router transition, so the application loads fresh on arrival.
- One-time owner steps: the `app` DNS record, `App__BaseUrl` and the Google redirect URI in
  `/opt/sycapooke/.env`, re-registering the Google OAuth client, and removing the old `web`
  container.

### Risks
- The first deploy after the `app` DNS record may fail its smoke test while Let's Encrypt
  issues the new certificate. Mitigation: re-run the workflow; the edge keeps retrying.
- Both workflows copy the same compose file and edge Caddyfile; both are versioned with the
  code, so the two copies only differ between two runs of the same commit, never in content.
- A changed `edge/Caddyfile` alone does not recreate the `edge` container (`up -d` compares
  the service definition, not a bind-mounted file) and Caddy does not watch it. Mitigation:
  both workflows run `caddy reload` inside the edge container after the copy, which applies
  the file gracefully.

## References

- [ADR platform/0012](0012-low-cost-single-vm-continuous-deployment.md)
- [ADR platform/0013](0013-uat-host-in-the-production-web-image.md) (superseded)
- [ADR platform/0011](0011-marketing-feature-library.md)
- [deploy-to-production design](../../detailed-designs/platform/deploy-to-production/README.md)
- [docs/deployment.md](../../deployment.md)
