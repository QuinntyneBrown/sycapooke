"""Build the Heirloom logo SVGs with Fraunces glyphs converted to paths (no font dependency)."""
import os, sys
from fontTools.ttLib import TTFont
from fontTools.varLib.instancer import instantiateVariableFont
from fontTools.pens.svgPathPen import SVGPathPen
from fontTools.pens.transformPen import TransformPen

ROOT = sys.argv[1]
FONT = sys.argv[2]
OUT_DS = os.path.join(ROOT, 'docs', 'design-system', 'assets')
os.makedirs(OUT_DS, exist_ok=True)

font = instantiateVariableFont(TTFont(FONT), {'opsz': 24, 'wght': 600, 'SOFT': 0, 'WONK': 1})
cmap = font.getBestCmap()
glyphs = font.getGlyphSet()
upm = font['head'].unitsPerEm
hmtx = font['hmtx']

# simple pair kerning from GPOS (format 1 pair pos) if present
kern = {}
try:
    gpos = font['GPOS'].table
    for lookup in gpos.LookupList.Lookup:
        for st in lookup.SubTable:
            if st.LookupType == 9:
                st = st.ExtSubTable
            if st.LookupType != 2:
                continue
            if st.Format == 1:
                for i, g1 in enumerate(st.Coverage.glyphs):
                    ps = st.PairSet[i]
                    for pvr in ps.PairValueRecord:
                        v = pvr.Value1.XAdvance if pvr.Value1 and hasattr(pvr.Value1, 'XAdvance') else 0
                        if v:
                            kern[(g1, pvr.SecondGlyph)] = v
            elif st.Format == 2:
                cd1, cd2 = st.ClassDef1.classDefs, st.ClassDef2.classDefs
                for g1 in st.Coverage.glyphs:
                    c1 = cd1.get(g1, 0)
                    for g2 in cmap.values():
                        c2 = cd2.get(g2, 0)
                        rec = st.Class1Record[c1].Class2Record[c2]
                        v = rec.Value1.XAdvance if rec.Value1 and hasattr(rec.Value1, 'XAdvance') else 0
                        if v and (g1, g2) not in kern:
                            kern[(g1, g2)] = v
except Exception as e:  # noqa
    print('no kerning:', e)


def text_path(text, size, x=0.0, y=0.0, tracking_em=0.0):
    """Return (path d, advance) for text at font-size `size` (px), baseline at y."""
    scale = size / upm
    d = []
    pen_x = x
    prev = None
    for ch in text:
        g = cmap[ord(ch)]
        if prev is not None:
            pen_x += kern.get((prev, g), 0) * scale
        pen = SVGPathPen(glyphs)
        tp = TransformPen(pen, (scale, 0, 0, -scale, pen_x, y))
        glyphs[g].draw(tp)
        d.append(pen.getCommands())
        pen_x += hmtx[g][0] * scale + tracking_em * size
        prev = g
    return ' '.join(d), pen_x - x


def tag(x, y, s, string_color, face='#a04228', line='#fbf7ef', hole='#f6f0e4', letter='#fbf7ef'):
    """The 48-unit price tag scaled by s, top-left at (x, y)."""
    sd, _ = text_path('S', 21 * s)
    # text anchor middle at (30.5, 33.5): shift by half advance
    _, adv = text_path('S', 21 * s)
    return f'''<g transform="translate({x} {y}) scale({s})">
  <path d="M15.5 22.5 C10 16, 5 11, 9.5 5.5 C12.5 2, 17 4, 15 8" fill="none" stroke="{string_color}" stroke-width="1.6" stroke-linecap="round"/>
  <g transform="rotate(-12 26 26)">
    <path d="M19 10 H39 a4 4 0 0 1 4 4 V38 a4 4 0 0 1 -4 4 H19 L9 26 Z" fill="{face}"/>
    <path d="M20.2 13 H38.5 a1.5 1.5 0 0 1 1.5 1.5 V37.5 a1.5 1.5 0 0 1 -1.5 1.5 H20.2 L12.6 26 Z" fill="none" stroke="{line}" stroke-opacity="0.55" stroke-width="0.9" stroke-dasharray="1.6 1.6"/>
    <circle cx="17" cy="26" r="2.6" fill="{hole}"/>
    <path transform="translate({30.5 - adv / s / 2:.3f} 33.5) scale({1 / s})" d="{sd}" fill="{letter}"/>
  </g>
</g>'''


def logo_svg(text_color, tagline_color=None, bg=None, label='Sycapooke'):
    # 48-unit tag at scale 2 (96px) + wordmark at 84px, tagline optional
    size = 84
    wd, wadv = text_path('Sycapooke', size, tracking_em=-0.02)
    tag_s = 2.0
    tag_w = 48 * tag_s
    gap = 24
    width = tag_w + gap + wadv + 8
    height = 112
    baseline = 86
    parts = [f'<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 {width:.0f} {height}" width="{width:.0f}" height="{height}" role="img" aria-label="{label}">',
             f'  <title>{label}</title>']
    if bg:
        parts.append(f'  <rect width="100%" height="100%" fill="{bg}"/>')
    parts.append(tag(0, 8, tag_s, text_color))
    parts.append(f'  <path transform="translate({tag_w + gap:.1f} {baseline})" d="{wd}" fill="{text_color}"/>')
    parts.append('</svg>')
    return '\n'.join(parts)


def icon_svg(size=512):
    # paper rounded square, tag centred and enlarged
    s = size / 48 * 0.86
    off = (size - 48 * s) / 2
    return f'''<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 {size} {size}" width="{size}" height="{size}" role="img" aria-label="Sycapooke app icon">
  <title>Sycapooke</title>
  <rect width="{size}" height="{size}" rx="{size * 0.22:.0f}" fill="#f6f0e4"/>
{tag(off, off, s, '#2b2118')}
</svg>'''


open(os.path.join(OUT_DS, 'logo.svg'), 'w', encoding='utf-8', newline='\n').write(logo_svg('#2b2118'))
open(os.path.join(OUT_DS, 'logo-dark.svg'), 'w', encoding='utf-8', newline='\n').write(logo_svg('#fbf7ef'))
open(os.path.join(OUT_DS, 'app-icon.svg'), 'w', encoding='utf-8', newline='\n').write(icon_svg())
# html wrappers for rendering
wrap = os.path.join(ROOT, '..', '_render')
print('wrote', OUT_DS)
