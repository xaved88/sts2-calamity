<#
.SYNOPSIS
  Builds Calamity (DLL + PCK) inside a .NET 9 SDK container and deploys it, plus BaseLib, to the game's mods folder.

.EXAMPLE
  ./build.ps1             # render art, build, deploy
  ./build.ps1 -NoDeploy   # build only (output in build/mods)
  ./build.ps1 -SkipArt    # reuse the PNGs already in Calamity/images
#>
param(
    [string]$GameDir = "C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2",
    [string]$Configuration = "Release",
    [switch]$NoDeploy,
    [switch]$SkipArt
)

$ErrorActionPreference = "Stop"
$root = $PSScriptRoot
$dataDir = Join-Path $GameDir "data_sts2_windows_x86_64"
$outDir = Join-Path $root "build"
$stageDir = Join-Path $outDir "mods"

if (-not (Test-Path (Join-Path $dataDir "sts2.dll"))) {
    throw "sts2.dll not found under '$dataDir'. Pass -GameDir."
}

if (-not $SkipArt) {
    & (Join-Path $root "tools\art\render.ps1")
}

if (Test-Path $stageDir) { Remove-Item -Recurse -Force $stageDir }
New-Item -ItemType Directory -Force $stageDir | Out-Null

# Build from a copy inside the container so obj/ and .godot/ never land in the repo. ModsPath points the template's
# copy targets at /out/mods; BaseLib's runtime files come from the same NuGet package we compile against.
$script = @"
set -e
mkdir -p /work
tar -C /src --exclude=./reference --exclude=./build --exclude=./obj --exclude=./bin --exclude=./.godot --exclude=./.git -cf - . | tar -C /work -xf -
cd /work
dotnet build Calamity.csproj -c $Configuration -p:Sts2DataDir=/game -p:ModsPath=/out/mods/ -nologo -clp:NoSummary -v:minimal
mkdir -p /out/mods/BaseLib
cp ~/.nuget/packages/alchyr.sts2.baselib/*/Content/BaseLib.* /out/mods/BaseLib/
rm -f /out/mods/BaseLib/BaseLib.props
cp ~/.nuget/packages/alchyr.sts2.baselib/*/lib/net9.0/BaseLib.dll /out/mods/BaseLib/
"@

# Windows PowerShell 5.1 turns native stderr (docker pull progress, compiler warnings) into terminating errors
# under "Stop", so judge docker by its exit code only.
$ErrorActionPreference = "Continue"
docker run --rm `
    -v "${root}:/src:ro" `
    -v "${dataDir}:/game:ro" `
    -v "${outDir}:/out" `
    -v "calamity-nuget:/root/.nuget/packages" `
    mcr.microsoft.com/dotnet/sdk:9.0 `
    sh -c ($script -replace "`r", "") 2>&1 | ForEach-Object { "$_" }
$exit = $LASTEXITCODE
$ErrorActionPreference = "Stop"
if ($exit -ne 0) { throw "Build failed (exit $exit)." }

foreach ($required in "Calamity\Calamity.dll", "Calamity\Calamity.json", "Calamity\Calamity.pck", "BaseLib\BaseLib.dll", "BaseLib\BaseLib.json", "BaseLib\BaseLib.pck") {
    if (-not (Test-Path (Join-Path $stageDir $required))) { throw "Build output missing: $required" }
}
Write-Host "Staged -> $stageDir"

if ($NoDeploy) { return }

if (Get-Process -Name "SlayTheSpire2" -ErrorAction SilentlyContinue) {
    throw "Slay the Spire 2 is running; close it before deploying (the DLL is locked while loaded)."
}
foreach ($mod in "BaseLib", "Calamity") {
    $dest = Join-Path $GameDir "mods\$mod"
    New-Item -ItemType Directory -Force $dest | Out-Null
    Copy-Item (Join-Path $stageDir "$mod\*") $dest -Force
    Write-Host "Deployed -> $dest"
}
