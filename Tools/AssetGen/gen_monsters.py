"""
gen_monsters.py - High-fidelity monster sprite generator for Pulau Monster RPG.
Generates 12 monster sprite sheets:
4 frames horizontal (_0 Idle0, _1 Idle1, _2 Attack, _3 Hurt).
Stage 1: 16x16 cells -> 64x16 sheet
Stage 2: 20x20 cells -> 80x20 sheet
Stage 3: 24x24 cells -> 96x24 sheet
"""

import os
from PIL import Image
from palette import PALETTE

INK = PALETTE["INK"]
WHITE = PALETTE["WHITE"]
TRANSPARENT = PALETTE["TRANSPARENT"]
CREAM = PALETTE["CREAM"]

# --- PYRO FAMILY ---
def draw_pyrit(cell, frame, s):
    # s = 16. Pyrit: Cute fiery salamander/fox with flaming crest & cream belly
    pix = cell.load()
    c_orange = PALETTE["PYRO_ORANGE"]
    c_light = PALETTE["PYRO_LIGHT"]
    c_flame = PALETTE["PYRO_YELLOW"]
    bob = 1 if frame == 1 else 0
    atk = 1 if frame == 2 else 0

    # Body geometry
    for y in range(5 + bob, 14):
        for x in range(3, 13):
            dx = (x - 7.5) / 4.5
            dy = (y - (9.5 + bob)) / 4.0
            if dx*dx + dy*dy <= 1.0:
                pix[x, y] = c_orange

    # Cream belly
    for y in range(8 + bob, 13):
        for x in range(5, 11):
            dx = (x - 7.5) / 2.6
            dy = (y - (10.5 + bob)) / 2.4
            if dx*dx + dy*dy <= 1.0:
                pix[x, y] = CREAM

    # Flame crest on head
    fy = 2 + bob - atk
    pix[7, fy] = WHITE; pix[8, fy] = c_flame
    pix[6, fy + 1] = c_flame; pix[7, fy + 1] = c_light; pix[8, fy + 1] = c_orange
    pix[7, fy + 2] = c_orange; pix[8, fy + 2] = c_orange

    # Fiery tail on left
    pix[2, 9 + bob] = c_flame; pix[2, 10 + bob] = c_orange; pix[1, 10 + bob] = c_flame

    # Eyes with white catchlight
    ey = 7 + bob
    pix[5, ey] = INK; pix[5, ey - 1] = WHITE
    pix[10, ey] = INK; pix[10, ey - 1] = WHITE
    
    if frame == 2: # Attack open mouth / spark
        pix[7, 9 + bob] = INK; pix[8, 9 + bob] = INK
        pix[12, 8 + bob] = c_flame; pix[13, 8 + bob] = WHITE

    # Feet
    pix[4, 14] = INK; pix[5, 14] = c_orange
    pix[10, 14] = c_orange; pix[11, 14] = INK

def draw_pyrago(cell, frame, s):
    # s = 20. Pyrago: Stately fire beast with twin horns, flame mane & tail
    pix = cell.load()
    c_orange = PALETTE["PYRO_ORANGE"]
    c_light = PALETTE["PYRO_LIGHT"]
    c_flame = PALETTE["PYRO_YELLOW"]
    bob = 1 if frame == 1 else 0

    # Body
    for y in range(6 + bob, 17):
        for x in range(4, 16):
            dx = (x - 9.5) / 5.5
            dy = (y - (11.5 + bob)) / 5.0
            if dx*dx + dy*dy <= 1.0:
                pix[x, y] = c_orange

    # Light chest
    for y in range(10 + bob, 16):
        for x in range(6, 14):
            dx = (x - 9.5) / 3.5
            dy = (y - (13.0 + bob)) / 3.0
            if dx*dx + dy*dy <= 1.0:
                pix[x, y] = c_light

    # Twin flame horns
    pix[5, 3 + bob] = c_flame; pix[6, 4 + bob] = c_light
    pix[14, 3 + bob] = c_flame; pix[13, 4 + bob] = c_light
    pix[5, 2 + bob] = WHITE; pix[14, 2 + bob] = WHITE

    # Center mane flame
    pix[9, 3 + bob] = c_flame; pix[10, 3 + bob] = c_flame
    pix[9, 2 + bob] = WHITE

    # Tail flame
    pix[2, 11 + bob] = c_flame; pix[1, 10 + bob] = WHITE

    # Eyes
    ey = 8 + bob
    pix[6, ey] = INK; pix[6, ey - 1] = WHITE
    pix[13, ey] = INK; pix[13, ey - 1] = WHITE
    if frame == 2:
        pix[9, 10 + bob] = INK; pix[10, 10 + bob] = INK
        pix[15, 11 + bob] = c_flame; pix[16, 11 + bob] = WHITE

    # Claws
    for fx in [5, 8, 12, 14]:
        pix[fx, 17] = INK

