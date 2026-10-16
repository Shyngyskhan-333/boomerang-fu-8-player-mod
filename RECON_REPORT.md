# RECON_REPORT — Boomerang Fu 8-player mod

All facts below were read from the installed game / decompiled assemblies on this PC (2026-10-11), not from documentation.

## Installation
| Item | Value |
|---|---|
| Game dir | `C:\games\videogames\Boomerang.Fu.v1.3.6\Boomerang.Fu.v1.3.6\game` |
| Executable | `Boomerang Fu.exe` (Unity player, product version `2022.3.54f1`) |
| Scripting backend | Mono (`MonoBleedingEdge\`, managed code in `Boomerang Fu_Data\Managed\`) |
| Main assembly | `Assembly-CSharp.dll` (1,072,128 B, SHA256 `FEA8BC27…0A51`; backup in `mod8p\backup\`) |
| Input | Rewired (`Rewired_Core.dll`, `Rewired_Windows.dll`) |
| Mod loader | BepInEx **5.4.23.5** (x64 Mono) via Doorstop (`winhttp.dll`, `doorstop_config.ini`) — was already installed |
| Studio / save dir | `%USERPROFILE%\AppData\LocalLow\Cranky Watermelon\Boomerang Fu\` (`options.json`, `save.data`) |
| OS | Windows 11, GTX 1650, 1920×1080 |
| .NET | only runtimes 6.0.16 / 8.0.x installed (no SDK). Portable SDK 8.0.425 downloaded to `mod8p\tools\dotnet` |
| Decompiler | `ilspycmd 8.2.0.7535` installed to `mod8p\tools\ilspy` |
| Existing plugin found | `BepInEx\plugins\BoomerangFu8Players.dll` (an earlier 8-player attempt, no source). Its log showed Harmony failures (“Parameter … not found”) and `Rewired: [ERROR] Player id 6 does not exist!`. Moved to `mod8p\backup\BoomerangFu8Players.dll.removed` (original hash SHA256 `E627DAE6…68D4`). |
| Game settings | `options.json`: `xinput = false`, `secondKeyboardAllowed = true` |

The game is 1.3.6, no 4-controller XInput problem on this PC: `xinput=false` means Rewired uses Raw Input (no 4-pad cap). Steam Input was not inspected/changed.

## How six-player support works today (verified)
* Rewired `InputManager` prefab holds **Player0–Player5 + System** (`userData` had 7 entries). `ReInput.players.GetPlayer(6)` therefore failed.
* `UIMenuLobby.uiPlayers` = 6 `UILobbyPlayer` scene objects; lobby slot *i* ⇄ Rewired player *i* ⇄ `Player.playerID`.
* `GameManager.powerupBars/hintBars/alertBars` = 6 (collected by `GetComponentsInChildren` in `GameManager.Setup`).
* `UIMenuRoundEnd.uiScoreRows` = 6, `UIMenuMatchEnd.uiPlayers` = 6, `UICharacterGridCell.SetupCharacter` creates `for (i<6)` player icons.
* Every one of the 57 playable level scenes has exactly **6** `Level.playerSpawnPoints`. `LevelAsset.maxPlayers` is 6 (or 4) on all levels; `LevelManager.BuildMatchPlaylist` drops levels with `maxPlayers < players.Count` → with 7+ players the playlist would be empty.
* `Level.Init` → `ShuffleSpawnPoints` indexes `spawnPoints[0..players.Count)` (crashes with >6 points missing).
* `GameManager.PrepareRound` assigns spawn index sequentially; team matches use a hard-coded 6-entry table (`0,1,4,5,3,2` / `2,3,5,4,1,0`).
* Colours: `characterColors` dictionary 0–5, `ColorManager.teamColors[3]` (only team index 0–2 used; solo players use team 0).

## Level geometry facts (from `Probe`, see SPAWN_BALANCE.md)
57 level scenes; boxes 43×43 … 89×60 units; all have 6 spawn points, min spacing 8–28 units.
Layers: `8:Obstacle 16:Floor 17:Hazard 21:Plant 4:Water 30:MovingFloor …`. Public helpers `LevelManager.IsPointOnSolidGround` / `HazardContactPoints` were reused for validation.

## Not available / limits
* No eight physical controllers on this PC → controller independence for pads 7–8 is **not** hardware-tested.
* No A* graph at `Level.Init` time (graph loads afterwards), so spawn reachability is geometric only.
* Game language on this PC is Russian (screenshots show Russian UI).
