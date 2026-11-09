"""
gen_tiles.py - High-fidelity overworld tileset generator for Pulau Monster.
Generates Tileset_Overworld.png (192x96: 12 columns x 6 rows of 16x16 cells = 72 tiles).

Visual Upgrade:
- Forest: Clean, lush, seamless green grass carpet with natural subtle blade tufts and clovers.
- Gua Petir: Deep subterranean dark slate/purple bedrock, natural craggy rock cliff overhangs,
  rugged stone ledges, and glowing Electric Volt Crystals with bright cyan/blue facets and spark crackles.
- Proper tree foliage & trunks, wooden fence, wildflowers, and village architecture.
"""

import os
from PIL import Image
from palette import PALETTE

INK = PALETTE["INK"]
WHITE = PALETTE["WHITE"]
TRANSPARENT = PALETTE["TRANSPARENT"]

# Environment palette
GRASS_D = PALETTE["GRASS_DARK"]
GRASS_M = PALETTE["GRASS_MID"]
GRASS_L = PALETTE["GRASS_LIGHT"]

SAND_D = PALETTE["SAND_DARK"]
SAND_M = PALETTE["SAND_MID"]
SAND_L = PALETTE["SAND_LIGHT"]

WATER_D = PALETTE["WATER_DARK"]
WATER_M = PALETTE["WATER_MID"]
WATER_L = PALETTE["WATER_LIGHT"]

WOOD_D = PALETTE["WOOD_DARK"]
WOOD_M = PALETTE["WOOD_MID"]
WOOD_L = PALETTE["WOOD_LIGHT"]

ROOF_R = PALETTE["ROOF_RED"]
ROOF_D = PALETTE["ROOF_DARK"]
WALL_W = PALETTE["WALL_WHITE"]

# Cave Subterranean Palette
CAVE_FL = PALETTE["CAVE_FLOOR_BASE"]
CAVE_ST = PALETTE["CAVE_FLOOR_STONE"]
CAVE_CR = PALETTE["CAVE_FLOOR_CRACK"]
CAVE_D = PALETTE["CAVE_ROCK_DARK"]
CAVE_M = PALETTE["CAVE_ROCK_MID"]
CAVE_L = PALETTE["CAVE_ROCK_LIGHT"]
CAVE_AC = PALETTE["CAVE_ROCK_ACCENT"]

# Electric Volt Crystals
CRYSTAL_Y = PALETTE["CRYSTAL_YELLOW"] # Electric Cyan
CRYSTAL_B = PALETTE["CRYSTAL_BLUE"]   # Electric Azure Blue
CRYSTAL_CORE = PALETTE["CRYSTAL_CORE"] # Glowing White-Cyan Core
CRYSTAL_D = PALETTE["CRYSTAL_DARK"]   # Deep refraction shadow
CRYSTAL_P = PALETTE["CRYSTAL_PURPLE"] # Electric violet
CRYSTAL_SP = PALETTE["CRYSTAL_SPARK"] # Spark glow
GOLD = PALETTE["GOLD"]

def tile_grass_fill(variant=0):
    # Base: solid rich green carpet
    im = Image.new("RGBA", (16, 16), GRASS_M)
    p = im.load()
    
    # Outer borders (x=0,15, y=0,15) stay 100% GRASS_M for seamless tiling
    if variant == 0:
        # Standard clean meadow with 2 delicate blade tufts
        p[4, 4] = GRASS_L; p[4, 5] = GRASS_D
        p[5, 4] = GRASS_L
        p[11, 9] = GRASS_L; p[11, 10] = GRASS_D
        p[12, 9] = GRASS_L
    elif variant == 1:
        # Gentle clover tuft
        p[5, 10] = GRASS_L; p[5, 11] = GRASS_D
        p[6, 10] = GRASS_L
        p[10, 3] = GRASS_L; p[10, 4] = GRASS_D
        p[11, 4] = GRASS_L
    elif variant == 2:
        # Subtle wild clover blade
        p[11, 5] = GRASS_L; p[11, 6] = GRASS_D
        p[6, 12] = WHITE; p[7, 12] = GOLD # Tiny white wildflower speck
        p[6, 13] = GOLD
        
    return im

