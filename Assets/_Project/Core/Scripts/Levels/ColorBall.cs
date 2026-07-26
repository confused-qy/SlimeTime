using UnityEngine;

/// <summary>
/// A collectible ball. When the slime touches it, the walls bound to this ball
/// (<see cref="wallsToReveal"/>) are activated, then the ball disappears.
///
/// The walls should start INACTIVE in the scene (uncheck the box at the top-left of
/// their Inspector) so they don't exist until the ball is eaten. Each ball can carry
/// its own colour and its own set of walls, so different-coloured balls reveal
/// different walls.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class ColorBall : MonoBehaviour
{
    [Header("触发范围")]
    [Tooltip("自动让 Box Collider 2D 覆盖按钮图片的完整范围。")]
    [SerializeField] private bool fitColliderToSprite = true;

    [Tooltip("在按钮图片范围之外额外增加的触发距离。")]
    [Min(0f)]
    [SerializeField] private float triggerPadding = 0.05f;

    [Header("Colour")]
    [Tooltip("This ball's colour. Optionally applied to the ball and its walls so they visually match.")]
    [SerializeField] private Color color = Color.white;
    [Tooltip("Tint this ball's sprite to the colour above.")]
    [SerializeField] private bool tintBall = true;
    [Tooltip("Tint the revealed walls to the same colour.")]
    [SerializeField] private bool tintWalls = true;

    [Header("Behaviour")]
    [Tooltip("On: eating flips the walls on/off, so a second same-colour ball hides them again. " +
             "Off: eating only ever reveals them.")]
    [SerializeField] private bool toggle = true;

    [Header("Walls bound to this ball")]
    [Tooltip("Walls that appear when this ball is eaten. They should start INACTIVE in the scene.")]
    [SerializeField] private GameObject[] wallsToReveal;

    [Header("音效")]
    [Tooltip("史莱姆碰到这个彩色按钮时播放的音效。")]
    [SerializeField] private AudioClip triggerSound;

    [Tooltip("彩色按钮触发音效的音量。")]
    [Range(0f, 1f)]
    [SerializeField] private float triggerSoundVolume = 1f;

    // Makes the collider a trigger automatically when the component is added.
    private void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
        FitColliderToSprite();
    }

    private void OnValidate()
    {
        Collider2D buttonCollider = GetComponent<Collider2D>();
        if (buttonCollider != null)
            buttonCollider.isTrigger = true;

        FitColliderToSprite();
    }

    private void Awake()
    {
        FitColliderToSprite();

        if (tintBall)
        {
            var sr = GetComponentInChildren<SpriteRenderer>();
            if (sr != null) sr.color = color;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Only the slime can eat the ball (detected by its SlimeMovement component).
        if (other.GetComponentInParent<SlimeMovement>() == null) return;
        Eat();
    }

    private bool _eaten;
    private void Eat()
    {
        if (_eaten) return;
        _eaten = true;

        PlayTriggerSound();

        foreach (var wall in wallsToReveal)
        {
            if (wall == null) continue;

            // Toggle mode: flip the wall on/off. Reveal mode: always turn it on.
            bool show = !toggle || !wall.activeSelf;
            wall.SetActive(show);

            if (show && tintWalls)
            {
                var sr = wall.GetComponentInChildren<SpriteRenderer>();
                if (sr != null) sr.color = color;
            }
        }

        Destroy(gameObject);   // the ball is consumed
    }

    private void PlayTriggerSound()
    {
        if (triggerSound == null) return;

        // ColorBall is destroyed after use, so let a temporary 2D AudioSource
        // remain until the sound has finished playing.
        var soundObject = new GameObject("Color Ball Trigger Sound");
        var source = soundObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = false;
        source.spatialBlend = 0f;
        source.volume = 1f;
        source.PlayOneShot(triggerSound, triggerSoundVolume);

        Destroy(soundObject, triggerSound.length + 0.1f);
    }

    private void FitColliderToSprite()
    {
        if (!fitColliderToSprite) return;
        if (!TryGetComponent(out BoxCollider2D boxCollider)) return;

        SpriteRenderer spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer == null || spriteRenderer.sprite == null) return;

        // Convert the child Sprite bounds into this object's local space so this
        // also works when the visible SpriteRenderer is on a child object.
        Bounds spriteBounds = spriteRenderer.sprite.bounds;
        Vector3 worldCenter = spriteRenderer.transform.TransformPoint(spriteBounds.center);
        Vector3 worldMin = spriteRenderer.transform.TransformPoint(spriteBounds.min);
        Vector3 worldMax = spriteRenderer.transform.TransformPoint(spriteBounds.max);
        Vector3 localCenter = transform.InverseTransformPoint(worldCenter);
        Vector3 localMin = transform.InverseTransformPoint(worldMin);
        Vector3 localMax = transform.InverseTransformPoint(worldMax);

        boxCollider.offset = localCenter;
        boxCollider.size = new Vector2(
            Mathf.Abs(localMax.x - localMin.x) + triggerPadding * 2f,
            Mathf.Abs(localMax.y - localMin.y) + triggerPadding * 2f);
    }
}
