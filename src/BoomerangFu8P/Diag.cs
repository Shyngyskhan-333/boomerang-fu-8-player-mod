using System;
using System.Collections;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace BoomerangFu8P
{
    internal class Diag : MonoBehaviour
    {
        float t; bool done;
        void Update()
        {
            if (done) return;
            t += Time.unscaledDeltaTime;
            if (t < 8f) return;
            t = 0f;
            var gm = Singleton<GameManager>.Instance;
            if (gm == null) return;
            var lobby = Singleton<UIManager>.Instance != null ? Singleton<UIManager>.Instance.GetMenu(UIMenuBase.MenuType.Lobby) as UIMenuLobby : null;
            if (lobby == null) return;
            done = true;
            if (Cfg.ProbeLevels.Value) StartCoroutine(new Probe().Run());
            try { Dump(gm, lobby); } catch (Exception e) { Plugin.Log.LogError(e); }
        }

        static void Dump(GameManager gm, UIMenuLobby lobby)
        {
            var sb = new StringBuilder();
            sb.AppendLine("== DIAG ==");
            sb.AppendLine("Rewired players: " + Rewired.ReInput.players.playerCount);
            foreach (var rp in Rewired.ReInput.players.AllPlayers) sb.AppendLine("  rewired id=" + rp.id + " name=" + rp.name + " desc=" + rp.descriptiveName);
            sb.AppendLine("lobby.uiPlayers: " + lobby.uiPlayers.Length);
            foreach (var f in typeof(GameManager).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                if (f.FieldType.IsArray) { var a = f.GetValue(gm) as Array; sb.AppendLine("GameManager." + f.Name + "[" + (a == null ? -1 : a.Length) + "]"); }
            var re = Singleton<UIManager>.Instance.GetMenu(UIMenuBase.MenuType.RoundEnd) as UIMenuRoundEnd; if (re != null) sb.AppendLine("RoundEnd.uiScoreRows " + re.uiScoreRows.Length);
            var me = Singleton<UIManager>.Instance.GetMenu(UIMenuBase.MenuType.MatchEnd) as UIMenuMatchEnd; if (me != null) sb.AppendLine("MatchEnd.uiPlayers " + me.uiPlayers.Length + " matchEndPlayers " + me.matchEndPlayers.Length);
            sb.AppendLine("gridCells " + lobby.characterGridCells.Length + " levels " + Singleton<LevelManager>.Instance.levelAssets.Count);
            foreach (var la in Singleton<LevelManager>.Instance.levelAssets) sb.AppendLine("  level " + la.path + " min" + la.minPlayers + " max" + la.maxPlayers + " " + la.complexity);
            var cm = Singleton<ColorManager>.Instance;
            if (cm != null) sb.AppendLine("ColorManager.teamColors: " + cm.teamColors.Length);
            Plugin.Log.LogInfo(sb.ToString());
        }
    }
}
