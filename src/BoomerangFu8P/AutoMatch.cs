using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace BoomerangFu8P
{
    // Test driver: fills the lobby with bots, starts a match and saves periodic screenshots.
    internal class AutoMatch : MonoBehaviour
    {
        System.Collections.Generic.HashSet<UIMenuBase.MenuType> dumped = new System.Collections.Generic.HashSet<UIMenuBase.MenuType>(); float t; bool started; float menuTimer; bool dwellShot; UIMenuBase.MenuType lastMenu = UIMenuBase.MenuType.None; float shotTimer; int shotIndex; string dir;

        void Update()
        {
            if (!Cfg.AutoMatch.Value) return;
            t += Time.unscaledDeltaTime;
            if (!started)
            {
                if (t < 10f) return;
                var ui = Singleton<UIManager>.Instance;
                var gm = Singleton<GameManager>.Instance;
                if (ui == null || gm == null) return;
                var lobby = ui.GetMenu(UIMenuBase.MenuType.Lobby) as UIMenuLobby;
                if (lobby == null) return;
                started = true;
                try { Begin(ui, lobby); } catch (Exception e) { Plugin.Log.LogError("AutoMatch: " + e); }
                return;
            }
            var cur = Singleton<UIManager>.Instance.GetCurrentMenu();
            if (cur != null && cur.MenuID == UIMenuBase.MenuType.Instructions) { Singleton<UIManager>.Instance.PopMenu(); Plugin.Log.LogInfo("AUTOMATCH dismissed instructions"); }
            if (Singleton<GameManager>.Instance.gameState == GameManager.GameState.Paused && (cur == null || cur.MenuID == UIMenuBase.MenuType.Pause))
            {
                if (cur != null) Singleton<UIManager>.Instance.PopMenu();
                Singleton<UIManager>.Instance.Unpause();
                Plugin.Log.LogInfo("AUTOMATCH unpaused");
            }
            var mt = cur != null ? cur.MenuID : UIMenuBase.MenuType.None;
            if ((mt == UIMenuBase.MenuType.RoundEnd || mt == UIMenuBase.MenuType.MatchEnd) && mt != lastMenu && !dumped.Contains(mt)) { dumped.Add(mt); var sbd = new System.Text.StringBuilder(); Helpers.DumpRects(cur.transform, 0, 4, sbd); Plugin.Log.LogInfo("RECTDUMP " + mt + " " + sbd); }
            if (mt != lastMenu) { lastMenu = mt; menuTimer = 0f; dwellShot = false; Plugin.Log.LogInfo("AUTOMATCH menu -> " + mt); }
            menuTimer += Time.unscaledDeltaTime;
            if (mt == UIMenuBase.MenuType.RoundEnd && menuTimer > 4.5f && !dwellShot) { dwellShot = true; ScreenCapture.CaptureScreenshot(Path.Combine(dir, "roundend_" + shotIndex++ + ".png")); }
            if (mt == UIMenuBase.MenuType.MatchEnd && menuTimer > 10f && !dwellShot) { dwellShot = true; ScreenCapture.CaptureScreenshot(Path.Combine(dir, "matchend_" + shotIndex++ + ".png")); }
            if (mt == UIMenuBase.MenuType.RoundEnd && menuTimer > 6f) { menuTimer = -100f; (cur as UIMenuRoundEnd).UIResponse_Continue(); Plugin.Log.LogInfo("AUTOMATCH continue from round end"); }
            shotTimer += Time.unscaledDeltaTime;
            if (shotTimer >= Cfg.ShotInterval.Value && dir != null)
            {
                shotTimer = 0f;
                string f = Path.Combine(dir, "shot_" + (shotIndex++).ToString("D3") + ".png");
                ScreenCapture.CaptureScreenshot(f);
                Plugin.Log.LogInfo("AUTOMATCH screenshot " + f + " state=" + Singleton<GameManager>.Instance.gameState + " players=" + Singleton<GameManager>.Instance.players.Count + " menu=" + (cur != null ? cur.MenuID.ToString() : "none") + " timeScale=" + Time.timeScale);
            }
        }

        IEnumerator FillLobby(UIMenuLobby lobby, int nb)
        {
            yield return new WaitForSecondsRealtime(3f);
            int ad = 0; for (int i = 0; i < nb - 1; i++) if (lobby.AddBot()) ad++;
            for (int i = 0; i < lobby.uiPlayers.Length; i++) { lobby.uiPlayers[i].gameObject.SetActive(true); lobby.uiPlayers[i].SetReadyState(UILobbyPlayer.ReadyState.Available, true); }
            Plugin.Log.LogInfo("AUTOMATCH lobby bots " + ad + " uiPlayers=" + lobby.uiPlayers.Length);
            yield return new WaitForSecondsRealtime(1.5f);
            var st = new System.Text.StringBuilder();
            for (int i = 0; i < lobby.uiPlayers.Length; i++) st.Append(i + ":" + lobby.uiPlayers[i].readyState + "/" + lobby.uiPlayers[i].playerType + "/active=" + lobby.uiPlayers[i].gameObject.activeSelf + " ");
            Plugin.Log.LogInfo("AUTOMATCH lobby states " + st);
            var sbd = new System.Text.StringBuilder(); Helpers.DumpRects(lobby.uiPlayers[0].transform.parent, 0, 2, sbd); Plugin.Log.LogInfo("RECTDUMP lobbyPlayers " + sbd);
        }

        void Begin(UIManager ui, UIMenuLobby lobby)
        {
            dir = Cfg.ShotDir.Value;
            if (Cfg.LobbyOnly.Value)
            {
                int nb = Cfg.AutoPlayers.Value;
                ui.PushMenu(lobby);
                StartCoroutine(FillLobby(lobby, nb));
                return;
            }
            Directory.CreateDirectory(dir);
            var s = Singleton<SettingsManager>.Instance;
            s.teamMatch = Cfg.AutoTeam.Value;
            s.matchType = (SettingsManager.MatchType)Enum.Parse(typeof(SettingsManager.MatchType), Cfg.AutoMatchType.Value);
            s.SelectVictoryCondition(0);
            int n = Cfg.AutoPlayers.Value;
            s.numBots = n;
            s.numPlayers = 0;
            s.botDifficulty = SettingsManager.BotDifficulty.medium;
            s.MatchSettings.includeTutorial = false;
            s.matchScoreGoal = Cfg.AutoScoreGoal.Value;
            lobby.ResetAllPlayers();
            int added = 0;
            for (int i = 0; i < n; i++) if (lobby.AddBot()) added++;
            if (s.teamMatch) for (int i = 0; i < lobby.uiPlayers.Length; i++) if (lobby.uiPlayers[i].readyState == UILobbyPlayer.ReadyState.Ready) lobby.uiPlayers[i].SetTeam(1 + (i % 2));
            Plugin.Log.LogInfo("AUTOMATCH bots added " + added + "/" + n + " team=" + s.teamMatch);
            StartCoroutine(ui.WaitForMatchStartThenClose());
        }
    }
}
