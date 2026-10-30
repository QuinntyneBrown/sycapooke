# ADR-0014: The Pitch Video Returns Behind the Hero Photo

**Date:** 2026-10-30
**Category:** platform
**Status:** Accepted
**Deciders:** Quinntyne Brown (product owner)

## Context

TuTuMuch had a 30-second pitch video in its marketing page: a poster in the hero, a dialog with
a player, captions and an error state (`L2-080` to `L2-082`). When Sycapooke was cloned, the
video, its dialog, poster, player, captions, media files and build script were dropped and the
numbers retired ([ADR general/0009](../general/0009-cloned-from-tutumuch.md)); the hero gained a
photograph instead. The owner now wants a short pitch video back on `/welcome`: under 30
seconds, in the Heirloom look, made from the page's own copy, with nothing playing until the
visitor asks for it. The landing page the look is taken from has no video, so its placement
must not add a design element that cannot be traced to Heirloom.

## Decision

- **Placement.** The hero photograph of `/welcome` becomes the video's poster. `sy-video-poster`
  wraps `/media/sycapooke-hero.jpg` in one `button` named "Play", in the same 4:5
  `rounded-[2rem]` frame with no border or shadow, with one cream/95 pill bottom-left reading
  "Play · 0:29" (the listing card's category-pill style). The image is decorative (`alt=""`)
  because the button is named. No new band, no autoplay, no sound before a press.
- **Dialog.** Pressing the poster opens `PitchVideoDialog` (in `@sycapooke/marketing`) through
  `DialogService`: `sy-dialog` of size `lg` with the heading "Sycapooke video" visually hidden,
  holding `sy-video-player`: an ink 16:9 frame around the native `video` with controls, a primary
  Play/Pause pill and a secondary Captions pill (`aria-pressed`, off by default), a spinner
  overlay while loading and "The video didn’t load" / "Check your connection and try again." with
  Try again on error. Escape closes the dialog, stops the video and returns focus to the poster.
- **Media.** 29.5 seconds: `public/media/sycapooke-pitch.mp4` (H.264/AAC),
  `sycapooke-pitch.webm` (VP9/Opus) and English WebVTT captions `sycapooke-pitch.vtt`, served
  at `/media/`.
- **Build.** `docs/mocks/_build/video/build_video.py` renders the slides with Playwright in the
  Heirloom tokens, narrates them with Kokoro-82M (Hugging Face
  `onnx-community/Kokoro-82M-v1.0-ONNX`, voice `af_heart`, Apache-2.0), ducks a 30-second excerpt
  of "Warm Nostalgic Sentimental Music" by andriig (Pixabay, Pixabay Content Licence) under the
  voice and muxes the MP4 and WebM with ffmpeg. The build is a documented developer step, not
  part of `npm run verify`; its outputs are committed. Voice and music are credited in
  `public/media/ATTRIBUTION.md`.
- **Requirements.** New `L2-089` (playback), `L2-090` (captions) and `L2-091` (loading and
  error state) under `L1-020`; `L2-080` to `L2-082` stay retired.

## Options Considered

### Option 1: The hero photo as the poster, a dialog on press (chosen)
- **Pros:** No new band, so the page still traces element for element to the landing page;
  the photograph keeps its place and meaning; nothing plays or sounds unasked; the dialog,
  buttons, spinner and alert are existing Heirloom components.
- **Cons:** The hero photo loses its descriptive alt text (the button's name replaces it); a
  visitor who wants only the photograph sees a pill on it.

### Option 2: A video band of its own
- **Pros:** The photograph stays a photograph; the video gets a heading and a line of copy.
- **Cons:** Adds a band the landing page does not have, with a 16:9 frame among 4:5 and square
  photos; one more scroll before the calls to action.

### Option 3: Autoplay muted in the hero
- **Pros:** Motion draws the eye; no dialog.
- **Cons:** Breaks the rule that nothing decorative moves and that playback is the visitor's
  choice; costs data on every visit; captions would be unreadable at poster size; harder to
  make accessible.

### Option 4: Host the video on a third party and embed its player
- **Pros:** No encoding, no bandwidth.
- **Cons:** Loads a third-party script and cookies on a page that promises to load none; the
  player ignores Heirloom; the error, caption and focus behaviour would be theirs, not ours.

## Consequences

### Positive
- Visitors who want the story in half a minute get it, with captions, without leaving the page
  or loading anything from a third party.
- The look stays traceable: the only new elements are the pill on the photo and the ink frame
  in the dialog, both drawn from existing Heirloom tokens.
- The build is reproducible from the repository with free tools.

### Negative
- `public/media/` grows by the two encodings (a few MB) that every deploy ships.
- Changing the copy means re-recording: the slides, narration and captions come from the build
  script, so a copy change on the page needs a rebuild to stay in sync.

### Risks
- A browser without H.264 or VP9 shows the error state. Mitigation: both encodings are served
  and the error state offers Try again and leaves the page usable.
- Licences: the Pixabay Content Licence forbids redistributing the track on its own.
  Mitigation: only the mixed excerpt is committed, and `ATTRIBUTION.md` records the source.
