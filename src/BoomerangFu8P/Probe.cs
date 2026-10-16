using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BoomerangFu8P
{
    // Debug/validation tool: loads every level scene additively (no gameplay) and reports spawn geometry.
    internal class Probe : MonoBehaviour
    {
        public static bool Running;
        public IEnumerator Run()
        {
            Running = true;
            var lm = Singleton<LevelManager>.Instance;
            var sb = new StringBuilder();
            var layers = new List<string>();
            for (int i = 0; i < 32; i++) { var n = LayerMask.LayerToName(i); if (!string.IsNullOrEmpty(n)) layers.Add(i + ":" + n); }
            Plugin.Log.LogInfo("LAYERS " + string.Join(", ", layers.ToArray()));
            Plugin.Log.LogInfo("floorLayers=" + lm.floorLayers.value + " avoidClose=" + lm.powerupSpawnAvoidLayersClose.value + " avoidFar=" + lm.powerupSpawnAvoidLayersFar.value);
            foreach (var la in lm.levelAssets)
            {
                if (la.complexity == LevelAsset.Complexity.Tutorial) continue;
                var op = SceneManager.LoadSceneAsync(la.path, LoadSceneMode.Additive);
                yield return op;
                yield return null;
                var level = UnityEngine.Object.FindObjectOfType<Level>();
                try { ReportLevel(la, level); }
                catch (Exception e) { Plugin.Log.LogError("Probe " + la.path + ": " + e); }
                yield return SceneManager.UnloadSceneAsync(la.path);
                yield return null;
            }
            Plugin.Log.LogInfo("PROBE DONE");
            Running = false;
        }

        static void ReportLevel(LevelAsset la, Level level)
        {
            if (level == null) { Plugin.Log.LogWarning("PROBE " + la.path + ": no Level"); return; }
            var box = level.box;
            var sb = new StringBuilder();
            sb.Append("PROBE ").Append(la.path).Append(" type=").Append(level.GetType().Name);
            sb.Append(" spawns=").Append(level.playerSpawnPoints.Count);
            if (box != null) sb.AppendFormat(" box c=({0:F1},{1:F1}) size=({2:F1},{3:F1}) rot={4:F0}", box.transform.position.x + box.offset.x, box.transform.position.y + box.offset.y, box.size.x * box.transform.lossyScale.x, box.size.y * box.transform.lossyScale.y, box.transform.eulerAngles.z);
            sb.Append(" cloud=").Append(level.isCloudLevel).Append(" falling=").Append(level.isFallingApart);
            sb.Append(" pts=[");
            foreach (var t in level.playerSpawnPoints) if (t != null) sb.AppendFormat("({0:F1},{1:F1}) ", t.position.x, t.position.y);
            sb.Append("]");
            Plugin.Log.LogInfo(sb.ToString());
            var plan = SpawnPlanner.Plan(level, Plugin.Target);
            Plugin.Log.LogInfo("PLAN " + la.path + " " + plan.Summary());
            var pp = new StringBuilder();
            for (int i = 0; i < plan.Points.Count; i++)
                pp.Append(i < plan.OriginalCount ? "o" : "n").Append(string.Format(System.Globalization.CultureInfo.InvariantCulture, "({0:F1};{1:F1}) ", plan.Points[i].x, plan.Points[i].y));
            Plugin.Log.LogInfo("PLANPTS " + la.path + " " + pp);
            foreach (var t in level.playerSpawnPoints)
            {
                if (t == null) continue;
                var cols = Physics2D.OverlapCircleAll(t.position, 1.0f);
                var names = new List<string>();
                foreach (var c in cols) names.Add(LayerMask.LayerToName(c.gameObject.layer) + "/" + c.name);
                if (false) Plugin.Log.LogInfo("   spawn (" + t.position.x.ToString("F1") + "," + t.position.y.ToString("F1") + ") hazard=" + Singleton<LevelManager>.Instance.HazardContactPoints(t.position, 1.2f) + " solid=" + Singleton<LevelManager>.Instance.IsPointOnSolidGround(t.position, 1.2f) + " touching=" + string.Join(",", names.ToArray()));
            }
        }
    }
}
