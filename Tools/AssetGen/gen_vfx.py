"""
gen_vfx.py - Dynamic high-impact combat VFX generator for Pulau Monster RPG.
Generates:
- VFX_Slash.png (128x32: 4 frames 32x32 horizontal).
Features:
- Curved crescent energy slash blade with radiant core.
- Trailing motion blur & elemental spark burst.
"""

import os
import math
from PIL import Image
from palette import PALETTE

WHITE = PALETTE["WHITE"]
YELLOW = PALETTE["VOLT_LIGHT"]
ORANGE = PALETTE["PYRO_ORANGE"]
CYAN = PALETTE.get("CRYSTAL_BLUE", (92, 225, 230, 255))
TRANSPARENT = PALETTE["TRANSPARENT"]

def draw_slash_frame(frame):
    im = Image.new("RGBA", (32, 32), TRANSPARENT)
    p = im.load()
    
    # Elegant curved crescent slice from (27, 4) down-left to (5, 27)
    if frame == 0:
        # Frame 0: Quick flash of the initial strike tip
        for t in range(12):
            val = t / 11.0
            x = int(27 - 12 * val)
            y = int(4 + 10 * val)
            p[x, y] = WHITE
            if x + 1 < 32: p[x + 1, y] = CYAN
            if y - 1 >= 0: p[x, y - 1] = YELLOW
            
    elif frame == 1:
        # Frame 1: Full impactful crescent energy blade with multi-shade glow
        for t in range(40):
            val = t / 39.0
            # Curved trajectory: slight arc curvature
            x = 28 - 24 * val + 3 * math.sin(val * math.pi)
            y = 3 + 25 * val
            ix, iy = int(x), int(y)
            
            if 1 <= ix < 31 and 1 <= iy < 31:
                # White-hot razor edge
                p[ix, iy] = WHITE
                p[ix - 1, iy] = WHITE
                # Cyan energetic outer sheen
                p[ix + 1, iy] = CYAN
                p[ix, iy + 1] = CYAN
                # Radiant golden trailing wake
                p[ix - 2, iy] = YELLOW
                p[ix - 1, iy - 1] = ORANGE
                
        # Sharp impact sparks at center
        for sx, sy in [(16, 15), (17, 16), (15, 17), (18, 14)]:
            p[sx, sy] = WHITE

    elif frame == 2:
        # Frame 2: Slash expanding outwards into dissipating light ribbons & spark burst
        for t in range(25):
            val = t / 24.0
            x = 27 - 23 * val + 4 * math.sin(val * math.pi)
            y = 4 + 24 * val
            ix, iy = int(x), int(y)
            if (t % 2 == 0) and 0 <= ix < 32 and 0 <= iy < 32:
                p[ix, iy] = WHITE
                if ix - 1 >= 0: p[ix - 1, iy] = YELLOW
                
        # Explosive impact sparks
        sparks = [
            (8, 24), (11, 20), (14, 25), (17, 13), (20, 17), (23, 9), (26, 12),
            (12, 16), (19, 14), (15, 19), (22, 10), (9, 21)
        ]
        for sx, sy in sparks:
            if 0 <= sx < 32 and 0 <= sy < 32:
                p[sx, sy] = WHITE
                if sx + 1 < 32: p[sx + 1, sy] = CYAN
                if sy + 1 < 32: p[sx, sy + 1] = YELLOW

    elif frame == 3:
        # Frame 3: Lingering starry motes fading into air
        lingering = [
            (6, 26), (10, 22), (13, 27), (16, 12), (21, 16), (25, 8), (28, 11),
            (15, 17), (18, 15)
        ]
        for sx, sy in lingering:
            if 0 <= sx < 32 and 0 <= sy < 32:
                p[sx, sy] = YELLOW
                if (sx + sy) % 2 == 0:
                    p[sx, sy] = CYAN

    return im

def generate_all_vfx(out_dir):
    os.makedirs(out_dir, exist_ok=True)
    sheet = Image.new("RGBA", (128, 32), TRANSPARENT)
    for f in range(4):
        frame_img = draw_slash_frame(f)
        sheet.paste(frame_img, (f * 32, 0))
    path = os.path.join(out_dir, "VFX_Slash.png")
    sheet.save(path)
    return { "VFX_Slash.png": path }

if __name__ == "__main__":
    generate_all_vfx("temp_vfx")
    print("VFX generated.")
