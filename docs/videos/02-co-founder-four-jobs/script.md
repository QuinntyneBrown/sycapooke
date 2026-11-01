# 02 · Your four jobs: taking the shop live

This video is for you, the technical co-founder. The founder has watched video one. She is photographing treasures and opening a Stripe account, and she will invite you into it as a Developer. Your part is step three: four jobs that turn the deployed shop into one that takes money. Every path I mention is in the README next to this video.

## Where the shop stands

Production already runs at app dot sycapooke dot com. But three pieces are still stand-ins, and you can see all of them in DependencyInjection.cs in the Infrastructure project. The payment port is UnconfiguredPaymentProvider, which throws, so checkout stops safely at the address step. Email is LoggingEmailSender, which writes every message to the log and sends nothing. And Shop__FounderMemberId in the server's env file is empty, so nobody can sell, and the public catalog is empty.

So your four jobs are: connect Stripe, switch on real email, make the founder the owner, and run a real test sale. One piece of advice on order. Do email first. It is the smallest job, and the founder cannot verify her account without it, which blocks job three.

## Job one: connect Stripe

Start by accepting her invite. You get Developer access, so you never need her password, and she never sends you a key. Stay in Stripe's test mode until job four.

On the server, the whole provider sits behind one port: IPaymentProvider, in Ports.cs in the Application project. It has four methods, and the Stripe adapter replaces UnconfiguredPaymentProvider in the Payments folder of Infrastructure. No handler or page changes.

CreatePaymentAsync creates a Stripe PaymentIntent for the amount in cents and confirms it with the payment method from the browser. Put Sycapooke's payment ID in the intent's metadata, and pass the request's idempotency key as Stripe's idempotency key, so a retried request charges once. Return the intent's ID. The result does not come back here. It arrives later by webhook.

ReadWebhook verifies the signature over the raw body with the webhook secret, then maps payment intent succeeded and payment intent failed to a PaymentEvent, reading the payment ID back from the metadata. The card brand and last four digits are the only card details it may carry.

Here is the one surprise. PaymentWebhookController reads a header called Payment-Signature. Stripe signs with a header called Stripe-Signature. So change the controller to read Stripe's header, or every real webhook will be refused as a bad signature.

RefundAsync creates a refund for the intent, again with the idempotency key. QuoteTaxAsync is a business decision. Use Stripe Tax if the founder's accountant says she must collect sales tax. Otherwise return zero. Either way, it must stop throwing.

In the browser, the card fields come from PAYMENT_FIELDS in the api library, in payment-fields.ts. Today its factory makes TestModePaymentFields, a dropdown of test cards. Write a Stripe factory instead. It loads Stripe dot js from Stripe's own site, mounts the card element in the host, and returns the payment method ID from tokenize. Card numbers are typed into Stripe's frame and never touch Sycapooke.

Then configuration. Three secrets go in the env file on the server, never in git: PaymentProvider__ApiKey, PaymentProvider__WebhookSecret, and PaymentProvider__PublishableKey. Only the publishable key may reach the browser. In the Stripe dashboard, add a webhook endpoint at app dot sycapooke dot com slash api slash payments slash webhook, for the succeeded and failed events.

Keep the tests offline. The API tests already swap in FakePaymentProvider. Test your adapter against sample Stripe payloads, and never let a test call Stripe.

## Job two: switch on real email

The port is IEmailSender, and its contract is strict. Throw HttpRequestException or TimeoutException for a failure worth retrying. Throw EmailRejectedException when the provider refuses an address for good. Pass the message's idempotency key through, and send its extra headers, such as List-Unsubscribe on marketing email. The worker's email dispatch job already retries with back off for up to twenty four hours.

Pick a transactional email service with an HTTP API, and replace LoggingEmailSender with it. There is no sender address setting yet, so add one next to the API key, on a sycapooke dot com address. Then, at Namecheap, add the SPF, DKIM and DMARC records the service gives you, or the founder's sale emails will land in spam.

To check it, join the shop with your own address. The verification email should reach your inbox within two minutes.

## Job three: make her the owner

The shop has exactly one founder. Follow Provision the founder in deployment.md.

First, she joins and verifies her email, like any customer. Next, look up her member ID in the Members table. Set Shop__FounderMemberId to that ID in the env file in slash opt slash sycapooke.

Now a trap. Docker compose restart does not reread the env file. Recreate the containers instead, with docker compose up, dash d, no deps, api and worker.

Then run the migrator once with the provision founder flag: docker compose run, dash dash rm, migrator, dash dash provision founder. Running it again with the same ID changes nothing, and a different ID is refused. Tell her when it's done. She will see a new button at the top of the page: Sell an item.

## Job four: a real test sale

Do it twice. First, in test mode. Use Stripe's test keys and pay with the card number four two four two, repeated four times. Check that the order is paid, that the buyer's confirmation and the founder's You made a sale email both arrive within two minutes, and that the order appears under Sales. Then try Stripe's declined test card and check that the buyer sees a clear message.

Then go live. Swap in the live keys, and remember that the live webhook endpoint has its own secret. Buy a low priced treasure with a real card. Have the founder add tracking, because a problem can only be reported once an order has shipped. Then report a problem on it as the buyer, and have the founder resolve it with a full refund. Watch the refund appear in Stripe, and the refund email arrive.

## Also on your list

Carrier tracking is still a stand-in too. UnconfiguredCarrierTracking accepts tracking numbers, but nothing detects delivery automatically yet. And check that Privacy__FieldEncryptionKeys is set, with the keys backed up apart from the database. Lose them, and the shipping addresses they sealed cannot be read.

## Recap

Things to remember. Email first, because it unblocks the founder. Stripe is one adapter, one header fix, one browser factory and three secrets. Recreate the containers after editing the env file. Run the whole sale in test mode before you go live. And never take a key from the founder: Developer access is enough.

When you are done, tell her. Her next step is to publish her first treasure. Thank you for watching.
