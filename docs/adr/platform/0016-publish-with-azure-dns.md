# ADR-0016: Publish with Azure DNS, without a custom domain

**Date:** 2026-10-30
**Category:** platform
**Status:** Accepted
**Deciders:** Quinntyne Brown (product owner)

## Context

The owner clarified that the objective is automatic publication to Azure on commits to
main. Custom-domain registration or DNS changes are not a prerequisite. ADR-0015's
custom hosts prevented successful public smoke tests even though Azure deployments ran.

## Decision

Use the existing VM public IP's Azure DNS label:
`sycapooke.canadacentral.cloudapp.azure.com`. Serve the full application at `/` and the
independently built marketing site at `/welcome/`. This supersedes ADR-0015's host names
and host routing; separate images, tags, workflows, migration ordering and rollback stay.

Caddy terminates HTTPS for the Azure name, strips `/welcome/` before proxying to the
marketing service, and sends other requests to the app service. The marketing build has
base href `/welcome/` and absolute links to the application origin. Both workflows set
`SITE_ADDRESS` to `SITE_HOST` and check their public Azure URLs after deploying.

Serialize shared remote mutations with `flock`; isolate Docker registry configuration by
workflow run so one workflow cannot log out the other. CI and tests continue to use fakes
and never contact the production database.

## Consequences

No new hosting resource or custom-domain account is needed. Azure maintains the DNS
record for the VM's static IP. Both sites use one HTTPS origin and remain independently
deployable. Changing the Azure DNS label requires changing the marketing application
origin, the production `SITE_HOST` secret and the VM's `App__BaseUrl`.

## References

- [Azure VM DNS names](https://learn.microsoft.com/en-us/azure/virtual-machines/custom-domain)
- [Caddy routing](https://caddyserver.com/docs/caddyfile/directives/handle_path)
- [Deployment](../../deployment.md)
