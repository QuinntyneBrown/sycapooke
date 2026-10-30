"""Marketing page at /welcome (L1-020, L2-078, L2-079, L2-083).

Copy rules (ADR general/0005): headlines of 3 to 6 words, one supporting line, scannable
points; everything is vintage (at least 20 years old) and the copy never promises anything
newer. The copy is identical to the app's WelcomePage (src/frontend/projects/marketing/src/lib/welcome/).
"""
from mk import *

STATIC_NA = {"loading": "Static content ships with the app; the page calls no Sycapooke API endpoint.",
             "empty": "The page always has content.",
             "error": "Static content cannot fail to load once the app is served."}

SHOPPER_POINTS = [("Four focused categories", "Jewelry, clothing, housewares and electronics. Nothing to filter out."),
                  ("Listed by a real person", "Every piece comes from a family with a story to tell about it."),
                  ("Honest condition", "Five grades, and flaws named up front.")]
SELLER_POINTS = [("Big buttons, clear steps", "Photos, a price and the condition. That’s it."),
                 ("A real person to call", "Stuck? Ring 1-800-555-0123 and a person picks up.")]
STEPS = [("List it", "Add photos, a price and the condition."),
         ("Sell it", "A buyer pays. You ship within 5 business days."),
         ("Get paid", "The order completes and your payout is on its way.")]
QUOTE = ("It’s the made-up word I whispered to my son when he was a baby. When it came time to clear out our family home, "
         "I realized every teacup and brooch deserved that same word — and someone who’d see it too.")
FOUNDER = "Founder, age 70, grandmother of five"


def shapes(quiet: bool = False) -> str:
    q = " shapes--quiet" if quiet else ""
    return (f'<div class="shapes{q}" aria-hidden="true"><span class="shapes__shape shapes__shape--brand"></span>'
            '<span class="shapes__shape shapes__shape--tone"></span><span class="shapes__shape shapes__shape--light"></span>'
            '<span class="shapes__stripes"></span></div>')


def seller_href(member: bool) -> str:
    return P("sell") if member else P("register")


def hero_photo() -> str:
    """The hero photo slot: /media/sycapooke-hero.jpg in the app, a placeholder illustration here."""
    return (f'<figure class="marketing-hero__photo"><img src="{A}hero.svg" alt="A brooch, a teacup and a transistor radio laid out on a linen cloth" '
            'width="1200" height="900"></figure>')


def hero(member: bool) -> str:
    actions = btn("Shop the catalog", "primary lg", P("catalog")) + btn("Start selling", "lg", seller_href(member))
    return (f'<section class="marketing-hero" aria-labelledby="welcome-title">{shapes()}<div class="marketing-hero__inner">'
            '<div><p class="marketing-hero__eyebrow">Jewelry · Clothing · Housewares · Electronics</p>'
            '<h1 class="marketing-hero__title" id="welcome-title">A home for the things that <em>mattered.</em></h1>'
            '<p class="marketing-hero__lede">Sycapooke is the marketplace made only for vintage — for families downsizing a lifetime of treasures, and the people who’ll love them next.</p>'
            f'<div class="marketing-hero__actions">{actions}</div></div>'
            f'<div class="marketing-hero__media">{hero_photo()}</div></div></section>')


def points(items) -> str:
    return '<ul class="feature-band__points" role="list">' + "".join(
        f'<li class="feature-band__point"><strong>{t}</strong><span>{d}</span></li>' for t, d in items) + "</ul>"


def band(bid: str, eyebrow: str, title: str, body: str, action: str = "", mod: str = "") -> str:
    a = f'<div class="feature-band__actions">{action}</div>' if action else ""
    return (f'<section class="feature-band {mod}" aria-labelledby="{bid}"><div class="feature-band__inner">'
            f'<div><p class="feature-band__eyebrow">{eyebrow}</p><h2 class="feature-band__title" id="{bid}">{title}</h2>{a}</div>'
            f'<div>{body}</div></div></section>')


def steps() -> str:
    return '<ol class="steps">' + "".join(
        f'<li class="steps__item"><h3 class="steps__title">{t}</h3><p class="steps__text">{d}</p></li>' for t, d in STEPS) + "</ol>"


def story() -> str:
    """Our story: the founder's quote beside the founder photo (/media/sycapooke-founder.jpg in the app)."""
    return (f'<div class="story"><blockquote class="quote quote--story"><p>“{QUOTE}”</p><cite>{FOUNDER}</cite></blockquote>'
            f'<figure class="story__photo"><img src="{A}founder.svg" alt="The founder at her kitchen table, a teacup in hand" width="800" height="1000"></figure></div>')


def closing(member: bool) -> str:
    actions = btn("Shop the catalog", "primary lg", P("catalog"))
    return (f'<section class="marketing-hero marketing-hero--closing" aria-labelledby="closing-title">{shapes(quiet=True)}'
            '<div class="marketing-hero__inner"><div><h2 class="marketing-hero__title" id="closing-title">Treasures with a <em>story.</em></h2>'
            f'<p class="marketing-hero__lede">New treasures arrive daily, every one at least 20 years old.</p>'
            f'<div class="marketing-hero__actions">{actions}</div></div></div></section>')


def welcome(member: bool) -> Page:
    main = (hero(member)
            + band("shoppers-title", "For shoppers", "Every piece is at least 20 years old.", points(SHOPPER_POINTS),
                   btn("Browse the catalog", "primary", P("catalog")))
            + band("sellers-title", "For sellers", "Downsizing? Let your treasures find their next home.", points(SELLER_POINTS),
                   btn("List your first item", "primary", seller_href(member)), "feature-band--surface")
            + band("how-title", "How it works", "Three steps. That’s it.", steps(), mod="feature-band--stacked")
            + band("story-title", "Our story", "sy·ca·pooke", story(), mod="feature-band--stacked")
            + closing(member))
    return Page(main=main, user="member" if member else "guest", main_class="marketing")


wl = screen("welcome", "page", "Treasures with a story", route="/welcome", reqs=["L2-078", "L2-079", "L2-083"], na=STATIC_NA)


@wl.state("default", "Signed out: hero with the hero photo, shoppers, sellers, three steps, our story and a closing CTA; Start selling goes to /join.")
def _():
    pg = welcome(False)
    pg.notes = note("Shapes drift slowly; prefers-reduced-motion stops them. The hero and founder photos are /media/sycapooke-hero.jpg and /media/sycapooke-founder.jpg in the app.",
                    "top: 6rem; right: 1rem;")
    return pg


@wl.state("signed-in", "Signed in: the same page; Start selling and List your first item go to /sell.")
def _():
    return welcome(True)
