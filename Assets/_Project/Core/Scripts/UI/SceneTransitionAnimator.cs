using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SlimeTime.UI
{
    /// <summary>
    /// Animates a scene's UI content when entering or leaving the scene.
    /// </summary>
    public class SceneTransitionAnimator : MonoBehaviour
    {
        [Header("播放设置")]
        [InspectorName("进入场景时播放动画")]
        [Tooltip("开启后，进入这个场景时会自动播放内容淡入和 Q 弹动画。")]
        [SerializeField] private bool playEntranceOnStart = true;

        [Header("动画内容")]
        [InspectorName("内容根物体")]
        [Tooltip("拖入需要一起播放动画的 UI 根物体，例如包含标题和所有按钮的 MenuContent。")]
        [SerializeField] private RectTransform contentRoot;

        [InspectorName("内容初始缩放")]
        [Tooltip("入场动画开始时内容的缩放比例。建议保持在 0.95 到 0.98 之间。")]
        [Range(0.9f, 1f)]
        [SerializeField] private float entranceStartScale = 0.96f;

        [InspectorName("入场上移距离")]
        [Tooltip("内容从下方向上移动的像素距离。")]
        [Min(0f)]
        [SerializeField] private float entranceMoveDistance = 24f;

        [Header("动画时间")]
        [InspectorName("离场动画时间")]
        [Tooltip("点击切换场景后，当前内容缩小并淡出所需的时间（秒）。")]
        [Min(0.01f)]
        [SerializeField] private float coverDuration = 0.35f;

        [InspectorName("入场动画时间")]
        [Tooltip("进入新场景后，内容淡入并 Q 弹到正常大小所需的时间（秒）。")]
        [Min(0.01f)]
        [SerializeField] private float revealDuration = 0.55f;

        private Vector3 contentOriginalScale;
        private Vector2 contentOriginalPosition;
        private bool transitioning;

        private void Awake()
        {
            if (contentRoot == null)
            {
                Debug.LogError(
                    $"{nameof(SceneTransitionAnimator)} on '{name}' needs a Content Root.",
                    this);
                enabled = false;
                return;
            }

            contentOriginalScale = contentRoot.localScale;
            contentOriginalPosition = contentRoot.anchoredPosition;

            if (playEntranceOnStart)
            {
                contentRoot.localScale = contentOriginalScale * entranceStartScale;
                contentRoot.anchoredPosition =
                    contentOriginalPosition + Vector2.down * entranceMoveDistance;
            }
            else
            {
                RestoreContent();
            }
        }

        private void Start()
        {
            if (playEntranceOnStart)
            {
                StartCoroutine(PlayEntrance());
            }
        }

        /// <summary>
        /// Covers the current scene, then loads the requested scene.
        /// </summary>
        public void LoadScene(string sceneName)
        {
            if (transitioning || string.IsNullOrEmpty(sceneName))
            {
                return;
            }

            StartCoroutine(CoverAndLoad(sceneName));
        }

        private IEnumerator PlayEntrance()
        {
            transitioning = true;
            float elapsed = 0f;

            while (elapsed < revealDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = Mathf.Clamp01(elapsed / revealDuration);

                float scaleProgress = EaseOutBack(progress);
                float scale = Mathf.LerpUnclamped(entranceStartScale, 1f, scaleProgress);
                contentRoot.localScale = contentOriginalScale * scale;
                contentRoot.anchoredPosition = Vector2.LerpUnclamped(
                    contentOriginalPosition + Vector2.down * entranceMoveDistance,
                    contentOriginalPosition,
                    EaseOutCubic(progress));

                yield return null;
            }

            RestoreContent();
            transitioning = false;
        }

        private IEnumerator CoverAndLoad(string sceneName)
        {
            transitioning = true;

            Vector3 startScale = contentRoot.localScale;
            Vector2 startPosition = contentRoot.anchoredPosition;
            float elapsed = 0f;

            while (elapsed < coverDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = Mathf.Clamp01(elapsed / coverDuration);
                float smoothProgress = SmoothStep(progress);
                contentRoot.localScale = Vector3.Lerp(
                    startScale,
                    contentOriginalScale * entranceStartScale,
                    smoothProgress);
                contentRoot.anchoredPosition = Vector2.Lerp(
                    startPosition,
                    contentOriginalPosition + Vector2.down * entranceMoveDistance,
                    smoothProgress);
                yield return null;
            }

            Time.timeScale = 1f;
            yield return null;

            AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneName);
            while (!loadOperation.isDone)
            {
                yield return null;
            }
        }

        private void RestoreContent()
        {
            contentRoot.localScale = contentOriginalScale;
            contentRoot.anchoredPosition = contentOriginalPosition;
        }

        private void OnDisable()
        {
            if (contentRoot != null)
            {
                RestoreContent();
            }
        }

        private static float SmoothStep(float value)
        {
            return value * value * (3f - 2f * value);
        }

        private static float EaseOutCubic(float value)
        {
            float inverse = 1f - value;
            return 1f - inverse * inverse * inverse;
        }

        private static float EaseOutBack(float value)
        {
            const float overshoot = 1.70158f;
            float shifted = value - 1f;
            return 1f + (overshoot + 1f) * shifted * shifted * shifted
                   + overshoot * shifted * shifted;
        }
    }
}
