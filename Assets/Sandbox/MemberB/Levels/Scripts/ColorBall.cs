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

    // Makes the collider a trigger automatically when the component is added.
    private void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void Awake()
    {
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
}
