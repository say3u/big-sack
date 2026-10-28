# Draws the toilet map pictures: toilet_back, toilet_walls and toilet_swirl
# run it with: python3 make_toilet_map.py Assets/Art
# if you move things here, change the numbers in ToiletLevelBuilder.cs too
import sys, os
import numpy as np
from PIL import Image

out = sys.argv[1]
os.makedirs(out, exist_ok=True)
LO, UP = 9, 1                         # 9 pixels per unit
X0, X1, Y0, Y1 = -64, 64, -50, 110    # edges of the map in Unity units
W, H = (X1 - X0) * LO, (Y1 - Y0) * LO
X, Y = np.meshgrid(X0 + (np.arange(W) + .5) / LO, Y1 - (np.arange(H) + .5) / LO)
AX = np.abs(X)

def C(h): return np.array([int(h[i:i+2], 16) for i in (1, 3, 5)] + [255], np.uint8)
OUT = C("#434a5f")
WHITE, PORC, PORC_SH, PORC_DK = C("#ffffff"), C("#e8ecf1"), C("#c3cbd6"), C("#a3aebd")
M_HI, M_BASE, M_MID, M_DARK = C("#b7c4c8"), C("#8a9aa0"), C("#6d7c82"), C("#4f5d63")

def shift(m, dx, dy):
    r = np.zeros_like(m); h, w = m.shape
    r[max(dy,0):h+min(dy,0), max(dx,0):w+min(dx,0)] = m[max(-dy,0):h+min(-dy,0), max(-dx,0):w+min(-dx,0)]
    return r

def outline(img, mask, color=OUT):
    dil = mask | shift(mask, 1, 0) | shift(mask, -1, 0) | shift(mask, 0, 1) | shift(mask, 0, -1)
    img[dil & ~mask] = color

def porcelain(img, mask, shine=None):
    # white with a shadow on the right side
    img[mask] = PORC
    rows = np.where(mask.any(1))[0]
    for r in rows:
        cols = np.where(mask[r])[0]
        left, right = cols.min(), cols.max()
        cut = left + int((right - left) * .68)
        img[r, cut:right + 1][mask[r, cut:right + 1]] = PORC_SH
    img[mask & ~shift(mask, 0, 1)] = WHITE
    img[mask & ~shift(mask, 0, -1)] = PORC_DK
    if shine is not None:
        img[mask & shine] = WHITE
    outline(img, mask)

def stripes(x0, slope, *offsets_widths):
    # shiny lines
    d = X - (Y * slope) - x0
    m = np.zeros_like(X, bool)
    for off, w in offsets_widths: m |= (d > off) & (d < off + w)
    return m

def rbox(cx, cy, rx, ry, p=4):
    return (np.abs(X - cx) / rx) ** p + (np.abs(Y - cy) / ry) ** p <= 1

def rect(x0, x1, y0, y1): return (X >= x0) & (X <= x1) & (Y >= y0) & (Y <= y1)

# toilet parts
FLOOR = -2
foot  = (Y >= FLOOR) & (Y <= 3) & (AX <= 6.5 + 2.5 * np.clip((3 - Y) / 5, 0, 1))
neck  = (Y > 3) & (Y <= 8.5) & (AX <= 5 - 1.0 * np.sin(np.clip((Y - 3) / 5.5, 0, 1) * np.pi))

BOWL_BOT, BOWL_TOP = 8, 40
def bowl_out(y):  # how wide the bowl is at height y
    t = np.clip((y - BOWL_BOT) / (BOWL_TOP - BOWL_BOT), 0, 1)
    return 6 + 17 * np.sqrt(1 - (1 - t) ** 2.4)
bowl = (Y >= BOWL_BOT) & (Y <= BOWL_TOP) & (AX <= bowl_out(Y))
SHELL = 1.6
bowl_in = (Y >= BOWL_BOT) & (Y <= BOWL_TOP + 3) & (AX <= np.where(Y <= BOWL_TOP, bowl_out(Y) - SHELL, bowl_out(BOWL_TOP) - SHELL))
rim   = rect(-24.5, 24.5, BOWL_TOP, BOWL_TOP + 3.2)

tank     = (Y >= BOWL_TOP + 3.2) & (Y <= 78) & (AX <= 18.5 + .08 * (Y - BOWL_TOP))
tank_lid = rbox(0, 79.6, 22, 1.8, 8)
lid      = rbox(0, 61, 14, 16.5, 2.6) & (Y >= BOWL_TOP + 3.2)
seat_u   = (rbox(0, 61, 16.5, 18.5, 2.6) & ~rbox(0, 62.5, 11, 14.5, 2.6)) & (Y >= BOWL_TOP + 3.2)
hinge_l, hinge_r = rbox(-8, BOWL_TOP + 4, 1.6, .9), rbox(8, BOWL_TOP + 4, 1.6, .9)
handle   = rbox(-15.5, 74, 2.2, .55) | rbox(-12.8, 74, .8, .8, 2)

