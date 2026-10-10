<#
.SYNOPSIS
  Packages the last build for sharing:
    * build/release/Calamity-<version>.zip  - drop-in zip for friends (mods/Calamity + mods/BaseLib + INSTALL.md)
    * build/workshop/Calamity               - a workspace for Mega Crit's ModUploader (Steam Workshop)

  Run ./build.ps1 first.

  The zip also carries a ready-made playtest profile (see below) and a one-click installer for testers.

.EXAMPLE
  ./build.ps1 -NoDeploy; ./package.ps1
  ./package.ps1 -NoProfile   # mods + installer only
#>
param([switch]$NoProfile)
$ErrorActionPreference = "Stop"
$root = $PSScriptRoot
$staged = Join-Path $root "build\mods"
foreach ($required in "Calamity\Calamity.dll", "Calamity\Calamity.json", "Calamity\Calamity.pck", "BaseLib\BaseLib.dll") {
    if (-not (Test-Path (Join-Path $staged $required))) { throw "Missing $required; run ./build.ps1 first." }
}
$version = (Get-Content (Join-Path $staged "Calamity\Calamity.json") -Raw | ConvertFrom-Json).version

# --- Zip for friends -------------------------------------------------------------------------------------------
$release = Join-Path $root "build\release"
$zipRoot = Join-Path $release "Calamity-$version"
if (Test-Path $zipRoot) { Remove-Item -Recurse -Force $zipRoot }
New-Item -ItemType Directory -Force (Join-Path $zipRoot "mods") | Out-Null
foreach ($mod in "Calamity", "BaseLib") {
    New-Item -ItemType Directory -Force (Join-Path $zipRoot "mods\$mod") | Out-Null
    # Ship only what the loader needs (no .pdb).
    Get-ChildItem (Join-Path $staged $mod) -File | Where-Object { $_.Extension -in ".dll", ".json", ".pck" } |
        Copy-Item -Destination (Join-Path $zipRoot "mods\$mod")
}
foreach ($file in "INSTALL.md", "install.ps1", "Install-Calamity.bat") {
    Copy-Item (Join-Path $root "tools\share\$file") $zipRoot
}

# Playtest profile: your most recently used modded progress (unlocks, A11), so testers can play immediately.
# Only progress.save ships: no run history, replays, in-progress run or personal settings (prefs.save). unique_id is
# stripped so the game generates a fresh one per tester (it only feeds crash reports and metrics).
if (-not $NoProfile) {
    $source = Get-ChildItem (Join-Path $env:APPDATA "SlayTheSpire2\steam\*\modded\profile1\saves\progress.save") -ErrorAction SilentlyContinue |
        Sort-Object LastWriteTime -Descending | Select-Object -First 1
    if (-not $source) { throw "No modded profile found to bundle; play once with mods, or pass -NoProfile." }
    $profileSaves = Join-Path $zipRoot "profile\saves"
    New-Item -ItemType Directory -Force $profileSaves | Out-Null
    $json = [IO.File]::ReadAllText($source.FullName)
    $json = [regex]::Replace($json, ',\s*"unique_id"\s*:\s*"[^"]*"', '')
    $json = [regex]::Replace($json, '"unique_id"\s*:\s*"[^"]*"\s*,\s*', '')
    $utf8 = New-Object System.Text.UTF8Encoding($false)
    foreach ($name in "progress.save", "progress.save.backup") {
        [IO.File]::WriteAllText((Join-Path $profileSaves $name), $json, $utf8)
    }
    Write-Host "Profile  <- $($source.FullName)"
}
$zip = "$zipRoot.zip"
if (Test-Path $zip) { Remove-Item -Force $zip }
Compress-Archive -Path (Join-Path $zipRoot "*") -DestinationPath $zip
Write-Host "Zip      -> $zip"

# --- Steam Workshop workspace ----------------------------------------------------------------------------------
# Layout expected by github.com/megacrit/sts2-mod-uploader: content/, workshop.json, image.png (< 1 MB).
# mod_id.txt is written there by the uploader on first upload; keep it so later uploads update the same item.
$workspace = Join-Path $root "build\workshop\Calamity"
$content = Join-Path $workspace "content"
if (Test-Path $content) { Remove-Item -Recurse -Force $content }
New-Item -ItemType Directory -Force $content | Out-Null
Get-ChildItem (Join-Path $staged "Calamity") -File | Where-Object { $_.Extension -in ".dll", ".json", ".pck" } |
    Copy-Item -Destination $content
if (-not (Test-Path (Join-Path $workspace "workshop.json"))) {
    Copy-Item (Join-Path $root "tools\share\workshop.json") $workspace
}
Copy-Item (Join-Path $root "Calamity\images\relics\cracked_femur.png") (Join-Path $workspace "image.png") -Force
Write-Host "Workshop -> $workspace (set BaseLib's Workshop ID in workshop.json 'dependencies' before uploading)"
