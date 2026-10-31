"""Founder support queue and problem detail, error pages, legal pages, cookies and system notices
(L1-008, L1-014, L1-018, L2-036, L2-056, L2-068)."""
from mk import *

# ---------------------------------------------------------------------------
# Shared pieces
# ---------------------------------------------------------------------------
STATIC_NA = {"loading": "Static content ships with the app; nothing is fetched.",
             "empty": "The page always has content.",
             "error": "Static content cannot fail to load once the app is served."}


def head(title: str, sub: str = "", actions: str = "", crumb: tuple[str, str] | None = None) -> str:
    c = (f'<nav aria-label="Breadcrumb"><ol class="breadcrumb"><li><a href="{crumb[1]}">{crumb[0]}</a></li>'
         f'<li aria-current="page">{title}</li></ol></nav>') if crumb else ""
    s = f'<p class="subtitle">{sub}</p>' if sub else ""
    return f'<div class="page-header"><div class="stack stack--sm">{c}<h1>{title}</h1>{s}</div>{actions}</div>'


def center(code: str, title: str, text: str, actions: str, ref: str = "", icon: str | None = None) -> str:
    big = f'<span class="code" aria-hidden="true">{code}</span>' if code else ""
    i = f'<div class="empty__icon">{ic(icon, "icon icon--lg")}</div>' if icon else ""
    r = f'<span class="ref">{ref}</span>' if ref else ""
    return f'<div class="center-state">{big}{i}<h1>{title}</h1><p class="text-muted">{text}</p>{r}<div class="actions">{actions}</div></div>'


def skel_rows(n: int = 4) -> str:
    row = ('<li><div class="item-row" style="padding-block: var(--space-5);" aria-hidden="true"><span class="skeleton skeleton--rect" style="width: 3.5rem; height: 3.5rem;"></span>'
           '<div class="item-row__body"><span class="skeleton skeleton--text" style="width: 45%;"></span><span class="skeleton skeleton--text" style="width: 25%;"></span></div></div></li>')
    return f'<ul class="queue" role="list" aria-busy="true" aria-label="Loading">{row * n}</ul>'


def skel_detail() -> str:
    return ('<div class="split" aria-busy="true" aria-label="Loading"><div class="stack stack--lg"><span class="skeleton skeleton--title" style="width: 60%;"></span>'
            '<span class="skeleton skeleton--rect" style="height: 10rem;"></span><div><span class="skeleton skeleton--text"></span><span class="skeleton skeleton--text"></span></div></div>'
            '<div class="panel panel--sunken stack"><span class="skeleton skeleton--text" style="width: 40%;"></span><span class="skeleton skeleton--rect" style="height: 3rem;"></span></div></div>')


def load_error(what: str, ref: str) -> str:
    return empty("refresh", f"Couldn’t load {what}", f'Something went wrong on our side. <span class="ref">Ref {ref}</span>',
                 btn("Try again", "primary", icon="refresh"), cls="empty--error")


# ---------------------------------------------------------------------------
# Support queue and problem detail (founder only; L2-036, L2-030, L2-034)
# ---------------------------------------------------------------------------
IDA = PEOPLE[FOUNDER]["name"]
CUST = PEOPLE[ME]["name"]
PIA = PEOPLE["pia_collects"]["name"]


def founder_page(main: str, notes: str = "") -> Page:
    return Page(main=main, user="founder", account="support", notes=notes)


def support_li(pid: str, kind: str, oid: str, who: str, st: str, when: str, href: str) -> str:
    o = ORDERS[oid]
    return (f'<li><a href="{href}">{item_row(o["item"], f"{pid} · {kind} · {who} · {when}")}'
            f'<span class="order-list__end"><span class="price">{money(o["total"])}</span>{status(st)}</span></a></li>')


