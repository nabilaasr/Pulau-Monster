"""
gen_characters.py - High-fidelity 16x16 chibi character sprite sheet generator for Pulau Monster.
Generates 5 character sheets (48x64):
Player, NPC_Elder, NPC_Girl, NPC_Guard, NPC_Merchant.
Each sheet is 3 cols x 4 rows of 16x16 cells:
Rows: Down (0..2), Left (3..5), Right (6..8), Up (9..11)
Cols: Idle (0), Step A (1), Step B (2)
"""

import os
from PIL import Image
from palette import PALETTE

INK = PALETTE["INK"]
WHITE = PALETTE["WHITE"]
CREAM = PALETTE["CREAM"]
TRANSPARENT = PALETTE["TRANSPARENT"]
GOLD = PALETTE["GOLD"]

# Vibrant natural skin tones
SKIN_BASE = (252, 214, 182, 255)
SKIN_SHADOW = (226, 172, 138, 255)
SKIN_BLUSH = (255, 170, 160, 255)

def create_blank_sheet():
    return Image.new("RGBA", (48, 64), TRANSPARENT)

def draw_chibi_character(sheet, char_type):
    for row in range(4): # 0: Down, 1: Left, 2: Right, 3: Up
        for col in range(3): # 0: Idle, 1: StepA, 2: StepB
            cell = Image.new("RGBA", (16, 16), TRANSPARENT)
            pixels = cell.load()
            
            is_down = (row == 0)
            is_left = (row == 1)
            is_right = (row == 2)
            is_up = (row == 3)
            
            step = col # 0: idle, 1: step A, 2: step B
            
            # Palette config for this character
            if char_type == 'player':
                hair_color = (115, 60, 30, 255)
                hair_light = (165, 95, 55, 255)
                shirt_color = (225, 50, 50, 255)
                shirt_dark = (165, 30, 30, 255)
                pants_color = (48, 70, 140, 255)
                shoe_color = (75, 48, 30, 255)
            elif char_type == 'elder':
                hair_color = (240, 240, 250, 255)
                hair_light = WHITE
                shirt_color = (80, 60, 135, 255)
                shirt_dark = (52, 38, 92, 255)
                pants_color = (65, 50, 110, 255)
                shoe_color = (45, 32, 68, 255)
            elif char_type == 'girl':
                hair_color = (85, 45, 28, 255)
                hair_light = (135, 78, 52, 255)
                shirt_color = (52, 175, 120, 255)
                shirt_dark = (32, 130, 85, 255)
                pants_color = (245, 225, 190, 255)
                shoe_color = (135, 70, 45, 255)
            elif char_type == 'guard':
                hair_color = (145, 155, 172, 255)
                hair_light = (205, 215, 230, 255)
                shirt_color = (175, 185, 202, 255)
                shirt_dark = (115, 125, 140, 255)
                pants_color = (78, 84, 96, 255)
                shoe_color = (52, 58, 66, 255)
            elif char_type == 'merchant':
                hair_color = (150, 92, 48, 255)
                hair_light = (200, 130, 72, 255)
                shirt_color = (215, 150, 60, 255)
                shirt_dark = (160, 105, 38, 255)
                pants_color = (90, 68, 50, 255)
                shoe_color = (60, 45, 32, 255)

            # --- 1. Legs & Walking Physics ---
            if is_down or is_up:
                # Left leg x: 5..6, Right leg x: 9..10
                ly = 13; ry = 13
                if step == 1: ly = 12; ry = 14
                elif step == 2: ly = 14; ry = 12
                
                # Pants
                for y in range(11, 14):
                    pixels[5, y] = pants_color; pixels[6, y] = pants_color
                    pixels[9, y] = pants_color; pixels[10, y] = pants_color
                # Boots
                pixels[5, 14] = shoe_color; pixels[6, 14] = shoe_color
                pixels[9, 14] = shoe_color; pixels[10, 14] = shoe_color
                pixels[5, 15] = INK; pixels[6, 15] = INK
                pixels[9, 15] = INK; pixels[10, 15] = INK
                if step == 1:
                    pixels[5, 13] = shoe_color
                elif step == 2:
                    pixels[10, 13] = shoe_color
            else: # Side view (Left or Right)
                sx = 7 if is_right else 8
                ox = 1 if is_right else -1
                for y in range(11, 14):
                    pixels[sx, y] = pants_color
                    pixels[sx + ox, y] = pants_color
                if step == 0:
                    pixels[sx, 14] = shoe_color; pixels[sx + ox, 14] = shoe_color
                    pixels[sx, 15] = INK; pixels[sx + ox, 15] = INK
                elif step == 1:
                    pixels[sx + ox*2, 14] = shoe_color; pixels[sx + ox*2, 15] = INK
                    pixels[sx, 13] = shoe_color
                else:
                    pixels[sx - ox, 14] = shoe_color; pixels[sx - ox, 15] = INK
                    pixels[sx, 14] = shoe_color

            # --- 2. Torso / Shirt / Belt ---
            for y in range(8, 12):
                for x in range(5, 11):
                    pixels[x, y] = shirt_color
            # Belt & shading
            for x in range(5, 11):
                pixels[x, 11] = INK
            if char_type == 'player':
                pixels[7, 11] = GOLD; pixels[8, 11] = GOLD # Gold belt buckle
            elif char_type == 'elder':
                pixels[7, 9] = GOLD; pixels[8, 9] = GOLD   # Holy amulet
            for x in range(5, 11):
                pixels[x, 12] = shirt_dark

            # Arms
            arm_bob = -1 if step > 0 else 0
            if is_down:
                pixels[4, 9 + arm_bob] = shirt_color; pixels[4, 10 + arm_bob] = SKIN_BASE
                pixels[11, 9 + arm_bob] = shirt_color; pixels[11, 10 + arm_bob] = SKIN_BASE
            elif is_up:
                pixels[4, 9 + arm_bob] = shirt_dark; pixels[4, 10 + arm_bob] = shirt_dark
                pixels[11, 9 + arm_bob] = shirt_dark; pixels[11, 10 + arm_bob] = shirt_dark
            elif is_left:
                pixels[7, 9 + arm_bob] = shirt_color; pixels[7, 10 + arm_bob] = SKIN_BASE
            elif is_right:
                pixels[8, 9 + arm_bob] = shirt_color; pixels[8, 10 + arm_bob] = SKIN_BASE

            # --- 3. Head & Face ---
            for y in range(2, 8):
                for x in range(4, 12):
                    pixels[x, y] = SKIN_BASE
                    
            # Chin shading
            for x in range(5, 11):
                pixels[x, 8] = SKIN_SHADOW

            # Hair & Faces per direction
            if is_down:
                # Hair on top
                for y in range(1, 5):
                    for x in range(4, 12):
                        pixels[x, y] = hair_color
                # Hair highlights
                pixels[6, 2] = hair_light; pixels[7, 2] = hair_light; pixels[8, 2] = hair_light
                # Hair bangs
                pixels[4, 5] = hair_color; pixels[5, 5] = hair_color
                pixels[10, 5] = hair_color; pixels[11, 5] = hair_color
                
                # Eyes with shiny catchlights
                pixels[5, 6] = INK; pixels[5, 5] = WHITE
                pixels[10, 6] = INK; pixels[10, 5] = WHITE
                
                # Cheeks blush
                pixels[4, 7] = SKIN_BLUSH; pixels[11, 7] = SKIN_BLUSH
                
                if char_type == 'player':
                    # Red adventurer cap feather
                    pixels[4, 1] = GOLD; pixels[5, 1] = WHITE
                elif char_type == 'elder':
                    # Flowing white beard
                    for y in range(7, 10):
                        for x in range(6, 10):
                            pixels[x, y] = WHITE
                    pixels[7, 10] = WHITE; pixels[8, 10] = WHITE
                elif char_type == 'girl':
                    # Flower hairpin on left
                    pixels[3, 3] = (255, 120, 140, 255); pixels[4, 3] = WHITE
                    # Side hair twintails
                    pixels[3, 6] = hair_color; pixels[3, 7] = hair_color
                    pixels[12, 6] = hair_color; pixels[12, 7] = hair_color
                elif char_type == 'guard':
                    # Steel helm with glowing eye visor & crimson plume
                    for y in range(1, 5):
                        for x in range(4, 12): pixels[x, y] = hair_color
                    pixels[7, 0] = (230, 40, 40, 255); pixels[8, 0] = (230, 40, 40, 255) # Plume
                    for x in range(5, 11): pixels[x, 5] = INK; pixels[x, 6] = (45, 52, 64, 255)
                    pixels[6, 6] = (120, 220, 255, 255); pixels[9, 6] = (120, 220, 255, 255) # Glowing blue visor slit
                elif char_type == 'merchant':
                    # Merchant wide feathered hat
                    for x in range(3, 13): pixels[x, 3] = (175, 110, 55, 255)
                    pixels[7, 2] = (215, 150, 75, 255); pixels[8, 2] = (215, 150, 75, 255)
                    pixels[9, 1] = WHITE # Feather

            elif is_up: # Back view
                for y in range(1, 7):
                    for x in range(4, 12):
                        pixels[x, y] = hair_color
                pixels[6, 2] = hair_light; pixels[7, 2] = hair_light
                if char_type == 'girl':
                    # Long braided hair down back
                    for y in range(7, 12):
                        pixels[7, y] = hair_color; pixels[8, y] = hair_color
                    pixels[7, 12] = (255, 120, 140, 255) # Hair tie
                elif char_type == 'guard':
                    pixels[7, 0] = (230, 40, 40, 255); pixels[8, 0] = (230, 40, 40, 255)
                elif char_type == 'merchant':
                    # Huge explorer backpack
                    for by in range(8, 13):
                        for bx in range(5, 11):
                            pixels[bx, by] = (120, 80, 40, 255)
                    pixels[6, 8] = GOLD; pixels[9, 8] = (80, 180, 240, 255) # Potion bottle

            elif is_left or is_right: # Profile
                fx = 5 if is_left else 10
                bx = 10 if is_left else 5
                for y in range(1, 6):
                    for x in range(4, 12):
                        pixels[x, y] = hair_color
                pixels[fx, 5] = SKIN_BASE; pixels[fx, 6] = SKIN_BASE
                pixels[fx, 6] = INK; pixels[fx, 5] = WHITE # Profile eye
                pixels[fx, 7] = SKIN_BLUSH

            # Paste into character spritesheet
            sheet.paste(cell, (col * 16, row * 16))

def generate_all_characters(out_dir):
    os.makedirs(out_dir, exist_ok=True)
    chars = ["player", "elder", "girl", "guard", "merchant"]
    filenames = {
        "player": "Player_Walk_16x16.png",
        "elder": "NPC_Elder_16x16.png",
        "girl": "NPC_Girl_16x16.png",
        "guard": "NPC_Guard_16x16.png",
        "merchant": "NPC_Merchant_16x16.png"
    }
    paths = {}
    for c in chars:
        sheet = create_blank_sheet()
        draw_chibi_character(sheet, c)
        p = os.path.join(out_dir, filenames[c])
        sheet.save(p)
        paths[filenames[c]] = p
    return paths

if __name__ == "__main__":
    generate_all_characters("temp_chars")
    print("Characters generated.")
