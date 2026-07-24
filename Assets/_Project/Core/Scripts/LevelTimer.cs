using UnityEngine;
using TMPro;

namespace SlimeTime.Core
{
    /// <summary>
    /// Counts down from <see cref="timeLimit"/>. Raises <see cref="OnTimeUp"/>
    /// when it hits zero. Player code subscribes to OnTimeUp to handle losing.
    /// </summary>
    public class LevelTimer : MonoBehaviour
    {
        public static event System.Action OnTimeUp;

        [Header("Config (set in Inspector per scene)")]
        [Tooltip("Time limit in seconds. <= 0 disables the timer.")]
        public float timeLimit = 60f;

        [Tooltip("Optional UI label. Leave null if you don't want a HUD timer.")]
        public TMP_Text label;

        [Header("State (read-only)")]
        [SerializeField] float _remaining;
        [SerializeField] bool _running;
        bool _ended;

        public float Remaining => _remaining;
        public bool Running => _running;

        void OnEnable()
        {
            _remaining = timeLimit;
            _running = timeLimit > 0f;
            _ended = false;
        }

        void Update()
        {
            if (!_running || _ended) return;
            _remaining -= Time.deltaTime;
            if (_remaining <= 0f)
            {
                _remaining = 0f;
                _ended = true;
                _running = false;
                OnTimeUp?.Invoke();
            }
        }

        void LateUpdate()
        {
            if (label == null) return;
            if (!gameObject.activeInHierarchy) return;
            label.text = $"{_remaining:0.0}";
        }
    }
}
