# 02 · Your four jobs: taking the shop live

> **Runtime:** ~8.4 min · **Audience:** the technical co-founder · **Prerequisites:** [video 01](../01-next-steps-to-selling/README.md) (step 3), [deployment](../../deployment.md)

**Video:** [02-co-founder-four-jobs.mp4](02-co-founder-four-jobs.mp4) · [Slides](slides.html) · **Captions:** [02-co-founder-four-jobs.vtt](02-co-founder-four-jobs.vtt) · [Transcript](script.md)

## Why this video exists

Video 01 tells the founder that her partner has four jobs: payments, real email, making her the owner and a test
sale. This video is the partner's side of that step. The shop is deployed but `DependencyInjection.cs` still
registers `UnconfiguredPaymentProvider` and `LoggingEmailSender`, and `Shop__FounderMemberId` is empty. It explains
what to build, what to configure on the server, the order to do it in, and the two traps found while preparing it
(the webhook signature header and `docker compose restart` not rereading `.env`).

## Learning objectives

By the end, the co-founder can:

- explain why email comes first (the founder cannot verify, so cannot be provisioned, without it);
- implement a Stripe adapter of `IPaymentProvider` and a Stripe `PAYMENT_FIELDS` factory, and fix the webhook header;
- set the three `PaymentProvider__*` secrets and the Stripe webhook endpoint;
- implement `IEmailSender` against a transactional email service, with SPF, DKIM and DMARC on `sycapooke.com`;
- provision the founder and recreate the containers so the setting takes effect;
- run the test sale in test mode, then live, through to a refund.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| Why is checkout stopped today? | `UnconfiguredPaymentProvider.QuoteTaxAsync` throws, so checkout stops at the address step (ADR orders/0001). |
| What does a Stripe webhook need beyond the adapter? | `PaymentWebhookController.SignatureHeader` is `Payment-Signature`; Stripe sends `Stripe-Signature`. |
| How does the founder get access? | Join and verify, set `Shop__FounderMemberId`, `docker compose up -d --no-deps api worker`, then `migrator --provision-founder`. |
| How is the live test sale refunded? | Founder adds tracking (problems need a Shipped order, `L2-036`), buyer reports a problem, founder resolves with a full refund. |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `src/backend/Sycapooke.Infrastructure/DependencyInjection.cs` | The three stand-ins |
| `src/backend/Sycapooke.Application/Abstractions/Ports.cs` | `IPaymentProvider` and `IEmailSender` contracts |
| `src/backend/Sycapooke.Api/Controllers/PaymentWebhookController.cs` | `SignatureHeader = "Payment-Signature"` |
| `src/frontend/projects/api/src/lib/orders/payment-fields.ts` | `PAYMENT_FIELDS`, `TestModePaymentFields` |
| `deploy/.env.example`, `deploy/docker-compose.yml` | `Shop__FounderMemberId`, the `migrator` service |
| `docs/mocks/pages/checkout/default.html`, `declined.html` | Card fields (ringed) and a declined card |
| `docs/mocks/pages/my-listings/default.html` | Sell an item, ringed |

Background music: *Kingstown Carnival Nights* (instrumental soca, Pixabay), set in `music.json`; see
[assets/music/ATTRIBUTION.md](../assets/music/ATTRIBUTION.md).

`pictures/` holds the screenshots; `build_video.py --pictures` regenerates them from the mocks.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:34 | Welcome | Who this is for; step 3 is four jobs |
| 00:34-01:28 | Where the shop stands | Three stand-ins; do email first |
| 01:28-04:31 | Job 1 · Stripe | Invite, port, PaymentIntent, webhook, header fix, refund and tax, browser fields, secrets, endpoint, offline tests |
| 04:31-05:35 | Job 2 · Email | `IEmailSender` contract, provider, sender address, SPF/DKIM/DMARC, inbox check |
| 05:35-06:38 | Job 3 · Owner | Provision the founder; recreate containers; migrator |
| 06:38-07:43 | Job 4 · Test sale | Test mode with 4242, declined card; live with refund through a problem |
| 07:43-08:03 | Also on your list | Carrier tracking stand-in; field encryption keys |
| 08:03-08:40 | Recap | Five things to remember; hand back to the founder |

## Demo commands

```bash
python -m pip install playwright kokoro-onnx onnxruntime huggingface_hub numpy
python -I docs/videos/_build/build_video.py docs/videos/02-co-founder-four-jobs --check
python -I docs/videos/_build/build_video.py docs/videos/02-co-founder-four-jobs
```

On the server (from `docs/deployment.md`):

```bash
ssh -i ~/.ssh/sycapooke_deploy azureuser@20.63.41.137
cd /opt/sycapooke
docker compose up -d --no-deps api worker
docker compose run --rm migrator --provision-founder
```

## Pitfalls

- There is no sender address setting yet; the email adapter has to add one.
- The publishable key reaches the browser; nothing serves it yet, so the `PAYMENT_FIELDS` factory needs a way to read it.
- The app pages set no Content Security Policy today (the API's is `default-src 'none'`); if one is added, allow Stripe's origins.
- Stripe's live and test webhook endpoints have different signing secrets.
- Stripe screens and event names change; re-check them against Stripe's docs before a rebuild.

## References

- [Stripe: Payment Intents API](https://docs.stripe.com/payments/payment-intents)
- [Stripe: Webhooks and signature verification](https://docs.stripe.com/webhooks)
- [Stripe: Idempotent requests](https://docs.stripe.com/api/idempotent_requests)
- [Stripe: Testing cards](https://docs.stripe.com/testing)
- [Stripe Tax](https://docs.stripe.com/tax)
- [Stripe user roles](https://docs.stripe.com/get-started/account/teams/roles)
