using UnityEngine;
using SlimeTime.Core;

/// <summary>
/// Drives the slime's size from the level timer, and handles being crushed.
/// Replaces SlimeScaleByTimer + SlimeCrushDeath. Attach to the object whose scale
/// (and collider) should change — the slime root.
///
///  - Size = original scale * (Remaining / secondsToShrinkToZero), unclamped so the
///    slime can grow or shrink without limit as Remaining goes above/below that
///    reference value (e.g. after reversing the timer through TimeReverseGate).
///  - While squeezed between walls on BOTH sides, GROWTH IS PAUSED (it may still shrink).
///  - If it stays squeezed while trying to grow for crushTime seconds, the slime dies.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class SlimeSizeController : MonoBehaviour
{
    /// <summary>Raised once when the slime is crushed. GameMenu listens to show Game Over.</summary>
    public static event System.Action OnDied;

    [Header("Timer")]
    [Tooltip("The level timer. Auto-found in the scene if empty.")]
    [SerializeField] private LevelTimer timer;

    [Header("Size (multipliers of the original scale)")]
    [Tooltip("At this many seconds of Remaining, the slime is at 1x (its original size). " +
             "Size scales linearly with the timer: factor = Remaining / secondsToShrinkToZero. " +
             "Remaining = 0 -> 0x, Remaining = 2*secondsToShrinkToZero -> 2x, etc.")]
    [SerializeField] private float secondsToShrinkToZero = 10f;
    [Tooltip("How quickly it eases toward the target size. 0 = instant.")]
    [SerializeField] private float smoothing = 5f;

    [Header("Crush")]
    [Tooltip("Seconds squeezed on both sides (while trying to grow) before dying.")]
    [SerializeField] private float crushTime = 2f;
    [Tooltip("Contact-normal threshold. 0.5 = within 60 degrees of an axis counts as a pinching wall.")]
    [SerializeField] private float sideThreshold = 0.5f;

    private Vector3 baseScale;
    private float squeezeTimer;
    private bool died;

    // Cached from OnCollisionStay2D so Update always sees fresh contacts.
    private bool hasLeft, hasRight, hasUp, hasDown;

    private void Awake()
    {
        baseScale = transform.localScale;
        if (timer == null) timer = FindAnyObjectByType<LevelTimer>();
    }

    private void Update()
    {
        if (died || timer == null || secondsToShrinkToZero <= 0f) return;

        // Target size from the timer (unclamped -> can grow or shrink without limit).
        float targetFactor = timer.Remaining / secondsToShrinkToZero;

        // Is the target bigger than we are right now? (i.e. it wants to grow)
        float currentFactor = baseScale.x != 0f ? transform.localScale.x / baseScale.x : targetFactor;
        bool wantsToGrow = targetFactor > currentFactor + 0.0001f;

        Vector3 target = baseScale * targetFactor;

        bool squeezed = IsSqueezed();

        if (wantsToGrow && squeezed)
        {
            // Squeezed: pause growth (hold current size) and count toward crush death.
            target = transform.localScale;

            squeezeTimer += Time.deltaTime;   // deltaTime is 0 while paused, so this freezes too
            if (squeezeTimer >= crushTime) { Die(); return; }
        }
        else
        {
            squeezeTimer = 0f;   // not squeezed, or shrinking -> reset the count
        }

        transform.localScale = smoothing > 0f
            ? Vector3.Lerp(transform.localScale, target, 1f - Mathf.Exp(-smoothing * Time.deltaTime))
            : target;
    }

    // True when walls pinch the slime on opposite sides: left+right OR top+bottom.
    // Reads the cached flags filled by OnCollisionStay2D (runs after physics step, so
    // contacts are guaranteed fresh — unlike calling GetContacts in Update).
    private bool IsSqueezed()
    {
        return (hasLeft && hasRight) || (hasUp && hasDown);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        // OR-accumulate across all colliders this frame (slime has 3 CapsuleCollider2Ds;
        // each one fires its own callback, and we don't want later ones to wipe earlier).
        int n = collision.contactCount;
        for (int i = 0; i < n; i++)
        {
            ContactPoint2D c = collision.GetContact(i);
            if (c.normal.x > sideThreshold) hasLeft = true;    // wall on the left
            if (c.normal.x < -sideThreshold) hasRight = true;  // wall on the right
            if (c.normal.y > sideThreshold) hasDown = true;    // floor below
            if (c.normal.y < -sideThreshold) hasUp = true;     // ceiling above
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        // Hard to know what flags are still valid, so clear all and let the next
        // OnCollisionStay2D rebuild them.
        hasLeft = hasRight = hasUp = hasDown = false;
    }

    // Reset the cached flags once per physics step so we don't carry stale "squeezed"
    // state from a frame that has since moved on. OnCollisionStay2D will refill them.
    private void FixedUpdate()
    {
        hasLeft = hasRight = hasUp = hasDown = false;
    }

    private void Die()
    {
        died = true;
        OnDied?.Invoke();
    }
}
