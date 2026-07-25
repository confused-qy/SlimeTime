using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.UI;
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

        [Header("Title Image")]
        [Tooltip("The Image used to display the Pause, Win, or Lost title.")]
        [SerializeField] Image titleImage;
        [SerializeField] Sprite pauseTitleSprite;
        [SerializeField] Sprite winTitleSprite;
        [SerializeField] Sprite lostTitleSprite;

        [Header("Buttons (assign the button GameObjects)")]
        [SerializeField] GameObject resumeButton;       // Pause only
        [SerializeField] GameObject retryButton;        // Lost only
        [SerializeField] GameObject nextLevelButton;    // Win only, and only if a next level exists
        [SerializeField] GameObject levelSelectButton;  // all modes
        [SerializeField] GameObject quitButton;         // all modes

        [Header("Scene")]
        [SerializeField] string levelSelectSceneName = "LevelSelect";

        [Header("ESC 音效")]
        [Tooltip("按 ESC 打开或关闭暂停菜单时播放的音效。")]
        [SerializeField] AudioClip escapeSound;

        [Tooltip("ESC 音效的音量。")]
        [Range(0f, 1f)]
        [SerializeField] float escapeSoundVolume = 1f;

        [Tooltip("用于播放 ESC 音效的 Audio Source。不设置时会自动创建。")]
        [SerializeField] AudioSource escapeAudioSource;

        Mode mode = Mode.None;
        string nextLevelSceneName = "";

        void Awake()
        {
            //SlimeTime.Core.GameProgress.ResetAll();   // TEMP: reset unlock progress for testing
            if (panel != null) panel.SetActive(false);
            Time.timeScale = 1f;
            PrepareEscapeAudioSource();
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

            if (mode == Mode.Pause)
            {
                PlayEscapeSound();
                Resume();
            }
            else if (mode == Mode.None)
            {
                PlayEscapeSound();
                Show(Mode.Pause);
            }
            // Esc is ignored while Win/Lost is showing (the round is over).
        }

        void PrepareEscapeAudioSource()
        {
            if (escapeAudioSource == null)
                escapeAudioSource = GetComponent<AudioSource>();

            if (escapeAudioSource == null)
                escapeAudioSource = gameObject.AddComponent<AudioSource>();

            escapeAudioSource.playOnAwake = false;
            escapeAudioSource.loop = false;
            escapeAudioSource.spatialBlend = 0f;
        }

        void PlayEscapeSound()
        {
            if (escapeAudioSource != null && escapeSound != null)
                escapeAudioSource.PlayOneShot(escapeSound, escapeSoundVolume);
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

            if (titleImage != null)
            {
                titleImage.sprite = m == Mode.Pause ? pauseTitleSprite
                                  : m == Mode.Win ? winTitleSprite
                                  : lostTitleSprite;
                titleImage.enabled = titleImage.sprite != null;
            }

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