def support_list(only: str = "") -> str:
    rows = [("open", "P-0912", "Problem: not as described", "SY-48140", CUST, "Open", "opened yesterday", P("problem-detail")),
            ("open", "C-0044", "Cancellation request", "SY-48302", PIA, "Open", "today", P("sale-detail", "cancel-requested")),
            ("resolved", "P-0907", "Problem: not as described", "SY-48213", CUST, "Refunded", "resolved Sat 24 Oct", P("problem-detail", "resolved"))]
    lis = "".join(support_li(*r[1:]) for r in rows if not only or r[0] == only)
    return f'<ul class="order-list">{lis}</ul>'


def support_filters(active: str = "all") -> str:
    def b(label, key):
        cur = ' aria-current="true"' if active == key else ""
        return f'<a class="btn btn--sm{" btn--primary" if active == key else ""}" href="{P("support", "default" if key == "all" else "filtered")}"{cur}>{label}</a>'
    return f'<div class="cluster" role="group" aria-label="Filter support requests">{b("All", "all")}{b("Open", "open")}</div>'


su = screen("support", "page", "Support queue", route="/shop/support", reqs=["L2-036"])


@su.state("default", "Open and resolved customer problems and cancellation requests, newest first.")
def _():
    return founder_page(head("Support", "2 open") + support_filters() + support_list())


@su.state("filtered", "Filtered to open requests only.")
def _():
    return founder_page(head("Support", "2 open") + support_filters("open") + support_list("open"))


@su.state("loading", "Skeleton rows in the list layout.")
def _():
    return founder_page(head("Support") + skel_rows())


@su.state("empty", "Nothing to deal with: no customer has raised a problem.")
def _():
    return founder_page(head("Support") + empty("check-circle", "No requests", "When a customer reports a problem or asks to cancel, it shows up here."))


@su.state("error", "The support queue failed to load; the shop navigation stays.")
def _():
    return founder_page(head("Support") + load_error("support requests", "7a21-d4e0"))


def problem_main(mode: str) -> str:
    """mode: default | replied | resolved | closed | pending | failed"""
    o = ORDERS["SY-48140"]
    photos = '<ul class="uploader" role="list">' + "".join(
        f'<li>{photo("pyrex-bowls", tag=False, alt=f"Customer photo {i} of the bowls", tone=t)}</li>' for i, t in [(1, 1), (2, 3)]) + "</ul>"
    claim = (f'<div class="thread__item"><span class="text-caption">{CUST} · customer · Sun 25 Oct</span>'
             '<p>Two of the four bowls have chips on the rim and the red one has a crack through the base. Listing said excellent.</p>' + photos +
             f'<p class="text-caption">{ic("lock", "icon icon--sm")} Private: only you and {CUST} can see these photos.</p></div>')
    reply = (f'<div class="thread__item"><span class="text-caption">{IDA} · you · Mon 26 Oct</span>'
             '<p>I wrapped them twice and they left here without a chip. I’m so sorry. Let me make this right.</p></div>')
    top, action, comp, st = "", btn("Resolve", "primary", D("resolve-problem")), "Paused", "Delivered"
    thread = claim + (reply if mode != "default" else "")
    if mode == "resolved":
        top = alert("success", "Refunded in full", f"{money(o['total'])} refunded to {CUST}, who has your explanation. Today 11:05 ET.")
        action, comp, st = "", "Refunded", "Refunded"
    elif mode == "closed":
        top = alert("info", "Closed without a refund", f"{CUST} has your explanation and the order is Completed. Today 11:05 ET.")
        action, comp, st = "", "Completed", "Completed"
    elif mode == "pending":
        top = alert("warning", "Refund pending", f"You authorised a full refund of {money(o['total'])}. Waiting for the payment provider to confirm it. The order is not marked Refunded until it does.", icon="clock")
        action, comp = "", "Refund pending"
    elif mode == "failed":
        top = alert("danger", "The refund failed", "The payment provider couldn’t process it. Nothing was refunded and the order is unchanged.",
                    actions=btn("Retry refund", "primary", D("resolve-problem", "failed"), icon="refresh"))
        action = ""
    reply_form = ""
    if mode == "default":
        reply_form = (field("fr-reply", "Your reply", '<textarea class="textarea" id="fr-reply" rows="3"{db}></textarea>', help="Visible to the customer.") +
                      f'<div>{btn("Send reply", "primary")}</div>')
    side = (f'<aside class="panel panel--sunken stack"><span class="eyebrow">Order SY-48140</span>{item_row("pyrex-bowls")}'
            f'<dl class="facts"><dt>Status</dt><dd>{status(st)}</dd><dt>Total</dt><dd class="num">{money(o["total"])}</dd>'
            f'<dt>Delivered</dt><dd>{o["delivered"]}</dd><dt>Completion</dt><dd>{comp}</dd></dl>{action}</aside>')
    body = f'<div class="stack stack--lg">{top}<div class="thread">{thread}</div>{reply_form}</div>'
    return head("Not as described", "Order problem P-0912 · opened yesterday", crumb=("Support", P("support"))) + f'<div class="split">{body}{side}</div>'


