# Close customer account — estimate

## Basis

Intermediate Angular and ASP.NET Core developer. [Detailed design](../../detailed-designs/accounts/close-customer-account/README.md)

## Full design

| Work item | Hours |
|-----------|------:|
| Requirements, mocks, design and ADR alignment | 6 |
| Close command, retention rules for orders and financial history, session and token revocation | 12 |
| Anonymisation of personal data and notification-preference cleanup | 8 |
| Founder-account refusal and open-order blocking | 4 |
| API tests for every path and refusal | 8 |
| Frontend confirmation dialog, states and Playwright tests | 10 |
| **Total** | **48 (6 days)** |

## Descoping options

- Defer anonymisation to a scheduled job and only disable the account at close (saves 6 hours, loses the immediate-erasure criterion).

## Status

Not started: 0%. Reusable account and session code is identified in the design.
