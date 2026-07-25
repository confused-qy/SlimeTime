using UnityEngine;

/// <summary>
/// Makes the camera follow a target (the slime) WITHOUT being its child, so the
/// target's scaling or rotation never affect the camera. Put this on the camera and
/// set Target to the slime.
///
/// Clamps the camera so the view never shows past the background sprite. Assign
/// the background's SpriteRenderer to <see cref="background"/>; if left empty the
/// script looks for any SpriteRenderer tagged or named "background" in the scene.
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [Tooltip("What to follow (the slime root).")]
    [SerializeField] private Transform target;

    [Tooltip("Offset from the target. Keep Z negative (e.g. -10) so the 2D camera stays in front.")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);

    [Tooltip("Follow smoothing. 0 = snap instantly; higher = snappier.")]
    [SerializeField] private float smoothing = 10f;

    [Tooltip("The background SpriteRenderer that defines the play area. " +
             "If empty, the first SpriteRenderer in the scene is used.")]
    [SerializeField] private SpriteRenderer background;

    private void Awake()
    {
        if (background == null)
        {
            // Fallback: pick the first SpriteRenderer in the scene.
            background = FindAnyObjectByType<SpriteRenderer>();
        }
    }

    // LateUpdate: move the camera AFTER the slime has finished moving this frame.
    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 desired = target.position + offset;
        desired = ClampToBackground(desired);

        transform.position = smoothing > 0f
            ? Vector3.Lerp(transform.position, desired, 1f - Mathf.Exp(-smoothing * Time.deltaTime))
            : desired;
    }

    // Clamp the camera's X/Y so the view stays inside the background's bounds.
    // The background's world-space bounds (min/max) come from SpriteRenderer.bounds,
    // which already accounts for its position and any scale (e.g. FitToCamera's cover).
    private Vector3 ClampToBackground(Vector3 desired)
    {
        if (background == null || background.sprite == null) return desired;

        Camera cam = GetComponent<Camera>();
        if (cam == null || !cam.orthographic) return desired;

        Bounds b = background.bounds;
        float halfH = cam.orthographicSize;
        float halfW = halfH * cam.aspect;

        float minX = b.min.x + halfW;
        float maxX = b.max.x - halfW;
        float minY = b.min.y + halfH;
        float maxY = b.max.y - halfH;

        // If the background is smaller than the viewport on an axis, lock to its centre.
        float clampedX = (maxX < minX) ? (b.min.x + b.max.x) * 0.5f : Mathf.Clamp(desired.x, minX, maxX);
        float clampedY = (maxY < minY) ? (b.min.y + b.max.y) * 0.5f : Mathf.Clamp(desired.y, minY, maxY);

        return new Vector3(clampedX, clampedY, desired.z);
    }
}


