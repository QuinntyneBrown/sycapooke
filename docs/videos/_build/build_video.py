"""Build a narrated founder video from docs/videos/NN-topic/{script.md, slides.html}.

Steps   1. pictures  screenshot the HTML mocks named in PICTURES (docs/mocks/) into NN-topic/pictures/,
                     hiding the mock toolbar and notes and drawing a mustard ring round the thing to press
        2. audio     Kokoro-82M (voice af_heart) speaks each script.md paragraph; clips and silences are
                     joined into NN-topic.mp3 and their times recorded
        3. cues      every slide's data-cue is found verbatim (and once) in the script; a slide starts when
                     the narration reaches its cue (paragraph starts exact, inside a paragraph by characters)
        4. slides    slides.html?slide=N is screenshot at 1920x1080 in headless Chromium
        5. video     ffmpeg joins the slides and the narration into NN-topic.mp4 (H.264/AAC) with a soft caption
                     track (off until the viewer turns it on); NN-topic.vtt holds the same captions
Needs   ffmpeg/ffprobe; Python packages playwright, kokoro-onnx, onnxruntime, huggingface_hub, numpy.
Run     python -I docs/videos/_build/build_video.py docs/videos/01-next-steps-to-selling [--check | --pictures | --slides-only | --dry-run]
        --dry-run      parse the script, print word count and estimated length (no audio, no browser)
        --check        dry-run plus cue resolution against estimated timings
        --pictures     only refresh pictures/
        --slides-only  pictures, audio and slide PNGs (in .cache/videos/<folder>/), no encode
"""
import json, pathlib, re, shutil, subprocess, sys, wave

REPO = pathlib.Path(__file__).resolve().parents[3]
MOCKS = REPO / "docs/mocks"
HF_REPO, VOICE, SPEED, LANG = "onnx-community/Kokoro-82M-v1.0-ONNX", "af_heart", 0.85, "en-us"
LEAD, GAP, SECTION_GAP, TAIL = 1.5, 0.8, 1.4, 3.0  # seconds of silence
EST_WPM = 135  # words per minute used by --dry-run and --check
CHROMIUM = "/opt/pw-browsers/chromium"  # used when present; otherwise Playwright's own browser

# Pictures of the real shop, from the mocks: name -> (mock page, element to scroll into view, elements to ring).
PICTURES = {
    "01-next-steps-to-selling": {
        "welcome": ("pages/catalog/guest", None, []),
        "join": ("pages/register/default", "button[type=submit]", ["button[type=submit]"]),
        "sell-button": ("pages/my-listings/default", None, [".topbar__sell"]),
        "add-photos": ("pages/sell/filled", ".uploader__add", [".uploader__add"]),
        "category": ("pages/sell/filled", "#cat", [".field:has(#cat)", ".field:has(#sub)"]),
        "details": ("pages/sell/filled", "#title", [".field:has(#title)", ".field:has(#brand)"]),
        "publish": ("pages/sell/filled", "button:has-text('Publish') >> visible=true", ["button:has-text('Publish') >> visible=true"]),
        "inventory": ("pages/my-listings/default", None, [".topbar__link:has-text('Inventory')"]),
        "sales": ("pages/sales/default", None, []),
        "ship-by": ("pages/sale-detail/default", None, [":text('Ship by')"]),
        "tracking": ("dialogs/add-tracking/default", None, []),  # a dialog sits in the top layer, above any ring
        "support": ("pages/support/default", None, []),
    },
}
SHOT_W, SHOT_H = 1440, 720
RING_CSS = ("position:fixed;z-index:99999;pointer-events:none;border:8px solid #d9a441;border-radius:22px;"
            "box-shadow:0 0 0 10px rgb(217 164 65 / .35)")


def dur(p):
    return float(subprocess.check_output(["ffprobe", "-v", "error", "-show_entries", "format=duration",
                                          "-of", "csv=p=0", str(p)]).strip())


def ts(t, sep="."):
    return f"{int(t // 3600):02}:{int(t % 3600 // 60):02}:{t % 60:06.3f}".replace(".", sep)


