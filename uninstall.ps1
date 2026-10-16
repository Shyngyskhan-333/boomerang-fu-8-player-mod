<#
  Boomerang Fu 8-Player Mod - uninstaller
  Usage:  powershell -ExecutionPolicy Bypass -File uninstall.ps1 [-GameDir "C:\path\to\game"] [-RestoreOld] [-KeepConfig]

  Removes only BoomerangFu8P.dll (+ its config unless -KeepConfig). The game files were never modified,
  so nothing else needs restoring. -RestoreOld re-enables an older plugin that install.ps1 had disabled.
#>
param(
    [string]$GameDir = "",
    [switch]$RestoreOld,
    [switch]$KeepConfig
)
$ErrorActionPreference = "Stop"
$here = Split-Path -Parent $MyInvocation.MyCommand.Path
if (-not $GameDir) {
    foreach ($c in @((Get-Location).Path, (Split-Path -Parent $here))) {
        if (Test-Path (Join-Path $c "Boomerang Fu.exe")) { $GameDir = $c; break }
    }
}
if (-not $GameDir -or -not (Test-Path (Join-Path $GameDir "Boomerang Fu.exe"))) { throw "Pass -GameDir." }
if (Get-Process -Name "Boomerang Fu" -ErrorAction SilentlyContinue) { throw "Close Boomerang Fu first." }

$plugins = Join-Path $GameDir "BepInEx\plugins"
$dll = Join-Path $plugins "BoomerangFu8P.dll"
if (Test-Path $dll) { Remove-Item $dll; Write-Host "Removed $dll" } else { Write-Host "Plugin DLL not present." }
if (-not $KeepConfig) {
    $cfg = Join-Path $GameDir "BepInEx\config\local.boomerangfu.eightplayers.cfg"
    if (Test-Path $cfg) { Remove-Item $cfg; Write-Host "Removed $cfg" }
}
if ($RestoreOld) {
    $d = Get-ChildItem $plugins -Filter "BoomerangFu8Players.dll.disabled-by-8P-*" | Sort-Object LastWriteTime | Select-Object -Last 1
    if ($d) { Move-Item $d.FullName (Join-Path $plugins "BoomerangFu8Players.dll"); Write-Host "Restored old plugin from $($d.Name)" }
}
Write-Host "Uninstalled. Original game files were never changed."
