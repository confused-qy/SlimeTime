using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SlimeTime.UI
{
    /// <summary>
    /// Plays one shared sound for all registered UI buttons in the current scene.
    /// </summary>
    public class ButtonSoundManager : MonoBehaviour
    {
        private const string GlobalPlayerName = "Global Button Sound Player";
        private static AudioSource globalAudioSource;

        [Header("按钮")]
        [Tooltip("自动寻找这个物体及其所有子物体中的 Button，包括暂时隐藏的按钮。")]
        [SerializeField] private bool automaticallyFindChildButtons = true;

        [Tooltip("需要播放点击音效的按钮。自动查找未覆盖的按钮也可以手动添加到这里。")]
        [SerializeField] private List<Button> buttons = new List<Button>();

        [Header("音效")]
        [Tooltip("所有按钮共用的点击音效。")]
        [SerializeField] private AudioClip clickSound;

        [Tooltip("按钮点击音效的音量。")]
        [Range(0f, 1f)]
        [SerializeField] private float volume = 1f;

        private readonly List<Button> registeredButtons = new List<Button>();

        private void Awake()
        {
            EnsureGlobalAudioSource();
            RegisterButtons();
        }

        private static void EnsureGlobalAudioSource()
        {
            if (globalAudioSource != null)
            {
                return;
            }

            var playerObject = new GameObject(GlobalPlayerName);
            globalAudioSource = playerObject.AddComponent<AudioSource>();
            globalAudioSource.playOnAwake = false;
            globalAudioSource.loop = false;
            globalAudioSource.spatialBlend = 0f;
            globalAudioSource.volume = 1f;

            DontDestroyOnLoad(playerObject);
        }

        private void RegisterButtons()
        {
            var uniqueButtons = new HashSet<Button>();

            if (automaticallyFindChildButtons)
            {
                foreach (Button button in GetComponentsInChildren<Button>(true))
                {
                    uniqueButtons.Add(button);
                }
            }

            foreach (Button button in buttons)
            {
                if (button != null)
                {
                    uniqueButtons.Add(button);
                }
            }

            foreach (Button button in uniqueButtons)
            {
                button.onClick.AddListener(PlayClickSound);
                registeredButtons.Add(button);
            }
        }

        private void PlayClickSound()
        {
            EnsureGlobalAudioSource();

            if (clickSound != null)
            {
                globalAudioSource.PlayOneShot(clickSound, volume);
            }
        }

        private void OnDestroy()
        {
            foreach (Button button in registeredButtons)
            {
                if (button != null)
                {
                    button.onClick.RemoveListener(PlayClickSound);
                }
            }

            registeredButtons.Clear();
        }
    }
}
