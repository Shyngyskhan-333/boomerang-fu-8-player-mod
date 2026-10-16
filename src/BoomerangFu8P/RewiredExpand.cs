using System;
using HarmonyLib;
using Rewired;

namespace BoomerangFu8P
{
    // Rewired's player list is baked into the InputManager prefab (Player0..Player5 + System).
    // Grow it before InputManager.Awake runs, cloning the last gameplay player (keeps its map/category layout).
    internal static class RewiredExpand
    {
        public static void Apply(Harmony h)
        {
            var m = AccessTools.Method(typeof(InputManager_Base), "Awake");
            h.Patch(m, prefix: new HarmonyMethod(typeof(RewiredExpand), nameof(Prefix)));
        }

        static void Prefix(InputManager_Base __instance)
        {
            try
            {
                var ud = __instance.userData;
                // userData list = gameplay players + the built-in System player
                int want = Plugin.Target + 1;
                Plugin.Log.LogInfo("Rewired userData entries before: " + ud.playerCount);
                while (ud.playerCount < want && ud.playerCount > 1)
                    ud.DuplicatePlayer(ud.playerCount - 1);
                Plugin.Log.LogInfo("Rewired userData entries after: " + ud.playerCount);
            }
            catch (Exception e) { Plugin.Log.LogError("Rewired expand failed: " + e); }
        }
    }
}
