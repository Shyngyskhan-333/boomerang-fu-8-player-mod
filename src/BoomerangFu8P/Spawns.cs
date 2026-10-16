using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using HarmonyLib;
using UnityEngine;

namespace BoomerangFu8P
{
    // Runtime spawn logic for 7-8 player matches. Matches with <= 6 players keep the game's own behaviour untouched.
    internal static class Spawns
    {
        public const int Threshold = 7;
        static int roundCounter;

        public static void Apply(Harmony h)
        {
            h.Patch(AccessTools.Method(typeof(LevelManager), "BuildMatchPlaylist"),
                prefix: new HarmonyMethod(typeof(Spawns), nameof(PlaylistPrefix)),
                postfix: new HarmonyMethod(typeof(Spawns), nameof(PlaylistPostfix)));
            // prefix on the base Level.Init: its ShuffleSpawnPoints indexes spawnPoints[0..players.Count) and would throw for 7-8 players
            h.Patch(AccessTools.Method(typeof(Level), "Init", new[] { typeof(int) }), prefix: new HarmonyMethod(typeof(Spawns), nameof(LevelInitPrefix)));
            h.Patch(AccessTools.Method(typeof(GameManager), "StartRound"), prefix: new HarmonyMethod(typeof(Spawns), nameof(StartRoundPrefix)));
        }

        // ---- playlist: let probed levels host 7-8 players, restoring original limits afterwards
        static readonly Dictionary<LevelAsset, int> savedMax = new Dictionary<LevelAsset, int>();

        static void PlaylistPrefix(LevelManager __instance)
        {
            savedMax.Clear();
            var gm = Singleton<GameManager>.Instance;
            if (gm == null || gm.players.Count < Threshold) return;
            int raised = 0, total = 0;
            foreach (var la in __instance.levelAssets)
            {
                total++;
                if (!Eligible8.Paths.Contains(la.path)) continue;
                if (la.maxPlayers < Plugin.Target) { savedMax[la] = la.maxPlayers; la.maxPlayers = Plugin.Target; raised++; }
            }
            Plugin.Log.LogInfo("Playlist for " + gm.players.Count + " players: " + Eligible8.Paths.Count + "/" + total + " levels eligible, raised maxPlayers on " + raised);
        }

        static void PlaylistPostfix()
        {
            foreach (var kv in savedMax) kv.Key.maxPlayers = kv.Value;
            savedMax.Clear();
        }

        // ---- make sure the level has enough spawn transforms
        static void LevelInitPrefix(Level __instance)
        {
            try
            {
                var gm = Singleton<GameManager>.Instance;
                if (gm == null || gm.players.Count < Threshold) return;
                if (__instance.playerSpawnPoints.Count >= gm.players.Count) return;
                var plan = SpawnPlanner.Plan(__instance, Plugin.Target);
                Plugin.Log.LogInfo("Spawn plan " + __instance.gameObject.scene.name + ": " + plan.Summary());
                SpawnPlanner.Apply(__instance, plan);
            }
            catch (Exception e) { Plugin.Log.LogError("LevelInitPrefix: " + e); }
        }

        // ---- assignment of players to spawn points
        static void StartRoundPrefix()
        {
            try { Assign(); }
            catch (Exception e) { Plugin.Log.LogError("Spawn assign failed: " + e); }
        }