def tile_sand_fill():
    # Warm cobbled sand path
    im = Image.new("RGBA", (16, 16), SAND_M)
    p = im.load()
    
    for y in range(16):
        for x in range(16):
            if (x * 11 + y * 7) % 8 == 0:
                p[x, y] = SAND_L
            elif (x * 3 + y * 13) % 10 == 0:
                p[x, y] = SAND_D
                
    cobbles = [(3, 4), (9, 3), (5, 10), (12, 11)]
    for cx, cy in cobbles:
        p[cx, cy] = SAND_L
        p[cx+1, cy] = SAND_L
        p[cx, cy+1] = SAND_D
        p[cx+1, cy+1] = SAND_D
        
    return im

def tile_water_fill():
    im = Image.new("RGBA", (16, 16), WATER_M)
    p = im.load()
    
    for y in range(16):
        for x in range(16):
            if y % 4 == 0:
                p[x, y] = WATER_D
                
    for x in range(2, 7): p[x, 3] = WATER_L
    p[4, 4] = WHITE; p[5, 4] = WATER_L
    for x in range(8, 14): p[x, 9] = WATER_L
    p[10, 10] = WHITE; p[11, 10] = WATER_L
    for x in range(3, 8): p[x, 14] = WATER_L
    
    return im

def make_path_edge(top, bottom, left, right):
    im = tile_grass_fill(0)
    p = im.load()
    
    for y in range(16):
        for x in range(16):
            in_sand = True
            if top and y < 4: in_sand = False
            if bottom and y > 11: in_sand = False
            if left and x < 4: in_sand = False
            if right and x > 11: in_sand = False
            
            if in_sand:
                p[x, y] = SAND_M
                if (x + y) % 5 == 0: p[x, y] = SAND_L
                elif (x * y) % 7 == 0: p[x, y] = SAND_D
            else:
                if top and y == 3 and not (left and x < 4) and not (right and x > 11):
                    p[x, y] = GRASS_D
                if left and x == 3 and not (top and y < 4) and not (bottom and y > 11):
                    p[x, y] = GRASS_D
                    
    return im

def make_water_edge(top, bottom, left, right):
    im = tile_grass_fill(0)
    p = im.load()
    
    for y in range(16):
        for x in range(16):
            in_water = True
            if top and y < 4: in_water = False
            if bottom and y > 11: in_water = False
            if left and x < 4: in_water = False
            if right and x > 11: in_water = False
            
            if in_water:
                p[x, y] = WATER_M
                is_rim = (top and y == 4) or (bottom and y == 11) or (left and x == 4) or (right and x == 11)
                if is_rim:
                    p[x, y] = WATER_L
                    if (x + y) % 2 == 0: p[x, y] = WHITE
                else:
                    if y % 5 == 2 and (x % 3 == 0): p[x, y] = WATER_L
                    elif y % 5 == 3 and (x % 3 == 1): p[x, y] = WATER_D
            else:
                is_shore = (top and y == 3) or (bottom and y == 12) or (left and x == 3) or (right and x == 12)
                if is_shore:
                    p[x, y] = SAND_D
                    
    return im

# --- CAVE TILES GENERATION ---

def tile_cave_floor():
    # Subterranean dark slate bedrock
    im = Image.new("RGBA", (16, 16), CAVE_FL)
    p = im.load()
    
    # Natural chiseled stone block contours and fissures
    for x in range(1, 9): p[x, 5] = CAVE_CR
    for x in range(7, 15): p[x, 11] = CAVE_CR
    for y in range(5, 12): p[7, y] = CAVE_CR
    p[8, 11] = CAVE_CR; p[14, 12] = CAVE_CR
    
    # Subtle stone surface highlights
    for x, y in [(3, 3), (4, 3), (11, 2), (12, 2), (3, 8), (4, 8), (10, 8), (11, 8), (5, 13), (6, 13)]:
        p[x, y] = CAVE_ST
        
    return im

