"""
gen_backgrounds.py - High-fidelity fantasy RPG background generator for Pulau Monster.
Generates 4 backgrounds:
- BG_Battle_Grassland.png (320x180)
- BG_Battle_Forest.png (320x180)
- BG_Battle_Cave.png (320x180)
- BG_Title_World.png (480x180)
"""

import os
import math
from PIL import Image
from palette import PALETTE

INK = PALETTE["INK"]
WHITE = PALETTE["WHITE"]
GRASS_D = PALETTE["GRASS_DARK"]
GRASS_M = PALETTE["GRASS_MID"]
GRASS_L = PALETTE["GRASS_LIGHT"]
WATER_D = PALETTE["WATER_DARK"]
WATER_M = PALETTE["WATER_MID"]
WATER_L = PALETTE["WATER_LIGHT"]
SAND_D = PALETTE["SAND_DARK"]
SAND_M = PALETTE["SAND_MID"]
SAND_L = PALETTE["SAND_LIGHT"]
WOOD_D = PALETTE["WOOD_DARK"]
WOOD_M = PALETTE["WOOD_MID"]
WOOD_L = PALETTE["WOOD_LIGHT"]
CAVE_FL = PALETTE["PANEL_NAVY"]
CAVE_D = PALETTE["CAVE_WALL_DARK"]
CAVE_M = PALETTE["CAVE_WALL_MID"]
CAVE_L = PALETTE["CAVE_WALL_LIGHT"]
CRYSTAL_Y = PALETTE["CRYSTAL_YELLOW"]
CRYSTAL_B = PALETTE["CRYSTAL_BLUE"]
GOLD = PALETTE["GOLD"]

