using UnityEngine;
using System.Collections;

/// <summary>
/// A lethal hazard (spikes, etc.). When the Player touches it, they are sent back
/// to the last registered checkpoint. Sanity is NOT reset (it stays as pressure).
/// Put the object in the InsanityVisionEffect's revealOnShift list to make it a
/// DARK-WORLD-only hazard (it will only be active/solid while shifted).
/// Requires a trigger Collider2D.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class LethalHazard : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Leave empty to auto-find the InsanityBar in the scene (used for the checkpoint position).")]
    public InsanityBar insanityBar;

    [Header("Settings")]
    [Tooltip("Delay before respawning the player (0 = instant).")]
    public float respawnDelay = 0f;

    private bool isRespawning;

    private void Start()
    {
        if (insanityBar == null)
            insanityBar = FindFirstObjectByType<InsanityBar>();

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isRespawning) return;
        if (!other.CompareTag("Player")) return;

        if (respawnDelay <= 0f)
            SendToCheckpoint(other.transform);
        else
            StartCoroutine(SendToCheckpointDelayed(other.transform));
    }

    private IEnumerator SendToCheckpointDelayed(Transform player)
    {
        isRespawning = true;
        yield return new WaitForSeconds(respawnDelay);
        SendToCheckpoint(player);
        isRespawning = false;
    }

private void SendToCheckpoint(Transform player)
    {
        if (player == null) return;

        Vector3 target = insanityBar != null
            ? insanityBar.GetLastCheckpointPosition()
            : player.position;

        ScreenFader.Respawn(player, target);
    }
}
