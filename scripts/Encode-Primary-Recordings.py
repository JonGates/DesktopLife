"""Encode real main-screen PNG captures and capture timestamps into README GIFs."""
import argparse
import json
from pathlib import Path
from PIL import Image, ImageChops

parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument("--input", type=Path, default=Path("artifacts/primary-recordings"))
parser.add_argument("--output", type=Path, default=Path("docs/images"))
args = parser.parse_args()
args.output.mkdir(parents=True, exist_ok=True)

for name, crop, size in [
    ("desktop", (0, 0, 1920, 1080), (960, 540)),
    ("fly", (1160, 200, 1800, 600), (960, 600)),
    ("settings", (590, 30, 1330, 990), (666, 864)),
]:
    folder = args.input / name
    timing_files = sorted(folder.glob("timing*.json"), key=lambda p: int(p.stem.split("-")[-1]) if "-" in p.stem else 0)
    if not timing_files:
        raise ValueError(f"Missing capture timestamps for {name}")
    frames, durations = [], []
    for timing_file in timing_files:
        entries = json.loads(timing_file.read_text())
        if len(entries) < 2:
            raise ValueError("A recording needs multiple frames")
        # Round cumulative timestamps to GIF's 10 ms units to avoid playback drift.
        times = [round(entry["timeMs"] / 10) * 10 for entry in entries]
        delays = [times[i + 1] - times[i] for i in range(len(times) - 1)]
        delays.append(delays[-1])
        if min(delays) <= 0:
            raise ValueError("Capture timestamps must increase")
        for entry, delay in zip(entries, delays):
            with Image.open(folder / entry["file"]) as source:
                if source.size != (1920, 1080):
                    raise ValueError("Expected a 1920×1080 primary-screen recording")
                frames.append(source.convert("RGB").crop(crop).resize(size, Image.Resampling.LANCZOS))
            durations.append(delay)
    samples = frames[::max(1, len(frames) // 16)]
    atlas = Image.new("RGB", (size[0], size[1] * len(samples)))
    for index, frame in enumerate(samples):
        atlas.paste(frame, (0, index * size[1]))
    palette = atlas.quantize(colors=255)
    colors = palette.getpalette()
    colors[765:768] = [255, 0, 255]  # Reserve index 255 for unchanged pixels.
    palette.putpalette(colors)
    indexed = [frame.quantize(palette=palette, dither=Image.Dither.NONE) for frame in frames]
    if any(255 in frame.getdata() for frame in indexed):
        raise ValueError("A source pixel used the reserved transparent color")
    delta_frames = [indexed[0]]
    for previous, current in zip(indexed, indexed[1:]):
        unchanged = ImageChops.difference(previous, current).point(lambda value: 255 if value == 0 else 0, "1")
        delta = current.copy()
        delta.paste(255, mask=unchanged)
        delta_frames.append(delta)
    target = args.output / f"live-{name}.gif"
    delta_frames[0].save(target, save_all=True, append_images=delta_frames[1:], duration=durations, loop=0, optimize=False, disposal=1, transparency=255)
    with Image.open(target) as gif:
        assert gif.info["loop"] == 0
        actual_duration = 0
        for index in range(gif.n_frames):
            gif.seek(index)
            gif.load()
            if ImageChops.difference(gif.convert("RGB"), indexed[index].convert("RGB")).getbbox():
                raise ValueError(f"Delta encoding changed visible pixels in {name} frame {index}")
            actual_duration += gif.info["duration"]
        assert actual_duration == sum(durations)
        print(f"{target}: {gif.n_frames} frames, {actual_duration / 1000:.2f}s, {target.stat().st_size:,} bytes")
