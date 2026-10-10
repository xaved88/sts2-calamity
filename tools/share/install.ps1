# Calamity installer for playtesters. Run via Install-Calamity.bat (or: powershell -ExecutionPolicy Bypass -File install.ps1).
# Copies mods\Calamity and mods\BaseLib into the Slay the Spire 2 folder. Saves are never touched.
$ErrorActionPreference = "Stop"
$here = $PSScriptRoot

function Pause-Exit([int]$code) {
    Write-Host ""
    Read-Host "Press Enter to close"
    exit $code
}

if (Get-Process -Name "SlayTheSpire2" -ErrorAction SilentlyContinue) {
    Write-Host "Slay the Spire 2 is running. Close it and run this again." -ForegroundColor Red
    Pause-Exit 1
}

# Find the game in any Steam library listed in libraryfolders.vdf.
$steam = (Get-ItemProperty -Path "HKCU:\Software\Valve\Steam" -Name SteamPath -ErrorAction SilentlyContinue).SteamPath
$libraries = @()
if ($steam) {
    $libraries += $steam
    $vdf = Join-Path $steam "steamapps\libraryfolders.vdf"
    if (Test-Path $vdf) {
        $libraries += Select-String -Path $vdf -Pattern '"path"\s+"([^"]+)"' | ForEach-Object { $_.Matches[0].Groups[1].Value -replace '\\\\', '\' }
    }
}
$libraries += "C:\Program Files (x86)\Steam"
$game = $libraries | ForEach-Object { Join-Path $_ "steamapps\common\Slay the Spire 2" } |
    Where-Object { Test-Path (Join-Path $_ "SlayTheSpire2.exe") } | Select-Object -First 1
if (-not $game) {
    Write-Host "Couldn't find Slay the Spire 2. Copy the 'mods' folder into your game folder by hand" -ForegroundColor Red
    Write-Host "(Steam > right-click Slay the Spire 2 > Manage > Browse local files)."
    Pause-Exit 1
}

foreach ($mod in "Calamity", "BaseLib") {
    $dest = Join-Path $game "mods\$mod"
    New-Item -ItemType Directory -Force $dest | Out-Null
    Copy-Item (Join-Path $here "mods\$mod\*") $dest -Force
}
Write-Host "Installed Calamity and BaseLib into $game\mods" -ForegroundColor Green
Write-Host ""
Write-Host "Next: launch the game. The first time, it asks to enable mods and closes itself; launch it again."
Write-Host 'Then open the console with the ` key (left of 1) and type:  unlock all'
Pause-Exit 0