def draw_infernox(cell, frame, s):
    # s = 24. Infernox: Colossal fire dragon-beast with flaming wings & obsidian horns
    pix = cell.load()
    c_orange = PALETTE["PYRO_ORANGE"]
    c_light = PALETTE["PYRO_LIGHT"]
    c_flame = PALETTE["PYRO_YELLOW"]
    bob = 1 if frame == 1 else 0

    # Flaming wings (left & right)
    for wy in range(5 + bob, 14 + bob):
        for wx in range(1, 6):
            if (wx - 3)**2 + (wy - (9 + bob))**2 <= 9:
                pix[wx, wy] = c_flame if wx <= 2 else c_orange
        for wx in range(18, 23):
            if (wx - 20)**2 + (wy - (9 + bob))**2 <= 9:
                pix[wx, wy] = c_flame if wx >= 21 else c_orange

    # Dragon Body
    for y in range(7 + bob, 22):
        for x in range(5, 19):
            dx = (x - 11.5) / 6.8
            dy = (y - (14.5 + bob)) / 7.0
            if dx*dx + dy*dy <= 1.0:
                pix[x, y] = c_orange

    # Fiery underbelly core
    for y in range(12 + bob, 21):
        for x in range(7, 17):
            dx = (x - 11.5) / 4.2
            dy = (y - (16.5 + bob)) / 4.2
            if dx*dx + dy*dy <= 1.0:
                pix[x, y] = c_light
                if dx*dx + dy*dy <= 0.4:
                    pix[x, y] = c_flame

    # Crown of fire & Obsidian horns
    pix[7, 3 + bob] = INK; pix[8, 4 + bob] = INK # Horn L
    pix[16, 3 + bob] = INK; pix[15, 4 + bob] = INK # Horn R
    pix[10, 3 + bob] = c_flame; pix[11, 2 + bob] = WHITE; pix[12, 2 + bob] = WHITE; pix[13, 3 + bob] = c_flame

    # Dragon Eyes
    ey = 9 + bob
    pix[8, ey] = INK; pix[8, ey - 1] = WHITE
    pix[15, ey] = INK; pix[15, ey - 1] = WHITE
    if frame == 2: # Fire breath
        pix[11, 12 + bob] = c_flame; pix[12, 12 + bob] = WHITE
        pix[13, 12 + bob] = c_flame

    # Dragon Feet & Claws
    for cx in [6, 8, 15, 17]:
        pix[cx, 22] = INK

