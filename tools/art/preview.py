"""Contact sheet of every rendered relic and power on a map-like background. Usage: python -I tools/art/preview.py"""
from pathlib import Path

from PIL import Image, ImageDraw

root = Path(__file__).resolve().parents[2]
img = root / "Calamity" / "images"
icons = sorted((img / "relics").glob("*.png")) + sorted((img / "powers" / "big").glob("*.png")) + [img / "map" / "calamity.png"]

tile, pad, label, cols = 128, 10, 14, 8
rows = (len(icons) + cols - 1) // cols
sheet = Image.new("RGBA", (cols * (tile + pad) + pad, rows * (tile + pad + label) + pad), "#3a4a48")
draw = ImageDraw.Draw(sheet)
for i, path in enumerate(icons):
    x, y = pad + (i % cols) * (tile + pad), pad + (i // cols) * (tile + pad + label)
    sheet.alpha_composite(Image.open(path).convert("RGBA").resize((tile, tile), Image.LANCZOS), (x, y))
    draw.text((x, y + tile), path.stem[:20], fill="#e8e0d0")
out = root / "build" / "art_preview.png"
out.parent.mkdir(exist_ok=True)
sheet.save(out)
print(out)