def make_battle_forest():
    # 320x180: Lush enchanted forest with sunbeams, layered tree canopies, wildflower field, and battle pedestals
    im = Image.new("RGBA", (320, 180), (85, 145, 110, 255))
    p = im.load()
    
    # 1. Distant canopy & sunbeam gradient sky (y: 0..75)
    for y in range(75):
        t = y / 75.0
        # Morning forest mist: soft warm gold down to deep emerald
        r = int(140 * (1 - t) + 45 * t)
        g = int(195 * (1 - t) + 115 * t)
        b = int(145 * (1 - t) + 75 * t)
        for x in range(320):
            p[x, y] = (r, g, b, 255)
            
    # Soft sunbeams streaming from top-left (angle ~55 deg)
    for y in range(70):
        for x in range(320):
            beam = math.sin((x - y * 1.4) * 0.08)
            if beam > 0.65:
                curr = p[x, y]
                alpha = (beam - 0.65) * 0.7
                p[x, y] = (
                    min(255, int(curr[0] + 35 * alpha)),
                    min(255, int(curr[1] + 45 * alpha)),
                    min(255, int(curr[2] + 20 * alpha)),
                    255
                )

    # 2. Far silhouette layer of trees (y: 35..85)
    for tx in range(0, 320, 18):
        h = int(22 + 8 * math.sin(tx * 0.4))
        for ty in range(65 - h, 75):
            for t_x in range(tx - 12, tx + 13):
                if 0 <= t_x < 320:
                    dx = (t_x - tx) / 11.0
                    dy = (ty - (65 - h * 0.5)) / (h * 0.6)
                    if dx*dx + dy*dy <= 1.0:
                        p[t_x, ty] = (42, 92, 60, 255)

    # 3. Main midground ancient trees (trunks at y 65..108, huge leafy crowns)
    tree_centers = [25, 75, 130, 185, 240, 295]
    for tc in tree_centers:
        # Trunk
        for ty in range(70, 108):
            w = 3 if ty < 90 else 4
            for tx in range(tc - w, tc + w + 1):
                if 0 <= tx < 320:
                    shade = WOOD_D if tx < tc else WOOD_M
                    if tx == tc: shade = WOOD_L
                    p[tx, ty] = shade
                    
        # Canopy clusters
        for oy in [-28, -14, 0]:
            cy = 60 + oy
            rx, ry = 18, 14
            for ty in range(cy - ry, cy + ry + 1):
                for tx in range(tc - rx, tc + rx + 1):
                    if 0 <= tx < 320 and 0 <= ty < 180:
                        dx = (tx - tc) / float(rx)
                        dy = (ty - cy) / float(ry)
                        dist = dx*dx + dy*dy
                        if dist <= 1.0:
                            if dist < 0.35 and ty < cy:
                                p[tx, ty] = (95, 175, 85, 255) # Highlight
                            elif dist < 0.7:
                                p[tx, ty] = (62, 138, 68, 255) # Mid
                            else:
                                p[tx, ty] = (38, 90, 48, 255) # Shadow

    # Hanging vines
    for vx, vlen in [(45, 16), (95, 20), (160, 18), (210, 22), (270, 15)]:
        for vy in range(55, 55 + vlen):
            if 0 <= vx < 320 and vy < 180:
                p[vx, vy] = (50, 120, 55, 255)
                if vy % 4 == 0 and vx + 1 < 320:
                    p[vx + 1, vy] = (85, 165, 75, 255)

    # 4. Foreground rich grass meadow (y: 100..180)
    for y in range(100, 180):
        t = (y - 100) / 80.0
        r = int(58 * (1 - t) + 40 * t)
        g = int(125 * (1 - t) + 95 * t)
        b = int(52 * (1 - t) + 42 * t)
        for x in range(320):
            p[x, y] = (r, g, b, 255)
            
    # Grass tufts and wildflowers
    for y in range(108, 178, 6):
        for x in range(8, 312, 12):
            ox = (x * 7 + y * 13) % 9 - 4
            gx = x + ox
            if 0 <= gx < 319:
                p[gx, y] = (88, 165, 72, 255)
                p[gx, y - 1] = (115, 195, 92, 255)
                if (x + y) % 36 == 0: # Wildflower (yellow / white / blue)
                    flower_col = WHITE if (x % 2 == 0) else ((255, 220, 70, 255) if x % 3 == 0 else (120, 190, 255, 255))
                    p[gx, y - 2] = flower_col
                    
    # 5. Battle Pedestals (Player mound: bottom-left ~x=75, y=145; Enemy mound: top-right ~x=245, y=105)
    def draw_pedestal(cx, cy, rx, ry):
        for y in range(cy - ry, cy + ry + 4):
            for x in range(cx - rx, cx + rx + 1):
                if 0 <= x < 320 and 0 <= y < 180:
                    dx = (x - cx) / float(rx)
                    dy = (y - cy) / float(ry)
                    if dx*dx + dy*dy <= 1.0:
                        # Top earthen grass surface
                        if dy < 0.2:
                            p[x, y] = (92, 178, 78, 255) if (dx*dx + dy*dy < 0.5) else (68, 142, 60, 255)
                        else:
                            p[x, y] = (140, 95, 52, 255) # Soil rim
                    elif dx*dx + ((y - (cy + 2)) / float(ry))**2 <= 1.05 and y >= cy:
                        p[x, y] = (95, 62, 32, 255) # Shadow rim

    draw_pedestal(75, 148, 48, 14)
    draw_pedestal(245, 108, 44, 12)
    
    return im

