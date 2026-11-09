"""
build_all.py - Master asset generation entry point for Pulau Monster.
Generates all pixel art, audio, data, and fonts into:
Assets/_Project/ThirdParty/StarterAssetPack/
Creates out_preview/contact_sheet.png and out_preview/tileset_preview.png.
Performs strict automated validation according to Fase 0 specifications.
"""

import os
import sys
import wave
from PIL import Image, ImageDraw, ImageFont

# Local generator modules
import palette
from gen_characters import generate_all_characters
from gen_monsters import generate_all_monsters
from gen_tiles import generate_tileset
from gen_ui import generate_all_ui
from gen_backgrounds import generate_all_backgrounds
from gen_vfx import generate_all_vfx
from gen_audio import generate_all_audio
from gen_fonts import generate_fonts
from gen_data import generate_all_data

ROOT_DIR = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
OUTPUT_DIR = os.path.join(ROOT_DIR, "Assets", "_Project", "ThirdParty", "StarterAssetPack")
PREVIEW_DIR = os.path.join(ROOT_DIR, "Tools", "AssetGen", "out_preview")

LICENSE_TEXT = """Pulau Monster Asset Pack
Dibuat sendiri oleh tim, bebas dipakai.
Lisensi: Bebas digunakan untuk keperluan praktikum dan pengembangan game Pulau Monster.
"""

def generate_previews(tileset_path, char_files, monster_files):
    os.makedirs(PREVIEW_DIR, exist_ok=True)
    
    # 1. Tileset preview (scaled 4x nearest)
    t_img = Image.open(tileset_path)
    t_preview = t_img.resize((t_img.width * 4, t_img.height * 4), Image.Resampling.NEAREST)
    t_preview_path = os.path.join(PREVIEW_DIR, "tileset_preview.png")
    t_preview.save(t_preview_path)
    
    # 2. Contact sheet (characters + monsters enlarged 4x)
    cs_w = 640
    cs_h = 720
    cs = Image.new("RGBA", (cs_w, cs_h), (20, 18, 36, 255))
    draw = ImageDraw.Draw(cs)
    
    # Title
    draw.text((20, 15), "PULAU MONSTER - STARTER ASSET PACK CONTACT SHEET (4x)", fill=(255, 198, 26, 255))
    
    # Characters row
    y_cursor = 50
    draw.text((20, y_cursor), "CHARACTERS (Idle Down Frame):", fill=(237, 233, 247, 255))
    y_cursor += 25
    x_cursor = 20
    for name, path in char_files.items():
        c_img = Image.open(path)
        # Crop frame 0 (down idle)
        frame0 = c_img.crop((0, 0, 16, 16)).resize((64, 64), Image.Resampling.NEAREST)
        cs.paste(frame0, (x_cursor, y_cursor), frame0)
        draw.text((x_cursor, y_cursor + 68), name.split("_")[1], fill=(169, 163, 201, 255))
        x_cursor += 100
        
    # Monsters rows
    y_cursor += 105
    draw.text((20, y_cursor), "MONSTERS (12 Species, 4 Frames each):", fill=(237, 233, 247, 255))
    y_cursor += 25
    
    m_list = list(monster_files.items())
    for i, (name, path) in enumerate(m_list):
        m_img = Image.open(path)
        m_h = m_img.height
        frame0 = m_img.crop((0, 0, m_h, m_h)).resize((48, 48), Image.Resampling.NEAREST)
        row = i // 4
        col = i % 4
        px = 20 + col * 150
        py = y_cursor + row * 85
        cs.paste(frame0, (px, py), frame0)
        draw.text((px + 56, py + 16), name.split("_")[2], fill=(255, 214, 36, 255))
        
    cs_path = os.path.join(PREVIEW_DIR, "contact_sheet.png")
    cs.save(cs_path)
    return cs_path, t_preview_path