# sewer tunnels under the floor (the tutorial part)
PITS = ((-9.5, -8),)            # the real pit
dip = rect(-18, -16, -31, -29.9)  # small dip you can jump out of
tunnels = [
    rect(-26, 16, -30, -24),     # start
    rect(6,   16, -30, -12),     # right shaft
    rect(-26, 16, -18, -12),     # back left
    rect(-26, -16, -18, -4),     # left shaft
    rect(-26,  4, -9, -4),       # to the middle
    (AX <= 4) & (Y >= -9) & (Y <= BOWL_BOT + 1),    # pipe up into the toilet
]
pits = [rect(x0, x1, Y0, -29.9) for x0, x1 in PITS]
pipe_in = np.zeros_like(X, bool)
for t in tunnels + pits + [dip]: pipe_in |= t
grow = pipe_in.copy()
for _ in range(LO):  # make the walls 1 unit thick
    grow = grow | shift(grow, 1, 0) | shift(grow, -1, 0) | shift(grow, 0, 1) | shift(grow, 0, -1)
pipe_wall = grow & ~pipe_in & (Y <= BOWL_BOT + .5)
# keep the pits open
for x0, x1 in PITS:
    pipe_wall &= ~rect(x0, x1, Y0, -29.9)

# background
back = np.zeros((H, W, 4), np.uint8)
back[:] = C("#7fd3f2")                                   # bathroom wall
back[((np.floor(X * LO) % (3 * LO)) == 0) | ((np.floor(Y * LO) % (3 * LO)) == 0)] = C("#6cc3e6")
board = rect(X0, X1, FLOOR, FLOOR + 1.2)
back[board] = WHITE
floor = rect(X0, X1, FLOOR - 1.5, FLOOR)
back[floor] = np.where((np.floor(X / 1.5) % 2 == 0)[..., None], WHITE, C("#959ab1"))[floor]
back[rect(X0, X1, FLOOR - .12, FLOOR)] = OUT
dirt = Y < FLOOR - 1.5
back[dirt] = C("#cb815e")
rng = np.random.default_rng(3)
back[dirt & (rng.random((H, W)) < .02)] = C("#9f5a52")
back[dirt & (rng.random((H, W)) < .006)] = C("#f4ac66")
back[rect(X0, X1, FLOOR - 1.62, FLOOR - 1.5)] = OUT
for x0, x1 in PITS:                                       # pits
    back[rect(x0, x1, Y0, -31)] = C("#2a2f3d")

# stuff on the wall
def framed(cx, cy, rx, ry, inner):
    f = rect(cx - rx, cx + rx, cy - ry, cy + ry)
    back[f] = C("#c48a4a"); outline(back, f)
    i = rect(cx - rx + .8, cx + rx - .8, cy - ry + .8, cy + ry - .8)
    back[i] = inner; outline(back, i)
    return i
# picture of a happy poop
pic = framed(-40, 48, 6, 7, C("#fff6e2"))
for (cx, cy, rx, ry) in ((-40, 44.5, 4, 1.4), (-40, 46.6, 3, 1.2), (-40, 48.5, 2, 1.1), (-39.6, 50.2, .9, .8)):
    b = rbox(cx, cy, rx, ry, 2.2) & pic
    back[b] = C("#8a5a34"); back[b & ~shift(b, 0, 2)] = C("#b07a45")
back[rbox(-41.2, 46.6, .35, .45, 2)] = C("#22252f"); back[rbox(-38.8, 46.6, .35, .45, 2)] = C("#22252f")
back[rect(-41, -39, 44.6, 44.85)] = C("#22252f")
# towel bar with a towel
back[rect(-46, -34, 22.6, 23.2)] = M_HI; outline(back, rect(-46, -34, 22.6, 23.2))
towel = rect(-44, -36, 13, 22.6)
back[towel] = C("#f29a9a"); back[towel & ((np.floor(Y * LO) % 18) < 3)] = C("#e07a7a")
back[towel & (X > -38)] = C("#e07a7a"); outline(back, towel)
# toilet paper on a holder
back[rect(30, 36, 18.6, 19.2)] = M_HI; outline(back, rect(30, 36, 18.6, 19.2))
roll = rbox(33, 16.8, 2.4, 2.2, 3)
back[roll] = WHITE; back[roll & (X > 34)] = PORC_SH; outline(back, roll)
sheet = rect(31.6, 34.4, 9.5, 15)
back[sheet] = WHITE; back[sheet & (X > 33.6)] = PORC_SH; outline(back, sheet)
# vent up high
vent = rect(36, 46, 64, 70)
back[vent] = M_BASE; back[vent & ((np.floor(Y * LO) % 9) < 3)] = M_DARK; outline(back, vent)

