using UnityEngine;
using SlimeTime.Core;

/// <summary>
/// An invisible trigger zone that kills the slime on contact (fires OnDied so the
/// Game Over menu can show). Use it as a floor below the level: place a thin BoxCollider2D
/// set as a trigger, give it this component, and the slime dies when it falls through.
///
/// One zone per level. If empty, the script also kills any other Rigidbody2D it touches
/// (e.g. crates / balls) by destroying their gameObject — remove the if-block below if
/// you want that behaviour.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class KillZone : MonoBehaviour
{
    [Tooltip("Optional. If set, this is the only tag that triggers the kill. " +
             "Leave empty to kill anything with a Rigidbody2D that enters the zone.")]
    [SerializeField] private string requiredTag = "";

    [Tooltip("If true, also destroy anything that enters besides the slime. " +
             "Useful if you have balls, crates, etc. that should respawn/die too.")]
    [SerializeField] private bool destroyOtherRigidbodies = false;

    // Make sure the collider is a trigger — kill zones should never physically block.
    private void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Kill the slime specifically.
        var slime = other.GetComponentInParent<SlimeSizeController>();
        if (slime != null)
        {
            // Use SendMessage so we don't need to expose a Kill() method on the
            // controller. The OnDied event is raised from inside the controller, so
            // the GameMenu hookup stays the same.
            slime.SendMessage("Die", SendMessageOptions.DontRequireReceiver);
            return;
        }

        // Optional: clean up other physics objects that fall in.
        if (destroyOtherRigidbodies)
        {
            if (other.attachedRigidbody != null && other.attachedRigidbody.gameObject != gameObject)
            {
                if (string.IsNullOrEmpty(requiredTag) || other.CompareTag(requiredTag))
                {
                    Destroy(other.gameObject);
                }
            }
        }
    }
}
