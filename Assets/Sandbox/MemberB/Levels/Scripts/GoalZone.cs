using UnityEngine;
using UnityEngine.SceneManagement;
using SlimeTime.Core;

/// <summary>
/// Finish area. When the slime stays inside this trigger for <see cref="requiredTime"/>
/// seconds, the level is won: the next level is unlocked (via GameProgress) and an
/// optional win UI is shown. Attach to an empty GameObject with a Collider2D set as
/// "Is Trigger", sized/positioned to cover the goal area.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class GoalZone : MonoBehaviour
{
    [Header("Win condition")]
    [Tooltip("Seconds the slime must stay inside the zone to win.")]
    [SerializeField] private float requiredTime = 1f;

    [Header("On win")]
    [Tooltip("Scene name of the next level to unlock. Leave empty on the last level.")]
    [SerializeField] private string nextLevelSceneName = "";
    [Tooltip("If true, skip the win panel and load the next level immediately after winning.")]
    [SerializeField] private bool loadNextLevelOnWin = false;

    /// <summary>
    /// Raised once when the level is won; carries the next level's scene name
    /// (empty on the last level). WinPanel listens to this.
    /// </summary>
    public static event System.Action<string> OnWin;

    private bool playerInside;
    private float stayTimer;
    private bool won;

    // Makes the collider a trigger automatically when you add the component.
    private void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (won) return;
        if (!IsPlayer(other)) return;
        playerInside = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!IsPlayer(other)) return;
        playerInside = false;
        stayTimer = 0f;   // left before the count finished -> reset
    }

    private void Update()
    {
        if (won || !playerInside) return;

        // deltaTime is 0 while the game is paused, so the count freezes too.
        stayTimer += Time.deltaTime;
        if (stayTimer >= requiredTime)
            Win();
    }

    private static bool IsPlayer(Collider2D other)
    {
        // The slime prefab has SlimeMovement on its root collider object.
        return other.GetComponentInParent<SlimeMovement>() != null;
    }

    private void Win()
    {
        won = true;

        // Record the unlock so LevelSelect can offer the next level.
        if (!string.IsNullOrEmpty(nextLevelSceneName))
            GameProgress.Unlock(nextLevelSceneName);

        OnWin?.Invoke(nextLevelSceneName);

        if (loadNextLevelOnWin && !string.IsNullOrEmpty(nextLevelSceneName))
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(nextLevelSceneName);
        }
    }
}