        static void Assign()
        {
            var gm = Singleton<GameManager>.Instance;
            var lm = Singleton<LevelManager>.Instance;
            if (gm == null || lm == null || lm.level == null || gm.players.Count < Threshold) return;

            var active = new List<Player>();
            foreach (var p in gm.players) if (p.gameObject.activeSelf) active.Add(p);
            // tie-break rounds with few survivors: keep the game's own placement (it spreads 2 players apart)
            if (active.Count < Threshold) return;
            var pts = new List<Vector2>();
            foreach (var t in lm.level.playerSpawnPoints) if (t != null) pts.Add(t.position);
            if (pts.Count < active.Count)
            {
                Plugin.Log.LogError("Not enough spawn points (" + pts.Count + ") for " + active.Count + " players; keeping game placement");
                return;
            }

            var rng = new System.Random(Environment.TickCount + roundCounter * 7919);
            roundCounter++;

            // drop surplus points at random (7 players on 8 points)
            while (pts.Count > active.Count) pts.RemoveAt(rng.Next(pts.Count));

            var assignment = new Dictionary<Player, Vector2>();
            bool team = Singleton<SettingsManager>.Instance.teamMatch;
            if (!team)
            {
                Shuffle(pts, rng);
                for (int i = 0; i < active.Count; i++) assignment[active[i]] = pts[i];
            }
            else
            {
                var teamIds = new List<int>();
                foreach (var p in active) if (!teamIds.Contains(p.team)) teamIds.Add(p.team);
                if (teamIds.Count != 2)
                {
                    Shuffle(pts, rng);
                    for (int i = 0; i < active.Count; i++) assignment[active[i]] = pts[i];
                }
                else
                {
                    var a = active.FindAll(p => p.team == teamIds[0]);
                    var b = active.FindAll(p => p.team == teamIds[1]);
                    List<Vector2> ga, gb;
                    SplitPoints(pts, a.Count, lm.level, out ga, out gb);
                    if (rng.Next(2) == 0) { var tmp = ga; ga = gb; gb = tmp; }
                    Shuffle(ga, rng); Shuffle(gb, rng);
                    for (int i = 0; i < a.Count; i++) assignment[a[i]] = ga[i];
                    for (int i = 0; i < b.Count; i++) assignment[b[i]] = gb[i];
                }
            }

            var sb = new StringBuilder("Spawn assignment round " + roundCounter + " (" + lm.currentScene + ", team=" + team + "): ");
            foreach (var kv in assignment)
            {
                var p = kv.Key;
                p.transform.position = new Vector3(kv.Value.x, kv.Value.y, Actor.zPos);
                var it = p.GetComponent<InterpolatedTransform>();
                if (it != null) it.ForgetPreviousTransforms();
                sb.AppendFormat(CultureInfo.InvariantCulture, "P{0}t{1}=({2:F1},{3:F1}) ", p.playerID, p.team, kv.Value.x, kv.Value.y);
            }
            Plugin.Log.LogInfo(sb.ToString());
            if (Singleton<CameraFX>.Instance != null) Singleton<CameraFX>.Instance.ResetCamera();
        }

        // Split points into two groups (sizes nA and rest) by a straight line through the arena centre.
        // Chooses the line maximising the closest cross-team distance, then balances centre distances.
        static void SplitPoints(List<Vector2> pts, int nA, Level level, out List<Vector2> a, out List<Vector2> b)
        {
            var c = SpawnPlanner.Center(level);
            float bestScore = float.MinValue;
            a = new List<Vector2>(); b = new List<Vector2>();
            for (int deg = 0; deg < 180; deg += 5)
            {
                var dir = new Vector2(Mathf.Cos(deg * Mathf.Deg2Rad), Mathf.Sin(deg * Mathf.Deg2Rad));
                var sorted = new List<Vector2>(pts);
                sorted.Sort((p, q) => Vector2.Dot(p - c, dir).CompareTo(Vector2.Dot(q - c, dir)));
                var ga = sorted.GetRange(0, nA);
                var gb = sorted.GetRange(nA, sorted.Count - nA);
                float cross = float.MaxValue;
                foreach (var p in ga) foreach (var q in gb) cross = Mathf.Min(cross, Vector2.Distance(p, q));
                float ca = 0f, cb = 0f;
                foreach (var p in ga) ca += Vector2.Distance(p, c) / Mathf.Max(1, ga.Count);
                foreach (var q in gb) cb += Vector2.Distance(q, c) / Mathf.Max(1, gb.Count);
                float score = cross - 0.5f * Mathf.Abs(ca - cb);
                if (score > bestScore) { bestScore = score; a = ga; b = gb; }
            }
        }

        static void Shuffle<T>(IList<T> list, System.Random rng)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                T t = list[i]; list[i] = list[j]; list[j] = t;
            }
        }
    }
}