def tile_cave_rubble():
    im = tile_cave_floor()
    p = im.load()
    pebbles = [(4, 9), (11, 4), (12, 13), (3, 14)]
    for px, py in pebbles:
        p[px, py] = CAVE_L
        p[px+1, py] = CAVE_M
        p[px, py+1] = CAVE_D
        p[px+1, py+1] = CAVE_CR
    return im

def tile_cave_rock():
    # Stalagmite / Craggy Cave Boulder obstacle
    im = tile_cave_floor()
    p = im.load()
    for y in range(4, 14):
        w = int(5 - abs(y - 9) * 0.5)
        for x in range(8 - w, 8 + w + 1):
            p[x, y] = CAVE_M
            if x <= 7 and y <= 8:
                p[x, y] = CAVE_L
            elif x >= 9 or y >= 12:
                p[x, y] = CAVE_D
    p[7, 4] = CAVE_AC; p[8, 4] = CAVE_L
    for x in range(4, 13):
        p[x, 14] = CAVE_CR
    return im

def tile_crystal_cluster():
    # Glowing Electric Volt Crystal Cluster on dark cave bedrock
    im = tile_cave_floor()
    p = im.load()
    
    # Central Tall Crystal Spire (x: 6..10, y: 3..14)
    for y in range(3, 15):
        w = int(3 - abs(y - 9) * 0.3)
        for x in range(8 - w, 8 + w + 1):
            p[x, y] = CRYSTAL_B
            if x == 8: p[x, y] = CRYSTAL_CORE
            elif x == 7: p[x, y] = CRYSTAL_Y
            elif x > 8: p[x, y] = CRYSTAL_D
            
    # Left Angled Crystal Shard (x: 2..6, y: 7..14)
    for y in range(7, 15):
        w = int(2 - abs(y - 11) * 0.3)
        for x in range(4 - w, 4 + w + 1):
            p[x, y] = CRYSTAL_B
            if x == 4 and y <= 10: p[x, y] = CRYSTAL_Y
            elif x <= 3: p[x, y] = CRYSTAL_CORE if y == 8 else CRYSTAL_Y
            elif x >= 5: p[x, y] = CRYSTAL_D
            
    # Right Angled Crystal Shard (x: 10..14, y: 8..14)
    for y in range(8, 15):
        w = int(2 - abs(y - 11) * 0.3)
        for x in range(12 - w, 12 + w + 1):
            p[x, y] = CRYSTAL_B
            if x == 12: p[x, y] = CRYSTAL_Y
            elif x > 12: p[x, y] = CRYSTAL_D
            
    # Electric Sparks / Lightning crackles at the crystal tips
    p[8, 2] = CRYSTAL_CORE
    p[7, 1] = CRYSTAL_SP; p[9, 1] = CRYSTAL_Y
    p[3, 6] = CRYSTAL_CORE; p[2, 5] = CRYSTAL_SP
    p[13, 7] = CRYSTAL_Y; p[14, 6] = CRYSTAL_SP
    
    # Ambient electric blue floor reflection
    for x in range(3, 14):
        p[x, 15] = (24, 60, 95, 255)
        
    return im

def tile_crystal_spire():
    # Tall glowing electric crystal shard in vivid violet & cyan
    im = tile_cave_floor()
    p = im.load()
    
    for y in range(2, 15):
        w = int(2 - abs(y - 8) * 0.15)
        for x in range(8 - w, 8 + w + 1):
            p[x, y] = CRYSTAL_P
            if x == 8: p[x, y] = CRYSTAL_CORE
            elif x < 8: p[x, y] = CRYSTAL_Y
            else: p[x, y] = (50, 40, 110, 255)
            
    p[8, 1] = WHITE
    p[7, 0] = CRYSTAL_SP
    p[9, 2] = CRYSTAL_Y
    return im

