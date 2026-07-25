using UnityEngine;
using TMPro;

namespace SlimeTime.Core
{
    /// <summary>
    /// Counts down from <see cref="timeLimit"/> and raises <see cref="OnTimeUp"/> when it
    /// hits zero. Its direction can be flipped with <see cref="Reverse"/> so it counts back
    /// up (capped at <see cref="timeLimit"/>). Player code subscribes to OnTimeUp to lose.
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
        [SerializeField] float _direction = -1f;   // -1 = counting down, +1 = counting up
        bool _ended;

        public float Remaining => _remaining;
        public bool Running => _running;
        public bool CountingUp => _direction > 0f;

        /// <summary>Flip the timer direction (counting down &lt;-&gt; counting up).</summary>
        public void Reverse() => _direction = -_direction;

        /// <summary>Force direction: true = count up (refill), false = count down.</summary>
        public void SetCountingUp(bool up) => _direction = up ? 1f : -1f;

        void OnEnable()
        {
            _remaining = timeLimit;
            _running = timeLimit > 0f;
            _ended = false;
            _direction = -1f;   // always start counting down
        }

        void Update()
        {
            if (!_running || _ended) return;

            _remaining += _direction * Time.deltaTime;

            if (_remaining <= 0f)
            {
                _remaining = 0f;
                _ended = true;
                _running = false;
                OnTimeUp?.Invoke();
            }
            // No upper cap: counting up can grow past timeLimit without limit.
        }

        void LateUpdate()
        {
            if (label == null) return;
            if (!gameObject.activeInHierarchy) return;
            label.text = $"{_remaining:0.0}";
        }
    }
}
