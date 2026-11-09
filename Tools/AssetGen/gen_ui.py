"""
gen_ui.py - High-quality UI asset generator for Pulau Monster RPG.
Generates:
- UI_Panel_Navy.png (16x16, border 5)
- UI_Panel_Gold.png (16x16, border 5)
- UI_Panel_Narr.png (16x16, border 5)
- UI_Pixel.png (4x4 white)
- UI_Ring.png (64x64 radiant gold evolution ring with rune sparks)
- UI_Alert.png (8x16 crisp exclamation alert)
"""

import os
from PIL import Image
from palette import PALETTE

INK = PALETTE["INK"]
GOLD = PALETTE["GOLD"]
GOLD_DARK = PALETTE["GOLD_DARK"]
GOLD_LIGHT = PALETTE.get("PYRO_YELLOW", (255, 225, 82, 255))
PANEL_NAVY = PALETTE["PANEL_NAVY"]
NARR_NAVY = PALETTE["NARR_NAVY"]
NARR_BORDER = PALETTE["NARR_BORDER"]
WHITE = PALETTE["WHITE"]
HP_RED = PALETTE["HP_RED"]
TRANSPARENT = PALETTE["TRANSPARENT"]
MUTED = PALETTE["MUTED"]

def make_panel_navy():
    # 16x16 9-slice with 5px border.
    # Outer dark rim, rich gold filigree border with corner dots, inner deep navy body.
    im = Image.new("RGBA", (16, 16), (22, 18, 48, 255))
    p = im.load()
    
    # Outer ink outline
    for i in range(16):
        p[i, 0] = INK; p[i, 15] = INK
        p[0, i] = INK; p[15, i] = INK
        
    # Bevel border layer 1 (Gold trim)
    for i in range(1, 15):
        p[i, 1] = GOLD_LIGHT; p[i, 14] = GOLD_DARK
        p[1, i] = GOLD; p[14, i] = GOLD_DARK
        
    # Corner accent gems (Gold / Amber shine)
    p[1, 1] = WHITE; p[2, 2] = GOLD_LIGHT
    p[14, 1] = GOLD_LIGHT; p[13, 2] = GOLD
    p[1, 14] = GOLD_DARK; p[2, 13] = GOLD
    p[14, 14] = INK; p[13, 13] = GOLD_DARK
    
    # Layer 2 inner rim (Deep accent)
    for i in range(2, 14):
        p[i, 2] = (38, 28, 78, 255)
        p[i, 13] = (15, 12, 32, 255)
        p[2, i] = (38, 28, 78, 255)
        p[13, i] = (15, 12, 32, 255)
        
    # Center gradient / subtle texture
    for y in range(3, 13):
        for x in range(3, 13):
            # Soft vertical gradient
            t = (y - 3) / 10.0
            r = int(24 * (1 - t) + 18 * t)
            g = int(20 * (1 - t) + 15 * t)
            b = int(52 * (1 - t) + 40 * t)
            p[x, y] = (r, g, b, 250)
            
    return im

def make_panel_gold():
    # 16x16 9-slice with 5px border.
    # Radiant gold champion frame with deep amber inlay.
    im = Image.new("RGBA", (16, 16), (225, 175, 20, 255))
    p = im.load()
    
    for i in range(16):
        p[i, 0] = INK; p[i, 15] = INK
        p[0, i] = INK; p[15, i] = INK
        
    for i in range(1, 15):
        p[i, 1] = WHITE; p[i, 14] = (145, 95, 8, 255)
        p[1, i] = GOLD_LIGHT; p[14, i] = GOLD_DARK
        
    # Corner rivets
    p[2, 2] = WHITE; p[13, 2] = GOLD_LIGHT
    p[2, 13] = GOLD_DARK; p[13, 13] = INK
    
    for y in range(3, 13):
        for x in range(3, 13):
            p[x, y] = (245, 195, 30, 255)
            if (x + y) % 4 == 0:
                p[x, y] = (255, 215, 50, 255)
                
    return im

