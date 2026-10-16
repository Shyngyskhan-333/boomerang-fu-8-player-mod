# MODLOG

Chronological record (2026-10-11).

1. **Recon.** Found BepInEx 5.4.23.5 + old `BoomerangFu8Players.dll` (non-functional: HarmonyX “Parameter not found”, Rewired “Player id 6 does not exist”). Backed up plugin + `Assembly-CSharp.dll` (`mod8p\backup`). No SDK installed → portable .NET 8.0.425 + ilspycmd into `mod8p\tools`.
2. **Decompiled** `Assembly-CSharp.dll` (518 files) and the old plugin. Old plugin = stubs; discarded.
3. **Diagnostics plugin** (`Diag.cs`) logged real runtime sizes: Rewired 7 userData entries (6 players+System), `uiPlayers` 6, bars 6, 57 levels. `LevelAsset.maxPlayers` 6/4 everywhere.
4. **Rewired**: `UserData.DuplicatePlayer` in a Harmony prefix of `InputManager_Base.Awake`. First attempt targeted 8 userData entries → only ids 0–6; fixed to 9 (System counts) → ids 0–7. Verified in log.
5. **UI growth** by cloning last element: lobby `uiPlayers`, `UIScoreRow`, `PowerupBar/HintBar/AlertBar`, `UIMatchEndPlayer`; IL transpiler `ldc.i4.6→8` for `UICharacterGridCell.SetupCharacter`.
6. **Probe** (`Probe.cs`): loaded all 57 level scenes additively and logged boxes/spawn points. Found all have 6 spawns.
7. **SpawnPlanner**: farthest-point sampling over grid candidates validated by `IsPointOnSolidGround`, `HazardContactPoints`, obstacle overlap. 54/57 levels pass (Trap, Rivercrossing, TrapMaze2 do not).
8. **First 8-bot match crashed** (`ArgumentOutOfRangeException` in `Level.ShuffleSpawnPoints`, indexes `spawnPoints[0..players)`): fixed by moving the extension to a **prefix** of `Level.Init`. Next failure: playlist empty-risk → `maxPlayers` raised temporarily for probed levels.
9. **Test driver `AutoMatch`** (bots, screenshots). Found: instruction panel and an auto-pause wait for human input in bots-only matches → driver dismisses/unpauses (test only).
10. **MatchEnd crash** (`IndexOutOfRange` in `ShowMatchAwards`, `uiPlayers[6]`) → grow `UIMenuMatchEnd.uiPlayers` to 8.
11. **UI polish**: round-end 8 rows + trophy clipped at top → scale 0.86 and shift −65px when ≥7 rows; lobby “JOIN” prompts overlapped at 8 slots → scale 0.7 when ≥7 visible.
12. Golden Disc survival goal table stops at 6 players → 8 s (7 players) / 7 s (8 players).
13. Test matrix run with `tools\run_tests.py` (results in `results\`, screenshots `shots_*`). See TEST_REPORT.md.

## Behaviour changes vs original (documented)
* ≥7 players only: playlist limited to probed levels; spawn placement by planner; round-end block scaled; lobby prompts scaled; Golden Disc survival time 8/7 s.
* ≤6 players: no code path of the mod alters gameplay (only the extra hidden UI/Rewired slots exist).