def validate_all_assets():
    errors = []
    
    # Check CSV
    csv_path = os.path.join(OUTPUT_DIR, "Data", "MonsterDatabase.csv")
    if not os.path.exists(csv_path):
        errors.append("MonsterDatabase.csv not found!")
    else:
        with open(csv_path, "r", encoding="utf-8") as f:
            lines = [line.strip() for line in f if line.strip()]
        if len(lines) != 13: # 1 header + 12 rows
            errors.append(f"CSV row count is {len(lines)}, expected 13")
        pyrit_line = lines[1]
        if "Pyrit,Pyro,1,40,14" not in pyrit_line:
            errors.append(f"Pyrit stat mismatch: {pyrit_line}")
            
    # Check Tileset
    tileset_path = os.path.join(OUTPUT_DIR, "Tiles", "Tileset_Overworld.png")
    if not os.path.exists(tileset_path):
        errors.append("Tileset_Overworld.png not found!")
    else:
        im = Image.open(tileset_path)
        if im.size != (192, 96):
            errors.append(f"Tileset size is {im.size}, expected (192, 96)")
            
    # Check Audio
    audio_dir = os.path.join(OUTPUT_DIR, "Audio")
    expected_bgms = ["BGM_Title.wav", "BGM_Village.wav", "BGM_Forest.wav", "BGM_Battle.wav", "BGM_Boss.wav", "BGM_Lake.wav", "BGM_Cave.wav"]
    for bgm in expected_bgms:
        path = os.path.join(audio_dir, bgm)
        if not os.path.exists(path):
            errors.append(f"Missing {bgm}")
        else:
            with wave.open(path, 'r') as wf:
                dur = wf.getnframes() / float(wf.getframerate())
                if dur < 20.0 or dur > 40.0:
                    errors.append(f"{bgm} duration is {dur:.1f}s, expected 20-40s")
                    
    # Check Characters
    char_dir = os.path.join(OUTPUT_DIR, "Characters")
    for c in ["Player_Walk_16x16.png", "NPC_Elder_16x16.png", "NPC_Girl_16x16.png", "NPC_Guard_16x16.png", "NPC_Merchant_16x16.png"]:
        path = os.path.join(char_dir, c)
        if not os.path.exists(path):
            errors.append(f"Missing character {c}")
        else:
            im = Image.open(path)
            if im.size != (48, 64):
                errors.append(f"Character {c} size is {im.size}, expected (48, 64)")
                
    # Check Monsters
    mon_dir = os.path.join(OUTPUT_DIR, "Monsters")
    expected_mons = [
        ("Pyro_1_Pyrit_16x16.png", (64, 16)),
        ("Pyro_2_Pyrago_20x20.png", (80, 20)),
        ("Pyro_3_Infernox_24x24.png", (96, 24)),
        ("Aqua_1_Blubi_16x16.png", (64, 16)),
        ("Aqua_2_Finnox_20x20.png", (80, 20)),
        ("Aqua_3_Tidalord_24x24.png", (96, 24)),
        ("Sprout_1_Sprig_16x16.png", (64, 16)),
        ("Sprout_2_Bloomlet_20x20.png", (80, 20)),
        ("Sprout_3_Florazor_24x24.png", (96, 24)),
        ("Volt_1_Zapi_16x16.png", (64, 16)),
        ("Volt_2_Voltcat_20x20.png", (80, 20)),
        ("Volt_3_Raijugar_24x24.png", (96, 24)),
    ]
    for m_file, expected_sz in expected_mons:
        path = os.path.join(mon_dir, m_file)
        if not os.path.exists(path):
            errors.append(f"Missing monster {m_file}")
        else:
            im = Image.open(path)
            if im.size != expected_sz:
                errors.append(f"Monster {m_file} size {im.size} != {expected_sz}")
                
    # Check Fonts
    font_dir = os.path.join(OUTPUT_DIR, "Fonts")
    for f in ["PixelifySans-Regular.ttf", "PixelifySans-Bold.ttf", "PixelifySans_OFL.txt"]:
        if not os.path.exists(os.path.join(font_dir, f)):
            errors.append(f"Missing font file {f}")
            
    # Check UI
    ui_dir = os.path.join(OUTPUT_DIR, "UI")
    for u in ["UI_Panel_Navy.png", "UI_Panel_Gold.png", "UI_Panel_Narr.png", "UI_Pixel.png", "UI_Ring.png", "UI_Alert.png"]:
        if not os.path.exists(os.path.join(ui_dir, u)):
            errors.append(f"Missing UI {u}")

    # Check Backgrounds
    bg_dir = os.path.join(OUTPUT_DIR, "Backgrounds")
    for bg, sz in [("BG_Battle_Forest.png", (320, 180)), ("BG_Battle_Grassland.png", (320, 180)), ("BG_Battle_Cave.png", (320, 180)), ("BG_Title_World.png", (480, 180))]:
        path = os.path.join(bg_dir, bg)
        if not os.path.exists(path):
            errors.append(f"Missing background {bg}")
        else:
            im = Image.open(path)
            if im.size != sz:
                errors.append(f"Background {bg} size {im.size} != {sz}")

    return errors

def main():
    print("=== PULAU MONSTER ASSET GENERATOR (FASE 0) ===")
    os.makedirs(OUTPUT_DIR, exist_ok=True)
    
    # Subdirectories
    subdirs = ["Characters", "Monsters", "Tiles", "UI", "Backgrounds", "Vfx", "Fonts", "Audio", "Data"]
    for sub in subdirs:
        os.makedirs(os.path.join(OUTPUT_DIR, sub), exist_ok=True)
        
    # Write License
    with open(os.path.join(OUTPUT_DIR, "LICENSE_ASSETS.txt"), "w", encoding="utf-8") as f:
        f.write(LICENSE_TEXT)
        
    print("1. Generating Characters...")
    char_files = generate_all_characters(os.path.join(OUTPUT_DIR, "Characters"))
    
    print("2. Generating Monsters...")
    mon_files = generate_all_monsters(os.path.join(OUTPUT_DIR, "Monsters"))
    
    print("3. Generating Tileset...")
    tileset_path = generate_tileset(os.path.join(OUTPUT_DIR, "Tiles"))
    
    print("4. Generating UI Assets...")
    generate_all_ui(os.path.join(OUTPUT_DIR, "UI"))
    
    print("5. Generating Backgrounds...")
    generate_all_backgrounds(os.path.join(OUTPUT_DIR, "Backgrounds"))
    
    print("6. Generating VFX...")
    generate_all_vfx(os.path.join(OUTPUT_DIR, "Vfx"))
    
    print("7. Generating Audio...")
    generate_all_audio(os.path.join(OUTPUT_DIR, "Audio"))
    
    print("8. Generating Fonts...")
    generate_fonts(os.path.join(OUTPUT_DIR, "Fonts"))
    
    print("9. Generating Data & Manifest...")
    generate_all_data(os.path.join(OUTPUT_DIR, "Data"))
    
    print("10. Generating Previews (contact_sheet & tileset_preview)...")
    generate_previews(tileset_path, char_files, mon_files)
    
    print("11. Validating all assets against Fase 0 criteria...")
    errs = validate_all_assets()
    if errs:
        print("VALIDATION ERRORS:")
        for e in errs:
            print(" -", e)
        sys.exit(1)
    else:
        print("ALL ASSETS VALIDATED SUCCESSFULLY! (0 errors)")

if __name__ == "__main__":
    main()
