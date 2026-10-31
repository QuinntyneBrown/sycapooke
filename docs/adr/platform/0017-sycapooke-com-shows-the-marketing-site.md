# ADR-0017: sycapooke.com shows the marketing site

**Date:** 2026-10-31
**Category:** platform
**Status:** Accepted
**Deciders:** Quinntyne Brown (product owner)

## Context

[ADR platform/0016](0016-publish-with-azure-dns.md) publishes both sites on the VM's Azure DNS
name and needs no custom domain. The owner has registered `sycapooke.com` at Namecheap and wants
the domain to show the deployed marketing page, without changing how either site is built or
deployed.

## Decision

- **DNS at Namecheap.** `A @ → 20.63.41.137` (the VM's static public IP) and
  `CNAME www → sycapooke.com.`, both with a 5-minute TTL. The parking page and the apex URL
  redirect are gone. The Azure DNS name stays as it is.
- **The marketing application owns the domain.** `www.sycapooke.com` and `sycapooke.com` both
  serve the marketing application at `/`, with no redirect and no `/welcome` path. The
  `sycapooke-marketing` image carries its build twice: `/srv/marketing` (base href `/welcome/`,
  port 80, for the Azure host) and `/srv/marketing-root` (the same files with base href `/`,
  port 8080). The edge sends both domain hosts to `marketing:8080`. Caddy obtains both
  certificates from Let's Encrypt on its own.
- **Nothing else moves.** The marketing build keeps base href `/welcome/` and its absolute links
  to the application origin on the Azure name; `SITE_HOST`, `App__BaseUrl`, the Google OAuth
  client, both workflows and their smoke tests are unchanged. ADR platform/0016 still governs the
  application and the Azure name.

## Options Considered

### Option 1: Keep the domain parked (ADR platform/0016 as is)
- **Pros:** No change.
- **Cons:** The domain the owner bought shows a Namecheap parking page.

### Option 2: The domain serves the marketing application at its root; the application stays on the Azure name (chosen)
- **Pros:** The marketing page is its own application at `https://www.sycapooke.com/`; one image
  serves both base paths without a second Angular build; no new secret, resource or cost; the
  application's origin, cookies and Google sign-in do not move.
- **Cons:** Visitors cross from `sycapooke.com` to the Azure name when they open the application.

### Option 3: Move both sites to the domain (the ADR platform/0015 hosts)
- **Pros:** One branded origin for everything.
- **Cons:** Changes `App__BaseUrl`, the Google OAuth client, `SITE_HOST`, the marketing
  application origin and the smoke tests at once; this is what ADR platform/0016 backed away from.

## Consequences

- `https://www.sycapooke.com/` and `https://sycapooke.com/` show the marketing application at `/`.
  The Azure URLs keep working.
- Both deploy workflows copy the edge Caddyfile and reload Caddy, so the domain survives every
  deploy.
- If the VM's public IP ever changes, the Namecheap A record must change with it.

## References

- [ADR platform/0016](0016-publish-with-azure-dns.md)
- [Caddy automatic HTTPS](https://caddyserver.com/docs/automatic-https)
- [Deployment](../../deployment.md)
