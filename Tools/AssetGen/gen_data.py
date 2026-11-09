"""
gen_data.py - Generates MonsterDatabase.csv and AssetManifest.json
"""

import os
import csv
import json

CSV_HEADER = [
    "id", "name", "element", "stage", "baseHP", "baseATK", "baseDEF", "baseSPD",
    "skillName", "evolvesTo", "evolveLevel", "strongAgainst"
]

MONSTERS_DATA = [
    ["MON_001", "Pyrit", "Pyro", 1, 40, 14, 10, 10, "Semburan Api", "Pyrago", 5, "Sprout"],
    ["MON_002", "Pyrago", "Pyro", 2, 62, 21, 15, 14, "Semburan Api", "Infernox", 9, "Sprout"],
    ["MON_003", "Infernox", "Pyro", 3, 88, 30, 22, 18, "Semburan Api", "", 0, "Sprout"],
    ["MON_004", "Blubi", "Aqua", 1, 44, 12, 12, 9, "Tembakan Air", "Finnox", 5, "Pyro"],
    ["MON_005", "Finnox", "Aqua", 2, 66, 18, 18, 13, "Tembakan Air", "Tidalord", 9, "Pyro"],
    ["MON_006", "Tidalord", "Aqua", 3, 94, 25, 27, 16, "Tembakan Air", "", 0, "Pyro"],
    ["MON_007", "Sprig", "Sprout", 1, 42, 13, 11, 9, "Daun Tajam", "Bloomlet", 5, "Aqua"],
    ["MON_008", "Bloomlet", "Sprout", 2, 63, 19, 16, 13, "Daun Tajam", "Florazor", 9, "Aqua"],
    ["MON_009", "Florazor", "Sprout", 3, 90, 27, 24, 17, "Daun Tajam", "", 0, "Aqua"],
    ["MON_010", "Zapi", "Volt", 1, 36, 13, 9, 13, "Setrum Kilat", "Voltcat", 5, "Aqua"],
    ["MON_011", "Voltcat", "Volt", 2, 54, 20, 13, 19, "Setrum Kilat", "Raijugar", 9, "Aqua"],
    ["MON_012", "Raijugar", "Volt", 3, 78, 29, 19, 26, "Setrum Kilat", "", 0, "Aqua"],
]

TILES_NAME_INDEX = {
    "Path_TL": 0, "Path_T": 1, "Path_TR": 2, "Path_Inner_TL": 3, "Path_Inner_TR": 4,
    "Water_TL": 5, "Water_T": 6, "Water_TR": 7, "Water_Inner_TL": 8, "Water_Inner_TR": 9,
    "Grass_A": 10, "Grass_B": 11,
    "Path_L": 12, "Path_Center": 13, "Path_R": 14, "Path_Inner_BL": 15, "Path_Inner_BR": 16,
    "Water_L": 17, "Water_Center": 18, "Water_R": 19, "Water_Inner_BL": 20, "Water_Inner_BR": 21,
    "Flowers": 22, "SmallRock": 23,
    "Path_BL": 24, "Path_B": 25, "Path_BR": 26, "Sand": 27, "Sand_Light": 28,
    "Water_BL": 29, "Water_B": 30, "Water_BR": 31, "Sign": 32, "Bush": 33,
    "TallGrass_A": 34, "TallGrass_B": 35,
    "Fence_H": 36, "Fence_V": 37, "Fence_Post": 38,
    "Tree_Top": 39, "Tree_Bottom": 40, "Tree_Autumn_Top": 41, "Tree_Autumn_Bottom": 42,
    "SaveStone": 43, "Cave_Floor": 44, "Cave_Entrance": 45, "Crystal": 46, "Cave_Wall_Top": 47,
    "Cave_Wall_Bottom": 48, "Cave_Wall_Left": 49, "Cave_Wall_Right": 50,
    "Cave_Wall_Corner_TL": 51, "Cave_Wall_Corner_TR": 52, "Cave_Wall_Corner_BL": 53, "Cave_Wall_Corner_BR": 54,
    "Cave_Wall_Solid": 55, "House_00": 56, "House_01": 57, "House_02": 58, "House_10": 59,
    "House_11": 60, "House_12": 61, "House_20": 62, "House_21": 63, "House_22": 64,
    "Pond_Edge": 65, "Grass_C": 66, "Cave_Torch": 67, "Cave_Rubble": 68, "Path_Variant": 69,
    "Tree_Small": 70, "Decor_Stump": 71
}

def generate_csv(out_path):
    with open(out_path, "w", newline="", encoding="utf-8") as f:
        writer = csv.writer(f)
        writer.writerow(CSV_HEADER)
        writer.writerows(MONSTERS_DATA)

