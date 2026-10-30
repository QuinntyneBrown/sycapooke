"""Marketing page at /welcome and its 29-second pitch video dialog (L1-020, L2-078, L2-079, L2-083, L2-089 to L2-091).

Copy rules (ADR general/0005): headlines of 3 to 6 words, one supporting line, scannable
points; everything is vintage (at least 20 years old) and the copy never promises anything
newer. The copy is identical to the app's WelcomePage (src/frontend/projects/marketing/src/lib/welcome/).

Look (Heirloom, from the landing page): the hero sits on paper beside the hero photo; the
For sellers band and the closing band are terracotta with cream text and ink buttons; Our
story is a cream band under a line rule; everything else is paper. No shapes, no gradients.
The hero photo is the pitch video's poster (ADR platform/0014): one "Play · 0:29" pill opens the
video dialog; nothing plays before the visitor presses it.
"""
from mk import *

MEDIA = f"{A}media/"

STATIC_NA = {"loading": "Static content ships with the app; the page calls no Sycapooke API endpoint.",
             "empty": "The page always has content.",
             "error": "Static content cannot fail to load once the app is served; the video has its own error state."}

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


def seller_href(member: bool) -> str:
    return P("sell") if member else P("register")


def hero_photo() -> str:
    """The hero photo slot is the video poster: /media/sycapooke-hero.jpg in the app, a placeholder illustration here.
    4:5, 2rem corners, no frame; one cream pill reading "Play · 0:29" opens the pitch video dialog (L2-089)."""
    return (f'<button class="video-poster" type="button"><img src="{A}hero.svg" alt="" width="1200" height="1500">'
            f'<span class="video-poster__play">{ic("play")}Play<span class="video-poster__meta" aria-hidden="true">· 0:29</span></span></button>')


def hero(member: bool) -> str:
    actions = btn("Shop the catalog", "primary lg", P("catalog")) + btn("Start selling", "lg", seller_href(member))
    return (f'<section class="marketing-hero" aria-labelledby="welcome-title"><div class="marketing-hero__inner">'
            '<div><p class="marketing-hero__eyebrow">Jewelry · Clothing · Housewares · Electronics</p>'
            '<h1 class="marketing-hero__title" id="welcome-title">A home for the things that <em>mattered.</em></h1>'
            '<p class="marketing-hero__lede">Sycapooke is the marketplace made only for vintage — for families downsizing a lifetime of treasures, and the people who’ll love them next.</p>'
            f'<div class="marketing-hero__actions">{actions}</div></div>'
            f'<div class="marketing-hero__media">{hero_photo()}</div></div></section>')


def points(items) -> str:
    return '<ul class="feature-band__points" role="list">' + "".join(
        f'<li class="feature-band__point"><strong>{t}</strong><span>{d}</span></li>' for t, d in items) + "</ul>"


def band(bid: str, eyebrow: str, title: str, body: str, action: str = "", mod: str = "", lede: str = "") -> str:
    a = f'<div class="feature-band__actions">{action}</div>' if action else ""
    e = f'<p class="feature-band__eyebrow">{eyebrow}</p>' if eyebrow else ""
    l = f'<p class="feature-band__lede">{lede}</p>' if lede else ""
    b = f'<div>{body}</div>' if body else ""
    return (f'<section class="feature-band {mod}" aria-labelledby="{bid}"><div class="feature-band__inner">'
            f'<div>{e}<h2 class="feature-band__title" id="{bid}">{title}</h2>{l}{a}</div>'
            f'{b}</div></section>')


def steps() -> str:
    return '<ol class="steps">' + "".join(
        f'<li class="steps__item"><div class="steps__body"><h3 class="steps__title">{t}</h3><p class="steps__text">{d}</p></div></li>' for t, d in STEPS) + "</ol>"


def story() -> str:
    """Our story: the founder's quote (mustard rule) beside the founder photo (/media/sycapooke-founder.jpg in the app)."""
    return (f'<div class="story"><blockquote class="quote"><p>“{QUOTE}”</p><cite>{FOUNDER}</cite></blockquote>'
            f'<figure class="story__photo"><img src="{A}founder.svg" alt="The founder at her kitchen table, a teacup in hand" width="800" height="1000"></figure></div>')


def closing(member: bool) -> str:
    """The closing call to action is the terracotta band with an ink button."""
    return band("closing-title", "", "Treasures with a <em>story.</em>", "", btn("Shop the catalog", "inverse lg", P("catalog")),
                "feature-band--sell feature-band--stacked feature-band--closing", lede="New treasures arrive daily, every one at least 20 years old.")


