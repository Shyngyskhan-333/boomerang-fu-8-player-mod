using BepInEx.Configuration;
namespace BoomerangFu8P
{
    internal static class Cfg
    {
        public static ConfigEntry<bool> ProbeLevels, AutoMatch, AutoTeam, LobbyOnly;
        public static ConfigEntry<int> AutoPlayers, AutoScoreGoal;
        public static ConfigEntry<float> ShotInterval;
        public static ConfigEntry<string> ShotDir, AutoMatchType;
        public static void Init(ConfigFile c)
        {
            ProbeLevels = c.Bind("Debug", "ProbeLevels", false, "Load every level once at menu time and log spawn geometry (validation tool).");
            AutoMatch = c.Bind("Test", "AutoMatch", false, "Test driver: fill lobby with bots and start a match automatically.");
            LobbyOnly = c.Bind("Test", "LobbyOnly", false, "AutoMatch: open lobby, add bots, only take screenshots (no match start).");
            AutoMatchType = c.Bind("Test", "AutoMatchType", "DeathMatch", "AutoMatch: DeathMatch, GoldenDisc, KingOfTheHill or HideAndSeek.");
            AutoTeam = c.Bind("Test", "AutoTeam", false, "AutoMatch: team match.");
            AutoPlayers = c.Bind("Test", "AutoPlayers", 8, "AutoMatch: number of bots.");
            AutoScoreGoal = c.Bind("Test", "AutoScoreGoal", 2, "AutoMatch: points to win.");
            ShotInterval = c.Bind("Test", "ScreenshotInterval", 6f, "AutoMatch: seconds between screenshots.");
            ShotDir = c.Bind("Test", "ScreenshotDir", System.IO.Path.Combine(BepInEx.Paths.BepInExRootPath, "8p_shots"), "AutoMatch: screenshot folder.");
        }
    }
}
