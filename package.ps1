<#
.SYNOPSIS
  Packages the last build for sharing:
    * build/release/Calamity-<version>.zip  - drop-in zip for testers (mods + one-click installer + INSTALL.md)
    * build/workshop/Calamity               - a workspace for Mega Crit's ModUploader (Steam Workshop)

  Run ./build.ps1 first.

.EXAMPLE
  ./build.ps1 -NoDeploy; ./package.ps1
#>
$ErrorActionPreference = "Stop"
$root = $PSScriptRoot
$staged = Join-Path $root "build\mods"
foreach ($required in "Calamity\Calamity.dll", "Calamity\Calamity.json", "Calamity\Calamity.pck", "BaseLib\BaseLib.dll") {
    if (-not (Test-Path (Join-Path $staged $required))) { throw "Missing $required; run ./build.ps1 first." }
}
$version = (Get-Content (Join-Path $staged "Calamity\Calamity.json") -Raw | ConvertFrom-Json).version

# --- Zip for testers -------------------------------------------------------------------------------------------
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
