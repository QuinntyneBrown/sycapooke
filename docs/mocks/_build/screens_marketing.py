"""Marketing page at /welcome and its 29-second pitch video dialog (L1-020, L2-078, L2-079, L2-083, L2-089 to L2-091).

Copy rules (ADR general/0005): headlines of 3 to 6 words, one supporting line, scannable
points; everything is vintage (at least 20 years old) and the copy never promises anything
newer. The copy is identical to the app's WelcomePage (src/frontend/projects/marketing/src/lib/welcome/).

Look (Heirloom, from the landing page): the hero sits on paper beside the hero photo; the
From my collection band and the closing band are terracotta with cream text and ink buttons; Our
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
                  ("From the founder’s own collection", "Every piece is hers, kept with care and sent on with its story."),
                  ("Honest condition", "Five grades, and flaws named up front.")]
COLLECTION_POINTS = [("Chosen by hand", "Each treasure is picked from her own shelves."),
                     ("Described with care", "Condition, flaws and a little of its story, up front.")]
STEPS = [("Find a treasure", "Browse jewelry, clothing, housewares and electronics."),
         ("Pay securely", "Check out with a card; the piece is yours once you pay."),
         ("Receive its story", "It arrives packed with care, with a note about where it came from.")]
# Placeholder: awaiting the founder's approval of her own words (L2-078). No age, family or biography is stated.
QUOTE = "Placeholder quote: every piece in my collection has a story, and I would love to share it with you."
FOUNDER_CREDIT = "Founder of Sycapooke"


def hero_photo() -> str:
    """The hero photo slot is the video poster: /media/sycapooke-hero.jpg in the app, a placeholder illustration here.
    4:5, 2rem corners, no frame; one cream pill reading "Play · 0:29" opens the pitch video dialog (L2-089)."""
    return (f'<button class="video-poster" type="button"><img src="{A}hero.svg" alt="" width="1200" height="1500">'
            f'<span class="video-poster__play">{ic("play")}Play<span class="video-poster__meta" aria-hidden="true">· 0:29</span></span></button>')


def hero(member: bool) -> str:
    actions = btn("Shop the collection", "primary lg", P("catalog")) + btn("Meet the founder", "lg", "#our-story")
    return (f'<section class="marketing-hero" aria-labelledby="welcome-title"><div class="marketing-hero__inner">'
            '<div><p class="marketing-hero__eyebrow">Jewelry · Clothing · Housewares · Electronics</p>'
            '<h1 class="marketing-hero__title" id="welcome-title">A home for the things that <em>mattered.</em></h1>'
            '<p class="marketing-hero__lede">Antiques and vintage treasures from one Caribbean woman’s own collection. Every piece is at least 20 years old.</p>'
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
    return (f'<div class="story"><blockquote class="quote"><p>“{QUOTE}”</p><cite>{FOUNDER_CREDIT}</cite></blockquote>'
            f'<figure class="story__photo"><img src="{A}founder.svg" alt="Portrait of the founder, an older Caribbean woman, smiling in her home" width="800" height="1000"></figure></div>')


def closing(member: bool) -> str:
    """The closing call to action is the terracotta band with an ink button."""
    return band("closing-title", "", "Treasures with a <em>story.</em>", "", btn("Browse the collection", "inverse lg", P("catalog")),
                "feature-band--sell feature-band--stacked feature-band--closing", lede="Explore her collection. Every piece is at least 20 years old.")


def welcome(member: bool) -> Page:
    main = (hero(member)
            + band("shoppers-title", "For shoppers", "Every piece is at least 20 years old.", points(SHOPPER_POINTS),
                   btn("Browse the collection", "primary", P("catalog")))
            + band("collection-title", "From my collection", "Treasures I have kept, ready for their next home.", points(COLLECTION_POINTS),
                   btn("See what is on the shelf", "inverse", P("catalog")), "feature-band--sell")
            + band("how-title", "How it works", "Three steps. That’s it.", steps(), mod="feature-band--stacked")
            + band("story-title", "Our story", "sy·ca·pooke", story(), mod="feature-band--story feature-band--stacked").replace('<section class="feature-band', '<section id="our-story" class="feature-band', 1)
            + closing(member))
    return Page(main=main, user="member" if member else "guest", main_class="marketing")


wl = screen("welcome", "page", "Treasures with a story", route="/welcome", reqs=["L2-078", "L2-079", "L2-083", "L2-089", "L2-090", "L2-091"], na=STATIC_NA)


@wl.state("default", "Signed out: hero on paper with the hero photo as the video poster, shoppers, the terracotta From my collection band, three buying steps, the cream Our story band (#our-story) and the terracotta closing CTA; Meet the founder jumps to Our story.")
def _():
    pg = welcome(False)
    pg.notes = note("The founder quote is placeholder copy awaiting the founder’s approval; no age, family history or biography is stated. Flat bands: paper, terracotta (From my collection, closing) and cream (story). Nothing decorative moves. The hero photo is the video poster: its Play pill opens the pitch video dialog and nothing autoplays. The hero and founder photos are /media/sycapooke-hero.jpg and /media/sycapooke-founder.jpg in the app.",
                    "top: 6rem; right: 1rem;")
    return pg


@wl.state("signed-in", "Signed in as a customer: the same page; no selling or management links appear.")
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
