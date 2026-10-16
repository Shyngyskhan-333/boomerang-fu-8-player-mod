using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace BoomerangFu8P
{
    internal static class UiExpand
    {
        public static void Apply(Harmony h)
        {
            h.Patch(AccessTools.Method(typeof(UIMenuLobby), "Setup"), prefix: new HarmonyMethod(typeof(UiExpand), nameof(LobbySetupPrefix)));
            h.Patch(AccessTools.Method(typeof(UICharacterGridCell), "SetupCharacter"), transpiler: new HarmonyMethod(typeof(UiExpand), nameof(Six2Eight)));
            h.Patch(AccessTools.Method(typeof(UIMenuRoundEnd), "Setup"), prefix: new HarmonyMethod(typeof(UiExpand), nameof(RoundEndSetupPrefix)));
            h.Patch(AccessTools.Method(typeof(UIMenuMatchEnd), "Init"), prefix: new HarmonyMethod(typeof(UiExpand), nameof(MatchEndInitPrefix)));
            h.Patch(AccessTools.Method(typeof(UIMenuRoundEnd), "Init"), postfix: new HarmonyMethod(typeof(UiExpand), nameof(RoundEndInitPostfix)));
            h.Patch(AccessTools.Method(typeof(UIMenuLobby), "Update"), postfix: new HarmonyMethod(typeof(UiExpand), nameof(LobbyUpdatePostfix)));
            h.Patch(AccessTools.Method(typeof(SettingsManager), "SetupGoldenDisc"), postfix: new HarmonyMethod(typeof(UiExpand), nameof(GoldenDiscPostfix)));
            h.Patch(AccessTools.Method(typeof(GameManager), "Setup"), postfix: new HarmonyMethod(typeof(UiExpand), nameof(GameManagerSetupPostfix)));
        }

        static IEnumerable<CodeInstruction> Six2Eight(IEnumerable<CodeInstruction> ins)
        {
            int n = 0;
            foreach (var i in ins)
            {
                if (i.opcode == OpCodes.Ldc_I4_6) { n++; yield return new CodeInstruction(OpCodes.Ldc_I4_8) { labels = i.labels }; }
                else yield return i;
            }
            Plugin.Log.LogInfo("UICharacterGridCell.SetupCharacter: replaced " + n + " x ldc 6 -> 8");
        }

        static void LobbySetupPrefix(UIMenuLobby __instance)
        {
            try
            {
                __instance.uiPlayers = Helpers.GrowByCloning(__instance.uiPlayers, Plugin.Target);
            }
            catch (Exception e) { Plugin.Log.LogError("Lobby expand: " + e); }
        }

        static void RoundEndSetupPrefix(UIMenuRoundEnd __instance)
        {
            try { __instance.uiScoreRows = Helpers.GrowByCloning(__instance.uiScoreRows, Plugin.Target); }
            catch (Exception e) { Plugin.Log.LogError("RoundEnd expand: " + e); }
        }

        // With 7-8 visible lobby panels the circles are ~170px wide and the "JOIN" prompts overlap; shrink them.
        static readonly string[] PromptFields = { "joinPrompt", "readyPrompt", "lockedPrompt" };
        static void LobbyUpdatePostfix(UIMenuLobby __instance)
        {
            try
            {
                if (Singleton<UIManager>.Instance.GetCurrentMenu() != __instance) return;
                int visible = 0;
                foreach (var p in __instance.uiPlayers) if (p.gameObject.activeSelf) visible++;
                float scale = visible >= 7 ? 0.7f : 1f;
                foreach (var p in __instance.uiPlayers)
                    foreach (var f in PromptFields)
                    {
                        var go = Traverse.Create(p).Field(f).GetValue<GameObject>();
                        if (go != null && go.transform.localScale.x != scale) go.transform.localScale = new Vector3(scale, scale, 1f);
                    }
            }
            catch (Exception e) { Plugin.Log.LogError("LobbyUpdatePostfix: " + e); }
        }

        // The original table stops at 6 players (survival hold time 9s, shrinking 14/12/11/10/9s); continue the trend.
        static void GoldenDiscPostfix(SettingsManager __instance)
        {
            int n = Singleton<GameManager>.Instance.players.Count;
            if (n >= 7 && __instance.GetVictoryCondition == VictoryCondition.Survival)
            {
                __instance.goalTimeStart = n == 7 ? 8f : 7f;
                Plugin.Log.LogInfo("GoldenDisc survival goal time for " + n + " players: " + __instance.goalTimeStart);
            }
        }

        static void MatchEndInitPrefix(UIMenuMatchEnd __instance)
        {
            try { __instance.uiPlayers = Helpers.GrowByCloning(__instance.uiPlayers, Plugin.Target); }
            catch (Exception e) { Plugin.Log.LogError("MatchEnd expand: " + e); }
        }

        // 8 score rows + header do not fit the 1080p canvas at native size; shrink the block for 7-8 rows only.
        static Vector2? roundEndBasePos;
        static void RoundEndInitPostfix(UIMenuRoundEnd __instance)
        {
            try
            {
                int rows = Singleton<SettingsManager>.Instance.teamMatch ? 2 : Singleton<GameManager>.Instance.players.Count;
                float scale = rows >= 7 ? 0.86f : 1f;
                var rt = (RectTransform)__instance.content.transform;
                if (!roundEndBasePos.HasValue) roundEndBasePos = rt.anchoredPosition;
                rt.localScale = new Vector3(scale, scale, 1f);
                rt.anchoredPosition = roundEndBasePos.Value + new Vector2(0f, rows >= 7 ? -65f : 0f);
            }
            catch (Exception e) { Plugin.Log.LogError("RoundEnd scale: " + e); }
        }

        static void GameManagerSetupPostfix(GameManager __instance)
        {
            try
            {
                var colors = __instance.characterColors;
                for (int i = 6; i < Plugin.Target; i++)
                    if (!colors.ContainsKey(i)) colors.Add(i, colors[i % 6]);

                // bars are collected with GetComponentsInChildren(UIGameplay) in Setup; clone to 8 and re-collect
                var ui = __instance.UIGameplay;
                Helpers.GrowByCloning(ui.GetComponentsInChildren<PowerupBar>(true), Plugin.Target);
                Helpers.GrowByCloning(ui.GetComponentsInChildren<HintBar>(true), Plugin.Target);
                Helpers.GrowByCloning(ui.GetComponentsInChildren<AlertBar>(true), Plugin.Target);
                __instance.powerupBars = ui.GetComponentsInChildren<PowerupBar>();
                foreach (var b in __instance.powerupBars) b.IsDisplayed = false;
                __instance.hintBars = ui.GetComponentsInChildren<HintBar>();
                __instance.alertBars = ui.GetComponentsInChildren<AlertBar>();
                Plugin.Log.LogInfo("GameManager bars: " + __instance.powerupBars.Length + "/" + __instance.hintBars.Length + "/" + __instance.alertBars.Length);
            }
            catch (Exception e) { Plugin.Log.LogError("GameManager expand: " + e); }
        }
    }
}
