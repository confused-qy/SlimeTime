using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using SlimeTime.Core;

namespace SlimeTime.UI
{
    /// <summary>
    /// Level select screen. Each level has two buttons (unlocked + locked) — exactly
    /// one is shown at a time depending on <see cref="GameProgress.IsUnlocked"/>.
    /// Includes a Back button that returns to the main menu.
    /// </summary>
    public class LevelSelectUI : MonoBehaviour
    {
        [System.Serializable]
        public class LevelEntry
        {
            [Tooltip("Display name shown on the button (e.g. 'Level 1').")]
            public string displayName = "Level";
            [Tooltip("Scene to load when this level is picked.")]
            public string sceneName = "";
            [Tooltip("Button shown when the level is unlocked. Wire its OnClick to LoadCurrent().")]
            public Button unlockedButton;
            [Tooltip("Button shown when the level is NOT unlocked. Visual only — no click.")]
            public Button lockedButton;
            [Tooltip("Optional label inside either button.")]
            public TMP_Text unlockedLabel;
            public TMP_Text lockedLabel;
        }

        [Tooltip("One entry per level. First level (index 0) is always shown as unlocked.")]
        public LevelEntry[] levels = new LevelEntry[3];

        [Header("Navigation")]
        [Tooltip("Back button — returns to the main menu. Wire its OnClick to OnBackButton().")]
        [SerializeField] private Button backButton;
        [Tooltip("Scene to load when Back is pressed.")]
        [SerializeField] private string mainMenuSceneName = "MainMenu";

        void Start()
        {
            for (int i = 0; i < levels.Length; i++)
            {
                var entry = levels[i];
                if (entry == null) continue;

                if (entry.unlockedLabel != null) entry.unlockedLabel.text = entry.displayName;
                if (entry.lockedLabel != null) entry.lockedLabel.text = entry.displayName;

                // First level is always unlocked; the rest follow GameProgress.
                bool unlocked = i == 0 || GameProgress.IsUnlocked(entry.sceneName);

                // Show only the matching button.
                if (entry.unlockedButton != null) entry.unlockedButton.gameObject.SetActive(unlocked);
                if (entry.lockedButton != null) entry.lockedButton.gameObject.SetActive(!unlocked);

                if (unlocked && entry.unlockedButton != null)
                {
                    var scene = entry.sceneName;
                    entry.unlockedButton.onClick.AddListener(() => LoadScene(scene));
                }
            }

            if (backButton != null)
            {
                backButton.onClick.AddListener(OnBackButton);
            }
        }

        public void OnBackButton()
        {
            if (string.IsNullOrEmpty(mainMenuSceneName)) return;
            Time.timeScale = 1f;
            SceneManager.LoadScene(mainMenuSceneName);
        }

        void LoadScene(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName)) { Debug.LogError("LevelSelectUI: sceneName is empty."); return; }
            Time.timeScale = 1f;
            SceneManager.LoadScene(sceneName);
        }
    }
}