def parse_script(path):
    """Paragraphs as (section index, text). Fails on anything the narrator can't speak."""
    lines = path.read_text(encoding="utf-8").splitlines()
    if not lines or not re.match(r"^# \d\d · ", lines[0]):
        raise SystemExit(f"{path}: first line must be '# NN · Title'")
    paras, section, buf = [], 0, []

    def flush():
        if buf:
            paras.append((section, " ".join(buf)))
            buf.clear()

    for n, line in enumerate(lines[1:], 2):
        s = line.strip()
        if s.startswith("## "):
            flush(); section += 1
        elif not s:
            flush()
        elif s.startswith(("#", "|", "```", "<")) or re.search(r"\[[^\]]*\]\(", s):
            raise SystemExit(f"{path}:{n}: tables, code blocks, links and HTML are not allowed")
        else:
            buf.append(s.lstrip("- "))
    flush()
    return paras


def lexicon(folder):
    f = folder / "pronunciations.json"
    return json.loads(f.read_text(encoding="utf-8")) if f.exists() else {}


def spoken(text, lex):
    for k, v in lex.items():
        text = re.sub(rf"\b{re.escape(k)}\b", v, text)
    return text


def read_cues(slides_html):
    return [(re.search(r'data-cue="([^"]*)"', tag) or [None, ""])[1] for tag in re.findall(r"<section\b[^>]*>", slides_html)]


def resolve(cues, paras, starts, durs):
    """Start time of each slide; the first slide starts at 0."""
    texts = [t for _, t in paras]
    whole = "\n".join(texts)
    times, errors = [0.0], []
    for i, cue in enumerate(cues[1:], 1):
        cue = cue.replace("&amp;", "&").replace("&#39;", "'")
        if not cue:
            errors.append(f"slide {i}: no data-cue"); times.append(times[-1]); continue
        if whole.count(cue) != 1:
            errors.append(f"slide {i}: cue {cue!r} found {whole.count(cue)} times"); times.append(times[-1]); continue
        p = next(j for j, t in enumerate(texts) if cue in t)
        times.append(starts[p] + durs[p] * texts[p].index(cue) / len(texts[p]))
    for i in range(1, len(times)):
        if times[i] < times[i - 1]:
            errors.append(f"slide {i}: cue is before the previous slide's cue")
    return times, errors


def timeline(paras, durs):
    starts, t = [], LEAD
    for i, ((sec, _), d) in enumerate(zip(paras, durs)):
        if i:
            t += SECTION_GAP if sec != paras[i - 1][0] else GAP
        starts.append(t)
        t += d
    return starts, t + TAIL


def captions(paras, starts, durs, limit=90):
    """Sentence-sized captions, timed by characters inside each paragraph."""
    out = []
    for (_, text), s, d in zip(paras, starts, durs):
        sentences = re.findall(r"[^.!?]+[.!?]+[\"”]?|[^.!?]+$", text)
        chunks = []
        for sen in (x.strip() for x in sentences if x.strip()):
            if chunks and len(chunks[-1]) + len(sen) < limit // 2:
                chunks[-1] += " " + sen
            else:
                chunks.append(sen)
        total, pos = sum(len(c) for c in chunks), 0
        for c in chunks:
            a = s + d * pos / total
            pos += len(c)
            out.append((a, s + d * pos / total, c))
    return out


class Narrator:
    """Kokoro-82M via onnxruntime; model and voice download from Hugging Face into .cache/kokoro/."""

    def __init__(self):
        import numpy as np
        import onnxruntime as ort
        from huggingface_hub import hf_hub_download
        from kokoro_onnx.tokenizer import Tokenizer

        self.np = np
        k = REPO / ".cache/kokoro"
        k.mkdir(parents=True, exist_ok=True)
        for sub, name in (("onnx", "model.onnx"), ("voices", f"{VOICE}.bin")):
            if not (k / name).exists():
                shutil.copy(hf_hub_download(HF_REPO, f"{sub}/{name}"), k / name)
        self.tok = Tokenizer()
        self.sess = ort.InferenceSession(str(k / "model.onnx"))
        self.style = np.fromfile(k / f"{VOICE}.bin", dtype=np.float32).reshape(-1, 1, 256)

    def _chunk(self, text):
        np = self.np
        ids = self.tok.tokenize(self.tok.phonemize(text, lang=LANG))
        return self.sess.run(None, {"input_ids": np.array([[0, *ids, 0]], dtype=np.int64),
                                    "style": self.style[len(ids)],
                                    "speed": np.array([SPEED], dtype=np.float32)})[0].reshape(-1)

    def say(self, text, out):
        """Kokoro takes up to 510 phoneme tokens, so long paragraphs are spoken sentence by sentence."""
        np = self.np
        parts, cur = [], ""
        for sen in re.findall(r"[^.!?]+[.!?]+|[^.!?]+$", text):
            if cur and len(cur) + len(sen) > 220:
                parts.append(cur); cur = ""
            cur += sen
        parts.append(cur)
        pause = np.zeros(int(24000 * 0.35), dtype=np.float32)
        audio = []
        for p in (x.strip() for x in parts if x.strip()):
            a = self._chunk(p)
            audio += [np.trim_zeros(np.where(np.abs(a) < 0.004, 0, a)), pause]
        audio = np.concatenate(audio[:-1])
        with wave.open(str(out), "wb") as w:
            w.setnchannels(1); w.setsampwidth(2); w.setframerate(24000)
            w.writeframes((np.clip(audio, -1, 1) * 32767).astype(np.int16).tobytes())