# --- AQUA FAMILY ---
def draw_blubi(cell, frame, s):
    # s = 16. Blubi: Cute sapphire water droplet with glistening catchlight
    pix = cell.load()
    c_dark = PALETTE["AQUA_BLUE_DARK"]
    c_light = PALETTE["AQUA_BLUE_LIGHT"]
    bob = 1 if frame == 1 else 0

    # Teardrop body
    for y in range(4 + bob, 15):
        for x in range(3, 13):
            dx = (x - 7.5) / 4.5
            w_fac = max(0.4, (y - (3 + bob)) / 8.0)
            if (dx/w_fac)**2 + ((y - (10 + bob))/4.5)**2 <= 1.0:
                pix[x, y] = c_light

    # Tip droplet & dorsal fin
    pix[7, 2 + bob] = c_light; pix[8, 2 + bob] = WHITE
    pix[2, 10 + bob] = c_dark; pix[13, 10 + bob] = c_dark

    # Glossy water belly highlight
    for y in range(9 + bob, 14):
        for x in range(5, 11):
            if ((x - 7.5)/2.6)**2 + ((y - (11 + bob))/2.2)**2 <= 1.0:
                pix[x, y] = WHITE

    # Cute big eyes with white shines
    ey = 7 + bob
    pix[5, ey] = INK; pix[5, ey - 1] = WHITE
    pix[10, ey] = INK; pix[10, ey - 1] = WHITE
    if frame == 2:
        pix[7, 9 + bob] = c_dark; pix[8, 9 + bob] = c_dark
        pix[14, 8 + bob] = c_light; pix[13, 7 + bob] = WHITE # Water bubble

def draw_finnox(cell, frame, s):
    # s = 20. Finnox: Sleek aquatic sea fox with wave fins & hydrodynamic tail
    pix = cell.load()
    c_dark = PALETTE["AQUA_BLUE_DARK"]
    c_light = PALETTE["AQUA_BLUE_LIGHT"]
    bob = 1 if frame == 1 else 0

    # Wave fins
    for fy in range(9 + bob, 14 + bob):
        pix[2, fy] = c_light; pix[3, fy] = c_dark
        pix[17, fy] = c_dark; pix[18, fy] = c_light

    # Body
    for y in range(6 + bob, 18):
        for x in range(4, 16):
            dx = (x - 9.5) / 5.5
            dy = (y - (12.0 + bob)) / 5.5
            if dx*dx + dy*dy <= 1.0:
                pix[x, y] = c_light

    # White ocean underbelly
    for y in range(10 + bob, 17):
        for x in range(6, 14):
            dx = (x - 9.5) / 3.6
            dy = (y - (13.5 + bob)) / 3.2
            if dx*dx + dy*dy <= 1.0:
                pix[x, y] = WHITE

    # Fin ears / Crest
    pix[6, 3 + bob] = c_dark; pix[7, 4 + bob] = c_light
    pix[13, 3 + bob] = c_dark; pix[12, 4 + bob] = c_light
    pix[9, 3 + bob] = WHITE; pix[10, 3 + bob] = c_light

    # Eyes
    ey = 8 + bob
    pix[6, ey] = INK; pix[6, ey - 1] = WHITE
    pix[13, ey] = INK; pix[13, ey - 1] = WHITE
    if frame == 2:
        pix[9, 10 + bob] = c_dark; pix[10, 10 + bob] = c_dark
        pix[16, 9 + bob] = WHITE; pix[17, 8 + bob] = c_light

    for fx in [5, 8, 11, 14]:
        pix[fx, 18] = c_dark

