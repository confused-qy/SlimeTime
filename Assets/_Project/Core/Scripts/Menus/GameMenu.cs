using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;
using SlimeTime.Core;

namespace SlimeTime.UI
{
    /// <summary>
    /// One panel for all three states: Pause, Win, Lost. Replaces PauseMenu + WinPanel.
    ///   - Esc              -> Pause (only during normal play)
    ///   - GoalZone.OnWin   -> Win
    ///   - LevelTimer.OnTimeUp -> Lost
    /// The same buttons are reused; each is shown only in the modes that need it.
    /// Put this on an ALWAYS-ACTIVE object (the Canvas) and let it toggle the child 'panel'.
    /// </summary>
    public class GameMenu : MonoBehaviour
    {
        enum Mode { None, Pause, Win, Lost }

        [Header("Panel")]
        [Tooltip("The whole menu root, hidden during normal play.")]
        [SerializeField] GameObject panel;
        [SerializeField] TMP_Text titleLabel;

        [Header("Buttons (assign the button GameObjects)")]
        [SerializeField] GameObject resumeButton;       // Pause only
        [SerializeField] GameObject retryButton;        // Lost only
        [SerializeField] GameObject nextLevelButton;    // Win only, and only if a next level exists
        [SerializeField] GameObject levelSelectButton;  // all modes
        [SerializeField] GameObject quitButton;         // all modes

        [Header("Scene")]
        [SerializeField] string levelSelectSceneName = "LevelSelect";

        [Header("Titles")]
        [SerializeField] string pauseTitle = "Paused";
        [SerializeField] string winTitle = "You Win!";
        [SerializeField] string lostTitle = "Game Over";

        Mode mode = Mode.None;
        string nextLevelSceneName = "";

        void Awake()
        {
            //SlimeTime.Core.GameProgress.ResetAll();   // TEMP: reset unlock progress for testing
            if (panel != null) panel.SetActive(false);
            Time.timeScale = 1f;
        }

        void OnEnable()
        {
            GoalZone.OnWin += HandleWin;
            LevelTimer.OnTimeUp += HandleLost;
            SlimeSizeController.OnDied += HandleLost;
        }

        void OnDisable()
        {
            GoalZone.OnWin -= HandleWin;
            LevelTimer.OnTimeUp -= HandleLost;
            SlimeSizeController.OnDied -= HandleLost;
        }

        void Update()
        {
            var kb = Keyboard.current;
            if (kb == null || !kb.escapeKey.wasPressedThisFrame) return;

            if (mode == Mode.Pause) Resume();
            else if (mode == Mode.None) Show(Mode.Pause);
            // Esc is ignored while Win/Lost is showing (the round is over).
        }

        void HandleWin(string nextScene)
        {
            if (mode == Mode.Win || mode == Mode.Lost) return;  // already ended
            nextLevelSceneName = nextScene;
            Show(Mode.Win);
        }

        void HandleLost()
        {
            if (mode == Mode.Win || mode == Mode.Lost) return;  // already ended
            Show(Mode.Lost);
        }

        void Show(Mode m)
        {
            mode = m;
            if (panel != null) panel.SetActive(true);
            Time.timeScale = 0f;   // freeze the game in every mode

            if (titleLabel != null)
                titleLabel.text = m == Mode.Pause ? pauseTitle
                                : m == Mode.Win ? winTitle
                                : lostTitle;

            SetActive(resumeButton,      m == Mode.Pause);
            SetActive(retryButton,       m == Mode.Lost);
            SetActive(nextLevelButton,   m == Mode.Win && !string.IsNullOrEmpty(nextLevelSceneName));
            SetActive(levelSelectButton, true);
            SetActive(quitButton,        true);
        }

        static void SetActive(GameObject go, bool on)
        {
            if (go != null) go.SetActive(on);
        }

        // ---------- Button callbacks (wire to the UI Buttons' OnClick) ----------

        public void OnResumeButton()
        {
            Resume();
        }

        void Resume()
        {
            mode = Mode.None;
            if (panel != null) panel.SetActive(false);
            Time.timeScale = 1f;
        }

        public void OnRetryButton()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);  // reload current level
        }

        public void OnNextLevelButton()
        {
            if (string.IsNullOrEmpty(nextLevelSceneName)) return;
            Time.timeScale = 1f;
            SceneManager.LoadScene(nextLevelSceneName);
        }

        public void OnLevelSelectButton()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(levelSelectSceneName);
        }

        public void OnQuitButton()
        {
            Time.timeScale = 1f;
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
