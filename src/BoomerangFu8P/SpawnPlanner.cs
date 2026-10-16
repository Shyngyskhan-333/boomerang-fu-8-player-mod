using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace BoomerangFu8P
{
    internal class SpawnPlan
    {
        public List<Vector2> Points = new List<Vector2>();   // first OriginalCount entries are the level's own points
        public int OriginalCount;
        public float MinDist, MeanNearest, OrigMinDist;
        public float CenterDistMean, CenterDistStd;
        public int CandidatesValid, CandidatesTotal;
        public string Verdict;
        public bool Eligible;

        public string Summary()
        {
            return string.Format(CultureInfo.InvariantCulture,
                "orig={0} total={1} origMin={2:F1} min={3:F1} meanNN={4:F1} centerMean={5:F1} centerStd={6:F1} cand={7}/{8} eligible={9} {10}",
                OriginalCount, Points.Count, OrigMinDist, MinDist, MeanNearest, CenterDistMean, CenterDistStd,
                CandidatesValid, CandidatesTotal, Eligible, Verdict);
        }
    }

    // Builds extra, validated spawn points so a level can host 8 players.
    // Keeps the level's designed spawn points and adds points by farthest-point sampling over
    // positions that pass the game's own ground/hazard checks plus an obstacle clearance test.
    internal static class SpawnPlanner
    {
        const float Clearance = 1.1f;     // player body radius + margin
        const float EdgeMargin = 2.5f;
        const float Step = 1.5f;
        static int blockedMask = -1;

        static int BlockedMask()
        {
            if (blockedMask >= 0) return blockedMask;
            int m = 0;
            foreach (var n in new[] { "Obstacle", "Plant", "Hazard", "Destructables", "Switch", "Teleportal", "Torch", "Borders" })
            {
                int l = LayerMask.NameToLayer(n);
                if (l >= 0) m |= 1 << l;
            }
            blockedMask = m;
            return m;
        }

        public static bool IsValid(LevelManager lm, Vector2 p)
        {
            if (lm.HazardContactPoints(p, Clearance + 0.3f) > 0) return false;
            if (!lm.IsPointOnSolidGround(p, Clearance + 0.3f)) return false;
            if (Physics2D.OverlapCircle(p, Clearance, BlockedMask()) != null) return false;
            return true;
        }

        static bool InsideBox(Level level, Vector2 p)
        {
            var box = level.box;
            if (box == null) return true;
            Vector2 local = box.transform.InverseTransformPoint(p);
            local -= box.offset;
            var ls = box.transform.lossyScale;
            float hx = box.size.x * 0.5f - EdgeMargin / Mathf.Max(0.01f, Mathf.Abs(ls.x));
            float hy = box.size.y * 0.5f - EdgeMargin / Mathf.Max(0.01f, Mathf.Abs(ls.y));
            return Mathf.Abs(local.x) <= hx && Mathf.Abs(local.y) <= hy;
        }

        public static Vector2 Center(Level level)
        {
            var box = level.box;
            if (box == null) return Vector2.zero;
            return box.transform.TransformPoint(box.offset);
        }

        public static float MinPairDist(List<Vector2> pts)
        {
            float m = float.MaxValue;
            for (int i = 0; i < pts.Count; i++)
                for (int j = i + 1; j < pts.Count; j++) m = Mathf.Min(m, Vector2.Distance(pts[i], pts[j]));
            return m == float.MaxValue ? 0f : m;
        }

        public static SpawnPlan Plan(Level level, int need)
        {
            var lm = Singleton<LevelManager>.Instance;
            var plan = new SpawnPlan();
            foreach (var t in level.playerSpawnPoints) if (t != null) plan.Points.Add(t.position);
            plan.OriginalCount = plan.Points.Count;
            plan.OrigMinDist = MinPairDist(plan.Points);
            Vector2 center = Center(level);

            var cands = new List<Vector2>();
            var box = level.box;
            float ext = 60f;
            if (box != null)
            {
                var ls = box.transform.lossyScale;
                ext = Mathf.Max(box.size.x * Mathf.Abs(ls.x), box.size.y * Mathf.Abs(ls.y)) * 0.75f;
            }
            int total = 0;
            for (float x = -ext; x <= ext; x += Step)
                for (float y = -ext; y <= ext; y += Step)
                {
                    var p = center + new Vector2(x, y);
                    if (!InsideBox(level, p)) continue;
                    total++;
                    if (IsValid(lm, p)) cands.Add(p);
                }
            plan.CandidatesTotal = total;
            plan.CandidatesValid = cands.Count;

            float meanC = 0f;
            foreach (var p in plan.Points) meanC += Vector2.Distance(p, center);
            meanC = plan.Points.Count > 0 ? meanC / plan.Points.Count : 0f;

            while (plan.Points.Count < need && cands.Count > 0)
            {
                int best = -1; float bestScore = float.MinValue;
                for (int i = 0; i < cands.Count; i++)
                {
                    float md = float.MaxValue;
                    foreach (var q in plan.Points) md = Mathf.Min(md, Vector2.Distance(cands[i], q));
                    float score = md - 0.35f * Mathf.Abs(Vector2.Distance(cands[i], center) - meanC);
                    if (score > bestScore) { bestScore = score; best = i; }
                }
                plan.Points.Add(cands[best]);
                cands.RemoveAt(best);
            }

            plan.MinDist = MinPairDist(plan.Points);
            float nn = 0f;
            for (int i = 0; i < plan.Points.Count; i++)
            {
                float d = float.MaxValue;
                for (int j = 0; j < plan.Points.Count; j++) if (i != j) d = Mathf.Min(d, Vector2.Distance(plan.Points[i], plan.Points[j]));
                nn += d;
            }
            plan.MeanNearest = plan.Points.Count > 0 ? nn / plan.Points.Count : 0f;
            float mean = 0f;
            foreach (var p in plan.Points) mean += Vector2.Distance(p, center);
            mean /= Mathf.Max(1, plan.Points.Count);
            float var = 0f;
            foreach (var p in plan.Points) { float d = Vector2.Distance(p, center) - mean; var += d * d; }
            plan.CenterDistMean = mean;
            plan.CenterDistStd = Mathf.Sqrt(var / Mathf.Max(1, plan.Points.Count));

            float minReq = Mathf.Max(5f, 0.6f * plan.OrigMinDist);
            plan.Eligible = plan.Points.Count >= need && plan.MinDist >= minReq;
            plan.Verdict = plan.Eligible ? "OK" : (plan.Points.Count < need ? "too few valid points"
                : "spacing " + plan.MinDist.ToString("F1", CultureInfo.InvariantCulture) + " < " + minReq.ToString("F1", CultureInfo.InvariantCulture));
            return plan;
        }

        // Append plan points that are not yet represented as transforms.
        public static void Apply(Level level, SpawnPlan plan)
        {
            int have = level.playerSpawnPoints.Count;
            for (int i = have; i < plan.Points.Count; i++)
            {
                var go = new GameObject("8P_Spawn_" + i);
                go.transform.SetParent(level.transform, true);
                go.transform.position = new Vector3(plan.Points[i].x, plan.Points[i].y, 0f);
                level.playerSpawnPoints.Add(go.transform);
            }
        }
    }
}
