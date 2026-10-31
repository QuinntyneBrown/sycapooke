"""Build the Sycapooke pitch video (29.5 seconds).

Inputs  LINES below                          one narrated line per slide (caption text, spoken text)
        music/warm-nostalgic-sentimental-excerpt.mp3  "Warm Nostalgic Sentimental Music" (andriig, Pixabay), ducked under the voice
        kokoro/                              Kokoro-82M ONNX + af_heart voice from Hugging Face (auto-downloaded)
        docs/design-system/assets/logo.svg   the Heirloom logo
        src/frontend/projects/sycapooke/public/media/*.jpg and public/fonts/*.woff2  the photos and typefaces
        src/backend/Sycapooke.Cli/SeedPhotos/**/*.jpg  the category photos
Outputs sycapooke-pitch.mp4 (H.264/AAC), sycapooke-pitch.webm (VP9/Opus), sycapooke-pitch.vtt (captions),
        sycapooke-pitch.txt (transcript), written to the app's public media folder
        (src/frontend/projects/sycapooke/public/media/) and copied to docs/mocks/assets/media/.
        The poster is the hero photo (sycapooke-hero.jpg), so the page looks the same with or without the video.

Needs: ffmpeg/ffprobe, python playwright (chromium); for narration also kokoro-onnx, onnxruntime,
huggingface_hub and numpy.
Run:   python -I docs/mocks/_build/video/build_video.py            full build: narration and music
       python -I docs/mocks/_build/video/build_video.py --silent   no narration; music only if the file exists
"""
import base64, pathlib, shutil, subprocess, sys, tempfile, wave

from playwright.sync_api import sync_playwright

HERE = pathlib.Path(__file__).resolve().parent
REPO = HERE.parents[3]
APP = REPO / "src/frontend/projects/sycapooke/public"
SEED = REPO / "src/backend/Sycapooke.Cli/SeedPhotos"
OUT_DIRS = [APP / "media", REPO / "docs/mocks/assets/media"]
NAME = "sycapooke-pitch"
HF_REPO, VOICE, SPEED, LANG = "onnx-community/Kokoro-82M-v1.0-ONNX", "af_heart", 1.05, "en-us"
MUSIC = HERE / "music/warm-nostalgic-sentimental-excerpt.mp3"
MUSIC_DB, INTRO_DB, OUTRO_DB = -12, -7, 0  # under the voice; music alone before the first line; the swell after the last
SWELL = 1.2  # seconds the music takes to rise after the narration ends
TARGET, LEAD, TAIL, MIN_GAP = 29.5, 2.0, 4.0, 0.25  # total length; music-only seconds before the first line and after the last
SILENT_WPS = 2.4  # words per second used to time captions when there is no narration

# (caption text, text the narrator speaks)
LINES = [
    ("Every teacup has a story. Every brooch, every radio, every coat.",
     "Every teacup has a story. Every brooch, every radio, every coat."),
    ("Sycapooke is a home for antiques and vintage treasures.",
     "Sycapooke is a home for antiques and vintage treasures."),
    ("Jewelry, clothing, housewares and electronics. Every piece at least 20 years old.",
     "Jewelry, clothing, housewares and electronics. Every piece at least twenty years old."),
    ("From my collection. Treasures I have kept, ready for their next home.",
     "From my collection. Treasures I have kept, ready for their next home."),
    ("Find a treasure. Pay securely. Receive its story.", "Find a treasure. Pay securely. Receive its story."),
    ("Sycapooke. Treasures with a story.", "Sycapooke. Treasures with a story."),
]


def uri(path, mime):
    return f"data:{mime};base64," + base64.b64encode(pathlib.Path(path).read_bytes()).decode()


def jpg(path):
    return uri(path, "image/jpeg")


LOGO = uri(REPO / "docs/design-system/assets/logo.svg", "image/svg+xml")
HERO, FOUNDER = jpg(APP / "media/sycapooke-hero.jpg"), jpg(APP / "media/sycapooke-founder.jpg")
TILES = [("Jewelry", jpg(SEED / "Jewelry/gold-filigree-brooch.jpg")), ("Clothing", jpg(SEED / "Clothing/camel-wool-overcoat.jpg")),
         ("Housewares", jpg(SEED / "Housewares/bone-china-tea-set.jpg")), ("Electronics", jpg(SEED / "Electronics/walnut-transistor-radio.jpg"))]
FONTS = {k: uri(APP / "fonts" / f, "font/woff2") for k, f in (
    ("fraunces", "fraunces-latin.woff2"), ("fraunces_i", "fraunces-italic-latin.woff2"), ("sans", "source-sans-3-latin.woff2"))}

