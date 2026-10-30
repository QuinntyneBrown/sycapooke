# Sycapooke pitch video build

`build_video.py` builds the 29-second pitch video that plays from the hero of the marketing page
(`/welcome`, and `/` of the marketing build). The narration lines, the slide markup and the
Heirloom slide styles are in the script.

```
python -I docs/mocks/_build/video/build_video.py            # narration and music
python -I docs/mocks/_build/video/build_video.py --silent   # no narration; music only if present
```

It writes `sycapooke-pitch.mp4`, `sycapooke-pitch.webm`, `sycapooke-pitch.vtt` (captions, off until
the viewer turns them on) and `sycapooke-pitch.txt` (transcript) to
`src/frontend/projects/sycapooke/public/media/` (served at `/media/`) and copies them to
`docs/mocks/assets/media/` for the mocks. The poster is the hero photo (`sycapooke-hero.jpg`), so the
hero looks exactly as designed whether or not the visitor presses Play. When a `slides/` folder
exists here (git-ignored) the six slide PNGs are kept in it for review.

Needs ffmpeg and ffprobe and the Python package `playwright` (with `python -m playwright install
chromium`); narration also needs `kokoro-onnx`, `onnxruntime`, `huggingface_hub` and `numpy`.

- **Voice**: Kokoro-82M from Hugging Face, voice `af_heart` (American English) at speed 1.05, phonemised
  as `en-us`. The model and voice download into `kokoro/` (git-ignored, about 330 MB) on the first run.
- **Music**: `music/warm-nostalgic-sentimental-excerpt.mp3` is a 30-second excerpt of _Warm Nostalgic
  Sentimental Music_ by andriig on Pixabay (Pixabay Content Licence), committed so the build is
  repeatable. It was cut from the track downloaded from its Pixabay page with
  `ffmpeg -ss 0.3 -t 32 -i <download>.mp3 -b:a 160k music/warm-nostalgic-sentimental-excerpt.mp3`
  (the build trims it to length and fades it out). The music plays alone for the first 2 s, a
  sidechain compressor ducks it under the voice, and it swells over the last 4 s once the narration
  ends. Without the file the build runs with narration only.
- **Length**: the gaps between lines stretch so the video is exactly 29.5 s. The script stops if the
  narration no longer fits.
- **Look**: the slides use the Heirloom tokens by value (paper, cream, ink, terracotta, forest; Fraunces
  600 headings with italic terracotta emphasis; Source Sans 3 body; 2rem photo corners, 1rem tiles,
  pills) and the committed photos. Flat colour only: no shapes, no gradients, crossfades between slides.
- **Wording**: everything is vintage, at least 20 years old, listed by a real person; the copy never
  promises new or reproduction goods and never mentions the video's length
  ([ADR general/0005](../../../adr/general/0005-marketing-wording.md)).

Sources and licences: [`ATTRIBUTION.md`](ATTRIBUTION.md).
