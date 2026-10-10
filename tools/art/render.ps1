<#
.SYNOPSIS
  Renders the placeholder SVGs in tools/art/src to the PNG sizes the game expects, under Calamity/images.

  Sizes match vanilla: relic large 256, relic packed ~80 (atlas regions are 76x81), power 64 + large 256,
  map node 90. Outlines are the icon's silhouette, dilated and filled white, like vanilla's outline atlases.
  Replace an SVG (or drop a hand-made PNG over the output) to swap in real art.
#>
$ErrorActionPreference = "Stop"
$root = Resolve-Path (Join-Path $PSScriptRoot "..\..")

$script = @'
set -e
apk add --no-cache rsvg-convert imagemagick >/dev/null
src=/repo/tools/art/src
img=/repo/Calamity/images
rm -rf $img/relics $img/powers $img/map $img/cards $img/events
mkdir -p $img/relics/packed $img/relics/outline $img/powers/big $img/map $img/cards $img/events

outline() { magick "$1" -channel A -morphology Dilate Disk:$3 +channel -fill white -colorize 100 "$2"; }

for f in $src/relics/*.svg; do
  n=$(basename "$f" .svg)
  rsvg-convert -w 256 -h 256 "$f" -o $img/relics/$n.png
  rsvg-convert -w 80 -h 80 "$f" -o $img/relics/packed/$n.png
  outline $img/relics/packed/$n.png $img/relics/outline/$n.png 2
done
for f in $src/powers/*.svg; do
  n=$(basename "$f" .svg)
  rsvg-convert -w 256 -h 256 "$f" -o $img/powers/big/$n.png
  rsvg-convert -w 64 -h 64 "$f" -o $img/powers/$n.png
done
for f in $src/cards/*.svg; do
  rsvg-convert -w 1000 -h 760 "$f" -o $img/cards/$(basename "$f" .svg).png
done
for f in $src/map/*.svg; do
  n=$(basename "$f" .svg)
  rsvg-convert -w 90 "$f" -o $img/map/$n.png
  outline $img/map/$n.png $img/map/${n}_outline.png 4
done
for f in $src/events/*.svg; do
  rsvg-convert -w 3440 -h 1613 "$f" -o $img/events/$(basename "$f" .svg).png
done
find $img -name '*.png' | sort
'@

$ErrorActionPreference = "Continue"
docker run --rm -v "${root}:/repo" alpine:3.20 sh -c ($script -replace "`r", "") 2>&1 | ForEach-Object { "$_" }
$exit = $LASTEXITCODE
$ErrorActionPreference = "Stop"
if ($exit -ne 0) { throw "Art render failed (exit $exit)." }
