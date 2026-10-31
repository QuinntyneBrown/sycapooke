# Deployment

Every green push to `main` publishes both sites to the existing Azure VM automatically:

- Main application: https://sycapooke.canadacentral.cloudapp.azure.com/
- Marketing: https://sycapooke.canadacentral.cloudapp.azure.com/welcome/
- API health: https://sycapooke.canadacentral.cloudapp.azure.com/api/health
- Marketing on the domain: https://www.sycapooke.com/ and https://sycapooke.com/
- Application on the domain: https://app.sycapooke.com/

The public IP has Azure DNS label `sycapooke`, maintained by Azure. See
[ADR platform/0016](adr/platform/0016-publish-with-azure-dns.md). The registered domain
`sycapooke.com` (Namecheap) also points at the VM: `www.sycapooke.com` and `sycapooke.com` serve
the marketing application at `/` from the marketing image's second copy (base href `/`, port 8080). See
[ADR platform/0017](adr/platform/0017-sycapooke-com-shows-the-marketing-site.md).

## Pipeline

`ci.yml` runs `npm run verify` on every push to main and pull request. Successful push
runs trigger both `deploy-marketing.yml` and `deploy-app.yml` for the tested SHA.
Failed CI and pull requests never deploy.

Both build SHA-tagged images in GHCR, then connect to the Azure VM over SSH using the
GitHub `production` environment. Marketing builds its own Angular image with base href
`/welcome/`. App builds the full frontend, API, worker and migrator. Each workflow changes
only its own tag and services; each has a concurrency group that prevents overlapping
runs of that workflow. A shared remote file lock serializes changes to `.env` and the
running stack. Each workflow run has separate Docker registry credentials.

Both copy Compose and the edge Caddyfile to `/opt/sycapooke`, set `SITE_ADDRESS` from
`SITE_HOST`, pull images, start their own services and reload Caddy. The app deploy runs
the migrator before restarting app services; migration failure leaves old containers
running. Marketing never runs migrations. Images older than 72 hours are pruned.

The marketing workflow checks `/welcome/` for the Sycapooke page. The app workflow checks
`/api/health` and `/`. Failure after the retry window fails the workflow. Both web servers
send `Cache-Control: no-cache` for entry pages and SPA routes. These are production smoke
checks; backend tests use fakes/in-memory substitutes, and Playwright never runs the API.

## Topology

One Ubuntu VM runs `deploy/docker-compose.yml` with Docker and a 2 GB swap file:

| Service | Memory | Purpose |
| --- | ---: | --- |
| edge | 64 MB | Official Caddy image, public ports 80/443, managed HTTPS; strips `/welcome/` to marketing, other paths to app; `sycapooke.com` and `www` → marketing:8080 |
| marketing | 64 MB | Separate Angular marketing image; static files only; port 80 for `/welcome/`, port 8080 for the domain at `/` |
| app | 64 MB | Full Angular image; proxies `/api` and `/photos` to API |
| api | 512 MB | ASP.NET Core API; photos on Docker volume |
| worker | 384 MB | Background jobs; shares photos volume |
| migrator | 384 MB | One-off EF migrations before app restart |

All services use rotated `json-file` logs (10 MB, 3 files). Database: Azure SQL Basic,
firewall restricted to the VM public IP. There is one instance per service and no staging
or load balancer. The single VM is a shared failure boundary. A service restart can cause
a few seconds of unavailability.

## Resources and settings

| Azure resource | Value |
| --- | --- |
| Resource group | `sycapooke-prod`, Canada Central |
| VM | `sycapooke-vm`, Ubuntu 24.04, Standard_B1s, 30 GB Standard SSD |
| Static public IP | `sycapooke-vmPublicIP`, `20.63.41.137` |
| Azure DNS | `sycapooke.canadacentral.cloudapp.azure.com` |
| Domain (Namecheap) | `sycapooke.com`: `A @ → 20.63.41.137`, `CNAME www → sycapooke.com.`, `CNAME app → sycapooke.com.` |
| NSG | `sycapooke-vmNSG`, inbound 22, 80, 443 |
| SQL server | `sycapooke-sql-8b1047.database.windows.net` |
| Database | `Sycapooke`, Basic; firewall rule `vm` permits `20.63.41.137` |

GitHub's `production` environment contains `DEPLOY_HOST` (VM IP), `DEPLOY_SSH_KEY` and
`SITE_HOST` (`sycapooke.canadacentral.cloudapp.azure.com`). Application secrets stay only in
`/opt/sycapooke/.env`. Use `deploy/.env.example` as the template. `App__BaseUrl` must be
`https://app.sycapooke.com`. `MARKETING_TAG` and `APP_TAG` are
managed independently by their workflows. The deploy private key is locally available at
`~/.ssh/sycapooke_deploy`. Never commit the private key or `.env`.

## Rollback and operations

Run either deployment workflow manually with `ref` equal to a previous full commit SHA.
It rebuilds and redeploys only that unit. Alternatively, on the VM, change its tag in `.env`
and run `docker compose up -d --no-deps marketing` or `docker compose up -d --no-deps api worker app`.
Images must remain available in GHCR. Migrations are forward-only: incompatible schema
changes require a compatible migration before rollback. Azure SQL provides point-in-time
backups; photos need a separate backup of the `sycapooke_photos` Docker volume.

