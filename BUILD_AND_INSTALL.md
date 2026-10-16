# BUILD_AND_INSTALL

## Requirements
* Boomerang Fu 1.3.6 (Unity 2022.3.54f1, Mono) — the DLL references `Assembly-CSharp.dll` of this build.
* BepInEx **5.4.23.x x64** already installed (`BepInEx\core\BepInEx.dll` present). If not: download `BepInEx_win_x64_5.4.23.*.zip` from the BepInEx releases page and unzip into the game folder, start the game once.
* Recommended in-game option: **XInput OFF** (Options → controller). XInput mode caps Xbox pads at 4 (Windows API limit); with it OFF Rewired reads pads through Raw Input, which has no 4-pad cap. (This PC already has `xinput=false`.)

## Install
```powershell
# from the unzipped package folder
powershell -ExecutionPolicy Bypass -File install.ps1 -GameDir "C:\games\videogames\Boomerang.Fu.v1.3.6\Boomerang.Fu.v1.3.6\game"
```
* copies `BepInEx\plugins\BoomerangFu8P.dll`; an existing different DLL is kept as `.bak-<time>`;
* renames the older `BoomerangFu8Players.dll` (if present) to `…disabled-by-8P-<time>`.
Manual install: copy `BoomerangFu8P.dll` into `<game>\BepInEx\plugins\`.

## Uninstall / recovery
```powershell
powershell -ExecutionPolicy Bypass -File uninstall.ps1 -GameDir "<game>" [-RestoreOld]
```
Removes `BoomerangFu8P.dll` and its config. No game file is ever modified, so nothing else must be restored. Manual: delete the DLL (or add `.off`).

## Build from source
```powershell
cd src\BoomerangFu8P
dotnet build -c Release -o ..\..\build -p:GameDir="<game folder>"
```
Needs a .NET SDK (8.x works; a portable one is under `tools\dotnet`) and NuGet access for `Microsoft.NETFramework.ReferenceAssemblies`. Output: `build\BoomerangFu8P.dll`. Original game DLLs are referenced from the install, never copied into the package.

## Configuration
`BepInEx\config\local.boomerangfu.eightplayers.cfg` (created on first run). Everything is off by default; `[Debug] ProbeLevels` and `[Test] AutoMatch…` are developer test drivers.

## Troubleshooting
| Symptom | Check |
|---|---|
| Only 6 slots | `BepInEx\LogOutput.log` must contain `Rewired userData entries after: 9` and `Grew UILobbyPlayer[6] -> [8]` |
| `Player id 6 does not exist` | the old plugin or another mod is running; remove it |
| 7–8 players, match returns to menu | log line `Playlist for N players: 54/58 levels eligible` missing → mod not loaded |
| Using Boomerang Foo (Chaos Mod) | not compatible together (same methods patched) |
| Game updated | rebuild against the new `Assembly-CSharp.dll`; Harmony patches target method names listed in `MODDING_PLAN.md` |