SLIDES = [
    # 1. hero photo beside the opening line
    f'<div class="two"><div><p class="k">Jewelry · Clothing · Housewares · Electronics</p>'
    '<h1>Every teacup<br>has a <em>story.</em></h1><p class="sub">Every brooch, every radio, every coat.</p></div>'
    f'<img class="photo" src="{HERO}" alt=""></div>',
    # 2. logo and the promise
    f'<img class="logo" src="{LOGO}" alt=""><h1>A home for the things<br><em>that mattered.</em></h1>',
    # 3. the four categories as photo tiles
    '<h1 class="h1-sm">Every piece at least<br><em>twenty years old.</em></h1>'
    '<p class="sub">From the founder&rsquo;s own collection, with honest condition.</p><div class="tiles">'
    + "".join(f'<div class="tile"><img src="{src}" alt=""><span class="pill">{name}</span></div>' for name, src in TILES) + '</div>',
    # 4. the terracotta band for the founder's collection
    '<p class="k k-cream">Founder of Sycapooke</p><h1>From my<br><em>collection.</em></h1>'
    '<p class="sub sub-cream">Treasures I have kept,<br>ready for their next home.</p>',
    # 5. four ringed steps
    '<p class="k">How it works</p><ol class="steps"><li><b>1</b>Find a treasure</li><li><b>2</b>Pay securely</li>'
    '<li><b>3</b>Receive its story</li></ol>',
    # 6. the founder and the tagline
    f'<div class="two"><div><img class="logo logo-sm" src="{LOGO}" alt=""><h1>Treasures<br>with a <em>story.</em></h1>'
    '<p class="sub">www.sycapooke.com</p></div>'
    f'<img class="photo" src="{FOUNDER}" alt=""></div>',
]
TERRACOTTA = {3}  # slide indexes on the terracotta band

# Heirloom tokens by value (docs/design-system/tokens/tokens.css): Paper #f6f0e4, Cream #fbf7ef, Ink #2b2118,
# Muted #665748, Terracotta #a04228, Forest #2f4a3a; Fraunces 600 headings with italic terracotta emphasis,
# Source Sans 3 body; 2rem photo corners, 1rem tile corners, pills. Flat colour only: no shapes, no gradients.
CSS = f"""
@font-face{{font-family:Fraunces;font-weight:100 900;src:url({FONTS['fraunces']}) format('woff2')}}
@font-face{{font-family:Fraunces;font-style:italic;font-weight:100 900;src:url({FONTS['fraunces_i']}) format('woff2')}}
@font-face{{font-family:'Source Sans 3';font-weight:200 900;src:url({FONTS['sans']}) format('woff2')}}
html,body{{margin:0;width:1920px;height:1080px;overflow:hidden}}
body{{background:#f6f0e4;color:#2b2118;font-family:'Source Sans 3',sans-serif;display:flex;flex-direction:column;
  justify-content:center;align-items:flex-start;padding:0 160px;box-sizing:border-box}}
body.band{{background:#a04228;color:#fbf7ef}}
h1{{font:600 128px/1.05 Fraunces,Georgia,serif;letter-spacing:-.025em;margin:0}}
h1.h1-sm{{font-size:96px}}
em{{font-style:italic;color:#a04228}}
.band em{{color:#fbf7ef}}
.k{{font:600 36px 'Source Sans 3',sans-serif;color:#2f4a3a;margin:0 0 36px}}
.k-cream{{color:rgb(251 247 239 / .8)}}
.sub{{font:400 52px/1.3 'Source Sans 3',sans-serif;color:#665748;margin:40px 0 0}}
.sub-cream{{color:rgb(251 247 239 / .85)}}
.two{{display:grid;grid-template-columns:1fr 560px;gap:120px;align-items:center;width:100%}}
.photo{{width:560px;height:700px;object-fit:cover;border-radius:2rem;display:block}}
.logo{{width:620px;display:block;margin-bottom:56px}}
.logo-sm{{width:460px;margin-bottom:48px}}
.tiles{{display:grid;grid-template-columns:repeat(4,1fr);gap:32px;width:100%;margin-top:56px}}
.tile{{position:relative;border-radius:1rem;overflow:hidden;background:#fbf7ef;aspect-ratio:1}}
.tile img{{width:100%;height:100%;object-fit:cover;display:block}}
.pill{{position:absolute;left:16px;top:16px;background:rgb(251 247 239 / .95);color:#2b2118;border-radius:999px;
  padding:10px 22px;font:600 30px 'Source Sans 3',sans-serif}}
.steps{{list-style:none;display:grid;grid-template-columns:repeat(3,1fr);gap:48px;margin:0;padding:0;width:100%}}
.steps li{{font:600 56px/1.2 Fraunces,Georgia,serif}}
.steps b{{display:grid;place-items:center;width:112px;height:112px;border-radius:50%;border:4px solid #2b2118;
  font:600 56px Fraunces,Georgia,serif;margin-bottom:32px}}
"""