def draw_tidalord(cell, frame, s):
    # s = 24. Tidalord: Royal ocean leviathan titan with waterfall fins & tidal vortex core
    pix = cell.load()
    c_dark = PALETTE["AQUA_BLUE_DARK"]
    c_light = PALETTE["AQUA_BLUE_LIGHT"]
    bob = 1 if frame == 1 else 0

    # Tidal wing fins
    for wy in range(7 + bob, 17 + bob):
        for wx in range(1, 6):
            pix[wx, wy] = c_light if wx % 2 == 0 else c_dark
        for wx in range(18, 23):
            pix[wx, wy] = c_light if wx % 2 == 0 else c_dark

    # Titan Body
    for y in range(7 + bob, 22):
        for x in range(5, 19):
            dx = (x - 11.5) / 6.8
            dy = (y - (14.5 + bob)) / 7.0
            if dx*dx + dy*dy <= 1.0:
                pix[x, y] = c_dark

    # Glowing ocean vortex core
    for y in range(12 + bob, 21):
        for x in range(7, 17):
            dx = (x - 11.5) / 4.5
            dy = (y - (16.5 + bob)) / 4.2
            if dx*dx + dy*dy <= 1.0:
                pix[x, y] = c_light
                if dx*dx + dy*dy <= 0.4:
                    pix[x, y] = WHITE

    # Royal Ocean Crown
    pix[7, 3 + bob] = c_light; pix[8, 4 + bob] = c_dark
    pix[16, 3 + bob] = c_light; pix[15, 4 + bob] = c_dark
    pix[10, 3 + bob] = WHITE; pix[11, 2 + bob] = WHITE; pix[12, 2 + bob] = WHITE; pix[13, 3 + bob] = WHITE

    # Eyes
    ey = 9 + bob
    pix[8, ey] = INK; pix[8, ey - 1] = WHITE
    pix[15, ey] = INK; pix[15, ey - 1] = WHITE
    if frame == 2:
        pix[11, 11 + bob] = WHITE; pix[12, 11 + bob] = WHITE

    for cx in [6, 9, 14, 17]:
        pix[cx, 22] = INK

# --- SPROUT FAMILY ---
def draw_sprig(cell, frame, s):
    # s = 16. Sprig: Cute emerald sprout seedling with leaf ears & dewdrop
    pix = cell.load()
    c_dark = PALETTE["SPROUT_DARK"]
    c_light = PALETTE["SPROUT_LIGHT"]
    bob = 1 if frame == 1 else 0

    # Body
    for y in range(5 + bob, 14):
        for x in range(3, 13):
            dx = (x - 7.5) / 4.5
            dy = (y - (9.5 + bob)) / 4.2
            if dx*dx + dy*dy <= 1.0:
                pix[x, y] = c_light

    # Light tummy
    for y in range(9 + bob, 13):
        for x in range(5, 11):
            if ((x - 7.5)/2.6)**2 + ((y - (11 + bob))/2.0)**2 <= 1.0:
                pix[x, y] = CREAM

    # Sprout leaves on head
    pix[7, 2 + bob] = c_dark; pix[8, 2 + bob] = c_light
    pix[6, 3 + bob] = c_light; pix[9, 3 + bob] = c_light
    pix[7, 1 + bob] = WHITE # Morning dewdrop

    # Leaf ears
    pix[3, 5 + bob] = c_dark; pix[2, 4 + bob] = c_light
    pix[12, 5 + bob] = c_dark; pix[13, 4 + bob] = c_light

    # Eyes
    ey = 7 + bob
    pix[5, ey] = INK; pix[5, ey - 1] = WHITE
    pix[10, ey] = INK; pix[10, ey - 1] = WHITE
    if frame == 2:
        pix[7, 9 + bob] = INK; pix[8, 9 + bob] = INK
        pix[13, 7 + bob] = c_light; pix[14, 6 + bob] = WHITE

    pix[5, 14] = c_dark; pix[10, 14] = c_dark

