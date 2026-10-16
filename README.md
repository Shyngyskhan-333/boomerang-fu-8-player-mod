# Boomerang Fu — 8 Player Mod

A BepInEx plugin that lets **Boomerang Fu 1.3.6** (Steam, PC) run **8 local players** instead of 6.
Runtime patching only — no game file is modified, no game assets are redistributed.

> **Status: beta.** Verified in the real game with bot matches (8/7/6/5/4/3/2 players, 4v4 and 4v3 teams, Hide & Seek full matches; Golden Disc and King of the Hill ran rounds only – see [TEST_REPORT.md](TEST_REPORT.md)). **Eight physical controllers have not been tested** (no hardware).

## What it does
| Area | Change |
|---|---|
| Input | Grows Rewired's player list from 6 to 8 *before* `InputManager.Awake`, so players 7 and 8 get their own Rewired player (not a shared one). |
| Lobby / UI | 8 lobby slots, 8 character-grid icons, 8-row round-end scoreboard (scaled to fit 1080p), 8-character match-end podium, 8 HUD hint/alert/power-up bars. |
| Spawns | Every level ships with 6 spawn points. For 7–8 players the mod adds validated points (solid ground, no hazard, obstacle clearance, max-min spacing) and assigns players by a fresh random permutation each round; team matches split the points into two sides. 54 of 57 levels qualify; the rest are never played with 7–8 players. |
| Playlist | Raises `maxPlayers` to 8 only for the 54 probed levels while a 7–8 player playlist is built. |
| ≤ 6 players | Original behaviour; none of the spawn/playlist code runs. |
| Golden Disc | Survival goal time 8 s (7 players) / 7 s (8 players), continuing the original table. |

Details: [SPAWN_BALANCE.md](SPAWN_BALANCE.md), [MODDING_PLAN.md](MODDING_PLAN.md), [RECON_REPORT.md](RECON_REPORT.md), [MODLOG.md](MODLOG.md).

## Install
1. Install **BepInEx 5.4.23.x x64** into the game folder (start the game once).
2. Download `BoomerangFu8P-3.0.0.zip` from [Releases](../../releases), unzip, run
   `powershell -ExecutionPolicy Bypass -File install.ps1 -GameDir "<folder with Boomerang Fu.exe>"`
   (or copy `BepInEx\plugins\BoomerangFu8P.dll` into the game's `BepInEx\plugins`).
3. In-game options: turn **XInput off** — XInput caps Xbox pads at 4; with it off Rewired uses Raw Input.

Uninstall: `uninstall.ps1` (or delete the DLL). Full guide: [BUILD_AND_INSTALL.md](BUILD_AND_INSTALL.md).

## Build
```powershell
cd src\BoomerangFu8P
dotnet build -c Release -p:GameDir="C:\path\to\Boomerang Fu"
```
References the game's own DLLs from your install; nothing is copied into the repo.

## Known limits
* Physical controllers 7–8 untested; keyboard/human HUD in matches untested.
* Spawn fairness is geometric (spacing / ring evenness). Cover, line-of-fire and power-up access are not modelled; reachability is not path-verified.
* Not compatible with *Boomerang Foo (Chaos Mod)* at the same time (both patch the same methods).
* Only game version 1.3.6 (Unity 2022.3.54f1) was examined.

## Developer test drivers
`BepInEx\config\local.boomerangfu.eightplayers.cfg`: `[Debug] ProbeLevels` (log spawn geometry of all levels), `[Test] AutoMatch` (bot match + screenshots). Off by default. `tools\run_tests.py` runs the scenario matrix.

## License
MIT for this plugin's code ([LICENSE](LICENSE)). Boomerang Fu is © Cranky Watermelon; this project is unofficial and ships none of its assets or assemblies.