def dur(p):
    return float(subprocess.check_output(["ffprobe", "-v", "error", "-show_entries", "format=duration",
                                          "-of", "csv=p=0", str(p)]).strip())


def ts(t):
    return f"{int(t // 3600):02}:{int(t % 3600 // 60):02}:{t % 60:06.3f}"


class Narrator:
    """Kokoro-82M via onnxruntime; model and voice come from Hugging Face on first run."""

    def __init__(self):
        import numpy as np
        import onnxruntime as ort
        from huggingface_hub import hf_hub_download
        from kokoro_onnx.tokenizer import Tokenizer

        self.np = np
        k = HERE / "kokoro"
        k.mkdir(exist_ok=True)
        for sub, name in (("onnx", "model.onnx"), ("voices", f"{VOICE}.bin")):
            if not (k / name).exists():
                shutil.copy(hf_hub_download(HF_REPO, f"{sub}/{name}"), k / name)
        self.tok = Tokenizer()
        self.sess = ort.InferenceSession(str(k / "model.onnx"))
        self.style = np.fromfile(k / f"{VOICE}.bin", dtype=np.float32).reshape(-1, 1, 256)

    def say(self, text, out):
        np = self.np
        ids = self.tok.tokenize(self.tok.phonemize(text, lang=LANG))
        audio = self.sess.run(None, {"input_ids": np.array([[0, *ids, 0]], dtype=np.int64),
                                     "style": self.style[len(ids)],
                                     "speed": np.array([SPEED], dtype=np.float32)})[0].reshape(-1)
        audio = np.trim_zeros(np.where(np.abs(audio) < 0.004, 0, audio))  # trim leading/trailing silence
        with wave.open(str(out), "wb") as w:
            w.setnchannels(1); w.setsampwidth(2); w.setframerate(24000)
            w.writeframes((np.clip(audio, -1, 1) * 32767).astype(np.int16).tobytes())


