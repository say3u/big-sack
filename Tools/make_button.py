# Draws the porcelain button for the menus
# run it with: python3 make_button.py Assets/Art
# each letter is a color, . is see-through
import sys, os
from PIL import Image

out = sys.argv[1]
os.makedirs(out, exist_ok=True)

PAL = {
    "O": "#434a5f",  # outline
    "W": "#ffffff",  # shine on top
    "B": "#e8ecf1",  # porcelain
    "S": "#c3cbd6",  # shadow on the bottom
}

BUTTON = [
    ".OOOOOOOOOOOOOO.",
    "OWWWWWWWWWWWWWWO",
    "OWBBBBBBBBBBBBBO",
    "OBBBBBBBBBBBBBBO",
    "OBBBBBBBBBBBBBBO",
    "OBBBBBBBBBBBBBBO",
    "OBBBBBBBBBBBBBBO",
    "OBBBBBBBBBBBBBBO",
    "OBBBBBBBBBBBBBBO",
    "OBBBBBBBBBBBBBBO",
    "OBBBBBBBBBBBBBBO",
    "OBBBBBBBBBBBBBBO",
    "OBBBBBBBBBBBBBSO",
    "OSSSSSSSSSSSSSSO",
    "OSSSSSSSSSSSSSSO",
    ".OOOOOOOOOOOOOO.",
]

im = Image.new("RGBA", (16, 16), (0, 0, 0, 0))
for y, row in enumerate(BUTTON):
    for x, ch in enumerate(row):
        if ch != ".":
            c = PAL[ch]
            im.putpixel((x, y), tuple(int(c[i:i+2], 16) for i in (1, 3, 5)) + (255,))
im.save(os.path.join(out, "button_porcelain.png"))
