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
    [Header("触发范围")]
    [Tooltip("自动让 Box Collider 2D 覆盖 Gate 图片的完整范围。")]
    [SerializeField] private bool fitColliderToSprite = true;

    [Tooltip("在图片范围之外额外增加的触发距离。")]
    [Min(0f)]
    [SerializeField] private float triggerPadding = 0.05f;

    [Header("时间反转")]
    [Tooltip("The timer to reverse. If empty, the first LevelTimer in the scene is used.")]
    [SerializeField] private LevelTimer timer;

    [Tooltip("Seconds to ignore repeat triggers, so one pass only flips once.")]
    [SerializeField] private float cooldown = 0.3f;

    [Tooltip("How many times the slime can pass through before this gate disappears. 0 = never disappears.")]
    [SerializeField] private int passesBeforeDisappear = 0;

    [Header("音效")]
    [Tooltip("史莱姆触发 Gate 时播放的音效。")]
    [SerializeField] private AudioClip triggerSound;

    [Tooltip("Gate 触发音效的音量。")]
    [Range(0f, 1f)]
    [SerializeField] private float triggerSoundVolume = 1f;

    private float lastTriggerTime = -999f;
    private int passCount;

    // Makes the collider a trigger automatically when the component is added.
    private void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
        FitColliderToSprite();
    }

    private void OnValidate()
    {
        Collider2D gateCollider = GetComponent<Collider2D>();
        if (gateCollider != null)
            gateCollider.isTrigger = true;

        FitColliderToSprite();
    }

    private void Awake()
    {
        if (timer == null) timer = FindAnyObjectByType<LevelTimer>();
        FitColliderToSprite();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Only the slime triggers it (detected by its SlimeMovement component).
        if (other.GetComponentInParent<SlimeMovement>() == null) return;

        // Debounce so a single pass doesn't flip twice.
        if (Time.time - lastTriggerTime < cooldown) return;
        lastTriggerTime = Time.time;

        PlayTriggerSound();

        if (timer != null) timer.Reverse();

        passCount++;
        if (passesBeforeDisappear > 0 && passCount >= passesBeforeDisappear)
            Destroy(gameObject);   // gate used up -> disappears
    }

    private void FitColliderToSprite()
    {
        if (!fitColliderToSprite) return;
        if (!TryGetComponent(out BoxCollider2D boxCollider)) return;
        if (!TryGetComponent(out SpriteRenderer spriteRenderer)) return;
        if (spriteRenderer.sprite == null) return;

        Bounds spriteBounds = spriteRenderer.sprite.bounds;
        boxCollider.offset = spriteBounds.center;
        boxCollider.size = new Vector2(
            spriteBounds.size.x + triggerPadding * 2f,
            spriteBounds.size.y + triggerPadding * 2f);
    }

    private void PlayTriggerSound()
    {
        if (triggerSound == null) return;

        // The gate may destroy itself immediately after triggering, so play the
        // sound on a temporary object that can remain until the clip finishes.
        var soundObject = new GameObject("Gate Trigger Sound");
        var source = soundObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = false;
        source.spatialBlend = 0f;
        source.volume = 1f;
        source.PlayOneShot(triggerSound, triggerSoundVolume);

        Destroy(soundObject, triggerSound.length + 0.1f);
    }
}
