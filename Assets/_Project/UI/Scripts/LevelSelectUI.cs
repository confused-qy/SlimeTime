using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

namespace SlimeTime.UI
{
    /// <summary>
    /// 3 buttons on the level select screen, each loads a level scene.
    /// </summary>
    public class LevelSelectUI : MonoBehaviour
    {
        [System.Serializable]
        public class LevelEntry
        {
            public string displayName = "Level";
            public string sceneName = "";
            public Button button;
            public TMP_Text nameLabel;
        }

        public LevelEntry[] levels = new LevelEntry[3];

        void Start()
        {
            for (int i = 0; i < levels.Length; i++)
            {
                var entry = levels[i];
                if (entry == null) continue;

                if (entry.nameLabel != null) entry.nameLabel.text = entry.displayName;

                if (entry.button != null)
                {
                    var scene = entry.sceneName;
                    entry.button.onClick.AddListener(() => LoadScene(scene));
                }
            }
        }

        void LoadScene(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName)) { Debug.LogError("LevelSelectUI: sceneName is empty."); return; }
            SceneManager.LoadScene(sceneName);
        }
    }
}
