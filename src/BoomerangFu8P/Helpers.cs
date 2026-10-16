using System;
using System.Collections.Generic;
using UnityEngine;

namespace BoomerangFu8P
{
    internal static class Helpers
    {
        public static void DumpRects(Transform t, int depth, int maxDepth, System.Text.StringBuilder sb)
        {
            var rt = t as RectTransform;
            sb.Append(new string(' ', depth * 2)).Append(t.name).Append(t.gameObject.activeSelf ? "" : "(inactive)");
            if (rt != null) sb.AppendFormat(System.Globalization.CultureInfo.InvariantCulture, " pos=({0:F0},{1:F0}) size=({2:F0},{3:F0}) anchors=({4:F2},{5:F2})-({6:F2},{7:F2}) pivot=({8:F2},{9:F2}) scale={10:F2}", rt.anchoredPosition.x, rt.anchoredPosition.y, rt.rect.width, rt.rect.height, rt.anchorMin.x, rt.anchorMin.y, rt.anchorMax.x, rt.anchorMax.y, rt.pivot.x, rt.pivot.y, rt.localScale.x);
            foreach (var c in t.GetComponents<Component>()) { var n = c.GetType().Name; if (n.Contains("Layout") || n.Contains("Fitter") || n.Contains("Canvas") || n.Contains("Scaler")) sb.Append(" [").Append(n).Append("]"); }
            sb.AppendLine();
            if (depth >= maxDepth) return;
            foreach (Transform c in t) DumpRects(c, depth + 1, maxDepth, sb);
        }

        // Grow an array of scene components to n entries by cloning the last one next to itself.
        public static T[] GrowByCloning<T>(T[] arr, int n, Action<T, int> onClone = null) where T : Component
        {
            if (arr == null || arr.Length == 0 || arr.Length >= n) return arr;
            var res = new T[n];
            Array.Copy(arr, res, arr.Length);
            var src = arr[arr.Length - 1];
            for (int i = arr.Length; i < n; i++)
            {
                var c = UnityEngine.Object.Instantiate(src, src.transform.parent, false);
                c.gameObject.name = src.gameObject.name.Replace("(Clone)", "") + "_8P" + i;
                c.transform.localScale = src.transform.localScale;
                res[i] = c;
                onClone?.Invoke(c, i);
            }
            Plugin.Log.LogInfo("Grew " + typeof(T).Name + "[" + arr.Length + "] -> [" + n + "]");
            return res;
        }
    }
}
