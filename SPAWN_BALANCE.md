# SPAWN_BALANCE

## How the original game spawns (verified in decompiled code)
* Every level scene has **6** `Level.playerSpawnPoints` (57 scenes probed). For >2 non-team players `Level.Init` shuffles the list, then `GameManager.PrepareRound` gives player *i* point *i*. 2 players use `offset + 2*i`. Team matches (>5 points) use a fixed 6-entry table per team. Respawns pick the point farthest from living players (`FindFurthestRespawnPoint`). No overlap/obstacle check exists; the designers hand-placed the points.
* Original behaviour is **kept unchanged for ≤6 players** (new code is only entered with ≥7 players).

## Eight-player method (`SpawnPlanner.cs`, `Spawns.cs`)
1. At `Level.Init` (prefix) with ≥7 players the planner samples a 1.5-unit grid inside the level play box (oriented box, 2.5-unit edge margin).
2. A candidate must pass the game’s own `LevelManager.IsPointOnSolidGround` and `HazardContactPoints==0` (radius 1.4) and have no collider of layers Obstacle/Plant/Hazard/Destructables/Switch/Teleportal/Torch/Borders within 1.1 units.
3. The designed 6 points are kept. Points 7–8 are chosen by farthest-point sampling (max-min distance to all chosen points) minus 0.35×|distance-to-centre − mean centre distance of the designed points| so they sit at a comparable distance from the middle.
4. A level is **eligible** only if 8 points were found and min pairwise distance ≥ max(5, 0.6 × the original min spacing). Eligible levels are baked into `Eligible8.cs` and only those enter the 7–8 player playlist (their `maxPlayers` is raised temporarily). Others are never played with 7–8 players.
5. Assignment (`GameManager.StartRound` prefix): non-team = fresh random permutation each round/rematch (7 players: one random point dropped), so no player index owns a position. Team = best of 36 straight lines through the arena centre that splits points into the two team sizes, scored by largest closest cross-team distance minus 0.5×|difference of mean centre distance|; which team takes which side is random; order inside a team random.

## Per-level result (probe run on the installed game, all 57 scenes)
min = smallest pairwise distance of the final 8 points, meanNN = mean nearest-neighbour distance, cStd = std-dev of distance to arena centre (lower = more even ring), cand = valid/total grid candidates.

