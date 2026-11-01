# Videos

Narrated guide videos for the founder and the team ([ADR general/0011](../adr/general/0011-founder-guide-videos.md)).
The pitch video on `/welcome` is separate and built from `docs/mocks/_build/video/`.

| # | Video | Runtime | Audience |
| --- | --- | --- | --- |
| 01 | [Next steps to selling on Sycapooke](01-next-steps-to-selling/README.md) | ~8.5 min | The founder |
| 02 | [Your four jobs: taking the shop live](02-co-founder-four-jobs/README.md) | ~8.5 min | The technical co-founder |

Each folder holds `script.md` (narration), `slides.html` (one `<section>` per slide, cued to a script phrase),
`README.md` (outline), `pronunciations.json`, `pictures/` (ringed screenshots of the mocks), the `.mp4` with a soft
caption track and the `.vtt` captions. The `.mp3` is a git-ignored build output. Shared slide styles and runtime are in
`assets/`; the builder is `_build/build_video.py` (Kokoro voice `af_heart`, model cached in `.cache/kokoro/`). A folder's optional
`music.json` names a background track in `assets/music/` (sources in its `ATTRIBUTION.md`); it is looped, faded and
ducked under the voice.

```bash
python -I docs/videos/_build/build_video.py docs/videos/NN-topic --check        # cues and estimated timing
python -I docs/videos/_build/build_video.py docs/videos/NN-topic --pictures     # refresh pictures/ from the mocks
python -I docs/videos/_build/build_video.py docs/videos/NN-topic                # mp3, slides, mp4 and vtt
```

Founder videos are picture-first: a few large words per slide, real shop screens with a ring round the thing to
press, a slow voice, and the warm voice of [ADR general/0005](../adr/general/0005-marketing-wording.md).
