# Draws the sewer cleaner and his tool bag
# each letter is a color, . is see-through
# run it with: python3 make_cleaner.py Assets/Art
import sys, os
from PIL import Image

out = sys.argv[1]
os.makedirs(out, exist_ok=True)

PAL = {
    "O": "#434a5f",                                    # outline
    "H": "#f39c34", "h": "#c8701e", "L": "#ffc56b",    # hard hat
    "Y": "#fff36b",                                    # headlamp / buckle
    "S": "#f4cca1", "s": "#e2a87a", "E": "#22252f",    # face
    "B": "#2eb082", "b": "#22886a",                    # bandana
    "J": "#ffd23f", "j": "#e0a92a", "R": "#dce1e7",    # jacket
    "P": "#4f6b4a", "p": "#3e5752", "K": "#2a2f3d",    # pants, boots
    "C": "#3f7f8c", "c": "#2f5f6a", "A": "#8c5a3c",    # bag
    "M": "#b7c4c8", "r": "#7e3125", "X": "#dd442c", "W": "#c48a4a",  # wrench, plunger
}

IDLE = [
    "........OOOOOOOO........",
    "......OOLLHHHHHHOO......",
    ".....OLLHHHHHHHHHHO.....",
    "....OLHHHHHHHHHHhOYYO...",
    "....OHHHHHHHHHHhhOYYO...",
    "..OOOOOOOOOOOOOOOOOOOO..",
    "..OHHHHHHHHHHHHHHHHhhO..",
    "...OOOOOOOOOOOOOOOOOO...",
    "....OSSSSSSSSSSSSSSO....",
    "....OSSSSSSSSSSSSSSO....",
    "....OSSSSSEESSSSEESO....",
    "....OSSSSSEESSSSEESO....",
    "....OsSSSSSSSSSSSSSO....",
    "....OBBBBBBBBBBBBBBO....",
    "....OBBBBBBBBBBBBbbO....",
    "....ObbbbbbbbbbbbbbO....",
    "....OOOOOOOOOOOOOOOO....",
    "....OJJJJJJJJJJJJJjO....",
    "...KORRRRRRRRRRRRRROK...",
    "...KOJJJJJJJJJJJJJjOK...",
    "....OOOOOOOOOOOOOOOO....",
    ".....OPPPpO..OPPPpO.....",
    "....OKKKKKO..OKKKKKO....",
    "....OOOOOOO..OOOOOOO....",
]
# walking frame
STEP = ["." * 24] + IDLE[:21] + [
    "...OKKKKO....OKKKKO.....",
    "...OOOOOO....OOOOOO.....",
]

BAG = [
    "...........OOO....",
    "..OOOOO...OMOMO...",
    ".OXXXXXO..OMMMO...",
    ".OXXXXrO...OMO....",
    "..OOOOO....OMO....",
    "....OWO....OMO....",
    "..OOOWOOOOOOMOOO..",
    ".OCCCCCCCCCCCCCcO.",
    ".OAAAAAAAAAAAAAAO.",
    ".OCCCCCCCCCCCCCcO.",
    "OCCCCCCCCCCCCCCccO",
    "OCCCCCYYCCCCCCCccO",
    "OCCCCCYYCCCCCCCccO",
    "OCCCCCCCCCCCCCcccO",
    "OcCCCCCCCCCCCCcccO",
    ".OcccccccccccccccO",
    "..OOOOOOOOOOOOOO..",
    "..................",
]

def draw(rows, name):
    h, w = len(rows), len(rows[0])
    assert all(len(r) == w for r in rows), [i for i, r in enumerate(rows) if len(r) != w]
    im = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    for y, row in enumerate(rows):
        for x, ch in enumerate(row):
            if ch != ".":
                c = PAL[ch]
                im.putpixel((x, y), tuple(int(c[i:i+2], 16) for i in (1, 3, 5)) + (255,))
    im.save(os.path.join(out, name + ".png"))
    return im

draw(IDLE, "player_cleaner_idle")
draw(STEP, "player_cleaner_step")
draw(BAG, "sack_toolbag")
