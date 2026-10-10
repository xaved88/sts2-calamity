# Calamity installer for playtesters. Run via Install-Calamity.bat (or: powershell -ExecutionPolicy Bypass -File install.ps1).
# 1. Copies mods\Calamity and mods\BaseLib into the Slay the Spire 2 folder.
# 2. Installs the bundled, fully unlocked (A11) modded profile into every Steam account's modded profile slot 1,
#    backing up whatever was there. Your normal (unmodded) save is never touched.
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

# --- Find the game: every Steam library listed in libraryfolders.vdf --------------------------------------------
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

# --- Mods ------------------------------------------------------------------------------------------------------------
foreach ($mod in "Calamity", "BaseLib") {
    $dest = Join-Path $game "mods\$mod"
    New-Item -ItemType Directory -Force $dest | Out-Null
    Copy-Item (Join-Path $here "mods\$mod\*") $dest -Force
}
Write-Host "Installed mods into $game\mods" -ForegroundColor Green

# --- Profile -------------------------------------------------------------------------------------------------------
$saveRoot = Join-Path $env:APPDATA "SlayTheSpire2\steam"
$accounts = @(Get-ChildItem $saveRoot -Directory -ErrorAction SilentlyContinue)
if ($accounts.Count -eq 0) {
    Write-Host "No Slay the Spire 2 save folder yet: launch the game once without mods, quit, then run this again" -ForegroundColor Yellow
    Write-Host "(or skip it and start the modded profile from scratch)."
    Pause-Exit 0
}
$stamp = Get-Date -Format "yyyyMMdd-HHmmss"
foreach ($account in $accounts) {
    $saves = Join-Path $account.FullName "modded\profile1\saves"
    if (Test-Path $saves) {
        $backup = Join-Path $env:APPDATA "SlayTheSpire2\calamity-backups\$($account.Name)-modded-profile1-$stamp"
        New-Item -ItemType Directory -Force $backup | Out-Null
        Copy-Item (Join-Path $account.FullName "modded\profile1\*") $backup -Recurse -Force
        Write-Host "Backed up your existing modded profile to $backup"
    }
    New-Item -ItemType Directory -Force $saves | Out-Null
    Copy-Item (Join-Path $here "profile\saves\*") $saves -Force
    Write-Host "Installed the playtest profile for Steam account $($account.Name)" -ForegroundColor Green
}

Write-Host ""
Write-Host "ONE MORE STEP, before you launch:" -ForegroundColor Yellow
Write-Host "  Steam > right-click Slay the Spire 2 > Properties > General > turn OFF 'Keep game saves in the Steam Cloud'."
Write-Host "  Otherwise Steam replaces the profile we just installed. You can turn it back on after your first session"
Write-Host "  (if Steam asks, keep the files on this computer)."
Write-Host ""
Write-Host "Then launch. The first time, the game asks to enable mods and closes itself; launch it again."
Pause-Exit 0
