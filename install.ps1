<#
  Boomerang Fu 8-Player Mod - installer
  Usage:  powershell -ExecutionPolicy Bypass -File install.ps1 [-GameDir "C:\path\to\game"] [-Force]

  - Needs BepInEx 5.x (x64, Mono) already installed in the game folder.
  - Never overwrites anything silently: an existing, different plugin DLL is renamed to *.bak-<timestamp>.
  - Disables the older, incomplete "BoomerangFu8Players.dll" plugin if present (renamed, not deleted).
  - Does not touch Assembly-CSharp.dll or any other game file.
#>
param(
    [string]$GameDir = "",
    [switch]$Force
)
$ErrorActionPreference = "Stop"
$here = Split-Path -Parent $MyInvocation.MyCommand.Path

if (-not $GameDir) {
    # default: folder two levels above this package, or the current directory
    foreach ($c in @((Get-Location).Path, (Split-Path -Parent $here))) {
        if (Test-Path (Join-Path $c "Boomerang Fu.exe")) { $GameDir = $c; break }
    }
}
if (-not $GameDir -or -not (Test-Path (Join-Path $GameDir "Boomerang Fu.exe"))) {
    throw "Game folder not found. Pass -GameDir 'C:\...\Boomerang Fu' (the folder that contains 'Boomerang Fu.exe')."
}
$bepCore = Join-Path $GameDir "BepInEx\core\BepInEx.dll"
if (-not (Test-Path $bepCore)) {
    throw "BepInEx 5.x is not installed in '$GameDir'. Install BepInEx 5 (x64) first - see BUILD_AND_INSTALL.md."
}
if (Get-Process -Name "Boomerang Fu" -ErrorAction SilentlyContinue) {
    throw "Close Boomerang Fu before installing."
}

$plugins = Join-Path $GameDir "BepInEx\plugins"
New-Item -ItemType Directory -Force $plugins | Out-Null
$stamp = Get-Date -Format "yyyyMMdd-HHmmss"

# older incomplete mod: disable (rename), keep the file
$old = Join-Path $plugins "BoomerangFu8Players.dll"
if (Test-Path $old) {
    Move-Item $old "$old.disabled-by-8P-$stamp"
    Write-Host "Disabled old plugin: $old"
}

$src = Join-Path $here "BepInEx\plugins\BoomerangFu8P.dll"
$dst = Join-Path $plugins "BoomerangFu8P.dll"
if (-not (Test-Path $src)) { throw "Package is incomplete: $src missing." }
if (Test-Path $dst) {
    $h1 = (Get-FileHash $src).Hash; $h2 = (Get-FileHash $dst).Hash
    if ($h1 -eq $h2) { Write-Host "Already installed (identical DLL)."; exit 0 }
    if (-not $Force) { Write-Host "A different BoomerangFu8P.dll exists; it will be kept as a backup." }
    Move-Item $dst "$dst.bak-$stamp"
}
Copy-Item $src $dst
Write-Host "Installed: $dst"
Write-Host "Config (created on first launch): $GameDir\BepInEx\config\local.boomerangfu.eightplayers.cfg"
Write-Host "Done. Start the game normally; lobby shows up to 8 slots."