| Level | orig min | 8-pt min | meanNN | cStd | cand | 8P | new points (x;y) |
|---|---|---|---|---|---|---|---|
| LevelBambooGrove | 22.9 | 17.6 | 20.5 | 8.3 | 688/1200 | OK | (5.3;-21.8) (-24.8;-0.8) |
| LevelBigRaft | 8.0 | 8.0 | 8.5 | 5.0 | 308/1505 | OK | (-12.0;-12.0) (-21.0;-16.5) |
| LevelBridges | 16.5 | 16.5 | 19.8 | 7.4 | 619/1868 | OK | (-2.3;-3.8) (-14.3;-17.3) |
| LevelCircleMaze | 22.1 | 17.0 | 18.4 | 4.1 | 1133/1521 | OK | (21.0;0.0) (-19.5;0.0) |
| LevelCloudSweep | 18.0 | 15.5 | 18.1 | 6.2 | 813/1840 | OK | (-26.3;-3.8) (17.3;-14.3) |
| LevelClouds | 24.1 | 24.1 | 27.2 | 11.6 | 829/2592 | OK | (-12.8;27.8) (39.8;21.8) |
| LevelCourtyard | 20.5 | 20.5 | 21.3 | 6.3 | 609/1623 | OK | (-25.5;-12.0) (24.0;10.5) |
| LevelCrate | 15.0 | 15.0 | 15.2 | 2.5 | 465/841 | OK | (-19.5;0.0) (19.5;0.0) |
| LevelDonut | 15.8 | 15.8 | 16.1 | 1.7 | 779/1225 | OK | (18.0;18.0) (-15.0;-15.0) |
| LevelDrift | 14.1 | 12.0 | 12.7 | 4.3 | 367/1209 | OK | (-15.0;0.0) (15.0;0.0) |
| LevelEnclosed | 13.6 | 13.5 | 13.9 | 4.5 | 519/676 | OK | (3.8;18.8) (-18.8;-0.8) |
| LevelFalling | 15.0 | 15.0 | 19.2 | 8.1 | 655/1199 | OK | (-19.5;15.0) (-18.0;-12.0) |
| LevelFalling2 | 16.5 | 16.5 | 17.4 | 5.6 | 632/945 | OK | (0.0;-18.0) (0.0;9.0) |
| LevelForest | 16.2 | 16.2 | 20.9 | 7.0 | 1206/1333 | OK | (-1.5;-22.5) (-6.0;22.5) |
| LevelFourCogs | 15.6 | 15.6 | 16.7 | 5.8 | 894/1365 | OK | (-25.5;-22.5) (16.5;16.5) |
| LevelHenge | 15.0 | 15.0 | 17.0 | 3.5 | 889/1200 | OK | (29.3;-0.8) (-29.3;-0.8) |
| LevelIsland | 19.1 | 18.9 | 20.0 | 7.6 | 625/1600 | OK | (-11.3;-26.3) (2.2;3.7) |
| LevelIslandPair | 22.0 | 14.2 | 18.3 | 5.1 | 497/1271 | OK | (-15.0;1.5) (-30.0;13.5) |
| LevelJungleRuins | 17.1 | 17.1 | 18.5 | 6.3 | 735/945 | OK | (1.5;19.5) (-3.0;-19.5) |
| LevelMarketplace | 17.5 | 17.5 | 18.8 | 8.3 | 816/1247 | OK | (1.5;-21.0) (-30.0;-1.5) |
| LevelMaze | 13.1 | 13.1 | 15.7 | 4.8 | 466/945 | OK | (7.5;-19.5) (-25.5;3.0) |
| LevelMaze2 | 12.4 | 12.4 | 15.2 | 6.0 | 471/841 | OK | (-21.0;19.5) (21.0;-19.5) |
| LevelMiniIsland | 11.3 | 10.1 | 11.3 | 4.8 | 500/809 | OK | (-6.0;13.5) (-10.5;-10.5) |
| LevelMist | 16.8 | 16.8 | 18.9 | 8.0 | 627/1201 | OK | (14.6;13.0) (-21.4;-5.0) |
| LevelMoving | 16.0 | 15.0 | 15.4 | 5.8 | 455/742 | OK | (-0.8;-20.3) (0.8;20.3) |
| LevelOnsen | 20.8 | 20.8 | 23.8 | 11.7 | 1217/1705 | OK | (9.0;-22.5) (-9.0;22.5) |
| LevelPier | 11.8 | 11.8 | 21.5 | 4.7 | 1137/2072 | OK | (-11.4;27.6) (-8.4;-24.9) |
| LevelPinball | 16.6 | 16.6 | 18.4 | 5.5 | 676/1209 | OK | (-1.5;-19.5) (-1.5;19.5) |
| LevelPincer | 22.0 | 16.9 | 18.3 | 7.0 | 292/2165 | OK | (-24.0;13.5) (24.0;-13.5) |
| LevelPlants | 14.4 | 14.4 | 16.7 | 7.4 | 172/884 | OK | (0.8;-18.8) (-23.3;-0.8) |
| LevelPools | 14.8 | 14.8 | 19.1 | 9.4 | 804/1161 | OK | (6.0;19.5) (-13.5;19.5) |
| LevelRaftCrusher | 15.1 | 15.1 | 15.7 | 6.6 | 740/1241 | OK | (-27.0;6.0) (22.5;-19.5) |
| LevelRaftPair | 18.0 | 14.5 | 16.7 | 4.0 | 647/1443 | OK | (-24.0;1.5) (22.5;-3.0) |
| LevelRaftSwitches | 20.0 | 14.7 | 16.3 | 9.0 | 636/1457 | OK | (-21.0;-7.5) (21.0;7.5) |
| LevelRaftSwitches2 | 28.3 | 28.0 | 28.2 | 10.2 | 1511/2491 | OK | (-39.0;-34.5) (39.0;34.5) |
| LevelRing | 17.7 | 17.7 | 19.0 | 4.6 | 495/905 | OK | (7.5;-28.5) (-7.5;28.5) |
| LevelRivercrossing | 22.4 | 12.6 | 18.2 | 6.8 | 546/1155 | NO: spacing 12.6 < 13.4 | (3.0;-1.5) (-7.5;22.5) |
| LevelRollerSweep | 17.1 | 17.1 | 17.8 | 2.4 | 717/1591 | OK | (0.0;-28.5) (0.0;27.0) |
| LevelRooms | 13.5 | 13.5 | 16.2 | 5.4 | 577/960 | OK | (-0.8;-21.8) (-0.8;21.8) |
| LevelShrine | 15.5 | 15.5 | 15.5 | 4.3 | 676/1024 | OK | (-9.8;17.3) (9.8;-17.3) |
| LevelSlideTraps | 18.7 | 18.7 | 21.7 | 10.4 | 962/1247 | OK | (3.0;21.0) (27.0;1.5) |
| LevelSpin | 23.6 | 23.6 | 23.8 | 5.4 | 471/1287 | OK | (-24.0;-24.0) (24.0;24.0) |
| LevelSplit | 12.0 | 12.0 | 17.5 | 11.3 | 325/1340 | OK | (3.8;38.3) (-24.8;-17.3) |
| LevelSquare | 13.6 | 13.6 | 15.4 | 6.0 | 389/742 | OK | (15.8;-15.8) (-15.8;15.8) |
| LevelTeleportals | 11.0 | 11.0 | 15.1 | 7.0 | 454/906 | OK | (6.8;-18.8) (-6.8;15.8) |
| LevelTemple | 14.4 | 14.4 | 21.6 | 10.3 | 994/1581 | OK | (9.0;22.5) (4.5;-22.5) |
| LevelTempleGarden | 13.3 | 13.3 | 19.1 | 7.8 | 239/1161 | OK | (-1.5;-19.5) (-31.5;3.0) |
| LevelToaster | 19.5 | 19.5 | 22.3 | 8.0 | 936/1457 | OK | (27.0;-1.5) (0.0;-22.5) |
| LevelTracks | 13.8 | 13.8 | 17.6 | 8.0 | 737/1360 | OK | (2.3;24.8) (-24.8;-0.8) |
| LevelTrap | 17.9 | 6.5 | 14.8 | 7.6 | 240/1479 | NO: spacing 6.5 < 10.7 | (0.0;0.0) (-19.5;10.5) |
| LevelTrapIslands | 22.1 | 20.2 | 23.0 | 7.8 | 657/1763 | OK | (-3.0;-27.5) (-27.0;4.0) |
| LevelTrapMaze2 | 13.4 | 13.4 | 13.4 | 0.5 | 0/1681 | NO: too few valid points |  |
| LevelTurnTeleportals | 26.0 | 20.0 | 21.6 | 6.1 | 907/1275 | OK | (-25.5;-10.5) (25.5;10.5) |
| LevelTurnstile | 20.5 | 14.8 | 17.1 | 7.4 | 524/1196 | OK | (15.5;18.3) (-13.0;1.8) |
| LevelWasher | 12.2 | 12.2 | 17.7 | 5.3 | 1020/1739 | OK | (-3.0;-19.5) (4.5;19.5) |
| LevelWheel | 15.9 | 14.9 | 16.2 | 6.0 | 608/1122 | OK | (24.3;0.3) (-23.7;0.3) |
| LevelWhisk | 24.1 | 18.9 | 21.8 | 4.9 | 1043/1419 | OK | (-30.0;-1.5) (-9.0;-24.0) |