def make_panel_narr():
    # 16x16 9-slice with 5px border.
    # Elegant dark fantasy dialogue box with lavender/crystal trim.
    im = Image.new("RGBA", (16, 16), (18, 15, 38, 255))
    p = im.load()
    
    for i in range(16):
        p[i, 0] = INK; p[i, 15] = INK
        p[0, i] = INK; p[15, i] = INK
        
    for i in range(1, 15):
        p[i, 1] = (100, 120, 210, 255); p[i, 14] = (35, 45, 110, 255)
        p[1, i] = (75, 95, 180, 255); p[14, i] = (35, 45, 110, 255)
        
    p[1, 1] = (170, 190, 255, 255)
    p[14, 1] = (100, 120, 210, 255)
    p[1, 14] = (50, 65, 140, 255)
    p[14, 14] = INK
    
    for y in range(2, 14):
        for x in range(2, 14):
            t = (y - 2) / 12.0
            p[x, y] = (int(16 + 4*t), int(14 + 2*t), int(36 + 6*t), 245)
            
    return im

def make_pixel():
    im = Image.new("RGBA", (4, 4), WHITE)
    return im

def make_ring():
    # 64x64 radiant gold evolution ring with shimmering particles & 4 cardinal runes
    im = Image.new("RGBA", (64, 64), TRANSPARENT)
    p = im.load()
    cx, cy = 31.5, 31.5
    for y in range(64):
        for x in range(64):
            dist = ((x - cx)**2 + (y - cy)**2)**0.5
            if 26.5 <= dist <= 29.5:
                # Main ring
                p[x, y] = GOLD
                if dist <= 27.5: p[x, y] = GOLD_LIGHT
                elif dist >= 28.5: p[x, y] = GOLD_DARK
            elif 25.5 <= dist < 26.5 or 29.5 < dist <= 30.5:
                # Shimmer aura
                p[x, y] = (255, 214, 36, 120)
                
    # 4 Cardinal Rune Jewels (N, S, E, W)
    for ox, oy in [(0, -28), (0, 28), (-28, 0), (28, 0)]:
        rx = int(cx + ox)
        ry = int(cy + oy)
        if 0 <= rx < 64 and 0 <= ry < 64:
            p[rx, ry] = WHITE
            if 0 <= rx-1 < 64: p[rx-1, ry] = GOLD_LIGHT
            if 0 <= rx+1 < 64: p[rx+1, ry] = GOLD_LIGHT
            if 0 <= ry-1 < 64: p[rx, ry-1] = GOLD_LIGHT
            if 0 <= ry+1 < 64: p[rx, ry+1] = GOLD_LIGHT
            
    # Diagonal sparks
    for ox, oy in [(-20, -20), (20, -20), (-20, 20), (20, 20)]:
        rx = int(cx + ox)
        ry = int(cy + oy)
        if 0 <= rx < 64 and 0 <= ry < 64:
            p[rx, ry] = WHITE
            
    return im

def make_alert():
    # 8x16 crisp exclamation mark with glossy shine & 3D shadow
    im = Image.new("RGBA", (8, 16), TRANSPARENT)
    p = im.load()
    
    # Outer drop shadow
    for y in range(2, 11):
        p[2, y] = INK; p[5, y] = INK
    p[3, 1] = INK; p[4, 1] = INK
    p[3, 11] = INK; p[4, 11] = INK
    
    # Upper stick
    for y in range(2, 11):
        p[3, y] = (245, 55, 50, 255)
        p[4, y] = (205, 30, 30, 255)
        
    # Gloss highlight
    p[3, 2] = WHITE; p[4, 2] = (255, 180, 180, 255)
    p[3, 3] = WHITE
    
    # Dot
    p[3, 12] = INK; p[4, 12] = INK
    p[2, 13] = INK; p[5, 13] = INK
    p[2, 14] = INK; p[5, 14] = INK
    p[3, 15] = INK; p[4, 15] = INK
    p[3, 13] = WHITE; p[4, 13] = (245, 55, 50, 255)
    p[3, 14] = (245, 55, 50, 255); p[4, 14] = (205, 30, 30, 255)
    
    return im

def generate_all_ui(out_dir):
    os.makedirs(out_dir, exist_ok=True)
    ui_map = {
        "UI_Panel_Navy.png": make_panel_navy(),
        "UI_Panel_Gold.png": make_panel_gold(),
        "UI_Panel_Narr.png": make_panel_narr(),
        "UI_Pixel.png": make_pixel(),
        "UI_Ring.png": make_ring(),
        "UI_Alert.png": make_alert()
    }
    paths = {}
    for name, im in ui_map.items():
        p = os.path.join(out_dir, name)
        im.save(p)
        paths[name] = p
    return paths

if __name__ == "__main__":
    generate_all_ui("temp_ui")
    print("UI generated.")