# toilet
porcelain(back, tank, shine=stripes(-14, .35, (0, 1.2), (2.2, .6)))
porcelain(back, tank_lid)
back[tank_lid & (Y < 79.2)] = PORC_SH
outline(back, tank_lid)
back[handle] = M_HI; back[handle & (Y < 73.8)] = M_MID; outline(back, handle)
porcelain(back, lid)
porcelain(back, seat_u)
porcelain(back, hinge_l); porcelain(back, hinge_r)
porcelain(back, foot)
porcelain(back, neck)
porcelain(back, bowl)

# inside of the bowl
inside = bowl_in & (Y <= BOWL_TOP) & ~pipe_in
back[inside] = C("#d5dbe3")
back[inside & (AX > bowl_out(Y) - SHELL - 1.3)] = PORC_SH
back[inside & stripes(-16, .45, (0, 1.4), (2.6, .7)) & (Y > 20)] = WHITE
water = inside & (Y <= 14)
back[water] = C("#5ab0d8")
back[water & (Y > 13.6)] = C("#9fdcf2")
back[water & (np.abs(X - 2.5) < 1.6) & (np.abs(Y - 11.8) < .12)] = C("#9fdcf2")
back[water & (np.abs(X + 3) < 1.1) & (np.abs(Y - 10.2) < .12)] = C("#9fdcf2")
porcelain(back, rim)
back[rim & (Y > BOWL_TOP + 2.8)] = WHITE
back[rim & (Y < BOWL_TOP + .4)] = PORC_DK

back[pipe_in & (Y > -31)] = C("#3e5752")
ribs = ((np.floor(X * LO) % (3 * LO)) == 0) | ((np.floor(Y * LO) % (3 * LO)) == 0)
back[pipe_in & (Y > -31) & ribs] = C("#4d6a64")
for t in tunnels[:5]:   # gunk on the tunnel floors
    ys = Y[t]
    if ys.size:
        lo = ys.min()
        back[t & (Y < lo + .5)] = C("#6f7d3a")
        back[t & (Y >= lo + .5) & (Y < lo + .65)] = C("#93a356")
back[dip & (Y < -30.5)] = C("#6f7d3a")
for x0, x1 in PITS:  # no gunk over the pit so you can see it's a hole
    back[rect(x0, x1, -31, -29.4)] = C("#2a2f3d")

# walls picture
walls = np.zeros((H, W, 4), np.uint8)
walls[pipe_wall] = M_BASE
walls[pipe_wall & ~shift(pipe_wall, 0, 1)] = M_HI
walls[pipe_wall & ~shift(pipe_wall, 0, -1)] = M_MID
walls[pipe_wall & ((((X + 28) % 5) < .3) | (((Y + 34) % 5) < .3))] = M_DARK
outline(walls, pipe_wall)
shell = bowl & ~bowl_in
walls_bowl = np.zeros((H, W, 4), np.uint8)
porcelain(walls_bowl, shell)
m = walls_bowl[..., 3] > 0
walls[m] = walls_bowl[m]

def save(a, name):
    im = Image.fromarray(a, "RGBA")
    im.resize((im.width * UP, im.height * UP), Image.NEAREST).save(os.path.join(out, name))
save(back, "toilet_back.png")
save(walls, "toilet_walls.png")

# whirlpool for the flush
S = 4 * LO
c = (np.arange(S) + .5) - S / 2
SX, SY = np.meshgrid(c, -c)
r = np.hypot(SX, SY) / (S / 2); a = np.arctan2(SY, SX)
sw = np.zeros((S, S, 4), np.uint8)
disk = r <= .92
v = np.sin(3 * a + r * 8)
sw[disk] = C("#5ab0d8")
sw[disk & (v > .2)] = C("#9fdcf2")
sw[disk & (v > .85)] = WHITE
sw[disk & (r < .22)] = C("#2f6f99")
dil = disk | np.roll(disk, 1, 0) | np.roll(disk, -1, 0) | np.roll(disk, 1, 1) | np.roll(disk, -1, 1)
sw[dil & ~disk] = OUT
save(sw, "toilet_swirl.png")
