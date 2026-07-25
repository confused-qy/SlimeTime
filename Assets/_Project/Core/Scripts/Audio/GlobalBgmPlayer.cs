using UnityEngine;

namespace SlimeTime.Audio
{
    /// <summary>
    /// Keeps one BGM AudioSource alive while scenes change.
    /// Duplicate players are destroyed automatically.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    [RequireComponent(typeof(AudioSource))]
    public class GlobalBgmPlayer : MonoBehaviour
    {
        public static GlobalBgmPlayer Instance { get; private set; }

        [Header("背景音乐")]
        [Tooltip("需要在所有场景中持续播放的背景音乐。")]
        [SerializeField] private AudioClip bgmClip;

        [Tooltip("背景音乐音量。")]
        [Range(0f, 1f)]
        [SerializeField] private float volume = 0.5f;

        [Header("组件")]
        [Tooltip("同一个物体上的 Audio Source。不设置时会自动寻找。")]
        [SerializeField] private AudioSource audioSource;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
            }

            ConfigureAudioSource();
        }

        private void Start()
        {
            if (bgmClip == null)
            {
                Debug.LogWarning($"{nameof(GlobalBgmPlayer)} 没有设置背景音乐。", this);
                return;
            }

            if (!audioSource.isPlaying)
            {
                audioSource.Play();
            }
        }

        private void ConfigureAudioSource()
        {
            audioSource.playOnAwake = false;
            audioSource.loop = true;
            audioSource.spatialBlend = 0f;
            audioSource.volume = volume;
            audioSource.clip = bgmClip;
        }

        public void SetVolume(float newVolume)
        {
            volume = Mathf.Clamp01(newVolume);

            if (audioSource != null)
            {
                audioSource.volume = volume;
            }
        }

        public void Pause()
        {
            if (audioSource != null)
            {
                audioSource.Pause();
            }
        }

        public void Resume()
        {
            if (audioSource != null && bgmClip != null)
            {
                audioSource.UnPause();
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
