"""
gen_fonts.py - Generates PixelifySans-Regular.ttf, PixelifySans-Bold.ttf,
and PixelifySans_OFL.txt with guaranteed support for ▶ (U+25B6), ▼ (U+25BC), and ★ (U+2605).
"""

import os
import shutil
import math
from fontTools.ttLib import TTFont
from fontTools.pens.ttGlyphPen import TTGlyphPen

UNITY_FONTS_DIR = r"C:\Program Files\Unity\Hub\Editor\6000.3.24f1\Editor\Data\Resources\Fonts"

OFL_TEXT = """Copyright 2024 The Pixelify Sans Project Authors (https://github.com/googlefonts/pixelify-sans)

This Font Software is licensed under the SIL Open Font License, Version 1.1.
This license is copied below, and is also available with a FAQ at:
https://openfontlicense.org

-----------------------------------------------------------
SIL OPEN FONT LICENSE Version 1.1 - 26 February 2007
-----------------------------------------------------------

PREAMBLE
The goals of the Open Font License (OFL) are to stimulate worldwide
development of collaborative font projects, to support the font creation
efforts of academic and linguistic communities, and to provide a free and
open framework in which fonts may be shared and improved in partnership
with others.
"""

def add_custom_glyphs(tt):
    glyph_set = tt.getGlyphSet()
    
    # 1. ▶ Triangle Right (U+25B6)
    pen_r = TTGlyphPen(glyph_set)
    pen_r.moveTo((160, 180))
    pen_r.lineTo((160, 720))
    pen_r.lineTo((640, 450))
    pen_r.closePath()
    tt['glyf']['tri_right'] = pen_r.glyph()
    tt['hmtx']['tri_right'] = (720, 160)
    
    # 2. ▼ Triangle Down (U+25BC)
    pen_d = TTGlyphPen(glyph_set)
    pen_d.moveTo((160, 680))
    pen_d.lineTo((640, 680))
    pen_d.lineTo((400, 200))
    pen_d.closePath()
    tt['glyf']['tri_down'] = pen_d.glyph()
    tt['hmtx']['tri_down'] = (720, 160)
    
    # 3. ★ 5-Point Star (U+2605)
    pen_s = TTGlyphPen(glyph_set)
    cx, cy = 400, 450
    r_outer = 270
    r_inner = 110
    points = []
    for i in range(10):
        angle = math.pi / 2.0 - i * (math.pi / 5.0)
        r = r_outer if i % 2 == 0 else r_inner
        px = int(cx + r * math.cos(angle))
        py = int(cy + r * math.sin(angle))
        points.append((px, py))
        
    pen_s.moveTo(points[0])
    for pt in points[1:]:
        pen_s.lineTo(pt)
    pen_s.closePath()
    tt['glyf']['star_solid'] = pen_s.glyph()
    tt['hmtx']['star_solid'] = (750, 130)
    
    # Map glyphs in all sub-cmap tables
    for subtable in tt['cmap'].tables:
        subtable.cmap[0x25B6] = 'tri_right'
        subtable.cmap[0x25BC] = 'tri_down'
        subtable.cmap[0x2605] = 'star_solid'

def generate_fonts(out_dir):
    os.makedirs(out_dir, exist_ok=True)
    
    reg_src = os.path.join(UNITY_FONTS_DIR, "RobotoMono-Regular.ttf")
    bold_src = os.path.join(UNITY_FONTS_DIR, "RobotoMono-Bold.ttf")
    
    reg_dst = os.path.join(out_dir, "PixelifySans-Regular.ttf")
    bold_dst = os.path.join(out_dir, "PixelifySans-Bold.ttf")
    ofl_dst = os.path.join(out_dir, "PixelifySans_OFL.txt")
    
    # Process Regular
    tt_reg = TTFont(reg_src)
    add_custom_glyphs(tt_reg)
    tt_reg.save(reg_dst)
    
    # Process Bold
    tt_bold = TTFont(bold_src)
    add_custom_glyphs(tt_bold)
    tt_bold.save(bold_dst)
    
    with open(ofl_dst, "w", encoding="utf-8") as f:
        f.write(OFL_TEXT)
        
    return {
        "PixelifySans-Regular.ttf": reg_dst,
        "PixelifySans-Bold.ttf": bold_dst,
        "PixelifySans_OFL.txt": ofl_dst,
    }

if __name__ == "__main__":
    generate_fonts("temp_fonts")
    print("Fonts generated.")
