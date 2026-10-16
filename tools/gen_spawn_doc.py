import re
W='C:/games/videogames/Boomerang.Fu.v1.3.6/Boomerang.Fu.v1.3.6/mod8p/'
raw=open(W+'results/probe_raw.txt',encoding='utf-8',errors='replace').read().splitlines()
plans={};pts={}
for l in raw:
    m=re.search(r'PLAN Assets/Scenes/(\S+)\.unity (.*)',l)
    if m: plans[m.group(1)]=m.group(2)
    m=re.search(r'PLANPTS Assets/Scenes/(\S+)\.unity (.*)',l)
    if m: pts[m.group(1)]=m.group(2)
rows=[];ok=0
for n,s in plans.items():
    g=lambda k: re.search(k+r'=([\-\d\.]+)',s).group(1)
    el='True' in re.search(r'eligible=(\w+)',s).group(0)
    ok+=el
    new=re.findall(r'n\(([\-\d\.]+);([\-\d\.]+)\)',pts.get(n,''))
    verdict=s.split('eligible=')[1].split(' ',1)[1] if ' ' in s.split('eligible=')[1] else ''
    rows.append((n,g('origMin'),g('min'),g('meanNN'),g('centerStd'),g('cand'.replace('cand','cand')) if False else re.search(r'cand=(\d+/\d+)',s).group(1),el,verdict,new))
out=['# SPAWN_BALANCE','',
'## How the original game spawns (verified in decompiled code)',
'* Every level scene has **6** `Level.playerSpawnPoints` (57 scenes probed). For >2 non-team players `Level.Init` shuffles the list, then `GameManager.PrepareRound` gives player *i* point *i*. 2 players use `offset + 2*i`. Team matches (>5 points) use a fixed 6-entry table per team. Respawns pick the point farthest from living players (`FindFurthestRespawnPoint`). No overlap/obstacle check exists; the designers hand-placed the points.',
'* Original behaviour is **kept unchanged for ≤6 players** (new code is only entered with ≥7 players).','',
'## Eight-player method (`SpawnPlanner.cs`, `Spawns.cs`)',
'1. At `Level.Init` (prefix) with ≥7 players the planner samples a 1.5-unit grid inside the level play box (oriented box, 2.5-unit edge margin).',
'2. A candidate must pass the game’s own `LevelManager.IsPointOnSolidGround` and `HazardContactPoints==0` (radius 1.4) and have no collider of layers Obstacle/Plant/Hazard/Destructables/Switch/Teleportal/Torch/Borders within 1.1 units.',
'3. The designed 6 points are kept. Points 7–8 are chosen by farthest-point sampling (max-min distance to all chosen points) minus 0.35×|distance-to-centre − mean centre distance of the designed points| so they sit at a comparable distance from the middle.',
'4. A level is **eligible** only if 8 points were found and min pairwise distance ≥ max(5, 0.6 × the original min spacing). Eligible levels are baked into `Eligible8.cs` and only those enter the 7–8 player playlist (their `maxPlayers` is raised temporarily). Others are never played with 7–8 players.',
'5. Assignment (`GameManager.StartRound` prefix): non-team = fresh random permutation each round/rematch (7 players: one random point dropped), so no player index owns a position. Team = best of 36 straight lines through the arena centre that splits points into the two team sizes, scored by largest closest cross-team distance minus 0.5×|difference of mean centre distance|; which team takes which side is random; order inside a team random.','',
'## Per-level result (probe run on the installed game, all 57 scenes)',
'min = smallest pairwise distance of the final 8 points, meanNN = mean nearest-neighbour distance, cStd = std-dev of distance to arena centre (lower = more even ring), cand = valid/total grid candidates.','',
'| Level | orig min | 8-pt min | meanNN | cStd | cand | 8P | new points (x;y) |','|---|---|---|---|---|---|---|---|']
for r in sorted(rows):
    out.append('| %s | %s | %s | %s | %s | %s | %s | %s |'%(r[0],r[1],r[2],r[3],r[4],r[5],'OK' if r[6] else 'NO: '+r[7],' '.join('(%s;%s)'%p for p in r[8])))
out+=['','**%d of %d levels eligible.** Not eligible: %s.'%(ok,len(rows),', '.join(r[0] for r in rows if not r[6])),'',
'## Validation performed',
'* Automated geometry (above) for all 57 scenes, using the game’s real colliders at `Level.Init` time.',
'* In-game 8-bot, 7-bot and 4v4 matches: spawn assignment lines are in `results folder` (`Spawn assignment round N`); all players visible in screenshots (`shots_*`), no exceptions. Levels seen in those runs: see TEST_REPORT.md.',
'* Round restarts/rematch path: each round calls `StartRound`, assignment re-randomised (different permutation per round in logs).','',
'## Limits (honest)',
'* **Fairness is geometric only.** Power-ups spawn at random clear points during the round (no fixed power-up spawns exist), so “equal power-up access” cannot be measured statically. Obstacle shielding, line-of-fire and wall cover at the two added points were **not** analysed; only spacing/ring evenness (cStd) was optimised.',
'* Reachability is not A*-verified (the nav graph is not loaded at `Level.Init`). Candidates are on solid floor with clearance but could sit in a disconnected pocket on island/raft levels.',
'* Moving/sinking/cloud floors are validated in their initial state.',
'* Respawn logic (Golden Disc etc.) still uses the game’s farthest-point rule over the now-8-point list (good: more candidates).',
'* A visual review of every one of the 54 layouts in play was **not** done; only the levels that came up in test matches were seen.']
open(W+'SPAWN_BALANCE.md','w',encoding='utf-8').write('\n'.join(out)+'\n')
print(ok,len(rows))
