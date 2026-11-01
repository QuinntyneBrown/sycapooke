# 0011. Founder guide videos in docs/videos

Date: 2026-11-01

## Status

Accepted

## Context

The founder is not technical and asked what the next steps are to take the shop live and start
earning. Written runbooks such as `docs/deployment.md` are for the technical partner. She needs a short,
picture-first explanation she can watch and pause. The mandatory folder structure in AGENTS.md has no place
for guide videos, and the only video folder (`docs/mocks/_build/video/`) builds the pitch video.

## Decision

- Guide videos live in `docs/videos/NN-topic/` with `script.md`, `slides.html`, `README.md`,
  `pronunciations.json`, `pictures/`, the `.mp4` (soft captions) and the `.vtt`. The `.mp3` is a build output and is
  git-ignored.
- `docs/videos/assets/` holds the shared slide stylesheet and runtime (Heirloom tokens by value).
- `docs/videos/_build/build_video.py` builds them: screenshots of `docs/mocks/` with a ring round the control to
  press, Kokoro `af_heart` narration per paragraph, slides timed by verbatim cues, ffmpeg encode.
- Founder videos are picture-first with minimal on-screen text; captions are a soft track rather than burned in.

## Consequences

- Screens shown are the mocks, not the live app; when a mock changes, run `--pictures` and rebuild.
- Each video MP4 (about 12 MB for 8 minutes) is committed like the pitch video.
- Facts about third parties (Stripe fees and screens) are dated in the script and must be re-checked before a rebuild.