def make_battle_grassland():
    # 320x180: Wide bright open grassland, puffy clouds, distant blue mountains, blooming rolling hills
    im = Image.new("RGBA", (320, 180), (135, 195, 245, 255))
    p = im.load()
    
    # 1. Vibrant anime sky gradient (y: 0..85)
    for y in range(85):
        t = y / 85.0
        r = int(110 * (1 - t) + 190 * t)
        g = int(175 * (1 - t) + 225 * t)
        b = int(245 * (1 - t) + 252 * t)
        for x in range(320):
            p[x, y] = (r, g, b, 255)
            
    # Stylized clouds
    def draw_cloud(cx, cy, scale):
        for y in range(cy - int(8*scale), cy + int(9*scale)):
            for x in range(cx - int(24*scale), cx + int(25*scale)):
                if 0 <= x < 320 and 0 <= y < 180:
                    dx = (x - cx) / (22.0 * scale)
                    dy = (y - cy) / (8.0 * scale)
                    dist = dx*dx + dy*dy
                    if dist <= 1.0:
                        shade = WHITE if (dy < 0.1) else (215, 230, 248, 255)
                        p[x, y] = shade
                        
    draw_cloud(60, 25, 1.2)
    draw_cloud(170, 18, 0.9)
    draw_cloud(275, 32, 1.4)
    
    # 2. Distant mountain peaks (y: 40..85)
    for x in range(320):
        # 3 majestic peaks
        my1 = int(52 - 18 * max(0, 1.0 - abs(x - 90)/45.0))
        my2 = int(46 - 24 * max(0, 1.0 - abs(x - 210)/60.0))
        my = min(my1, my2)
        if my < 85:
            for y in range(my, 85):
                t = (y - my) / 40.0
                p[x, y] = (int(120 + 20*t), int(145 + 30*t), int(195 + 15*t), 255)
                # Mountain peak snowcap
                if y - my < 6:
                    p[x, y] = (240, 245, 255, 255)

    # 3. Rolling midground hills (y: 75..115)
    for x in range(320):
        hy = int(82 + 10 * math.sin(x * 0.025 + 0.5))
        for y in range(hy, 115):
            p[x, y] = (92, 172, 85, 255)
        # Tree silhouettes on ridge
        if x % 28 == 0:
            for ty in range(hy - 8, hy):
                p[x, ty] = (65, 130, 68, 255)
                p[x-1, ty] = (65, 130, 68, 255)

    # 4. Foreground lush green meadow (y: 105..180)
    for y in range(105, 180):
        t = (y - 105) / 75.0
        r = int(75 * (1 - t) + 52 * t)
        g = int(155 * (1 - t) + 115 * t)
        b = int(60 * (1 - t) + 48 * t)
        for x in range(320):
            p[x, y] = (r, g, b, 255)
            
    # Wildflower dots & lush grass detail
    for y in range(112, 178, 5):
        for x in range(6, 314, 10):
            ox = (x * 11 + y * 17) % 7 - 3
            gx = x + ox
            if 0 <= gx < 320:
                p[gx, y] = (105, 195, 82, 255)
                if (x * y) % 25 == 0:
                    p[gx, y - 1] = (255, 230, 90, 255) # Yellow buttercup
                elif (x * y) % 31 == 0:
                    p[gx, y - 1] = (255, 120, 120, 255) # Red poppy

    # 5. Battle Pedestals
    def draw_pedestal(cx, cy, rx, ry):
        for y in range(cy - ry, cy + ry + 4):
            for x in range(cx - rx, cx + rx + 1):
                if 0 <= x < 320 and 0 <= y < 180:
                    dx = (x - cx) / float(rx)
                    dy = (y - cy) / float(ry)
                    if dx*dx + dy*dy <= 1.0:
                        p[x, y] = (112, 198, 92, 255) if (dx*dx + dy*dy < 0.5) else (85, 162, 72, 255)
                    elif dx*dx + ((y - (cy + 2)) / float(ry))**2 <= 1.05 and y >= cy:
                        p[x, y] = (130, 92, 55, 255)

    draw_pedestal(75, 148, 48, 14)
    draw_pedestal(245, 108, 44, 12)
    
    return im