def browser(pw):
    return pw.chromium.launch(executable_path=CHROMIUM) if pathlib.Path(CHROMIUM).exists() else pw.chromium.launch()


def make_pictures(folder):
    from playwright.sync_api import sync_playwright

    spec = PICTURES.get(folder.name, {})
    out = folder / "pictures"
    out.mkdir(exist_ok=True)
    with sync_playwright() as pw:
        b = browser(pw)
        pg = b.new_page(viewport={"width": SHOT_W, "height": SHOT_H}, device_scale_factor=1)
        for name, (mock, scroll, rings) in spec.items():
            pg.goto((MOCKS / f"{mock}.html").as_uri())
            pg.add_style_tag(content=".mock-bar,.mock-note{display:none!important} html{scroll-behavior:auto!important}")
            pg.evaluate("document.fonts.ready")
            if scroll:
                pg.locator(scroll).first.evaluate("e => e.scrollIntoView({block: 'center', behavior: 'instant'})")
            pg.wait_for_timeout(200)
            boxes = [pg.locator(r).first.bounding_box() for r in rings]
            if None in boxes:
                raise SystemExit(f"{name}: a ring target in {rings} is not visible in {mock}")
            if boxes:
                x0 = min(bx["x"] for bx in boxes) - 14; y0 = min(bx["y"] for bx in boxes) - 14
                x1 = max(bx["x"] + bx["width"] for bx in boxes) + 14; y1 = max(bx["y"] + bx["height"] for bx in boxes) + 14
                pg.evaluate("""([css, x, y, w, h]) => { const d = document.createElement('div'); d.style.cssText = css;
                    Object.assign(d.style, {left: x + 'px', top: y + 'px', width: w + 'px', height: h + 'px'});
                    document.body.append(d); }""", [RING_CSS, x0, y0, x1 - x0, y1 - y0])
            pg.wait_for_timeout(300)
            pg.screenshot(path=str(out / f"{name}.png"))
            print(f"picture {name}.png")
        b.close()