def tile_cave_wall_top():
    # Natural craggy rock cliff overhang
    im = Image.new("RGBA", (16, 16), CAVE_M)
    p = im.load()
    
    for x in range(16):
        p[x, 0] = CAVE_L
        p[x, 1] = CAVE_L if (x * 3) % 4 != 0 else CAVE_AC
        p[x, 2] = CAVE_M
        
    for y in range(2, 13):
        for x in range(16):
            if (x == 3 and y > 3) or (x == 8 and y < 10) or (x == 13 and y > 4):
                p[x, y] = CAVE_D
            elif (x == 4 and y > 3) or (x == 9 and y < 10):
                p[x, y] = CAVE_AC
                
    for x in range(16):
        p[x, 13] = CAVE_D
        p[x, 14] = CAVE_CR
        p[x, 15] = INK
        
    return im

def tile_cave_wall_bot():
    # Natural craggy stone wall base
    im = Image.new("RGBA", (16, 16), CAVE_M)
    p = im.load()
    
    for x in range(16):
        p[x, 0] = CAVE_L
        p[x, 1] = CAVE_M
        
    for y in range(2, 13):
        for x in range(16):
            if (x == 5 and y < 9) or (x == 11 and y > 4):
                p[x, y] = CAVE_D
                
    for x in range(16):
        p[x, 13] = CAVE_D
        p[x, 14] = CAVE_CR
        p[x, 15] = INK
        
    return im

def tile_cave_wall_left():
    # Natural vertical rock face on left
    im = Image.new("RGBA", (16, 16), CAVE_M)
    p = im.load()
    
    for y in range(16):
        p[0, y] = INK
        p[1, y] = CAVE_D
        p[15, y] = CAVE_L
        p[14, y] = CAVE_AC
        
    for x in range(2, 14):
        p[x, 4] = CAVE_D; p[x, 5] = CAVE_L
        p[x, 10] = CAVE_D; p[x, 11] = CAVE_L
        
    return im

def tile_cave_wall_right():
    # Natural vertical rock face on right
    im = Image.new("RGBA", (16, 16), CAVE_M)
    p = im.load()
    
    for y in range(16):
        p[15, y] = INK
        p[14, y] = CAVE_D
        p[0, y] = CAVE_L
        p[1, y] = CAVE_AC
        
    for x in range(2, 14):
        p[x, 6] = CAVE_D; p[x, 7] = CAVE_L
        p[x, 12] = CAVE_D; p[x, 13] = CAVE_L
        
    return im

# --- FOREST TILES (TREES & FENCE) ---

def tile_tree_top():
    # Dense, lush, rounded RPG tree foliage canopy (16x16)
    im = Image.new("RGBA", (16, 16), TRANSPARENT)
    p = im.load()
    
    for y in range(1, 16):
        for x in range(0, 16):
            dx = (x - 7.5) / 7.5
            dy = (y - 8.5) / 7.5
            dist = dx*dx + dy*dy
            if dist <= 1.0:
                p[x, y] = GRASS_D
                if dy < -0.2 and dist < 0.7:
                    p[x, y] = (145, 230, 95, 255) # Top sunlit leaf highlight
                elif dist < 0.6:
                    p[x, y] = GRASS_M
                elif dy > 0.4:
                    p[x, y] = (40, 95, 30, 255) # Deep leaf shadow
    return im

