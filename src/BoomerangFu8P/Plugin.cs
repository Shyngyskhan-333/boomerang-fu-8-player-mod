using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace BoomerangFu8P
{
    [BepInPlugin("local.boomerangfu.eightplayers", "Boomerang Fu 8 Players", "3.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log;
        internal const int Target = 8;

        void Awake()
        {
            Log = Logger;
            var h = new Harmony("local.boomerangfu.eightplayers");
            RewiredExpand.Apply(h);
            UiExpand.Apply(h);
            Spawns.Apply(h);
            Log.LogInfo("8P plugin loaded");
            Cfg.Init(Config);
            gameObject.AddComponent<Diag>();
            gameObject.AddComponent<AutoMatch>();
        }
    }
}