pr = screen("problem-detail", "page", "Problem detail", route="/shop/problems/{reference}", reqs=["L2-036", "L2-030", "L2-034"],
            na={"empty": "A problem always has a customer claim and an order."})


@pr.state("default", "Customer claim with private photos; completion paused; the founder can reply or resolve.")
def _():
    return founder_page(problem_main("default"))


@pr.state("replied", "The founder replied; completion stays paused until she resolves it.")
def _():
    return founder_page(problem_main("replied"))


@pr.state("resolved", "Resolved in the customer’s favour: full refund confirmed by the payment provider.")
def _():
    return founder_page(problem_main("resolved"))


@pr.state("closed-no-refund", "Closed without a refund: the customer has her explanation and the order is Completed.")
def _():
    return founder_page(problem_main("closed"))


@pr.state("refund-pending", "Refund authorised and waiting for the provider’s confirmation; not shown as Refunded yet.")
def _():
    return founder_page(problem_main("pending"))


@pr.state("refund-failed", "The provider failed the refund: nothing changed, retry.")
def _():
    return founder_page(problem_main("failed"))


@pr.state("loading", "Problem detail skeleton.")
def _():
    return founder_page('<h1 class="visually-hidden">Loading problem</h1><div style="margin-top: var(--space-10);">' + skel_detail() + "</div>")


@pr.state("error", "The problem failed to load.")
def _():
    return founder_page(head("Order problem", crumb=("Support", P("support"))) + load_error("this problem", "5d10-be3c"))


# ---------------------------------------------------------------------------
# Error and status pages
# ---------------------------------------------------------------------------
def simple(sid, title, route, reqs, desc, page_fn, na=None):
    sc = screen(sid, "page", title, route=route, reqs=reqs, na=na or STATIC_NA)
    sc.state("default", desc)(page_fn)
    return sc


simple("not-found", "Not found", "/*", ["L2-068"], "Unknown URL: say so plainly, offer the shop.",
       lambda: Page(main=center("4<em>0</em>4", "Page not found", "The link may be old or mistyped.", btn("Back to shop", "primary", P("catalog")))),
       na={"loading": "Shown after routing; nothing to load.", "empty": "Not a collection.", "error": "This page is itself the error."})

def forbidden_page(text: str) -> Page:
    return Page(main=center("4<em>0</em>3", "You can’t open this", text, btn("Go to your account", "primary", P("account"))))


fb = simple("forbidden", "Forbidden", "/403", ["L2-003", "L2-010", "L2-068"], "Signed in but not allowed: who can see this and a way back.",
            lambda: forbidden_page("It belongs to another customer’s account."),
            na={"loading": "Shown after the API refuses; nothing to load.", "empty": "Not a collection.", "error": "This page is itself the error."})


@fb.state("management", "A signed-in customer opened a founder-only route (shop management, sales, support): 403, back to their account.")
def _():
    return forbidden_page("Shop management is only for the shop’s founder.")

