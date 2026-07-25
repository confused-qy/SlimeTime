using UnityEngine;
using SlimeTime.Core;

/// <summary>
/// A wall the slime can pass THROUGH. Passing through flips the level timer's
/// direction (counting down &lt;-&gt; counting up). Put a Collider2D set as
/// "Is Trigger" on it so the slime passes through instead of colliding.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class TimeReverseGate : MonoBehaviour
{
    [Tooltip("The timer to reverse. If empty, the first LevelTimer in the scene is used.")]
    [SerializeField] private LevelTimer timer;

    [Tooltip("Seconds to ignore repeat triggers, so one pass only flips once.")]
    [SerializeField] private float cooldown = 0.3f;

    [Tooltip("How many times the slime can pass through before this gate disappears. 0 = never disappears.")]
    [SerializeField] private int passesBeforeDisappear = 0;

    private float lastTriggerTime = -999f;
    private int passCount;

    // Makes the collider a trigger automatically when the component is added.
    private void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void Awake()
    {
        if (timer == null) timer = FindAnyObjectByType<LevelTimer>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Only the slime triggers it (detected by its SlimeMovement component).
        if (other.GetComponentInParent<SlimeMovement>() == null) return;

        // Debounce so a single pass doesn't flip twice.
        if (Time.time - lastTriggerTime < cooldown) return;
        lastTriggerTime = Time.time;

        if (timer != null) timer.Reverse();

        passCount++;
        if (passesBeforeDisappear > 0 && passCount >= passesBeforeDisappear)
            Destroy(gameObject);   // gate used up -> disappears
    }
}
