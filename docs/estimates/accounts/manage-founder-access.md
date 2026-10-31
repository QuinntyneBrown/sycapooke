# Manage founder access — estimate

## Basis

Intermediate Angular and ASP.NET Core developer. [Detailed design](../../detailed-designs/accounts/manage-founder-access/README.md)

## Full design

| Work item | Hours |
|-----------|------:|
| Requirements, design and ADR 0018 alignment | 4 |
| `ShopOptions`, `IFounderAccess`, start-up validation | 6 |
| `FounderAccessRequirement` policy replacing the seller checks; 403 `FOUNDER_REQUIRED` | 6 |
| `isFounder` on `CurrentMemberDto`, `AuthState`, route guards | 5 |
| `Sycapooke.Migrator --provision-founder` operation | 4 |
| API and guard tests, Playwright | 7 |
| **Total** | **32 (4 days)** |

## Descoping options

- Provision the founder by configuration only, without the migrator operation (saves 4 hours, loses the audited provisioning step).

## Status

Not started: 0%.
