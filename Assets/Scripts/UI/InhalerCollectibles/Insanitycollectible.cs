using UnityEngine;

/// <summary>
/// Attach to any collectible object.
/// Instead of destroying itself on pickup, it hides.
/// When the player resets to checkpoint, all collectibles reappear.
/// </summary>
public class InsanityCollectible : MonoBehaviour
{
    [Header("Restore Settings")]
    [Tooltip("Percentage (0-100) of insanity bar to restore on pickup")]
    public float restorePercent = 25f;

    [Tooltip("Leave empty to auto-find InsanityBar in scene")]
    public InsanityBar insanityBar;

    // -- Private state --------------------------------------------------------

    private bool isCollected = false;

    // -- Unity lifecycle ------------------------------------------------------

    private void Start()
    {
        if (insanityBar == null)
            insanityBar = FindFirstObjectByType<InsanityBar>();

        // Register this collectible to the manager so it can be reset
        CollectibleManager.Register(this);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (isCollected) return;

        Collect();
    }

    // -- Collect / Reset ------------------------------------------------------

    private void Collect()
    {
        isCollected = true;

        if (insanityBar != null)
            insanityBar.RestoreInsanity(restorePercent);

        // Hide instead of destroy so it can come back on reset
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Called by CollectibleManager when player resets to checkpoint.
    /// Makes this collectible reappear.
    /// </summary>
    public void ResetCollectible()
    {
        isCollected = false;
        gameObject.SetActive(true);
    }

    private void OnDestroy()
    {
        // Unregister when scene unloads
        CollectibleManager.Unregister(this);
    }
}