def main():
    args = [a for a in sys.argv[1:] if not a.startswith("--")]
    flags = {a for a in sys.argv[1:] if a.startswith("--")}
    if not args:
        raise SystemExit(__doc__)
    folder = (REPO / args[0]).resolve() if not pathlib.Path(args[0]).is_absolute() else pathlib.Path(args[0])
    name = folder.name
    paras = parse_script(folder / "script.md")
    cues = read_cues((folder / "slides.html").read_text(encoding="utf-8"))
    words = sum(len(t.split()) for _, t in paras)

    if flags & {"--dry-run", "--check"}:
        est = [len(t.split()) / EST_WPM * 60 for _, t in paras]
        starts, total = timeline(paras, est)
        print(f"{len(paras)} paragraphs, {words} words, about {total / 60:.1f} min at {EST_WPM} wpm, {len(cues)} slides")
        if "--check" in flags:
            times, errors = resolve(cues, paras, starts, est)
            for i, t in enumerate(times):
                nxt = times[i + 1] if i + 1 < len(times) else total
                print(f"  slide {i:2}  {ts(t)}  {nxt - t:5.1f}s  {cues[i][:60]}")
            if errors:
                raise SystemExit("\n".join(errors))
        return

    if "--pictures" in flags:
        make_pictures(folder); return
    make_pictures(folder)

    cache = REPO / ".cache/videos" / name
    cache.mkdir(parents=True, exist_ok=True)
    lex = lexicon(folder)
    narrator, durs = None, []
    for i, (_, text) in enumerate(paras):
        say = spoken(text, lex)
        wav, key = cache / f"p{i:03}.wav", cache / f"p{i:03}.txt"
        if not (wav.exists() and key.exists() and key.read_text(encoding="utf-8") == f"{VOICE} {SPEED}\n{say}"):
            narrator = narrator or Narrator()
            narrator.say(say, wav)
            key.write_text(f"{VOICE} {SPEED}\n{say}", encoding="utf-8")
        durs.append(dur(wav))
    starts, total = timeline(paras, durs)
    times, errors = resolve(cues, paras, starts, durs)
    if errors:
        raise SystemExit("\n".join(errors))

    # Narration: every clip placed at its start time on a silent bed.
    inputs, filt = [], []
    for i, s in enumerate(starts):
        inputs += ["-i", str(cache / f"p{i:03}.wav")]
        ms = int(s * 1000)
        filt.append(f"[{i}:a]aresample=48000,adelay={ms}|{ms}[a{i}]")
    filt.append("".join(f"[a{i}]" for i in range(len(starts))) +
                f"amix=inputs={len(starts)}:normalize=0,apad,atrim=0:{total:.3f},volume=1.6,alimiter=limit=0.95[aout]")
    mp3 = folder / f"{name}.mp3"
    subprocess.check_call(["ffmpeg", "-y", "-loglevel", "error", *inputs, "-filter_complex", ";".join(filt),
                           "-map", "[aout]", "-c:a", "libmp3lame", "-b:a", "128k", str(mp3)])

    from playwright.sync_api import sync_playwright
    with sync_playwright() as pw:
        b = browser(pw)
        pg = b.new_page(viewport={"width": 1920, "height": 1080})
        for i in range(len(cues)):
            pg.goto((folder / "slides.html").as_uri() + f"?slide={i}")
            pg.evaluate("document.fonts.ready")
            pg.wait_for_timeout(400)
            pg.locator("section.active").screenshot(path=str(cache / f"s{i:02}.png"))
        b.close()
    print(f"{len(cues)} slides rendered to {cache.relative_to(REPO)}")

    caps = captions(paras, starts, durs)
    vtt = ["WEBVTT", ""] + [f"{ts(a)} --> {ts(z)}\n{c}\n" for a, z, c in caps]
    (folder / f"{name}.vtt").write_text("\n".join(vtt), encoding="utf-8")
    srt = cache / "captions.srt"
    srt.write_text("\n".join(f"{i + 1}\n{ts(a, ',')} --> {ts(z, ',')}\n{c}\n" for i, (a, z, c) in enumerate(caps)), encoding="utf-8")
    if "--slides-only" in flags:
        return

    concat = cache / "slides.txt"
    lines = []
    for i, t in enumerate(times):
        nxt = times[i + 1] if i + 1 < len(times) else total
        lines += [f"file '{cache / f's{i:02}.png'}'", f"duration {nxt - t:.3f}"]
    lines.append(f"file '{cache / f's{len(times) - 1:02}.png'}'")
    concat.write_text("\n".join(lines) + "\n", encoding="utf-8")
    mp4 = folder / f"{name}.mp4"
    subprocess.check_call(["ffmpeg", "-y", "-loglevel", "error", "-f", "concat", "-safe", "0", "-i", str(concat),
                           "-i", str(mp3), "-i", str(srt), "-map", "0:v", "-map", "1:a", "-map", "2:s",
                           "-vf", "fps=30,format=yuv420p", "-c:v", "libx264", "-preset", "slow", "-crf", "26",
                           "-tune", "stillimage", "-c:a", "aac", "-b:a", "128k", "-c:s", "mov_text",
                           "-metadata:s:s:0", "language=eng", "-t", f"{total:.3f}", "-movflags", "+faststart", str(mp4)])
    print(f"built {mp4.relative_to(REPO)}: {total / 60:.1f} min, {len(cues)} slides, {words} words")


if __name__ == "__main__":
    main()