def make_battle_cave():
    # 320x180: High-atmosphere lightning crystal cavern, craggy rock strata, glowing crystals & electric motes
    im = Image.new("RGBA", (320, 180), (22, 18, 42, 255))
    p = im.load()
    
    # 1. Cavern wall texture with dark stone strata
    for y in range(180):
        t = y / 180.0
        r = int(32 * (1 - t) + 16 * t)
        g = int(28 * (1 - t) + 14 * t)
        b = int(58 * (1 - t) + 36 * t)
        for x in range(320):
            # Rugged stone noise
            stone_n = int(8 * math.sin(x * 0.15) * math.cos(y * 0.12))
            p[x, y] = (max(0, min(255, r + stone_n)),
                       max(0, min(255, g + stone_n)),
                       max(0, min(255, b + stone_n + 5)),
                       255)
                       
    # 2. Stalactites hanging from roof (y: 0..45)
    for cx, l in [(25, 28), (55, 18), (85, 34), (120, 22), (160, 30), (195, 24), (230, 36), (275, 20), (305, 32)]:
        for y in range(l):
            w = int((l - y) * 0.35)
            for x in range(cx - w, cx + w + 1):
                if 0 <= x < 320:
                    p[x, y] = (68, 62, 98, 255) if x <= cx else (45, 38, 72, 255)
                    if x == cx - w: p[x, y] = (95, 88, 135, 255) # Edge highlight

    # 3. Glowing Thunder & Cyan Crystals
    crystals = [
        (40, 85, CRYSTAL_Y, (255, 245, 160, 255)),
        (115, 65, CRYSTAL_B, (200, 250, 255, 255)),
        (180, 80, CRYSTAL_Y, (255, 245, 160, 255)),
        (280, 75, CRYSTAL_B, (200, 250, 255, 255)),
        (25, 135, CRYSTAL_Y, (255, 245, 160, 255)),
        (295, 140, CRYSTAL_Y, (255, 245, 160, 255)),
    ]
    
    for cx, cy, col_base, col_core in crystals:
        # Radial glow aura
        for gy in range(cy - 25, cy + 26):
            for gx in range(cx - 25, cx + 26):
                if 0 <= gx < 320 and 0 <= gy < 180:
                    dist = math.hypot(gx - cx, gy - cy)
                    if dist < 24:
                        intensity = (1.0 - dist / 24.0) ** 1.8
                        curr = p[gx, gy]
                        p[gx, gy] = (
                            min(255, int(curr[0] + col_base[0] * 0.4 * intensity)),
                            min(255, int(curr[1] + col_base[1] * 0.4 * intensity)),
                            min(255, int(curr[2] + col_base[2] * 0.4 * intensity)),
                            255
                        )
                        
        # Geometric crystal shards
        for oy in range(-12, 13):
            w = int(5 - abs(oy) * 0.38)
            for ox in range(-w, w + 1):
                x = cx + ox
                y = cy + oy
                if 0 <= x < 320 and 0 <= y < 180:
                    if abs(ox) <= 1:
                        p[x, y] = col_core
                    elif ox < 0:
                        p[x, y] = col_base
                    else:
                        p[x, y] = (int(col_base[0]*0.75), int(col_base[1]*0.75), int(col_base[2]*0.75), 255)

    # 4. Floating electric spark motes
    spark_locs = [(50, 75), (105, 55), (170, 70), (195, 95), (265, 80), (80, 120), (240, 125)]
    for sx, sy in spark_locs:
        p[sx, sy] = WHITE
        for dx, dy in [(-1, 0), (1, 0), (0, -1), (0, 1)]:
            if 0 <= sx+dx < 320 and 0 <= sy+dy < 180:
                p[sx+dx, sy+dy] = (255, 240, 100, 200)

    # 5. Cave stone floor (y: 115..180)
    for y in range(115, 180):
        t = (y - 115) / 65.0
        r = int(36 * (1 - t) + 24 * t)
        g = int(32 * (1 - t) + 20 * t)
        b = int(54 * (1 - t) + 38 * t)
        for x in range(320):
            p[x, y] = (r, g, b, 255)
            
    # Cave battle stone platforms
    def draw_cave_platform(cx, cy, rx, ry):
        for y in range(cy - ry, cy + ry + 4):
            for x in range(cx - rx, cx + rx + 1):
                if 0 <= x < 320 and 0 <= y < 180:
                    dx = (x - cx) / float(rx)
                    dy = (y - cy) / float(ry)
                    if dx*dx + dy*dy <= 1.0:
                        p[x, y] = (72, 65, 105, 255) if (dx*dx + dy*dy < 0.6) else (54, 48, 82, 255)
                    elif dx*dx + ((y - (cy + 2)) / float(ry))**2 <= 1.05 and y >= cy:
                        p[x, y] = (28, 24, 45, 255)

    draw_cave_platform(75, 148, 48, 14)
    draw_cave_platform(245, 108, 44, 12)
    
    return im

