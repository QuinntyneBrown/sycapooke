# Deployment

Sycapooke runs in production on one small Azure Linux VM with Docker Compose, backed by an
Azure SQL Database, at `www.sycapooke.com` (the marketing site) and `app.sycapooke.com` (the
full application). The two are separate images, deployed by two independent GitHub Actions
workflows ([ADR platform/0015](adr/platform/0015-separate-marketing-and-application-deployments.md)):
every push to `main` runs both once CI's `npm run verify` has passed on it, and each moves only
its own image tag and restarts only its own services, so a broken application build never
takes the marketing site down and the marketing site redeploys in a minute without touching
the API or the database. There is no manual step. The setup is sized for a few users and
mirrors 12L's ([ADR platform/0012](adr/platform/0012-low-cost-single-vm-continuous-deployment.md),
`L2-084` to `L2-087`, [design](detailed-designs/platform/deploy-to-production/README.md)).

## Topology

| Piece                  | Where                                   | Notes                                                                 |
| ---------------------- | --------------------------------------- | --------------------------------------------------------------------- |
| `edge` (Caddy)         | VM, ports 80/443, 64 MB                 | The official `caddy:2-alpine` image with `deploy/edge/Caddyfile` mounted read-only; terminates TLS (Let's Encrypt) for every host in `SITE_ADDRESS` and routes by host: the apex 301s to `www`, `app.<host>` goes to `app:80`, every other host to `marketing:80` |
| `marketing` (Caddy)    | VM, internal port 80, 64 MB             | `sycapooke-marketing` image, tag `MARKETING_TAG`: the marketing-only Angular build (`npm run build:marketing`) at `/srv/marketing`; serves files only, no proxy; `index.html` and SPA routes are sent with `Cache-Control: no-cache` |
| `app` (Caddy)          | VM, internal port 80, 64 MB             | `sycapooke-app` image, tag `APP_TAG`: the full application (`npm run build:all`) at `/srv/app`; proxies `/api/*` and `/photos/*` to `api:8080`; same `no-cache` rule |
| `api`                  | VM, internal port 8080, 512 MB          | `Sycapooke.Api`, tag `APP_TAG`; photos on the `photos` Docker volume at `/data/photos` |
| `worker`               | VM, 384 MB                              | `Sycapooke.Worker`, tag `APP_TAG`; shares the `photos` volume          |
| `migrator`             | VM, run once per app deploy, 384 MB     | `Sycapooke.Migrator`, tag `APP_TAG`; applies EF Core migrations before the application restarts |
| Database               | Azure SQL Database, Basic tier          | Firewall allows only the VM's public IP                               |
| DNS                    | Namecheap                               | `A` records for `@`, `www` and `app` of `sycapooke.com` point at the VM's static public IP; the edge redirects the bare domain to `www.sycapooke.com` |

One instance of each service, no load balancer, no staging environment. The former `uat`
host is gone: `app.sycapooke.com` is the production application, and its DNS record can be
removed. Every service logs through Docker's `json-file` driver, rotated at 10 MB with 3
files kept. The VM is a B1s (1 vCPU, 1 GB) or B1ms (1 vCPU, 2 GB) with a 2 GB swap file. A
deploy restarts only the services it owns, so the other site keeps serving; the restarted
one is unavailable for a few seconds.

All of it is defined in [`deploy/`](../deploy):

- `backend.Dockerfile` builds `api`, `worker` and `migrator` from one restore (context `src/backend`).
- `marketing/Dockerfile` builds the `marketing` configuration of the Angular workspace (`npm run build:marketing`) and packages it with Caddy and `marketing/Caddyfile` (context: repository root).
- `app/Dockerfile` builds the `production` configuration (`npm run build:all`) and packages it with Caddy and `app/Caddyfile`, which proxies `/api` and `/photos` to `api` (context: repository root).
- `edge/Caddyfile` is the host router, mounted into the official Caddy image; both workflows copy it to `/opt/sycapooke/edge/`.
- `docker-compose.yml` is the production stack; both workflows copy it to `/opt/sycapooke` on the VM.
- `.env.example` lists the settings the stack needs (`MARKETING_TAG`, `APP_TAG`, `SITE_ADDRESS`, `App__BaseUrl=https://app.sycapooke.com`, the Google client); the real `/opt/sycapooke/.env` lives only on the VM.
- `cloud-init.yaml` is the VM's first-boot script (Docker, swap, `/opt/sycapooke`).

The marketing build knows where the application lives: `environment.marketing.ts` sets
`appOrigin: 'https://app.sycapooke.com'`, which `provideAppOrigin` from `@sycapooke/components`
turns into absolute links in the top bar, nav menu, footer and welcome-page calls to action,
and the top-bar search sends the visitor to `https://app.sycapooke.com/?q=…`. In the
application build the origin is empty and the same components render router links.

## Deployment workflows

Two workflows, [`deploy-marketing.yml`](../.github/workflows/deploy-marketing.yml) and
[`deploy-app.yml`](../.github/workflows/deploy-app.yml), start when a `CI` run for a push to
`main` succeeds (`workflow_run`). A failed CI run, or a run for a pull request, deploys
nothing. Each workflow has its own concurrency group (`deploy-marketing`, `deploy-app`), so
runs of the same workflow queue and never overlap, while the two workflows run side by side.
Both copy `docker-compose.yml` and `edge/Caddyfile` to the VM, log in to GHCR with the job's
token on stdin, and rewrite `SITE_ADDRESS` in `.env` as `<SITE_HOST>, www.<SITE_HOST>,
app.<SITE_HOST>`, so the edge requests a certificate for each host and a new host needs only a
DNS record. Every remote script runs under `set -euo pipefail`.

### Deploy marketing

1. **build** (the only job allowed to write packages): builds `sycapooke-marketing` from
   `deploy/marketing/Dockerfile` at the commit CI tested and pushes it to GitHub Container
   Registry, tagged with the commit SHA and `latest`.
2. **deploy** (`production` environment): over SSH sets `MARKETING_TAG=<sha>` in `.env`,
   `docker compose pull marketing edge`, `docker compose up -d --no-deps marketing edge`,
   and prunes images older than 72 hours. The API, worker and application are not touched.
3. **smoke test**: polls `https://www.<SITE_HOST>/` for up to two minutes until the page
   answers 200 and contains "Sycapooke"; otherwise the run fails.

### Deploy app

1. **build**: a four-way matrix builds `sycapooke-api`, `sycapooke-worker` and
   `sycapooke-migrator` from `deploy/backend.Dockerfile` and `sycapooke-app` from
   `deploy/app/Dockerfile`, and pushes them tagged with the commit SHA and `latest`.
2. **deploy** (`production` environment): over SSH sets `APP_TAG=<sha>` in `.env`, pulls
   `api worker migrator app edge`, runs `docker compose run --rm -T migrator < /dev/null`,
   then `docker compose up -d --no-deps api worker app edge` and prunes old images. A failed
   migration stops the script before the restart and the old containers keep serving. The
   marketing service is not touched.
3. **smoke test**: polls `https://app.<SITE_HOST>/api/health` for up to two minutes; no 200
   fails the run. The health endpoint does not need the database
   ([ADR platform/0004](adr/platform/0004-health-check-without-database.md)).
4. **smoke test the application**: polls `https://app.<SITE_HOST>/` for up to two minutes;
   no 200 fails the run (the edge may still be obtaining the certificate for a brand-new host).

Secrets, in the repository's `production` environment only (never committed or printed), are
shared by both workflows:

| Secret           | Value                                                     |
| ---------------- | --------------------------------------------------------- |
| `DEPLOY_HOST`    | Public IP of the VM                                       |
| `DEPLOY_SSH_KEY` | Private key whose public half is in `azureuser`'s `authorized_keys` |
| `SITE_HOST`      | `sycapooke.com` (the `www` and `app` hosts are derived from it) |

Application secrets (connection string, signing keys, the Google OAuth client) live only in
`/opt/sycapooke/.env` on the VM.

## Sign in with Google

"Continue with Google" on `https://app.sycapooke.com/sign-in` and `/join` ([ADR accounts/0001](adr/accounts/0001-sign-in-with-google.md),
`L2-088`) is off until the API has a Google OAuth client. Nothing in the repository changes to
turn it on; the steps below are the owner's, outside the repository.

1. In the [Google Cloud Console](https://console.cloud.google.com/), create or pick a project and
   open **APIs & Services → OAuth consent screen**. Choose *External*, give the app the name
   Sycapooke, a support email and the `sycapooke.com` domain, and add the `email` and `openid`
   scopes. While the app is in *Testing*, only the test users you list can sign in; publish it to
   let anyone sign in (Google may ask to verify the domain).
2. Open **APIs & Services → Credentials → Create credentials → OAuth client ID**, type
   *Web application*, name `Sycapooke`. Add:
   - Authorised JavaScript origins: `https://app.sycapooke.com`
   - Authorised redirect URIs: `https://app.sycapooke.com/api/auth/google/callback`

   For local development add `http://localhost:4200` as an origin and
   `http://localhost:4200/api/auth/google/callback` as a redirect URI (the Angular dev server
   proxies `/api` to the API, so the browser only ever sees port 4200). The marketing host
   `www.sycapooke.com` has no sign-in and gets no entry. A client created for the former
   `uat.sycapooke.com` host must be re-registered with the `app` origin and redirect URI.
3. Copy the client ID and secret into `/opt/sycapooke/.env` on the VM:

   ```bash
   ssh azureuser@20.104.204.145
   cd /opt/sycapooke
   printf 'Auth__Google__ClientId=%s\n' '<client id>' >> .env
   printf 'Auth__Google__ClientSecret=%s\n' '<client secret>' >> .env
   docker compose up -d --no-deps api
   ```

   `env_file: .env` already passes every variable to the `api` container, so nothing else
   changes. The API refuses to start with only one of the two values set.
4. Check it: `curl -s https://app.sycapooke.com/api/auth/providers` answers `{"google":true}`,
   `/sign-in` shows "Continue with Google", and pressing it reaches Google's consent page
   (the API builds the redirect URI from the request host and `X-Forwarded-Proto`; set
   `Auth__Google__RedirectUri=https://app.sycapooke.com/api/auth/google/callback` in `.env` if
   Google reports a redirect URI mismatch).

To turn the feature off, remove both lines and restart `api`; existing Google-linked members then
need a password reset (`L2-009`, not built yet) to sign in.

Locally, keep the values out of `appsettings*.json` and use user secrets from `src/backend/Sycapooke.Api`:

```bash
dotnet user-secrets init
dotnet user-secrets set "Auth:Google:ClientId" "<client id>"
dotnet user-secrets set "Auth:Google:ClientSecret" "<client secret>"
```

## Rolling back

The marketing site and the application roll back separately. Either:

- **From GitHub**: Actions → *Deploy marketing* or *Deploy app* → Run workflow, with `ref`
  set to the full SHA of an earlier commit that passed CI. It rebuilds that commit (mostly
  from cache), runs the migrator (app only) and restarts that workflow's services on images
  tagged with that SHA. The other site is untouched.
- **On the VM**, without CI:

  ```bash
  ssh azureuser@<vm-ip>
  cd /opt/sycapooke
  # the marketing site
  sed -i 's/^MARKETING_TAG=.*/MARKETING_TAG=<older-sha>/' .env
  docker compose up -d --no-deps marketing
  # the application and its backend
  sed -i 's/^APP_TAG=.*/APP_TAG=<older-sha>/' .env
  docker compose up -d --no-deps api worker app
  ```

  This works while that SHA's images are still in GHCR.

Migrations are forward-only. Rolling back past a migration that the older code cannot read
needs a new, compatible migration first; restoring the database is covered by its
point-in-time backups (`L2-067`).

## Operating the server

```bash
ssh azureuser@<vm-ip>
cd /opt/sycapooke
docker compose ps                         # status
docker compose logs -f api                # or worker, app, marketing, edge
docker compose run --rm -T migrator       # apply migrations by hand
docker compose up -d                      # restart everything with the tags in .env
docker compose exec -T edge caddy reload --config /etc/caddy/Caddyfile --adapter caddyfile   # after editing edge/Caddyfile by hand
```

Both workflows copy `edge/Caddyfile`, run `docker compose up -d --no-deps … edge` and then
`caddy reload` inside the edge container, because Compose recreates a container only when its
service definition changes, not when a bind-mounted file does. The reload is graceful: no
connection is dropped and the certificates stay.

Photos live in the `sycapooke_photos` Docker volume. Back it up with
`docker run --rm -v sycapooke_photos:/data -v "$PWD":/backup alpine tar czf /backup/photos.tgz -C /data .`.

## Current resources

Created with the Azure CLI on 2026-10-26 in the Pay-As-You-Go subscription, region Canada Central:

| Resource | Name |
| --- | --- |
| Resource group | `sycapooke-prod` |
| VM | `sycapooke-vm`, Ubuntu 24.04, `Standard_B1s`, 30 GB Standard SSD, static IP `20.104.204.145`, NSG `sycapooke-vmNSG` (22, 80, 443) |
| SQL | server `sycapooke-sql` (`sycapooke-sql.database.windows.net`), database `Sycapooke`, Basic, firewall rule `vm` |
| Deploy key | `~/.ssh/sycapooke_deploy` on the owner's machine; the same key is `DEPLOY_SSH_KEY` |

The `production` environment holds `DEPLOY_HOST`, `DEPLOY_SSH_KEY` and `SITE_HOST`. The SQL
password and signing keys exist only in `/opt/sycapooke/.env` on the VM. DNS for
`sycapooke.com` is managed at Namecheap.

### One-time steps for the split (2026-10-30)

The move from one `web` image and the `uat` host to the `marketing`, `app` and `edge`
services needs these steps once, by the owner, on the VM and at Namecheap:

1. Add an `A` record for `app` pointing at the VM's IP; the `uat` record can be removed.
2. In `/opt/sycapooke/.env`, add `App__BaseUrl=https://app.sycapooke.com` and, if it is set,
   change `Auth__Google__RedirectUri` to `https://app.sycapooke.com/api/auth/google/callback`.
   `IMAGE_TAG` is no longer read; the workflows write `MARKETING_TAG` and `APP_TAG`.
3. Re-register the Google OAuth client's origin and redirect URI on the `app` host (above).
4. Let both workflows run (push to `main` or run each by hand). The old `web` container is
   left behind by `--no-deps`; remove it with `docker compose rm -sf web` once the edge is up.

## Monthly cost

At pay-as-you-go prices, roughly: a B1s VM about US$8 (B1ms about US$15), its 30 GB
standard SSD about US$2 to 5, a static public IP about US$4, and Azure SQL Basic about
US$5, so **about US$20 to 30 a month**. The second web image adds no Azure resource. GHCR
and GitHub Actions are free for this public repository's usage. Check the Azure pricing calculator for your region.

## Building from scratch

1. Create a resource group, a Basic Azure SQL server and database, and an Ubuntu VM (B1s or
   B1ms) with a static public IP, using `deploy/cloud-init.yaml` as custom data. Open ports
   22, 80 and 443.
2. Add the VM's public IP to the SQL server firewall.
3. On the VM, create `/opt/sycapooke/.env` from `deploy/.env.example` with real values.
4. Create an SSH key pair for deploys; add the public half to `azureuser`'s
   `~/.ssh/authorized_keys`.
5. Point `sycapooke.com`, `www.sycapooke.com` and `app.sycapooke.com` at the VM's IP.
6. In GitHub, create the `production` environment and set `DEPLOY_HOST`, `DEPLOY_SSH_KEY`
   and `SITE_HOST` in it.
7. Run *Deploy marketing* and *Deploy app* by hand once (or push to `main`). From then on
   every green push to `main` deploys both.