def draw_bloomlet(cell, frame, s):
    # s = 20. Bloomlet: Blossom guardian with opening pink/red lotus on back
    pix = cell.load()
    c_dark = PALETTE["SPROUT_DARK"]
    c_light = PALETTE["SPROUT_LIGHT"]
    c_petal = PALETTE["ROOF_RED"]
    bob = 1 if frame == 1 else 0

    # Body
    for y in range(7 + bob, 18):
        for x in range(4, 16):
            dx = (x - 9.5) / 5.5
            dy = (y - (12.5 + bob)) / 5.2
            if dx*dx + dy*dy <= 1.0:
                pix[x, y] = c_light

    # Lotus Flower Blossom on back
    for by in range(2 + bob, 7 + bob):
        for bx in range(7, 13):
            if (bx - 9.5)**2 + (by - (4.5 + bob))**2 <= 7:
                pix[bx, by] = c_petal
    pix[9, 4 + bob] = WHITE; pix[10, 4 + bob] = PALETTE["GOLD"]

    # Leaf mantle collar
    for lx in [4, 7, 12, 15]:
        pix[lx, 7 + bob] = c_dark

    # Eyes
    ey = 9 + bob
    pix[6, ey] = INK; pix[6, ey - 1] = WHITE
    pix[13, ey] = INK; pix[13, ey - 1] = WHITE
    if frame == 2:
        pix[9, 11 + bob] = INK; pix[10, 11 + bob] = INK

    for fx in [5, 8, 11, 14]:
        pix[fx, 18] = c_dark

def draw_florazor(cell, frame, s):
    # s = 24. Florazor: Ancient jungle treant with razor-petal blades & thorny vine scythes
    pix = cell.load()
    c_dark = PALETTE["SPROUT_DARK"]
    c_light = PALETTE["SPROUT_LIGHT"]
    c_petal = PALETTE["ROOF_RED"]
    bob = 1 if frame == 1 else 0

    # Scythe Petal Blades
    for sy in range(5 + bob, 16 + bob):
        pix[2, sy] = c_petal; pix[3, sy] = c_light
        pix[21, sy] = c_petal; pix[20, sy] = c_light

    # Beast Treant Body
    for y in range(7 + bob, 22):
        for x in range(5, 19):
            dx = (x - 11.5) / 6.8
            dy = (y - (14.5 + bob)) / 7.0
            if dx*dx + dy*dy <= 1.0:
                pix[x, y] = c_dark

    # Floral core chest
    for y in range(12 + bob, 21):
        for x in range(7, 17):
            dx = (x - 11.5) / 4.2
            dy = (y - (16.5 + bob)) / 4.2
            if dx*dx + dy*dy <= 1.0:
                pix[x, y] = c_light
                if dx*dx + dy*dy <= 0.35:
                    pix[x, y] = PALETTE["GOLD"]

    # Crown of thorny leaves
    pix[8, 3 + bob] = c_light; pix[9, 2 + bob] = c_petal
    pix[15, 3 + bob] = c_light; pix[14, 2 + bob] = c_petal
    pix[11, 2 + bob] = WHITE; pix[12, 2 + bob] = WHITE

    # Eyes
    ey = 9 + bob
    pix[8, ey] = INK; pix[8, ey - 1] = WHITE
    pix[15, ey] = INK; pix[15, ey - 1] = WHITE
    if frame == 2:
        pix[11, 12 + bob] = c_petal; pix[12, 12 + bob] = c_petal

    for cx in [6, 9, 14, 17]:
        pix[cx, 22] = INK

# --- VOLT FAMILY ---
def draw_zapi(cell, frame, s):
    # s = 16. Zapi: Electric yellow pup with jagged lightning tail & glowing cheek spark sacs
    pix = cell.load()
    c_yellow = PALETTE["VOLT_YELLOW"]
    c_light = PALETTE["VOLT_LIGHT"]
    c_orange = PALETTE["PYRO_ORANGE"]
    bob = 1 if frame == 1 else 0

    # Body
    for y in range(5 + bob, 14):
        for x in range(3, 13):
            dx = (x - 7.5) / 4.5
            dy = (y - (9.5 + bob)) / 4.2
            if dx*dx + dy*dy <= 1.0:
                pix[x, y] = c_yellow

    # Lightning ears
    pix[4, 2 + bob] = INK; pix[4, 3 + bob] = c_light
    pix[11, 2 + bob] = INK; pix[11, 3 + bob] = c_light

    # Zigzag lightning tail on left
    pix[1, 7 + bob] = c_light; pix[2, 7 + bob] = c_yellow
    pix[2, 8 + bob] = c_yellow; pix[1, 9 + bob] = c_light

    # Red/Orange electric cheek sacs
    pix[4, 9 + bob] = c_orange; pix[11, 9 + bob] = c_orange

    # Eyes
    ey = 7 + bob
    pix[5, ey] = INK; pix[5, ey - 1] = WHITE
    pix[10, ey] = INK; pix[10, ey - 1] = WHITE
    if frame == 2: # Sparking attack
        pix[7, 9 + bob] = INK; pix[8, 9 + bob] = INK
        pix[13, 7 + bob] = WHITE; pix[14, 6 + bob] = c_light

    pix[5, 14] = INK; pix[10, 14] = INK