def make_title_world():
    # 480x180: Glorious tropical island panorama, golden hour sunrise, turquoise ocean waves, volcano peak & coastal village
    im = Image.new("RGBA", (480, 180), (120, 180, 240, 255))
    p = im.load()
    
    # 1. Sunrise Sky Gradient (y: 0..90)
    for y in range(90):
        t = y / 90.0
        # Deep sapphire at top -> warm coral/peach -> golden glow at horizon
        if t < 0.45:
            st = t / 0.45
            r = int(60 * (1 - st) + 145 * st)
            g = int(95 * (1 - st) + 160 * st)
            b = int(185 * (1 - st) + 225 * st)
        else:
            st = (t - 0.45) / 0.55
            r = int(145 * (1 - st) + 255 * st)
            g = int(160 * (1 - st) + 215 * st)
            b = int(225 * (1 - st) + 145 * st)
            
        for x in range(480):
            p[x, y] = (r, g, b, 255)
            
    # Rising sun behind central peak (cx=240, cy=68)
    for y in range(25, 95):
        for x in range(180, 300):
            dist = math.hypot(x - 240, y - 68)
            if dist < 45:
                alpha = (1.0 - dist / 45.0) ** 1.5
                curr = p[x, y]
                p[x, y] = (
                    min(255, int(curr[0] + 90 * alpha)),
                    min(255, int(curr[1] + 80 * alpha)),
                    min(255, int(curr[2] + 40 * alpha)),
                    255
                )

    # Clouds in sky
    def draw_cloud(cx, cy, rx, ry):
        for y in range(cy - ry, cy + ry + 1):
            for x in range(cx - rx, cx + rx + 1):
                if 0 <= x < 480 and 0 <= y < 180:
                    dx = (x - cx) / float(rx)
                    dy = (y - cy) / float(ry)
                    if dx*dx + dy*dy <= 1.0:
                        p[x, y] = WHITE if dy < 0 else (255, 230, 210, 240)
                        
    draw_cloud(70, 32, 35, 10)
    draw_cloud(410, 28, 42, 11)
    draw_cloud(150, 48, 25, 7)
    draw_cloud(340, 52, 28, 8)

    # 2. Central Island Silhouette & Slopes (cx=240, peak at y=52)
    for x in range(480):
        # Island height profile
        dx = abs(x - 240)
        if dx < 170:
            # Volcano cone with gentle coastal spread
            h = int(60 * (1.0 - (dx / 170.0) ** 1.6))
            iy = 112 - h
            for y in range(iy, 115):
                # Mountain slopes: lush forest greens & rock ridge
                t = (y - iy) / float(max(1, 115 - iy))
                if y - iy < 6 and dx < 35:
                    p[x, y] = (165, 135, 110, 255) # Rock caldera peak
                else:
                    p[x, y] = (int(45 + 35*t), int(105 + 40*t), int(55 + 25*t), 255)

    # 3. Island sandy beach & coconut palm trees
    for x in range(480):
        dx = abs(x - 240)
        if dx < 185:
            # Sandy shoreline rim (y: 112..120)
            for y in range(113, 121):
                if p[x, y][3] != 0:
                    p[x, y] = (244, 215, 145, 255) if y < 117 else (210, 175, 110, 255)
                    
    # Village huts on shore (x=160..195)
    for hx in [170, 190, 215]:
        for hy in range(106, 114):
            for hx_sub in range(hx - 5, hx + 6):
                p[hx_sub, hy] = (240, 235, 225, 255) # Wall
        for ry in range(101, 107):
            w = 107 - ry
            for rx in range(hx - w - 2, hx + w + 3):
                p[rx, ry] = (201, 59, 59, 255) # Red Roof

    # 4. Tropical Ocean with turquoise gradient & wave foam (y: 118..180)
    for y in range(118, 180):
        t = (y - 118) / 62.0
        # Turquoise near shore -> deep azure in foreground
        r = int(55 * (1 - t) + 25 * t)
        g = int(165 * (1 - t) + 85 * t)
        b = int(225 * (1 - t) + 165 * t)
        for x in range(480):
            p[x, y] = (r, g, b, 255)
            
    # White foam wave crests
    for y in range(123, 178, 8):
        for x in range(480):
            wave_val = math.sin(x * 0.08 + y * 1.5)
            if wave_val > 0.85:
                p[x, y] = (210, 245, 255, 255)
                if wave_val > 0.94 and y > 120:
                    p[x, y - 1] = WHITE

    return im

def generate_all_backgrounds(out_dir):
    os.makedirs(out_dir, exist_ok=True)
    bg_map = {
        "BG_Battle_Forest.png": make_battle_forest(),
        "BG_Battle_Grassland.png": make_battle_grassland(),
        "BG_Battle_Cave.png": make_battle_cave(),
        "BG_Title_World.png": make_title_world()
    }
    paths = {}
    for name, im in bg_map.items():
        p = os.path.join(out_dir, name)
        im.save(p)
        paths[name] = p
    return paths

if __name__ == "__main__":
    generate_all_backgrounds("temp_bg")
    print("Backgrounds generated.")
