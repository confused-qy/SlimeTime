using UnityEngine;
using UnityEngine.SceneManagement;

namespace SlimeTime.UI
{
    /// <summary>
    /// Main menu buttons: Credits opens the credits scene and Exit quits the game.
    /// </summary>
    public class MainMenu : MonoBehaviour
    {
        [Header("场景")]
        [Tooltip("点击 Play 按钮后加载的关卡选择场景名称。")]
        [SerializeField] private string levelSelectionSceneName = "LevelSelect";

        [Tooltip("点击 Credits 按钮后加载的场景名称。")]
        [SerializeField] private string creditsSceneName = "Credits";

        [Tooltip("场景切换动画。不设置时会直接加载场景。")]
        [SerializeField] private SceneTransitionAnimator sceneTransition;

        public void OnLevelSelectionButton()
        {
            LoadScene(levelSelectionSceneName);
        }

        public void OnCreditsButton()
        {
            LoadScene(creditsSceneName);
        }

        private void LoadScene(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogError($"{nameof(MainMenu)} 没有设置需要加载的场景名称。", this);
                return;
            }

            Time.timeScale = 1f;

            if (sceneTransition != null)
                sceneTransition.LoadScene(sceneName);
            else
                SceneManager.LoadScene(sceneName);
        }

        public void OnExitButton()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;  // stop Play mode in the editor
#else
            Application.Quit();                                // quit the built game
#endif
        }
    }
}