simple("server-error", "Server error", "/error", ["L2-068"], "Unhandled 500: “Something went wrong”, Try again and the correlation ID.",
       lambda: Page(main=center("5<em>0</em>0", "Something went wrong", "It’s on our side. Your data is safe.",
                                btn("Try again", "primary", icon="refresh") + btn("Back to shop", href=P("catalog")), ref="Ref 3c8e-1d42-a7f0")),
       na={"loading": "Rendered from the error response; nothing to load.", "empty": "Not a collection.", "error": "This page is itself the error."})

simple("offline", "Offline", "(any)", ["L2-068"], "The device lost its connection: “You’re offline”, retry when back.",
       lambda: Page(main=center("", "You’re offline", "Check your connection. We’ll pick up where you left off.", btn("Try again", "primary", icon="refresh"), icon="wifi-off")),
       na={"loading": "Shown from the cached shell without a request.", "empty": "Not a collection.", "error": "This page is itself the error state."})

simple("maintenance", "Maintenance", "(any)", ["L2-066"], "Announced maintenance window with the time we’re back.",
       lambda: Page(main=center("", "Back at 3:00 AM ET", "Sycapooke is down for planned maintenance. Orders and payments are safe.",
                                btn("Check status", "primary", icon="refresh"), icon="wrench"), user="guest"),
       na={"loading": "Served as a static page during maintenance.", "empty": "Not a collection.", "error": "This page is itself the outage notice."})


# ---------------------------------------------------------------------------
# Legal and rules
# ---------------------------------------------------------------------------
def doc_page(title: str, sub: str, body: str) -> Page:
    return Page(main=f'<div class="page--narrow">{head(title, sub)}{body}</div>', user="guest")


def rules_body() -> str:
    rows = "".join(f'<tr><th scope="row">{c}</th><td>{", ".join(s)}</td><td>{size_tag(z)}</td></tr>' for c, s, z in CATEGORIES)
    sizes = ('<div class="table-wrap"><table class="table"><caption class="visually-hidden">Accepted categories and sizes</caption>'
             f'<thead><tr><th scope="col">Category</th><th scope="col">Includes</th><th scope="col">Size</th></tr></thead><tbody>{rows}</tbody></table></div>')
    ida = PEOPLE[FOUNDER]["name"]
    rules = [("Vintage only", "Jewelry, clothing, housewares and electronics. Every piece in the shop is at least 20 years old."),
             ("Honest condition", ", ".join(CONDITIONS) + ". Used - Fair has its flaws described."),
             ("Ships in 5 business days", f"{ida} adds tracking when she ships your order."),
             ("Paid straight to the shop", "Your payment goes directly to the shop’s own account. There is no middleman."),
             ("Problems handled by the founder", f"Tell {ida} within 3 days of delivery. She replies herself and can refund in full or explain why not."),
             ("US only", "Prices in US dollars, shipping within the United States.")]
    lst = "".join(f'<li class="settings-list__body" style="padding-block: var(--space-5); border-bottom: 1px solid var(--color-border-default);">'
                  f'<span class="settings-list__title">{t}</span><span class="settings-list__meta">{d}</span></li>' for t, d in rules)
    return f'<div class="stack stack--lg">{sizes}<ol class="rules-list" role="list">{lst}</ol></div>'


simple("rules", "Shop policies", "/rules", ["L2-011", "L2-016", "L2-033"],
       "The shop policies on one short page: vintage age, condition, shipping, direct payment, problems, US only.",
       lambda: doc_page("Shop policies", "Short, because it only has to cover one shop.", rules_body()))