def main():
    silent = "--silent" in sys.argv[1:]
    music = MUSIC.exists()
    if not music:
        print(f"note: {MUSIC.name} is missing, building without music (see README.md for the track and how to cut it)")
    tmp = pathlib.Path(tempfile.mkdtemp())
    voices = []
    if silent:
        voices = [(None, len(caption.split()) / SILENT_WPS) for caption, _ in LINES]
    else:
        narrator = Narrator()
        for i, (_, spoken) in enumerate(LINES):
            out = tmp / f"v{i}.wav"
            narrator.say(spoken, out)
            voices.append((out, dur(out)))

    # Timeline: the gaps between lines stretch so the video lands on TARGET seconds.
    speech = sum(d for _, d in voices)
    gap = (TARGET - LEAD - TAIL - speech) / (len(voices) - 1)
    if gap < MIN_GAP:
        raise SystemExit(f"narration is {speech:.1f}s; shorten LINES or raise SPEED to fit {TARGET:.0f}s")
    starts, t = [], LEAD
    for _, d in voices:
        starts.append(t)
        t += d + gap
    total = TARGET
    slide_d = [(starts[i + 1] if i + 1 < len(starts) else total) - (0 if i == 0 else starts[i]) for i in range(len(starts))]

    with sync_playwright() as pw:
        b = pw.chromium.launch()
        pg = b.new_page(viewport={"width": 1920, "height": 1080})
        for i, html in enumerate(SLIDES):
            cls = ' class="band"' if i in TERRACOTTA else ""
            pg.set_content(f"<!doctype html><html><head><style>{CSS}</style></head><body{cls}>{html}</body></html>")
            pg.evaluate("document.fonts.ready")
            pg.wait_for_timeout(700)
            pg.screenshot(path=str(tmp / f"s{i}.png"))
        b.close()

    inputs, filt, n = [], [], 0
    if not silent:
        # Narration placed at its start times, music ducked under it by a sidechain compressor.
        for i, (p, _) in enumerate(voices):
            inputs += ["-i", str(p)]
            ms = int(starts[i] * 1000)
            filt.append(f"[{i}:a]aresample=48000,adelay={ms}|{ms}[a{i}]")
        n = len(voices)
        mix = "".join(f"[a{i}]" for i in range(n)) + f"amix=inputs={n}:normalize=0,apad,atrim=0:{total:.3f},volume=1.7"
        filt.append(f"{mix},asplit=2[vx][key]" if music else f"{mix},alimiter=limit=0.95[aout]")
    if music:
        inputs += ["-i", str(MUSIC)]
        # Music alone at INTRO_DB, dipping to MUSIC_DB as the first line starts, rising to OUTRO_DB over SWELL
        # seconds once the last line ends, then fading out over the final 1.5 s.
        g0, g1, g2 = (10 ** (db / 20) for db in (INTRO_DB, MUSIC_DB, OUTRO_DB))
        s0, end = starts[0], starts[-1] + voices[-1][1]
        vol = (f"if(lt(t,{s0 - 0.6:.3f}),{g0:.4f},if(lt(t,{s0:.3f}),{g0:.4f}+({g1:.4f}-{g0:.4f})*(t-{s0 - 0.6:.3f})/0.6,"
               f"if(lt(t,{end:.3f}),{g1:.4f},if(lt(t,{end + SWELL:.3f}),{g1:.4f}+({g2:.4f}-{g1:.4f})*(t-{end:.3f})/{SWELL},{g2:.4f}))))")
        bg = (f"[{n}:a]aresample=48000,atrim=0:{total:.3f},asetpts=PTS-STARTPTS,volume='{vol}':eval=frame,"
              f"afade=t=in:d=0.8,afade=t=out:st={total - 1.5:.3f}:d=1.5")
        if silent:
            filt.append(f"{bg},alimiter=limit=0.95[aout]")
        else:
            filt.append(f"{bg}[bg0];[bg0][key]sidechaincompress=threshold=0.03:ratio=6:attack=15:release=200[bg];"
                        "[vx][bg]amix=inputs=2:normalize=0,alimiter=limit=0.95[aout]")
        n += 1
    elif silent:
        inputs += ["-f", "lavfi", "-t", f"{total:.3f}", "-i", "anullsrc=r=48000:cl=stereo"]
        filt.append(f"[{n}:a]anull[aout]")
        n += 1

    # Video: slides with short crossfades.
    for i in range(len(SLIDES)):
        inputs += ["-loop", "1", "-framerate", "30", "-t", f"{slide_d[i] + 0.4:.3f}", "-i", str(tmp / f"s{i}.png")]
    prev, off = f"[{n}:v]", 0.0
    for i in range(1, len(SLIDES)):
        off += slide_d[i - 1]
        filt.append(f"{prev}[{n + i}:v]xfade=transition=fade:duration=0.4:offset={off - 0.2:.3f}[x{i}]")
        prev = f"[x{i}]"
    filt.append(f"{prev}fade=t=in:d=0.3,fade=t=out:st={total - 0.5:.3f}:d=0.5,format=yuv420p[v1]")
    graph = ";".join(filt)

    out = OUT_DIRS[0]
    out.mkdir(parents=True, exist_ok=True)
    subprocess.check_call(["ffmpeg", "-y", "-loglevel", "error", *inputs, "-filter_complex", graph,
                           "-map", "[v1]", "-map", "[aout]", "-t", f"{total:.3f}", "-r", "30",
                           "-c:v", "libx264", "-preset", "slow", "-crf", "24", "-tune", "stillimage",
                           "-c:a", "aac", "-b:a", "128k", "-movflags", "+faststart", str(out / f"{NAME}.mp4")])
    subprocess.check_call(["ffmpeg", "-y", "-loglevel", "error", *inputs, "-filter_complex", graph,
                           "-map", "[v1]", "-map", "[aout]", "-t", f"{total:.3f}", "-r", "30",
                           "-c:v", "libvpx-vp9", "-b:v", "0", "-crf", "38", "-row-mt", "1", "-deadline", "good",
                           "-c:a", "libopus", "-b:a", "96k", str(out / f"{NAME}.webm")])

    vtt = ["WEBVTT", ""]
    for i, ((caption, _), (_, d)) in enumerate(zip(LINES, voices)):
        vtt += [str(i + 1), f"{ts(starts[i])} --> {ts(starts[i] + d)}", caption, ""]
    (out / f"{NAME}.vtt").write_text("\n".join(vtt), encoding="utf-8")
    (out / f"{NAME}.txt").write_text("\n".join(caption for caption, _ in LINES) + "\n", encoding="utf-8")
    for other in OUT_DIRS[1:]:
        other.mkdir(parents=True, exist_ok=True)
        for f in (f"{NAME}.mp4", f"{NAME}.webm", f"{NAME}.vtt", f"{NAME}.txt"):
            shutil.copy(out / f, other / f)
    for i in range(len(SLIDES)):
        shutil.copy(tmp / f"s{i}.png", HERE / "slides" / f"s{i}.png") if (HERE / "slides").exists() else None
    print(f"built {total:.1f}s video ({'silent' if silent else 'narrated'}, {'with' if music else 'no'} music), "
          f"speech {speech:.1f}s, gap {gap:.2f}s")


if __name__ == "__main__":
    main()