def tile_tree_bot():
    # Sturdy tree trunk on clean green grass base (16x16)
    im = tile_grass_fill(0)
    p = im.load()
    
    # Trunk (x: 5..10, y: 0..12)
    for y in range(0, 12):
        for x in range(5, 11):
            p[x, y] = WOOD_M
            if x <= 6: p[x, y] = WOOD_D
            elif x >= 9: p[x, y] = WOOD_L
            
    p[4, 11] = WOOD_D; p[4, 12] = INK
    p[11, 11] = WOOD_L; p[11, 12] = INK
    p[5, 12] = WOOD_D; p[10, 12] = WOOD_D
    
    for x in range(4, 12):
        p[x, 13] = GRASS_D
        
    for x in range(2, 14):
        p[x, 0] = GRASS_D
        p[x, 1] = GRASS_M
        
    return im

def tile_wooden_fence():
    # Clean rustic wooden fence on green grass (16x16)
    im = tile_grass_fill(0)
    p = im.load()
    
    for x in range(16):
        p[x, 5] = WOOD_L; p[x, 6] = WOOD_D
        p[x, 10] = WOOD_M; p[x, 11] = WOOD_D
        
    for y in range(2, 14):
        for x in range(7, 10):
            p[x, y] = WOOD_M
            if x == 7: p[x, y] = WOOD_L
            elif x == 9: p[x, y] = WOOD_D
            
    p[8, 1] = WOOD_L
    p[7, 14] = INK; p[8, 14] = INK; p[9, 14] = INK
    
    return im

def tile_tall_grass(variant=0):
    im = tile_grass_fill(0)
    p = im.load()
    blade_xs = [2, 5, 8, 11, 14] if variant == 0 else [3, 6, 9, 12]
    for bx in blade_xs:
        h = 10 if (bx % 2 == 0) else 12
        for y in range(14 - h, 14):
            if bx < 16:
                p[bx, y] = (45, 125, 35, 255)
            if bx - 1 >= 0 and y >= 14 - h + 1:
                p[bx - 1, y] = (135, 225, 90, 255)
        if bx - 1 >= 0: p[bx - 1, 14 - h] = (155, 240, 105, 255)
    return im

