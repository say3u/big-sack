# Makes the toilet sounds (duck boing, poop squish, plop, flush, whoosh, slide whistle)
# Kenney doesn't have bathroom sounds so these are made from sine waves and noise
# run it with: python3 make_sounds.py Assets/Audio
import sys, os, wave
import numpy as np

out = sys.argv[1]
os.makedirs(out, exist_ok=True)

RATE = 44100
rng = np.random.default_rng(7)  # same sounds every time you run it


def times(seconds):
    return np.arange(int(RATE * seconds)) / RATE


def tone(freq):
    # freq can change over time, so add it up to get the wave position
    return np.sin(2 * np.pi * np.cumsum(freq) / RATE)


def lowpass(x, amount):
    # smooths out noise. amount near 1 = more muffled
    y = np.zeros_like(x)
    last = 0.0
    for i in range(len(x)):
        last = last * amount + x[i] * (1 - amount)
        y[i] = last
    return y


def bandpass(x, center, q=6.0):
    # keeps only the sound near the center frequency (center can move)
    y = np.zeros_like(x)
    low = band = 0.0
    for i in range(len(x)):
        f = 2 * np.sin(np.pi * center[i] / RATE)
        low += f * band
        high = x[i] - low - band / q
        band += f * high
        y[i] = band
    return y


def fade(n, attack, release):
    env = np.ones(n)
    a, r = int(RATE * attack), int(RATE * release)
    env[:a] = np.linspace(0, 1, a)
    env[n - r:] *= np.linspace(1, 0, r)
    return env


def save(name, x, volume=0.8):
    x = x / np.max(np.abs(x)) * volume
    data = (x * 32767).astype(np.int16)
    with wave.open(os.path.join(out, name + ".wav"), "wb") as f:
        f.setnchannels(1)
        f.setsampwidth(2)
        f.setframerate(RATE)
        f.writeframes(data.tobytes())
    print("made", name)


# rubber duck boing: a soft springy sound that wobbles and fades
t = times(0.45)
pitch = 260 + 120 * np.sin(2 * np.pi * 14 * t) * np.exp(-t * 6) + 200 * t
boing = tone(pitch) + 0.1 * tone(pitch * 2)
save("duck_boing", boing * fade(len(t), 0.005, 0.1) * np.exp(-t * 4), 0.5)

# poop squish: a low wet blorp that drops in pitch, with a little muffled noise
for i in range(2):
    t = times(0.3)
    blorp = tone(np.linspace(220 - 30 * i, 90, len(t)))
    noise = lowpass(rng.uniform(-1, 1, len(t)), 0.97) * 3
    save(f"poop_squish{i + 1}", (blorp + noise) * np.exp(-t * 9), 0.55)

# plop: a water drop is a quick pitch going up, plus a little splash
t = times(0.25)
drop = tone(250 * np.exp(t * 14).clip(max=6)) * np.exp(-t * 18)
splash = lowpass(rng.uniform(-1, 1, len(t)), 0.8) * np.exp(-t * 25) * 0.3
save("plop", drop + splash, 0.6)

# flush: deep rushing water that gets loud then drains, with lots of gurgles
t = times(3.0)
rush = lowpass(rng.uniform(-1, 1, len(t)), 0.97)
swell = np.interp(t, [0, 0.4, 1.8, 3.0], [0, 1, 0.7, 0])
gurgle = np.zeros(len(t))
for start in rng.uniform(0.5, 2.7, 30):
    n = int(RATE * 0.12)
    s = int(RATE * start)
    bt = np.arange(n) / RATE
    bloop = tone(rng.uniform(150, 400) * np.exp(bt * 10)) * np.exp(-bt * 30)
    gurgle[s:s + n] += bloop[:len(gurgle) - s]
save("flush", (rush * 8 + gurgle * 0.4) * swell, 0.6)

# launch whoosh: soft air that sweeps up
t = times(0.4)
whoosh = bandpass(rng.uniform(-1, 1, len(t)), np.linspace(300, 1200, len(t)), q=2)
save("whoosh", whoosh * np.sin(np.pi * t / t[-1]) ** 2, 0.45)

# falling slide whistle: pitch slides down with a little wobble
t = times(0.7)
pitch = np.linspace(1300, 350, len(t)) + 15 * np.sin(2 * np.pi * 6 * t)
whistle = tone(pitch) + 0.15 * tone(pitch * 2)
save("slide_whistle", whistle * fade(len(t), 0.03, 0.15), 0.35)