def draw_voltcat(cell, frame, s):
    # s = 20. Voltcat: Agile lightning lynx with crackling ears & thunder stripes
    pix = cell.load()
    c_yellow = PALETTE["VOLT_YELLOW"]
    c_light = PALETTE["VOLT_LIGHT"]
    c_dark = (160, 120, 15, 255)
    bob = 1 if frame == 1 else 0

    # Body
    for y in range(6 + bob, 17):
        for x in range(4, 16):
            dx = (x - 9.5) / 5.5
            dy = (y - (11.5 + bob)) / 5.2
            if dx*dx + dy*dy <= 1.0:
                pix[x, y] = c_yellow

    # Thunder stripes on back
    for sy in [8, 11, 14]:
        pix[5, sy + bob] = c_dark; pix[14, sy + bob] = c_dark

    # Lynx ear tufts with lightning sparks
    pix[5, 2 + bob] = WHITE; pix[6, 3 + bob] = c_light
    pix[14, 2 + bob] = WHITE; pix[13, 3 + bob] = c_light

    # Lightning Tail
    pix[2, 8 + bob] = WHITE; pix[1, 9 + bob] = c_light; pix[2, 10 + bob] = c_yellow

    # Eyes
    ey = 8 + bob
    pix[6, ey] = INK; pix[6, ey - 1] = WHITE
    pix[13, ey] = INK; pix[13, ey - 1] = WHITE
    if frame == 2:
        pix[9, 10 + bob] = INK; pix[10, 10 + bob] = INK
        pix[16, 9 + bob] = WHITE; pix[17, 8 + bob] = c_light

    for fx in [5, 8, 11, 14]:
        pix[fx, 17] = INK

def draw_raijugar(cell, frame, s):
    # s = 24. Raijugar: Ferocious Thunder Lord Tiger with massive curved horns & roaring lightning fangs (BOSS)
    pix = cell.load()
    c_yellow = PALETTE["VOLT_YELLOW"]
    c_light = PALETTE["VOLT_LIGHT"]
    c_stripe = INK
    c_aura = PALETTE["CRYSTAL_YELLOW"]
    bob = 1 if frame == 1 else 0

    # Electric Aura & Lightning Horns
    pix[5, 2 + bob] = WHITE; pix[6, 3 + bob] = c_light; pix[7, 4 + bob] = c_yellow
    pix[18, 2 + bob] = WHITE; pix[17, 3 + bob] = c_light; pix[16, 4 + bob] = c_yellow

    # Tiger Body
    for y in range(6 + bob, 22):
        for x in range(4, 20):
            dx = (x - 11.5) / 7.5
            dy = (y - (14.0 + bob)) / 7.5
            if dx*dx + dy*dy <= 1.0:
                pix[x, y] = c_yellow

    # Black Thunder Tiger Stripes
    for sy in [9, 12, 15, 18]:
        pix[5, sy + bob] = c_stripe; pix[6, sy + bob] = c_stripe
        pix[17, sy + bob] = c_stripe; pix[18, sy + bob] = c_stripe

    # White fur mane & chest
    for y in range(12 + bob, 21):
        for x in range(8, 16):
            if ((x - 11.5)/3.6)**2 + ((y - (16.0 + bob))/3.8)**2 <= 1.0:
                pix[x, y] = WHITE

    # Golden Fierce Eyes
    ey = 9 + bob
    pix[7, ey] = PALETTE["GOLD"]; pix[8, ey] = INK; pix[7, ey - 1] = WHITE
    pix[16, ey] = PALETTE["GOLD"]; pix[15, ey] = INK; pix[16, ey - 1] = WHITE
    
    # Saber fangs
    pix[9, 12 + bob] = WHITE; pix[14, 12 + bob] = WHITE
    if frame == 2: # Roaring Lightning Attack
        pix[10, 11 + bob] = INK; pix[11, 11 + bob] = INK; pix[12, 11 + bob] = INK; pix[13, 11 + bob] = INK
        pix[11, 12 + bob] = c_light; pix[12, 12 + bob] = WHITE
        # Sparks bursting from mouth
        pix[11, 13 + bob] = WHITE; pix[12, 13 + bob] = c_aura

    # Tiger Paws & Claws
    for cx in [6, 9, 14, 17]:
        pix[cx, 22] = INK

