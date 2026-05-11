using UnityEngine;


public class InsanityCollectible : MonoBehaviour
{
    [Header("Restore Settings")]
    [Tooltip("Percentage (0-100) of insanity bar to restore on pickup")]
    public float restorePercent = 25f;

    [Tooltip("Leave empty to auto-find InsanityBar in scene")]
    public InsanityBar insanityBar;

    private void Start()
    {
        // Auto-find if not assigned in Inspector
        if (insanityBar == null)
            insanityBar = FindFirstObjectByType<InsanityBar>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (insanityBar != null)
            insanityBar.RestoreInsanity(restorePercent);

        Destroy(gameObject);
    }
}