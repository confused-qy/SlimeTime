using UnityEngine;
using TMPro;
using SlimeTime.Core;

namespace SlimeTime.Levels
{
    /// <summary>
    /// A platform that is solid/visible when the LevelTimer's remaining time is
    /// GREATER than <see cref="threshold"/>, and becomes translucent + non-solid
    /// when the timer drops below it.
    ///
    /// Requires a SpriteRenderer and a BoxCollider2D on the same GameObject.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(BoxCollider2D))]
    public class TimedPlatformGreaterThan : MonoBehaviour
    {
        [Tooltip("Platform is solid while remaining > threshold.")]
        public float threshold = 10f;

        [Tooltip("Alpha when hidden (0 = invisible, 1 = opaque).")]
        [Range(0f, 1f)] public float hiddenAlpha = 0.25f;

        [Tooltip("Optional child TMP_Text that displays the threshold value.")]
        public TMP_Text label;

        SpriteRenderer _sr;
        BoxCollider2D _col;
        LevelTimer _timer;
        Color _opaqueColor;
        Color _hiddenColor;

        void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
            _col = GetComponent<BoxCollider2D>();
            _opaqueColor = _sr.color;
            _hiddenColor = new Color(_opaqueColor.r, _opaqueColor.g, _opaqueColor.b, hiddenAlpha);
        }

        void Start()
        {
            if (label != null) label.text = $">{threshold:0}";
        }

        void Update()
        {
            if (_timer == null)
            {
                _timer = FindFirstObjectByType<LevelTimer>();
                if (_timer == null) return;
            }

            bool solid = _timer.Remaining > threshold;
            _sr.color = solid ? _opaqueColor : _hiddenColor;
            _col.enabled = solid;
        }
    }
}