**54 of 57 levels eligible.** Not eligible: LevelTrap, LevelRivercrossing, LevelTrapMaze2.

## Validation performed
* Automated geometry (above) for all 57 scenes, using the game’s real colliders at `Level.Init` time.
* In-game 8-bot, 7-bot and 4v4 matches: spawn assignment lines are in `results folder` (`Spawn assignment round N`); all players visible in screenshots (`shots_*`), no exceptions. Levels seen in those runs: see TEST_REPORT.md.
* Round restarts/rematch path: each round calls `StartRound`, assignment re-randomised (different permutation per round in logs).

## Limits (honest)
* **Fairness is geometric only.** Power-ups spawn at random clear points during the round (no fixed power-up spawns exist), so “equal power-up access” cannot be measured statically. Obstacle shielding, line-of-fire and wall cover at the two added points were **not** analysed; only spacing/ring evenness (cStd) was optimised.
* Reachability is not A*-verified (the nav graph is not loaded at `Level.Init`). Candidates are on solid floor with clearance but could sit in a disconnected pocket on island/raft levels.
* Moving/sinking/cloud floors are validated in their initial state.
* Respawn logic (Golden Disc etc.) still uses the game’s farthest-point rule over the now-8-point list (good: more candidates).
* A visual review of every one of the 54 layouts in play was **not** done; only the levels that came up in test matches were seen.