def welcome(member: bool) -> Page:
    main = (hero(member)
            + band("shoppers-title", "For shoppers", "Every piece is at least 20 years old.", points(SHOPPER_POINTS),
                   btn("Browse the catalog", "primary", P("catalog")))
            + band("sellers-title", "For sellers", "Downsizing? Let your treasures find their next home.", points(SELLER_POINTS),
                   btn("List your first item", "inverse", seller_href(member)), "feature-band--sell")
            + band("how-title", "How it works", "Three steps. That’s it.", steps(), mod="feature-band--stacked")
            + band("story-title", "Our story", "sy·ca·pooke", story(), mod="feature-band--story feature-band--stacked")
            + closing(member))
    return Page(main=main, user="member" if member else "guest", main_class="marketing")


wl = screen("welcome", "page", "Treasures with a story", route="/welcome", reqs=["L2-078", "L2-079", "L2-083", "L2-089", "L2-090", "L2-091"], na=STATIC_NA)


@wl.state("default", "Signed out: hero on paper with the hero photo as the video poster, shoppers, the terracotta sellers band, three steps, the cream Our story band and the terracotta closing CTA; Start selling goes to /join.")
def _():
    pg = welcome(False)
    pg.notes = note("Flat bands: paper, terracotta (sellers, closing) and cream (story). Nothing decorative moves. The hero photo is the video poster: its Play pill opens the pitch video dialog and nothing autoplays. The hero and founder photos are /media/sycapooke-hero.jpg and /media/sycapooke-founder.jpg in the app.",
                    "top: 6rem; right: 1rem;")
    return pg


@wl.state("signed-in", "Signed in: the same page; Start selling and List your first item go to /sell.")
def _():
    return welcome(True)


# ---------------------------------------------------------------------------
# Pitch video dialog (L2-089 to L2-091): the 29-second video in an lg dialog with a hidden heading
# ---------------------------------------------------------------------------
def player(state: str) -> str:
    """sy-video-player: ink 16:9 frame with the native video, a state overlay and Play/Pause + Captions pills.
    The poster is the hero photo (/media/sycapooke-hero.jpg in the app)."""
    track_default = " default" if state == "captions" else ""
    video = (f'<video controls playsinline preload="metadata" poster="{A}hero.svg" aria-label="Sycapooke video">'
             f'<source src="{MEDIA}sycapooke-pitch.mp4" type="video/mp4"><source src="{MEDIA}sycapooke-pitch.webm" type="video/webm">'
             f'<track kind="captions" src="{MEDIA}sycapooke-pitch.vtt" srclang="en" label="English"{track_default}></video>')
    overlay = ""
    if state == "loading":
        overlay = ('<div class="video-player__state" role="status"><span class="spinner spinner--lg" aria-hidden="true"></span>'
                   '<p>Loading video…</p></div>')
    elif state == "error":
        overlay = (f'<div class="video-player__state" role="alert">{ic("alert", "icon icon--lg")}<strong>The video didn’t load</strong>'
                   f'<p>Check your connection and try again.</p>{btn("Try again", "primary", icon="refresh")}</div>')
    playing = state in ("default", "captions")
    play = btn("Pause", "primary", icon="pause") if playing else btn("Play", "primary", icon="play")
    cc_on = state == "captions"
    cc = btn("Captions", "", icon="captions", attrs=f'aria-pressed="{"true" if cc_on else "false"}"')
    controls = "" if state == "error" else play + cc
    return (f'<div class="video-player"><div class="video-player__frame">{video}{overlay}</div>'
            f'<div class="video-player__controls">{controls}</div></div>')


pv = screen("pitch-video", "dialog", "Pitch video", opens_from="welcome", reqs=["L2-089", "L2-090", "L2-091"],
            na={"busy": "Playing a video sends nothing to the Sycapooke API.", "invalid": "Nothing to fill in."})


@pv.state("default", "Playing after the visitor pressed Play: no visible heading, native controls plus Pause and Captions; captions off by default.")
def _():
    d = Dialog(bg=("welcome", "default"), html=dialog("Sycapooke video", player("default"), size="lg", title_hidden=True))
    d.notes = note("Opens only from the poster's Play pill and plays with sound because the visitor asked. Space or K pauses; Esc closes, stops the video and returns focus to the poster.",
                   "top: 1rem; left: 1rem;")
    return d


@pv.state("loading", "The video is buffering: spinner over the poster; Play and Captions stay usable.")
def _():
    return Dialog(bg=("welcome", "default"), html=dialog("Sycapooke video", player("loading"), size="lg", title_hidden=True))


@pv.state("error", "The video failed to load: plain message and Try again; the page behind still works once closed.")
def _():
    return Dialog(bg=("welcome", "default"), html=dialog("Sycapooke video", player("error"), size="lg", title_hidden=True))


@pv.state("captions", "Captions on: the English track shows; the Captions button is pressed.")
def _():
    d = Dialog(bg=("welcome", "default"), html=dialog("Sycapooke video", player("captions"), size="lg", title_hidden=True))
    d.notes = note("Captions come from sycapooke-pitch.vtt and stay on for the rest of the visit.", "top: 1rem; left: 1rem;")
    return d
