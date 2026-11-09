"""
palette.py - Shared 32-color palette for Pulau Monster pixel art assets.
All sprites, backgrounds, tiles, and UI assets strictly use these colors.
"""

# Hex color dictionary
HEX_PALETTE = {
    # Dark outlines & UI panels
    "INK": "#121027",               # (18, 16, 39) Primary dark outline, text shadow
    "PANEL_NAVY": "#1C1540",        # (28, 21, 64) Standard dialogue & menu panel background
    "NARR_NAVY": "#120F26",         # (18, 15, 38) Narration panel background
    "DEEP_PURPLE": "#261C4E",       # (38, 28, 78) Cave dark shading
    "CAVE_WALL_DARK": "#2F2953",    # (47, 41, 83) Cave brick shadow
    "CAVE_WALL_MID": "#463F72",     # (70, 63, 114) Cave brick body
    "CAVE_WALL_LIGHT": "#655E94",   # (101, 94, 148) Cave brick highlight

    # Gold & UI Accents
    "GOLD": "#FFC61A",              # (255, 198, 26) Primary gold borders, cursor, level text
    "GOLD_DARK": "#D49B0D",         # (212, 155, 13) Gold border shading
    "CREAM": "#EDE9F7",             # (237, 233, 247) Primary text, light belly/faces
    "MUTED": "#A9A3C9",             # (169, 163, 201) Secondary text
    "DISABLED": "#5C5680",          # (92, 86, 128) Disabled menu items
    "NARR_BORDER": "#3B4896",       # (59, 72, 150) Narration panel blue border
    "WHITE": "#FFFFFF",             # (255, 255, 255) Flashes, highlights, hurt silhouettes
    "BLACK": "#000000",             # (0, 0, 0) Pure black (cutscenes, letterboxing)

    # Nature & Overworld Environment (Lush, clean, natural green meadow)
    "GRASS_DARK": "#489537",        # (72, 149, 55) Subtle blade base & shadow
    "GRASS_MID": "#5EB448",         # (94, 180, 72) Primary cohesive overworld grass carpet
    "GRASS_LIGHT": "#77C85E",       # (119, 200, 94) Soft sunlit blade highlight
    "SAND_DARK": "#C79659",         # (199, 150, 89) Path border / shadow
    "SAND_MID": "#E5BA7D",          # (229, 186, 125) Standard path base
    "SAND_LIGHT": "#F4D3A1",        # (244, 211, 161) Path highlights / pebble dots
    "WATER_DARK": "#2A67B2",        # (42, 103, 178) Deep water / water border
    "WATER_MID": "#438EE8",         # (67, 142, 232) Overworld lake water
    "WATER_LIGHT": "#7BC0FF",       # (123, 192, 255) Water surface sparkles / foam
    "WOOD_DARK": "#5B371B",         # (91, 55, 27) Tree trunk shadow, fence dark
    "WOOD_MID": "#8C532B",          # (140, 83, 43) Wooden fence, signposts
    "WOOD_LIGHT": "#BA7743",        # (186, 119, 67) Wood highlight
    "ROOF_RED": "#C93B3B",          # (201, 59, 59) Village house brick roof
    "ROOF_DARK": "#962121",         # (150, 33, 33) Roof shading
    "WALL_WHITE": "#F0EBE1",        # (240, 235, 225) Village house plaster walls
    "AUTUMN_ORANGE": "#D96B27",     # (217, 107, 39) Border trees autumn leaf

    # Cave Environment (Subterranean dark slate, basalt, and purple-charcoal stone)
    "CAVE_FLOOR_BASE": "#181527",   # (24, 21, 39) Deep subterranean bedrock
    "CAVE_FLOOR_STONE": "#231F38",  # (35, 31, 56) Chiseled cave rock surface
    "CAVE_FLOOR_CRACK": "#100E1C",  # (16, 14, 28) Deep crevice in cavern floor
    "CAVE_ROCK_DARK": "#1A172B",    # (26, 23, 43) Natural craggy rock shadow
    "CAVE_ROCK_MID": "#332D4C",     # (51, 45, 76) Natural cavern cliff body
    "CAVE_ROCK_LIGHT": "#4D466E",   # (77, 70, 110) Natural stone ledge highlight
    "CAVE_ROCK_ACCENT": "#665E8F",  # (102, 94, 143) Bluish-purple rock mineral sheen

    # Elements & FX
    "PYRO_ORANGE": "#E65A20",       # (230, 90, 32) Pyrit/Pyrago body
    "PYRO_LIGHT": "#FF9B38",        # (255, 155, 56) Fire highlights
    "PYRO_YELLOW": "#FFDC4B",       # (254, 220, 75) Fire tip, flame glow
    "AQUA_BLUE_DARK": "#1C69A8",    # (28, 105, 168) Blubi shadow, Tidalord
    "AQUA_BLUE_LIGHT": "#4AB3FF",   # (74, 179, 255) Water attack, Blubi highlight
    "SPROUT_DARK": "#2F8A43",       # (47, 138, 67) Sprig shadow, leaves
    "SPROUT_LIGHT": "#5CD672",      # (92, 214, 114) Plant petals, sprout tip
    "VOLT_YELLOW": "#FFD624",       # (255, 214, 36) Zapi / Raijugar body
    "VOLT_LIGHT": "#FFF176",        # (255, 241, 118) Spark glow, lightning
    "CRYSTAL_YELLOW": "#50E5FF",    # Remapped to Electric Cyan for full compatibility
    "CRYSTAL_BLUE": "#38B6FF",      # Electric Azure Blue
    "CRYSTAL_CORE": "#E5FDFF",      # Glowing electric white-cyan center
    "CRYSTAL_DARK": "#1268A6",      # Deep crystal refraction shadow
    "CRYSTAL_PURPLE": "#8A76FF",    # Electric bluish-purple variant
    "CRYSTAL_SPARK": "#FFF888",     # Tiny yellow-electric spark
    "HP_GREEN": "#43A047",          # (67, 160, 71) Health bar > 50%
    "HP_RED": "#E53935",            # (229, 57, 53) Health bar <= 20%
    "EXP_BLUE": "#6F9BFF",          # (111, 155, 255) EXP bar fill
}

def hex_to_rgba(hex_str, alpha=255):
    h = hex_str.lstrip("#")
    r, g, b = int(h[0:2], 16), int(h[2:4], 16), int(h[4:6], 16)
    return (r, g, b, alpha)

# Precomputed RGBA tuples
PALETTE = {k: hex_to_rgba(v) for k, v in HEX_PALETTE.items()}
PALETTE["TRANSPARENT"] = (0, 0, 0, 0)
