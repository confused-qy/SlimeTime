using UnityEngine;

namespace SlimeTime.Core
{
    /// <summary>
    /// Persistent level-unlock progress, keyed by scene name and stored in
    /// PlayerPrefs (survives quitting the game). Used by the pause menu to
    /// decide whether the "Next Level" option should be offered.
    /// </summary>
    public static class GameProgress
    {
        const string Prefix = "level_unlocked_";

        

        /// <summary>Is the given level scene unlocked?</summary>
        public static bool IsUnlocked(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName)) return false;
            return PlayerPrefs.GetInt(Prefix + sceneName, 0) == 1;
        }

        /// <summary>
        /// Unlock a level scene. Call this when the player beats the level that
        /// comes before it, e.g. GameProgress.Unlock("Level_002").
        /// </summary>
        public static void Unlock(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName)) return;
            PlayerPrefs.SetInt(Prefix + sceneName, 1);
            PlayerPrefs.Save();
        }

        /// <summary>Wipe all unlock progress (handy while testing).</summary>
        public static void ResetAll()
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
        }
    }
}
