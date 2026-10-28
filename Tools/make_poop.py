# Draws the poop islands and their faces
# run it with: python3 make_poop.py Assets/Art
import sys, os
import numpy as np
from PIL import Image
out = sys.argv[1]; os.makedirs(out, exist_ok=True)
def C(h): return np.array([int(h[i:i+2], 16) for i in (1, 3, 5)] + [255], np.uint8)
OUT = C("#434a5f")

def body(w, h, rows):
    # each row is one layer of the poop
    m = np.zeros((h, w), bool)
    for top, bot, inset in rows:
        for y in range(top, bot + 1):
            # round off the corners
            d = min(y - top, bot - y)
            pull = {0: 2, 1: 1}.get(d, 0)
            m[y, inset + pull: w - inset - pull] = True
    return m

def poop(crumbly=False):
    w, h = 54, 18
    m = body(w, h, [(8, 16, 1), (3, 9, 6)])
    m[0:4, 41:46] = False
    for y, x0, x1 in ((1, 42, 44), (2, 41, 45), (3, 40, 46)):  # tip on top
        m[y, x0:x1] = True
    if crumbly:
        for x0, x1 in ((7, 10), (27, 29), (45, 48)): m[15:17, x0:x1] = False
    base, hi, mid, dark = ((C("#a07049"), C("#c4935c"), C("#85593a"), C("#5e3d27")) if crumbly
                           else (C("#8a5a34"), C("#b07a45"), C("#6b4226"), C("#4a2c1a")))
    img = np.zeros((h, w, 4), np.uint8)
    img[m] = base
    up = np.zeros_like(m); up[1:] = m[:-1]
    dn = np.zeros_like(m); dn[:-1] = m[1:]
    img[m & ~up] = hi
    img[m & ~dn] = dark
    img[m & dn & ~np.vstack([m[2:], np.zeros((2, w), bool)])] = mid
    # line between the layers
    img[9, 7:47][m[9, 7:47]] = dark
    img[10, 7:47][m[10, 7:47]] = hi
    if crumbly:
        for x, y in ((14, 11), (15, 12), (15, 13), (38, 11), (39, 12), (24, 5), (25, 6)): img[y, x] = dark
    pad = m | np.roll(m, 1, 0) | np.roll(m, -1, 0) | np.roll(m, 1, 1) | np.roll(m, -1, 1)
    img[pad & ~m] = OUT
    return img

def face(worried=False):
    # same height as the island
    img = np.zeros((18, 18, 4), np.uint8)
    W_, D_, PINK = C("#ffffff"), C("#22252f"), C("#f29a9a")
    for ex in (5, 11):
        img[10:13, ex:ex + 2] = D_
        img[10, ex + 1] = W_
    if worried:
        for x in range(8, 10): img[14, x] = D_      # worried mouth
        img[13, 8:10] = D_; img[15, 8:10] = D_; img[14, 7] = D_; img[14, 10] = D_
        img[13, 8:10] = D_
        img[9, 4] = D_; img[8, 5] = D_; img[8, 12] = D_; img[9, 13] = D_   # eyebrows
    else:
        for x, y in ((6, 14), (7, 15), (8, 15), (9, 15), (10, 15), (11, 14)): img[y, x] = D_
        img[13, 3:5] = PINK; img[13, 13:15] = PINK
    return img

for name, a in (("island_poop", poop()), ("island_poop_crumbly", poop(True)),
                ("poop_face", face()), ("poop_face_worried", face(True))):
    Image.fromarray(a, "RGBA").save(os.path.join(out, name + ".png"))