def privacy_body() -> str:
    items = [("What we collect", "Email, sign-in details, display name, shipping addresses, and your purchases."),
             ("Never public", "Customer accounts are private: your name, email and address never appear on Sycapooke pages. Only the founder’s story is public."),
             ("Encrypted", "Shipping addresses are encrypted at rest. Card numbers never reach us."),
             ("Erasure", "Close your customer account and we erase your data within 30 days."),
             ("Cookies", "Only necessary cookies until you accept analytics.")]
    lst = "".join(f'<li class="settings-list__body" style="padding-block: var(--space-5); border-bottom: 1px solid var(--color-border-default);">'
                  f'<span class="settings-list__title">{t}</span><span class="settings-list__meta">{d}</span></li>' for t, d in items)
    return f'<ul class="rules-list" role="list">{lst}</ul><p class="text-sm" style="margin-top: var(--space-8);"><a href="{D("cookie-settings")}">Cookie settings</a></p>'


simple("privacy", "Privacy", "/privacy", ["L2-054", "L2-056", "L2-005"], "What Sycapooke keeps, what stays private, and erasure.",
       lambda: doc_page("Privacy", "Updated 1 Oct 2026", privacy_body()))


def terms_body() -> str:
    terms = ["You must be 18 or older to hold a customer account.",
             "Every piece in the shop is at least 20 years old, sold by its founder.",
             "Condition and flaws are described honestly, with photos of the actual item.",
             "Payment goes through our payment provider straight to the shop’s account.",
             "You can report a problem until 3 days after delivery. The founder handles it herself and can refund in full or explain why not.",
             "Sycapooke operates in the United States only."]
    lst = "".join(f"<li>{t}</li>" for t in terms)
    return (f'<ol class="terms-list">{lst}</ol><p class="text-sm text-muted" style="margin-top: var(--space-8);">See also the '
            f'<a href="{P("rules")}">shop policies</a> and <a href="{P("privacy")}">privacy</a>.</p>')


simple("terms", "Terms", "/terms", ["L2-001", "L2-036", "L2-030"], "Terms of service as a short numbered list.",
       lambda: doc_page("Terms of service", "Effective 1 Oct 2026", terms_body()))


# ---------------------------------------------------------------------------
# Dialogs
# ---------------------------------------------------------------------------
def reason_field(error: str | None = None, value: str = "", busy: bool = False) -> str:
    ro = " readonly" if busy else ""
    ctrl = f'<textarea class="textarea" id="reason" name="reason" rows="3"{{db}}{ro}>{esc(value)}</textarea>'
    return field("reason", "Your explanation", ctrl, help="Shown to the customer.", error=error)


def rp_dialog(error=None, value="", busy=False, fail=False) -> str:
    o = ORDERS["SY-48140"]
    ch = ('<fieldset><legend>Decision</legend>'
          f'<label class="choice"><input type="radio" name="outcome" checked><span class="choice__text">Refund in full<small>{money(o["total"])} back to {CUST}</small></span></label>'
          f'<label class="choice"><input type="radio" name="outcome"><span class="choice__text">Close without refund (order completes)<small>No refund; the order becomes Completed</small></span></label></fieldset>')
    prim = btn("Sending", "primary", attrs='aria-busy="true"') if busy else btn("Try again" if fail else "Send decision", "primary")
    top = alert("danger", "The refund didn’t go through", "The payment provider couldn’t process it. Nothing was refunded and the order is unchanged. Try again.") if fail else ""
    return dialog("Resolve problem P-0912", ch + reason_field(error, value, busy), btn("Cancel", attrs="disabled" if busy else "") + prim, top=top)


rp = screen("resolve-problem", "dialog", "Resolve problem", opens_from="problem-detail", reqs=["L2-036", "L2-030"])


@rp.state("default", "Refund in full or close without refund, with an explanation the customer sees.")
def _():
    return Dialog(bg=("problem-detail", "default"), html=rp_dialog())


@rp.state("invalid", "Explanation missing.")
def _():
    return Dialog(bg=("problem-detail", "default"), html=rp_dialog(error="Add an explanation. The customer sees it."))


@rp.state("busy", "Decision being sent: confirm busy.")
def _():
    return Dialog(bg=("problem-detail", "default"), html=rp_dialog(value="Chips on the rim and the crack were not disclosed.", busy=True))