```bash
ssh -i ~/.ssh/sycapooke_deploy azureuser@20.63.41.137
cd /opt/sycapooke
docker compose ps
docker compose logs --tail 100 api
docker compose exec -T edge caddy reload --config /etc/caddy/Caddyfile --adapter caddyfile
```

## Recreate the Azure host

1. Create the VM with `deploy/cloud-init.yaml`, static public IP and NSG ports 22/80/443.
2. Set the public IP DNS label to `sycapooke` (or update all origin settings if unavailable),
   and point the Namecheap `@` A record for `sycapooke.com` at the new static IP.
3. Create Azure SQL Basic and allow only the VM IP in its firewall.
4. Provision `/opt/sycapooke/.env` and the SSH public key for `azureuser`.
5. Set the three GitHub production environment secrets listed above.
6. Push to main: CI and both deploy workflows publish without manual steps.

## Provision the founder

The shop has one founder ([ADR platform/0018](adr/platform/0018-founder-access-and-shop-contracts.md)). Until she is
bound, nobody can sell, fulfil orders or resolve problems, and the public catalog is empty.

1. She joins and verifies her email like any customer.
2. Look up her member ID in the database (`SELECT Id FROM Members WHERE Email = ...`).
3. Set `Shop__FounderMemberId=<that GUID>` in `/opt/sycapooke/.env` and restart the API and worker.
4. Run `Sycapooke.Migrator --provision-founder` once with the same `Shop__FounderMemberId` in its environment. Repeating it with the same ID changes
   nothing; a different ID is refused. The API logs a warning at startup while the setting is empty.

## Field encryption keys

Shipping addresses are sealed with AES-256-GCM before they reach the database. Set
`Privacy__FieldEncryptionKeys=k1:<base64 of 32 random bytes>` in `.env` (`openssl rand -base64 32`). To
rotate, put a new `k2:...` first and keep `k1:...` after it: new writes use `k2`, old rows still read. A row
written before a key was set is read as plain text and sealed the next time it is saved. Losing every key
that sealed a row makes that row unreadable, so back the keys up apart from the database.

## Sign in with Google

"Continue with Google" on `https://sycapooke.canadacentral.cloudapp.azure.com/sign-in` and `/join` ([ADR accounts/0001](adr/accounts/0001-sign-in-with-google.md),
`L2-088`) is off until the API has a Google OAuth client. Nothing in the repository changes to
turn it on; the steps below are the owner's, outside the repository.

1. In the [Google Cloud Console](https://console.cloud.google.com/), create or pick a project and
   open **APIs & Services → OAuth consent screen**. Choose *External*, give the app the name
   Sycapooke, a support email and add the `email` and `openid`
   scopes. While the app is in *Testing*, only the test users you list can sign in; publish it to
   let anyone sign in (Google may ask to verify the domain).
2. Open **APIs & Services → Credentials → Create credentials → OAuth client ID**, type
   *Web application*, name `Sycapooke`. Add:
   - Authorised JavaScript origins: `https://sycapooke.canadacentral.cloudapp.azure.com`
   - Authorised redirect URIs: `https://sycapooke.canadacentral.cloudapp.azure.com/api/auth/google/callback`

   For local development add `http://localhost:4200` as an origin and
   `http://localhost:4200/api/auth/google/callback` as a redirect URI (the Angular dev server
   proxies `/api` to the API, so the browser only ever sees port 4200). The marketing page at `/welcome/` uses the same origin.
3. Copy the client ID and secret into `/opt/sycapooke/.env` on the VM:

   ```bash
   ssh -i ~/.ssh/sycapooke_deploy azureuser@20.63.41.137
   cd /opt/sycapooke
   printf 'Auth__Google__ClientId=%s\n' '<client id>' >> .env
   printf 'Auth__Google__ClientSecret=%s\n' '<client secret>' >> .env
   docker compose up -d --no-deps api
   ```

   `env_file: .env` already passes every variable to the `api` container, so nothing else
   changes. The API refuses to start with only one of the two values set.
4. Check it: `curl -s https://sycapooke.canadacentral.cloudapp.azure.com/api/auth/providers` answers `{"google":true}`,
   `/sign-in` shows "Continue with Google", and pressing it reaches Google's consent page
   (the API builds the redirect URI from the request host and `X-Forwarded-Proto`; set
   `Auth__Google__RedirectUri=https://sycapooke.canadacentral.cloudapp.azure.com/api/auth/google/callback` in `.env` if
   Google reports a redirect URI mismatch).

To turn the feature off, remove both lines and restart `api`; existing Google-linked members then
need a password reset (`L2-009`, not built yet) to sign in.

Locally, keep the values out of `appsettings*.json` and use user secrets from `src/backend/Sycapooke.Api`:

```bash
dotnet user-secrets init
dotnet user-secrets set "Auth:Google:ClientId" "<client id>"
dotnet user-secrets set "Auth:Google:ClientSecret" "<client secret>"
```
