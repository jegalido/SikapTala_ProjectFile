using UnityEngine;
using System.Collections;

/// <summary>
/// Attach to a GameObject with an EdgeCollider2D at the bottom of your level.
/// When the player falls into it, they teleport to the last registered checkpoint
/// and the insanity bar resets to 100%.
/// </summary>
public class DeathZone : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Leave empty to auto-find InsanityBar in scene")]
    public InsanityBar insanityBar;

    [Header("Settings")]
    [Tooltip("Delay in seconds before teleporting player back (0 = instant)")]
    public float respawnDelay = 0.5f;

    // -- Private state --------------------------------------------------------

    private bool isRespawning = false;
    private Transform playerTransform;
    private Rigidbody2D playerRb;

    // -- Unity lifecycle ------------------------------------------------------

    private void Start()
    {
        if (insanityBar == null)
            insanityBar = FindFirstObjectByType<InsanityBar>();

        // Make sure collider is a trigger
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (isRespawning) return;

        // Cache player references on first contact
        playerTransform = other.transform;
        playerRb = other.GetComponent<Rigidbody2D>();

        Debug.Log("DeathZone: Player fell! Sending to checkpoint.");

        if (respawnDelay <= 0f)
            RespawnPlayer();
        else
            StartCoroutine(RespawnWithDelay());
    }

    // -- Respawn logic --------------------------------------------------------

    private void RespawnPlayer()
    {
        if (insanityBar == null)
        {
            Debug.LogWarning("DeathZone: InsanityBar not found!");
            isRespawning = false;
            return;
        }

        // Get the last registered checkpoint position from InsanityBar
        Vector3 respawnPos = insanityBar.GetLastCheckpointPosition();

        // Teleport player
        if (playerTransform != null)
        {
            // Stop all velocity first so player doesn't fly away
            if (playerRb != null)
                playerRb.linearVelocity = Vector2.zero;

            playerTransform.position = respawnPos;
            Debug.Log("DeathZone: Player respawned at " + respawnPos);
        }

        // Reset insanity bar to 100%
        insanityBar.ResetInsanity();

        isRespawning = false;
    }

    private IEnumerator RespawnWithDelay()
    {
        isRespawning = true;
        yield return new WaitForSeconds(respawnDelay);
        RespawnPlayer();
    }

    // -- Editor helper --------------------------------------------------------

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.6f);

        EdgeCollider2D edge = GetComponent<EdgeCollider2D>();
        if (edge != null && edge.pointCount >= 2)
        {
            for (int i = 0; i < edge.pointCount - 1; i++)
            {
                Vector3 a = transform.TransformPoint(edge.points[i]);
                Vector3 b = transform.TransformPoint(edge.points[i + 1]);
                Gizmos.DrawLine(a, b);
            }
        }
    }
}