@rp.state("failed", "Provider failure: nothing refunded, explanation kept, retry.")
def _():
    return Dialog(bg=("problem-detail", "refund-failed"), html=rp_dialog(value="Chips on the rim and the crack were not disclosed.", fail=True))


def cookie_dialog(analytics: bool) -> str:
    body = (f'<label class="switch" style="justify-content: space-between; width: 100%;"><span class="choice__text">Strictly necessary<small>Sign-in and checkout. Always on.</small></span>'
            f'<input type="checkbox" role="switch" checked disabled></label>'
            f'<label class="switch" style="justify-content: space-between; width: 100%;"><span class="choice__text">Analytics<small>Helps us see which pages are slow.</small></span>'
            f'<input type="checkbox" role="switch"{" checked" if analytics else ""} autofocus></label>')
    return dialog("Cookie settings", body, btn("Cancel") + btn("Save", "primary"), size="sm")


cs = screen("cookie-settings", "dialog", "Cookie settings", opens_from="catalog", reqs=["L2-056"],
            na={"busy": "The choice is stored in the browser instantly; no request.", "invalid": "Only switches; nothing can be invalid."})


@cs.state("default", "Strictly necessary always on; analytics off until the visitor opts in.")
def _():
    return Dialog(bg=("catalog", "guest"), html=cookie_dialog(False))


@cs.state("analytics-on", "Analytics accepted; turning it off removes the cookies.")
def _():
    return Dialog(bg=("catalog", "guest"), html=cookie_dialog(True))


# ---------------------------------------------------------------------------
# Notifications
# ---------------------------------------------------------------------------
cc = screen("cookie-consent", "notification", "Cookie consent", opens_from="catalog", reqs=["L2-056"],
            na={"warning": "Consent has nothing to warn about.", "danger": "Choices are stored locally and cannot fail."})


@cc.state("info", "First visit: only necessary cookies are set until the visitor chooses.")
def _():
    html = (f'<section class="consent" aria-labelledby="consent-title"><h2 id="consent-title" style="font: var(--text-h4);">Cookies</h2>'
            f'<p class="text-sm">We use necessary cookies to run Sycapooke. Analytics only if you say yes.</p>'
            f'<div class="consent__actions">{btn("Accept analytics", "primary")}{btn("Necessary only")}</div>'
            f'<a class="text-sm" href="{D("cookie-settings")}">Cookie settings</a></section>')
    return Note(bg=("catalog", "guest"), after=html)


@cc.state("success", "Choice saved: short toast with a way to change it.")
def _():
    return Note(bg=("catalog", "guest"), toasts=toast_region(toast("success", "Cookie choice saved", "Necessary only. Change it any time in the footer.")))


sb = screen("system-banner", "notification", "System notices", opens_from="catalog", reqs=["L2-066", "L2-068"])


@sb.state("info", "Announced maintenance: persistent banner with the window in ET.")
def _():
    return Note(bg=("catalog", "default"), top=banner("info", "Planned maintenance Tue 27 Oct, 2:00–3:00 AM ET.", icon="wrench"))


@sb.state("success", "Connection restored: brief toast.")
def _():
    return Note(bg=("catalog", "default"), toasts=toast_region(toast("success", "Back online")))


@sb.state("warning", "Offline: persistent banner instead of a generic error.")
def _():
    return Note(bg=("catalog", "default"), top=banner("warning", "You’re offline. We’ll retry when you’re back.", dismiss=False, icon="wifi-off"))


@sb.state("danger", "A request failed with 500: error toast with Try again and the correlation ID; never auto-dismisses.")
def _():
    pg = Note(bg=("catalog", "default"), toasts=toast_region(toast("danger", "Something went wrong", "Ref 3c8e-1d42-a7f0", action="Try again")))
    pg.notes = note("Error toasts stay until dismissed.", "bottom: 9rem; right: 1rem;")
    return pg
