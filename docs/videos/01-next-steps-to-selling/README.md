# 01 · Next steps to selling on Sycapooke

> **Runtime:** ~8.5 min · **Audience:** the founder (non-technical) · **Prerequisites:** none

**Video:** [01-next-steps-to-selling.mp4](01-next-steps-to-selling.mp4) · [Slides](slides.html) · **Captions:** [01-next-steps-to-selling.vtt](01-next-steps-to-selling.vtt) · [Transcript](script.md)

## Why this video exists

The shop is deployed (see [deployment](../../deployment.md)) but cannot take money yet: the payment provider
is still `UnconfiguredPaymentProvider` ([ADR orders/0001](../../adr/orders/0001-payment-provider-behind-a-port-until-chosen.md)),
emails are only written to the log (`LoggingEmailSender`) and no founder is provisioned
(`Shop__FounderMemberId` is empty). The founder asked what happens next. This video answers in pictures,
with a few large words per slide, a calm voice and soft captions she can turn on.

## Learning objectives

By the end, the founder can:

- photograph a treasure (daylight, plain background, cover first, close ups, up to 8 photos);
- prepare a note for each piece with the fields the listing form asks for;
- open a Stripe account, turn on two-step sign in and invite her partner as a Developer, without sending any password or key;
- name her partner's four jobs, so she can ask how they are going;
- join the shop, publish a treasure, and change or withdraw it from Inventory;
- handle a sale: ship within 5 business days, add tracking, know when the money arrives, answer a support problem.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| Why can't the shop take money today? | No payment account is connected; she opens Stripe, her partner connects it. |
| What does a sale cost her? | Stripe's standard US card rate (2.9% + 30¢ as of November 2026, stripe.com/pricing); Sycapooke takes no commission (`L2-030`). |
| What must she never do? | Send passwords or secret keys by email or text; she invites her partner in Stripe instead. |
| What happens if she ships late? | After 5 business days without tracking the order is cancelled and refunded (`L2-033`). |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `docs/mocks/pages/catalog/guest.html` | "Your shop is live" |
| `docs/mocks/pages/register/default.html` | Join Sycapooke button, ringed |
| `docs/mocks/pages/my-listings/default.html` | Sell an item and Inventory in the top bar, ringed |
| `docs/mocks/pages/sell/filled.html` | Add photos, Category and Type, Title and Brand, Publish, each ringed |
| `docs/mocks/pages/sales/default.html`, `pages/sale-detail/default.html` | A sale; "Ship by" ringed |
| `docs/mocks/dialogs/add-tracking/default.html` | Add tracking dialog |
| `docs/mocks/pages/support/default.html` | Support queue |
| `src/backend/Sycapooke.Cli/SeedPhotos/**`, `public/media/sycapooke-hero.jpg`, `sycapooke-founder.jpg` | Treasure, hero and founder photos |

`pictures/` holds the ringed screenshots; `build_video.py --pictures` regenerates them from the mocks.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:40 | Welcome | The shop is live; three things missing |
| 00:40-01:08 | The road ahead | Five steps; who does which |
| 01:08-01:44 | Step 1 · Photos | Daylight, plain background, cover, close ups |
| 01:44-02:27 | Step 1 · Notes | Name, story, maker, condition, price, shipping; shipping tip |
| 02:27-04:05 | Step 2 · Stripe | What Stripe is, the fee, what to have ready, sign up, business, bank, two-step |
| 04:05-04:34 | Step 2 · Invite | Never send passwords; Settings → Team → Add member → Developer |
| 04:34-05:23 | Step 3 · Partner | Payments, real email, owner, test sale; sales tax |
| 05:23-06:33 | Step 4 · Add treasures | Join, Sell an item, photos, category, note, Publish, Inventory |
| 06:33-07:44 | Step 5 · Sell and ship | Sales, ship by, post office, Add tracking, money, Support |
| 07:44-08:03 | Share | Friends, family, social media |
| 08:03-08:27 | This week | Three-item checklist; thank you |

## For the technical partner: what step 3 means in this repository

1. **Payments.** Replace `UnconfiguredPaymentProvider` (`src/backend/Sycapooke.Infrastructure/Payments/`) with a Stripe
   adapter of `IPaymentProvider`, add a Stripe `PAYMENT_FIELDS` factory in `@sycapooke/api`, add Stripe's origins to the
   Content Security Policy and set `PaymentProvider:ApiKey`, `PaymentProvider:WebhookSecret` and
   `PaymentProvider:PublishableKey` in `/opt/sycapooke/.env` ([ADR orders/0001](../../adr/orders/0001-payment-provider-behind-a-port-until-chosen.md)).
   Sales tax comes from `QuoteTaxAsync`; decide with the founder's accountant whether to use Stripe Tax.
2. **Email.** Replace `LoggingEmailSender` (`src/backend/Sycapooke.Infrastructure/Email/`) with a real sender, or customers
   cannot verify their email and so cannot check out (`L2-027`).
3. **Founder.** After she joins and verifies, follow [Provision the founder](../../deployment.md#provision-the-founder).
4. **Test sale.** Buy a low-priced treasure with a real card, then refund it from the founder's problem flow or Stripe.

Carrier tracking (`UnconfiguredCarrierTracking`, [ADR orders/0003](../../adr/orders/0003-ship-order-status-token-weekday-deadline-and-carrier-port.md))
is also still a placeholder: tracking numbers can be entered, but delivery is not detected automatically.

## Demo commands

```bash
python -m pip install playwright kokoro-onnx onnxruntime huggingface_hub numpy
python -I docs/videos/_build/build_video.py docs/videos/01-next-steps-to-selling --check
python -I docs/videos/_build/build_video.py docs/videos/01-next-steps-to-selling
```

## Pitfalls

- Stripe's fee and screen names change; re-check stripe.com/pricing and the Team settings wording before re-recording.
- Every cue in `slides.html` must appear once, verbatim, in `script.md`; `--check` fails otherwise.
- A ring cannot sit above a `<dialog>` (top layer), so the Add tracking picture has none.

## References

- [Stripe pricing](https://stripe.com/pricing)
- [Invite team members to a Stripe account](https://support.stripe.com/questions/invite-team-members-or-developers-to-access-your-stripe-account)
- [Stripe user roles](https://docs.stripe.com/get-started/account/teams/roles)
