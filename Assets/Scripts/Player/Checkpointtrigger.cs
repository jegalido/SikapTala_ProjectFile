using UnityEngine;


public class CheckpointTrigger : MonoBehaviour
{
    [Header("Checkpoint Settings")]
    [Tooltip("Label this checkpoint so you can tell them apart in the Inspector")]
    public string checkpointName = "Checkpoint";

    [Tooltip("Offset the respawn position from this object's position")]
    public Vector2 respawnOffset = new Vector2(0f, 1f);

    [Header("Camera Confiner")]
    [Tooltip("Which confiner boundary index to switch to (0 = Level 1, 1 = Level 2, etc.)")]
    public int confinerIndex = 0;

    [Header("Wall Blocker")]
    [Tooltip("Assign a GameObject with a Collider2D that acts as a wall — starts inactive, activates when checkpoint is reached")]
    public GameObject wallBlocker;

    [Header("Optional Visuals")]
    [Tooltip("Optional visual that activates when checkpoint is reached (e.g. lit flag)")]
    public GameObject activatedVisual;

    // -- Private state --------------------------------------------------------

    private InsanityBar insanityBar;
    private CameraConfinerSwitcher confinerSwitcher;
    private bool isActivated = false;

    // -- Unity lifecycle ------------------------------------------------------

    private void Start()
    {
        insanityBar = FindFirstObjectByType<InsanityBar>();
        confinerSwitcher = FindFirstObjectByType<CameraConfinerSwitcher>();

        if (insanityBar == null)
            Debug.LogWarning(checkpointName + ": InsanityBar not found in scene!");

        if (confinerSwitcher == null)
            Debug.LogWarning(checkpointName + ": CameraConfinerSwitcher not found in scene!");

        // Make sure wall and visual start inactive
        if (wallBlocker != null)
            wallBlocker.SetActive(false);

        if (activatedVisual != null)
            activatedVisual.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (isActivated) return;

        isActivated = true;

        // Register respawn position
        Vector3 respawnPosition = transform.position + (Vector3)respawnOffset;
        insanityBar?.RegisterCheckpoint(respawnPosition);

        // Switch camera confiner
        confinerSwitcher?.SwitchConfiner(confinerIndex);

        // Activate wall blocker to prevent going back
        if (wallBlocker != null)
            wallBlocker.SetActive(true);

        // Show activated visual
        if (activatedVisual != null)
            activatedVisual.SetActive(true);

        Debug.Log(checkpointName + ": Activated! Wall blocker enabled, confiner switched to index " + confinerIndex);
    }

    // -- Editor helper --------------------------------------------------------

    private void OnDrawGizmos()
    {
        Gizmos.color = isActivated
            ? new Color(0f, 1f, 0.3f, 0.3f)
            : new Color(1f, 1f, 0f, 0.3f);

        Gizmos.DrawWireCube(transform.position, transform.localScale);

        // Respawn point
        Gizmos.color = Color.cyan;
        Vector3 respawn = transform.position + (Vector3)respawnOffset;
        Gizmos.DrawWireSphere(respawn, 0.2f);
    }
}