def draw_tileset():
    tileset = Image.new("RGBA", (192, 96), TRANSPARENT)
    tiles = {}
    
    # Row 0: 0..4 Path top edges, 5..9 Water top edges, 10..11 Grass variants
    tiles[0] = make_path_edge(top=True, bottom=False, left=True, right=False)
    tiles[1] = make_path_edge(top=True, bottom=False, left=False, right=False)
    tiles[2] = make_path_edge(top=True, bottom=False, left=False, right=True)
    tiles[3] = make_path_edge(top=True, bottom=False, left=True, right=False)
    tiles[4] = make_path_edge(top=True, bottom=False, left=False, right=True)
    
    tiles[5] = make_water_edge(top=True, bottom=False, left=True, right=False)
    tiles[6] = make_water_edge(top=True, bottom=False, left=False, right=False)
    tiles[7] = make_water_edge(top=True, bottom=False, left=False, right=True)
    tiles[8] = make_water_edge(top=True, bottom=False, left=True, right=False)
    tiles[9] = make_water_edge(top=True, bottom=False, left=False, right=True)
    
    tiles[10] = tile_grass_fill(0) # Main overworld clean grass
    tiles[11] = tile_grass_fill(1) # Grass variant
    
    # Row 1: 12..16 Path mid edges, 17..21 Water mid edges, 22 Flowers, 23 SmallRock
    tiles[12] = make_path_edge(top=False, bottom=False, left=True, right=False)
    tiles[13] = tile_sand_fill() # Path Center
    tiles[14] = make_path_edge(top=False, bottom=False, left=False, right=True)
    tiles[15] = make_path_edge(top=False, bottom=True, left=True, right=False)
    tiles[16] = make_path_edge(top=False, bottom=True, left=False, right=True)
    
    tiles[17] = make_water_edge(top=False, bottom=False, left=True, right=False)
    tiles[18] = tile_water_fill() # Water Center
    tiles[19] = make_water_edge(top=False, bottom=False, left=False, right=True)
    tiles[20] = make_water_edge(top=False, bottom=True, left=True, right=False)
    tiles[21] = make_water_edge(top=False, bottom=True, left=False, right=True)
    
    # Tile 22: Red & Yellow Wildflower Patch on grass
    flw = tile_grass_fill(0)
    fp = flw.load()
    fp[4, 6] = ROOF_R; fp[5, 5] = ROOF_R; fp[5, 7] = ROOF_R; fp[6, 6] = ROOF_R; fp[5, 6] = GOLD
    fp[11, 10] = GOLD; fp[10, 10] = GOLD; fp[12, 10] = GOLD; fp[11, 9] = GOLD; fp[11, 10] = WHITE
    fp[11, 4] = (92, 175, 255, 255); fp[12, 4] = (92, 175, 255, 255); fp[11, 5] = (60, 120, 220, 255)
    tiles[22] = flw
    
    # Tile 23: Granite Small Boulder on grass
    rck = tile_grass_fill(0)
    rp = rck.load()
    for y in range(7, 14):
        for x in range(5, 13):
            if ((x - 8.5)/3.5)**2 + ((y - 10.5)/3.0)**2 <= 1.0:
                rp[x, y] = CAVE_M
                if y <= 9 and x <= 9: rp[x, y] = CAVE_L
                elif y >= 12: rp[x, y] = INK
    rp[7, 8] = WHITE
    tiles[23] = rck
    
    # Row 2: 24..28 Path bottom edges, 29..31 Water bottom edges, 32 Sign, 33 Bush, 34 TallGrass_A, 35 TallGrass_B
    tiles[24] = make_path_edge(top=False, bottom=True, left=True, right=False)
    tiles[25] = make_path_edge(top=False, bottom=True, left=False, right=False)
    tiles[26] = make_path_edge(top=False, bottom=True, left=False, right=True)
    tiles[27] = tile_sand_fill()
    tiles[28] = tile_sand_fill()
    
    tiles[29] = make_water_edge(top=False, bottom=True, left=True, right=False)
    tiles[30] = make_water_edge(top=False, bottom=True, left=False, right=False)
    tiles[31] = make_water_edge(top=False, bottom=True, left=False, right=True)
    
    # Tile 32: Wooden Signboard on grass
    sgn = tile_grass_fill(0)
    sp = sgn.load()
    for y in range(3, 9):
        for x in range(2, 14):
            sp[x, y] = WOOD_M
            if y == 3: sp[x, y] = WOOD_L
            elif y == 8: sp[x, y] = WOOD_D
    for y in range(3, 9):
        sp[2, y] = INK; sp[13, y] = INK
    for y in range(9, 15):
        sp[7, y] = WOOD_D; sp[8, y] = WOOD_M
    tiles[32] = sgn
    
    # Tile 33: Round Bush on grass
    bsh = tile_grass_fill(0)
    bp = bsh.load()
    for y in range(3, 14):
        for x in range(2, 14):
            dx = (x - 7.5)/5.5
            dy = (y - 8.5)/5.0
            if dx*dx + dy*dy <= 1.0:
                bp[x, y] = GRASS_D
                if dy < -0.2: bp[x, y] = GRASS_L
                elif dy < 0.3: bp[x, y] = GRASS_M
    for bx, by in [(5, 7), (9, 6), (7, 10), (11, 9)]:
        bp[bx, by] = ROOF_R
    tiles[33] = bsh
    
    # Tile 34 & 35: Tall Encounter Grass
    tiles[34] = tile_tall_grass(0)
    tiles[35] = tile_tall_grass(1)
    
    # Row 3: 36 Tree Top, 37 Tree Bot, 38 Fence, 39 Dense Canopy, 40 Red Flowers, 41 Yellow Flowers, 42 Tall Grass C, 43 Bush, 44 Cave Floor, 45 Cave Rock, 46 Crystal Cluster, 47 Cave Wall Top
    tiles[36] = tile_tree_top()
    tiles[37] = tile_tree_bot()
    tiles[38] = tile_wooden_fence()
    tiles[39] = tile_tree_top()
    
    # 40 Wildflowers Red
    tiles[40] = tiles[22]
    
    # 41 Wildflowers Yellow on grass
    flw_y = tile_grass_fill(0)
    fyp = flw_y.load()
    fyp[6, 6] = GOLD; fyp[7, 5] = GOLD; fyp[7, 7] = GOLD; fyp[8, 6] = GOLD; fyp[7, 6] = WHITE
    tiles[41] = flw_y
    
    tiles[42] = tile_tall_grass(0)
    tiles[43] = tiles[33]
    
    # Cave Assets
    tiles[44] = tile_cave_floor()       # Cave Subterranean Bedrock
    tiles[45] = tile_cave_rock()        # Cave Stalagmite / Rock Boulder
    tiles[46] = tile_crystal_cluster() # Glowing Electric Volt Crystal Cluster
    tiles[47] = tile_cave_wall_top()   # Cave Wall Top (Rugged Cliff Overhang)
    
    # Row 4: 48 Cave Wall Bot, 49 Cave Wall L, 50 Cave Wall R, 51 Signpost, 52 Well, 53..55 House Roof, 56..58 House Upper Wall, 59 House Mid L
    tiles[48] = tile_cave_wall_bot()   # Cave Wall Bot
    tiles[49] = tile_cave_wall_left()  # Cave Wall Left
    tiles[50] = tile_cave_wall_right() # Cave Wall Right
    
    tiles[51] = tiles[32] # Signpost
    
    # Tile 52: Village Water Well
    well = tile_grass_fill(0)
    wp = well.load()
    for y in range(4, 13):
        for x in range(3, 13):
            wp[x, y] = CAVE_M
            if y == 4 or y == 12 or x == 3 or x == 12: wp[x, y] = CAVE_D
            else: wp[x, y] = WATER_M
    wp[7, 8] = WATER_L; wp[8, 8] = WATER_L
    tiles[52] = well
    
    # House Roof 53..55
    for idx, col_pos in [(53, 'L'), (54, 'M'), (55, 'R')]:
        rf = Image.new("RGBA", (16, 16), ROOF_R)
        rp = rf.load()
        for y in range(0, 16, 4):
            for x in range(16): rp[x, y] = (235, 85, 85, 255)
            for x in range(16): rp[x, min(15, y+3)] = ROOF_D
        if col_pos == 'L':
            for y in range(16): rp[0, y] = INK
        elif col_pos == 'R':
            for y in range(16): rp[15, y] = INK
        tiles[idx] = rf
        
    # House Upper Wall 56..58
    for idx, col_pos in [(56, 'L'), (57, 'M'), (58, 'R')]:
        wl = Image.new("RGBA", (16, 16), WALL_W)
        wlp = wl.load()
        for x in range(16): wlp[x, 0] = WOOD_D; wlp[x, 1] = WOOD_M
        if col_pos == 'L':
            for y in range(16): wlp[0, y] = WOOD_D; wlp[1, y] = WOOD_M
        elif col_pos == 'R':
            for y in range(16): wlp[15, y] = WOOD_D; wlp[14, y] = WOOD_M
        tiles[idx] = wl
        
    # Tile 59: House Mid L
    h59 = Image.new("RGBA", (16, 16), WALL_W)
    p59 = h59.load()
    for y in range(16): p59[0, y] = WOOD_D; p59[1, y] = WOOD_M
    tiles[59] = h59
    
    # Row 5: 60 House Mid Window, 61 House Mid R, 62 House Bot L, 63 House Bot Door, 64 House Bot R, 65 Pond Edge, 66 Grass_C, 67 Crystal Spire, 68 Cave Rubble, 69 Path, 70 Tree Small, 71 Tree Stump
    h60 = Image.new("RGBA", (16, 16), WALL_W)
    p60 = h60.load()
    for y in range(4, 12):
        for x in range(4, 12):
            p60[x, y] = (255, 220, 110, 255)
    for x in range(4, 12):
        p60[x, 4] = WOOD_D; p60[x, 11] = WOOD_D; p60[x, 7] = WOOD_D
    for y in range(4, 12):
        p60[4, y] = WOOD_D; p60[11, y] = WOOD_D; p60[7, y] = WOOD_D
    tiles[60] = h60
    
    h61 = Image.new("RGBA", (16, 16), WALL_W)
    p61 = h61.load()
    for y in range(16): p61[15, y] = WOOD_D; p61[14, y] = WOOD_M
    tiles[61] = h61
    
    h62 = Image.new("RGBA", (16, 16), WALL_W)
    p62 = h62.load()
    for y in range(16): p62[0, y] = WOOD_D
    for x in range(16): p62[x, 15] = CAVE_D; p62[x, 14] = CAVE_M
    tiles[62] = h62
    
    h63 = Image.new("RGBA", (16, 16), WALL_W)
    p63 = h63.load()
    for y in range(3, 16):
        for x in range(3, 13):
            p63[x, y] = WOOD_M
            if x == 3 or x == 12 or y == 3: p63[x, y] = WOOD_D
    for x in range(4, 12):
        p63[x, 6] = INK; p63[x, 12] = INK
    p63[11, 9] = GOLD
    tiles[63] = h63
    
    h64 = Image.new("RGBA", (16, 16), WALL_W)
    p64 = h64.load()
    for y in range(16): p64[15, y] = WOOD_D
    for x in range(16): p64[x, 15] = CAVE_D; p64[x, 14] = CAVE_M
    tiles[64] = h64
    
    tiles[65] = make_water_edge(top=False, bottom=False, left=False, right=False)
    tiles[66] = tile_grass_fill(2)     # Grass Variant 2 (meadow flower)
    tiles[67] = tile_crystal_spire()   # Tall Electric Volt Crystal Spire
    tiles[68] = tile_cave_rubble()     # Cave Rubble & Broken Slate
    tiles[69] = tile_sand_fill()
    
    # Tile 70: Tree Small
    tsm = tile_grass_fill(0)
    tsp = tsm.load()
    for y in range(2, 11):
        for x in range(3, 13):
            if ((x - 7.5)/4.5)**2 + ((y - 6.5)/4.0)**2 <= 1.0:
                tsp[x, y] = GRASS_M
                if y <= 5: tsp[x, y] = GRASS_L
                elif y >= 9: tsp[x, y] = GRASS_D
    for y in range(10, 15):
        tsp[7, y] = WOOD_D; tsp[8, y] = WOOD_M
    tiles[70] = tsm
    
    # Tile 71: Tree Stump
    stm = tile_grass_fill(0)
    stmp = stm.load()
    for y in range(8, 14):
        for x in range(4, 12):
            stmp[x, y] = WOOD_M
            if y == 8: stmp[x, y] = WOOD_L
            elif y == 13: stmp[x, y] = INK
    stmp[7, 9] = WOOD_D; stmp[8, 9] = WOOD_D
    tiles[71] = stm

    # Paste into tileset sheet
    for idx, img in tiles.items():
        row = idx // 12
        col = idx % 12
        tileset.paste(img, (col * 16, row * 16))
        
    return tileset, tiles

def generate_tileset(out_dir):
    os.makedirs(out_dir, exist_ok=True)
    tileset, tiles = draw_tileset()
    path = os.path.join(out_dir, "Tileset_Overworld.png")
    tileset.save(path)
    return path

if __name__ == "__main__":
    generate_tileset("temp_tiles")
    print("Tileset generated.")
