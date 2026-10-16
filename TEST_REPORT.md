# TEST_REPORT

Environment: this PC, Boomerang Fu 1.3.6, BepInEx 5.4.23.5, 1920×1080 (Windows, GTX 1650), Russian UI. Tests were driven by the plugin’s own `AutoMatch` driver (**bots only, no human input**) via `tools\run_tests.py`. Evidence: `results\*.txt` (log extracts + Unity exception scan of the game’s stdout), screenshots in `shots_*` (`shot_NNN`, `roundend_*`, `matchend_*`).

**Legend:** VERIFIED = observed in the running game. CODE = read/compiled only. NOT TESTED = not run.

## Summary
| Area | Status |
|---|---|
| Plugin builds, loads, patches apply | VERIFIED (`LogOutput.log`: “Rewired userData entries after: 9”, “Grew … → [8]”) |
| Rewired players 0–7 exist | VERIFIED (log lists ids 0–7 + System) |
| **Physical controllers 1–8 independent** | **NOT TESTED – no 8 pads available.** Only that Rewired player objects 6/7 exist is verified. |
| Keyboard players | NOT TESTED in a match (no human input possible in the automated runs) |
| Human lobby join/ready for slots 7–8 | NOT TESTED (lobby layout of 8 slots VERIFIED by screenshot with forced “Available” state) |

## Player-count matrix (bots, deathmatch, first to 2 points, ~2 min each)
| Scenario | Result (log + exception scan) |
|---|---|
| 8 bots | VERIFIED: 8 characters spawned, 2 rounds + match end + podium, 0 Unity exceptions (3 separate runs) |
| 7 bots | VERIFIED: full match, 0 exceptions |
| 6 bots | VERIFIED regression: no mod spawn code ran (no “Spawn plan/assignment”), full match, 0 exceptions |
| 5 / 4 bots | VERIFIED: full match, 0 exceptions |
| 3 / 2 bots | VERIFIED: full match to podium, 0 exceptions |
| 7-player teams (4v3) | VERIFIED: round(s) played, match end, 0 exceptions; assigned spawns distinct, min distance 17.5 |
| Golden Disc, 8 bots | PARTIAL: 2 rounds played (2 level spawn assignments, min distance 13.5–16.6), 0 exceptions, match did not reach the podium inside the 130 s window |
| King of the Hill, 8 bots | PARTIAL: round(s) played, 0 exceptions, no match end inside 130 s |
| Hide & Seek, 8 bots | VERIFIED: 2 rounds + match end, 0 exceptions |
| 1 player | NOT TESTED (a 1-participant match ends immediately) |
(An earlier batch was discarded because two test processes overlapped; the table above is from clean single-process runs.)
| 4v4 team match (8 bots) | VERIFIED: 2 rounds, team scoreboard, 0 exceptions |
| Rematch button, returning to menu, second match | NOT TESTED |
| Disconnect/reconnect | NOT TESTED |

## Spawn checks (from the logs, `tools\check_assign.py`)
Every logged 7/8-player assignment: all positions distinct; minimum pairwise distance 13.5–19.6 units (levels Pinball 16.6, Marketplace 17.4, Toaster 19.6, Moving 15.0, Enclosed 13.5, CircleMaze 17.0). Different random permutation each round. Geometry for all 57 scenes: see SPAWN_BALANCE.md (54 eligible). 2-player tie-break rounds originally went through the mod’s assignment and were found to be possibly adjacent; fixed (mod now skips when <7 players are active) and re-tested with an 8-bot match (full match, 0 exceptions).

## UI (screenshots inspected)
* 8 round-end rows + trophy fully visible after scaling (`shots_dm8\roundend_3.png`).
* Match-end podium with 8 characters + awards fits 1920 px (`shots_dm8\…matchend`, first run `shot_025`).
* Lobby with 8 join circles, prompts no longer overlap after 0.7× scale (forced Available state).
* Gameplay: 8 characters visible, camera frames them; offscreen pointers/HUD bars created for 8.
* 1280×720: NOT TESTED separately (same canvas, 16:9).
* HUD hint/powerup/alert bars for **human** players: NOT TESTED (bots-only).

## Defects found and fixed during testing
1. `ArgumentOutOfRangeException` in `Level.ShuffleSpawnPoints` (fixed: extend spawns in a `Level.Init` prefix).
2. `IndexOutOfRangeException` in `UIMenuMatchEnd.ShowMatchAwards` (fixed: grow `uiPlayers`).
3. Rewired id 7 missing after first cloning attempt (System entry counted).
4. Round-end header clipped, lobby prompt overlap (UI scaling).
5. Tie-break spawn adjacency (see above).

## Not covered
Physical 7th/8th controller, XInput-ON behaviour (Xbox pads are capped at 4 by Windows XInput; documented), Steam Input, power-up/boomerang edge cases with a human, long-run stability beyond ~2-round matches, other game versions, compatibility with other BepInEx mods (Chaos Mod is stated incompatible).
