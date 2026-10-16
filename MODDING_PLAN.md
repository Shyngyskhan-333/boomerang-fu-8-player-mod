# MODDING_PLAN — verified findings and architecture

## Existing ecosystem (inspected)
**Boomerang Foo – Chaos Mod** (`github.com/Jeffjewett27/BoomerangFoo`, HEAD `d11454e`, v0.5.4, BepInEx 5.4.23.x, netstandard2.1, Harmony).
* Patches `Level.ShuffleSpawnPoints`, `LevelManager` (sets `maxPlayers`), `UIMenuLobby` (`Array.Resize(uiPlayers)`), `UIMenuRoundEnd`, `GameManager` bars, `UICharacterGridCell` (`for i=6..MaxPlayers`).
* Its source comment says *“Cannot add ReInput at runtime”* and maps extra lobby slots to **Rewired player 5** (shared input), and fills missing spawn points by **duplicating existing ones** (characters overlap). So it neither gives independent controllers nor balanced spawns.
* Not installed on this PC; not reused. Decision: separate plugin (`BoomerangFu8P`) that does not depend on it. **Do not run both at once** (both patch the same methods).
* Original earlier plugin on this PC (`BoomerangFu8Players.dll`) was non-functional (all Harmony patches failed) → disabled, backed up.

## Findings that shaped the design
1. Rewired **can** grow at runtime: `InputManager_Base.userData.DuplicatePlayer(i)` before `InputManager_Base.Awake` (Harmony prefix on the private `Awake`). Verified: log shows ids 0–7 + System, no “Player id 6 does not exist” error. (`ud.playerCount` includes the System entry, hence target 9.)
2. `UIMenuLobby.uiPlayers` etc. are plain scene arrays → grow by `Instantiate` of the last element beside itself.
3. The spawn limit is three separate things: spawn *transform count*, `ShuffleSpawnPoints` index range, and the *playlist filter* (`maxPlayers`).
4. All paths (`StartMatch`, next round, rematch, round-end continue) call `GameManager.StartRound` after `PrepareRound` → single hook for final placement.

## Architecture (all runtime, no game file modified)
| File | Purpose |
|---|---|
| `Plugin.cs` | BepInEx entry, applies patches |
| `RewiredExpand.cs` | prefix `InputManager_Base.Awake`: clone Rewired players to 8 |
| `UiExpand.cs` | prefixes/postfixes: lobby `uiPlayers`→8, round-end rows→8 (+ layout shrink for ≥7 rows), match-end podium→8, HUD bars→8 (re-collected), transpiler `ldc 6→8` in `UICharacterGridCell.SetupCharacter`, `characterColors` 6–7, lobby prompt shrink for ≥7 visible slots, Golden-Disc survival goal for 7/8 players |
| `SpawnPlanner.cs` | validates grid candidates with the game’s own `IsPointOnSolidGround`/`HazardContactPoints` + obstacle-clearance overlap test, then adds points by farthest-point sampling (penalising centre-distance mismatch) |
| `Spawns.cs` | playlist (`maxPlayers`→8 for probed-OK levels only, restored afterwards), `Level.Init` **prefix** that appends planned spawn transforms, `StartRound` prefix that assigns players (random permutation; team matches: line-split into two groups maximising cross-team distance) |
| `Eligible8.cs` | generated allow-list of 54/57 levels that passed the probe |
| `Probe.cs`, `AutoMatch.cs`, `Diag.cs`, `Cfg.cs` | validation/test drivers (off by default) |

Matches with ≤6 players never enter the new spawn/playlist code → original behaviour preserved.

## Known risks
* Physical 7th/8th controller never tested (no hardware). Rewired clones inherit Player5’s map/assignment settings.
* Spawn reachability is geometric, not A*-verified.
* Moving/falling/cloud levels: points are validated at `Level.Init` time, before platforms move.
* UI scaling is tuned for 16:9 1920×1080; 1280×720 uses the same canvas scaler (not separately screenshot-tested).
