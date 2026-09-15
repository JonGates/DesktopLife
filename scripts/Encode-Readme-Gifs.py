"""Encode the PNG sequences produced by Diagnostics --readme-demo (requires Pillow)."""
import argparse
from pathlib import Path
from PIL import Image

parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument("--input", type=Path, default=Path("artifacts/readme-demo"))
parser.add_argument("--output", type=Path, default=Path("docs/images"))
args = parser.parse_args()
args.output.mkdir(parents=True, exist_ok=True)

for name, expected, duration in [("fly", 200, 50), ("multiscreen", 100, 50), ("settings", 6, 1800)]:
    paths = sorted((args.input / name).glob("*.png"))
    if len(paths) != expected:
        raise ValueError(f"{name}: expected {expected} frames, got {len(paths)}")
    frames = []
    for path in paths:
        with Image.open(path) as source:
            frames.append(source.convert("RGB"))
    # One shared palette avoids color flicker and keeps differential frames compact.
    samples = frames[::max(1, len(frames) // 20)]
    atlas = Image.new("RGB", (frames[0].width, frames[0].height * len(samples)))
    for index, frame in enumerate(samples):
        atlas.paste(frame, (0, index * frame.height))
    palette = atlas.quantize(colors=256)
    indexed = [frame.quantize(palette=palette, dither=Image.Dither.NONE) for frame in frames]
    target = args.output / f"demo-{name}.gif"
    indexed[0].save(target, save_all=True, append_images=indexed[1:], duration=duration, loop=0, optimize=True, disposal=1)
    with Image.open(target) as gif:
        assert gif.info["loop"] == 0
        total = 0
        for index in range(gif.n_frames):
            gif.seek(index)
            gif.load()
            total += gif.info["duration"]
        assert total == expected * duration, (name, total)
        print(f"{target}: {gif.size}, {gif.n_frames} frames, {total / 1000:.1f}s, {target.stat().st_size:,} bytes")