DRAW_FUNCS = {
    "Pyrit": draw_pyrit,
    "Pyrago": draw_pyrago,
    "Infernox": draw_infernox,
    "Blubi": draw_blubi,
    "Finnox": draw_finnox,
    "Tidalord": draw_tidalord,
    "Sprig": draw_sprig,
    "Bloomlet": draw_bloomlet,
    "Florazor": draw_florazor,
    "Zapi": draw_zapi,
    "Voltcat": draw_voltcat,
    "Raijugar": draw_raijugar,
}

MONSTERS_INFO = [
    ("Pyro", 1, "Pyrit", 16),
    ("Pyro", 2, "Pyrago", 20),
    ("Pyro", 3, "Infernox", 24),
    ("Aqua", 1, "Blubi", 16),
    ("Aqua", 2, "Finnox", 20),
    ("Aqua", 3, "Tidalord", 24),
    ("Sprout", 1, "Sprig", 16),
    ("Sprout", 2, "Bloomlet", 20),
    ("Sprout", 3, "Florazor", 24),
    ("Volt", 1, "Zapi", 16),
    ("Volt", 2, "Voltcat", 20),
    ("Volt", 3, "Raijugar", 24),
]

def make_hurt_frame(base_cell, s):
    hurt = Image.new("RGBA", (s, s), TRANSPARENT)
    hpix = hurt.load()
    bpix = base_cell.load()
    for y in range(s):
        for x in range(s):
            col = bpix[x, y]
            if col[3] > 0:
                hpix[x, y] = WHITE
    return hurt

def generate_all_monsters(out_dir):
    os.makedirs(out_dir, exist_ok=True)
    results = {}
    for elem, stage, name, s in MONSTERS_INFO:
        filename = f"{elem}_{stage}_{name}_{s}x{s}.png"
        sheet_w = s * 4
        sheet_h = s
        sheet = Image.new("RGBA", (sheet_w, sheet_h), TRANSPARENT)
        
        draw_func = DRAW_FUNCS[name]
        
        c0 = Image.new("RGBA", (s, s), TRANSPARENT)
        draw_func(c0, 0, s)
        sheet.paste(c0, (0 * s, 0))
        
        c1 = Image.new("RGBA", (s, s), TRANSPARENT)
        draw_func(c1, 1, s)
        sheet.paste(c1, (1 * s, 0))
        
        c2 = Image.new("RGBA", (s, s), TRANSPARENT)
        draw_func(c2, 2, s)
        sheet.paste(c2, (2 * s, 0))
        
        c3 = make_hurt_frame(c0, s)
        sheet.paste(c3, (3 * s, 0))
        
        path = os.path.join(out_dir, filename)
        sheet.save(path)
        results[filename] = path
        
    return results

if __name__ == "__main__":
    generate_all_monsters("temp_monsters")
    print("Monsters generated.")
