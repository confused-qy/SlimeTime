using UnityEngine;
using UnityEngine.SceneManagement;

namespace SlimeTime.UI
{
    /// <summary>
    /// Main menu buttons: Play loads the first level, Select Level opens the level
    /// select screen, Exit quits the game. Wire OnPlayButton / OnLevelSelectButton /
    /// OnExitButton to the buttons' OnClick events.
    /// </summary>
    public class MainMenu : MonoBehaviour
    {
        [Tooltip("Scene name of the level select screen to load on the Select Level button.")]
        [SerializeField] private string levelSelectSceneName = "LevelSelect";

        [Tooltip("Optional animated scene transition. Falls back to immediate loading if omitted.")]
        [SerializeField] private SceneTransitionAnimator sceneTransition;

        public void OnLevelSelectButton()
        {
            Time.timeScale = 1f;                          // in case it was left paused

            if (sceneTransition != null)
                sceneTransition.LoadScene(levelSelectSceneName);
            else
                SceneManager.LoadScene(levelSelectSceneName);
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