def generate_manifest(out_path):
    manifest = {
        "sheets": [
            # Characters
            {
                "path": "Characters/Player_Walk_16x16.png",
                "cellW": 16, "cellH": 16, "cols": 3, "rows": 4,
                "pattern": "Player_Walk_16x16_{index}",
                "pivot": [0.5, 0.0] # Bottom
            },
            {
                "path": "Characters/NPC_Elder_16x16.png",
                "cellW": 16, "cellH": 16, "cols": 3, "rows": 4,
                "pattern": "NPC_Elder_16x16_{index}",
                "pivot": [0.5, 0.0]
            },
            {
                "path": "Characters/NPC_Girl_16x16.png",
                "cellW": 16, "cellH": 16, "cols": 3, "rows": 4,
                "pattern": "NPC_Girl_16x16_{index}",
                "pivot": [0.5, 0.0]
            },
            {
                "path": "Characters/NPC_Guard_16x16.png",
                "cellW": 16, "cellH": 16, "cols": 3, "rows": 4,
                "pattern": "NPC_Guard_16x16_{index}",
                "pivot": [0.5, 0.0]
            },
            {
                "path": "Characters/NPC_Merchant_16x16.png",
                "cellW": 16, "cellH": 16, "cols": 3, "rows": 4,
                "pattern": "NPC_Merchant_16x16_{index}",
                "pivot": [0.5, 0.0]
            },
            # Monsters
            {
                "path": "Monsters/Pyro_1_Pyrit_16x16.png",
                "cellW": 16, "cellH": 16, "cols": 4, "rows": 1,
                "pattern": "Pyro_1_Pyrit_16x16_{index}",
                "pivot": [0.5, 0.0]
            },
            {
                "path": "Monsters/Pyro_2_Pyrago_20x20.png",
                "cellW": 20, "cellH": 20, "cols": 4, "rows": 1,
                "pattern": "Pyro_2_Pyrago_20x20_{index}",
                "pivot": [0.5, 0.0]
            },
            {
                "path": "Monsters/Pyro_3_Infernox_24x24.png",
                "cellW": 24, "cellH": 24, "cols": 4, "rows": 1,
                "pattern": "Pyro_3_Infernox_24x24_{index}",
                "pivot": [0.5, 0.0]
            },
            {
                "path": "Monsters/Aqua_1_Blubi_16x16.png",
                "cellW": 16, "cellH": 16, "cols": 4, "rows": 1,
                "pattern": "Aqua_1_Blubi_16x16_{index}",
                "pivot": [0.5, 0.0]
            },
            {
                "path": "Monsters/Aqua_2_Finnox_20x20.png",
                "cellW": 20, "cellH": 20, "cols": 4, "rows": 1,
                "pattern": "Aqua_2_Finnox_20x20_{index}",
                "pivot": [0.5, 0.0]
            },
            {
                "path": "Monsters/Aqua_3_Tidalord_24x24.png",
                "cellW": 24, "cellH": 24, "cols": 4, "rows": 1,
                "pattern": "Aqua_3_Tidalord_24x24_{index}",
                "pivot": [0.5, 0.0]
            },
            {
                "path": "Monsters/Sprout_1_Sprig_16x16.png",
                "cellW": 16, "cellH": 16, "cols": 4, "rows": 1,
                "pattern": "Sprout_1_Sprig_16x16_{index}",
                "pivot": [0.5, 0.0]
            },
            {
                "path": "Monsters/Sprout_2_Bloomlet_20x20.png",
                "cellW": 20, "cellH": 20, "cols": 4, "rows": 1,
                "pattern": "Sprout_2_Bloomlet_20x20_{index}",
                "pivot": [0.5, 0.0]
            },
            {
                "path": "Monsters/Sprout_3_Florazor_24x24.png",
                "cellW": 24, "cellH": 24, "cols": 4, "rows": 1,
                "pattern": "Sprout_3_Florazor_24x24_{index}",
                "pivot": [0.5, 0.0]
            },
            {
                "path": "Monsters/Volt_1_Zapi_16x16.png",
                "cellW": 16, "cellH": 16, "cols": 4, "rows": 1,
                "pattern": "Volt_1_Zapi_16x16_{index}",
                "pivot": [0.5, 0.0]
            },
            {
                "path": "Monsters/Volt_2_Voltcat_20x20.png",
                "cellW": 20, "cellH": 20, "cols": 4, "rows": 1,
                "pattern": "Volt_2_Voltcat_20x20_{index}",
                "pivot": [0.5, 0.0]
            },
            {
                "path": "Monsters/Volt_3_Raijugar_24x24.png",
                "cellW": 24, "cellH": 24, "cols": 4, "rows": 1,
                "pattern": "Volt_3_Raijugar_24x24_{index}",
                "pivot": [0.5, 0.0]
            },
            # Tileset
            {
                "path": "Tiles/Tileset_Overworld.png",
                "cellW": 16, "cellH": 16, "cols": 12, "rows": 6,
                "pattern": "Tileset_Overworld_{index}",
                "pivot": [0.5, 0.5] # Center
            },
            # VFX
            {
                "path": "Vfx/VFX_Slash.png",
                "cellW": 32, "cellH": 32, "cols": 4, "rows": 1,
                "pattern": "VFX_Slash_{index}",
                "pivot": [0.5, 0.5]
            }
        ],
        "single_sprites": [
            { "path": "UI/UI_Panel_Navy.png", "border": 4 },
            { "path": "UI/UI_Panel_Gold.png", "border": 4 },
            { "path": "UI/UI_Panel_Narr.png", "border": 4 },
            { "path": "UI/UI_Pixel.png", "border": 0 },
            { "path": "UI/UI_Ring.png", "border": 0 },
            { "path": "UI/UI_Alert.png", "border": 0 },
            { "path": "Backgrounds/BG_Battle_Grassland.png", "border": 0 },
            { "path": "Backgrounds/BG_Battle_Forest.png", "border": 0 },
            { "path": "Backgrounds/BG_Battle_Cave.png", "border": 0 },
            { "path": "Backgrounds/BG_Title_World.png", "border": 0 }
        ],
        "tile_names": TILES_NAME_INDEX
    }
    
    with open(out_path, "w", encoding="utf-8") as f:
        json.dump(manifest, f, indent=2)

def generate_all_data(out_dir):
    os.makedirs(out_dir, exist_ok=True)
    csv_path = os.path.join(out_dir, "MonsterDatabase.csv")
    json_path = os.path.join(out_dir, "AssetManifest.json")
    generate_csv(csv_path)
    generate_manifest(json_path)
    return { "MonsterDatabase.csv": csv_path, "AssetManifest.json": json_path }

if __name__ == "__main__":
    generate_all_data("temp_data")
    print("Data generated.")
