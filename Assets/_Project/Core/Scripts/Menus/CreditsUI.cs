using UnityEngine;
using UnityEngine.SceneManagement;

namespace SlimeTime.UI
{
    /// <summary>
    /// Controls navigation from the Credits scene back to the main menu.
    /// </summary>
    public class CreditsUI : MonoBehaviour
    {
        [Header("返回设置")]
        [Tooltip("点击返回按钮后加载的主菜单场景名称。")]
        [SerializeField] private string mainMenuSceneName = "MainMenu";

        [Tooltip("Credits 页面的场景切换动画。不设置时会直接返回主菜单。")]
        [SerializeField] private SceneTransitionAnimator sceneTransition;

        public void OnBackButton()
        {
            if (string.IsNullOrEmpty(mainMenuSceneName))
            {
                Debug.LogError($"{nameof(CreditsUI)} 没有设置主菜单场景名称。", this);
                return;
            }

            Time.timeScale = 1f;

            if (sceneTransition != null)
                sceneTransition.LoadScene(mainMenuSceneName);
            else
                SceneManager.LoadScene(mainMenuSceneName);
        }
    }
}
