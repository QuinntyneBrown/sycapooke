# Sycapooke pitch video: media and attribution

| File | What | Source | Licence |
| --- | --- | --- | --- |
| `sycapooke-pitch.mp4`, `sycapooke-pitch.webm` | The 29-second pitch video (H.264/AAC and VP9/Opus) | Built by `docs/mocks/_build/video/build_video.py` | © Sycapooke |
| `sycapooke-pitch.vtt`, `sycapooke-pitch.txt` | English captions and the transcript | Same script | © Sycapooke |
| Photos in the slides | `sycapooke-hero.jpg`, `sycapooke-founder.jpg` and the four category photos from `src/backend/Sycapooke.Cli/SeedPhotos/` | AI-generated for this project from the Sycapooke landing-page design (Magic Patterns, Oct 2026) | No third-party rights; see `public/media/ATTRIBUTION.md` and `SeedPhotos/ATTRIBUTION.md` |
| Music | _Warm Nostalgic Sentimental Music_ by andriig (2:12), a 32-second excerpt, alone at the start and end and ducked under the voice | [pixabay.com/music/acoustic-group-warm-nostalgic-sentimental-music-471262](https://pixabay.com/music/acoustic-group-warm-nostalgic-sentimental-music-471262/) | [Pixabay Content Licence](https://pixabay.com/service/license-summary/): free for commercial use, no attribution required |
| Narration | Kokoro-82M text-to-speech, voice `af_heart`, speed 1.05 | [huggingface.co/onnx-community/Kokoro-82M-v1.0-ONNX](https://huggingface.co/onnx-community/Kokoro-82M-v1.0-ONNX) (weights from [hexgrad/Kokoro-82M](https://huggingface.co/hexgrad/Kokoro-82M)) | Apache-2.0 |
| Typefaces | Fraunces and Source Sans 3, from `public/fonts/` | Google Fonts | SIL Open Font License 1.1 |

Visuals use only the Heirloom logo, tokens and the committed photos.

The music excerpt is committed as `docs/mocks/_build/video/music/warm-nostalgic-sentimental-excerpt.mp3`
(32 s from 0.3 s; the build trims and fades it). Rebuild with `python -I docs/mocks/_build/video/build_video.py`.
