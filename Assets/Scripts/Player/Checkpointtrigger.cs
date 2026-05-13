using UnityEngine;
using Unity.Cinemachine;

/// <summary>
/// Place this on each checkpoint GameObject in your level.
/// When the player walks into it:
///   - Registers the respawn point
///   - Switches the Cinemachine confiner to the matching boundary
///   - Activates a solid wall collider that blocks the player from going back
///   - Optionally changes the Cinemachine lens (orthographic size) for zoom effects
/// </summary>
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

    [Header("Camera Lens Override")]
    [Tooltip("Enable this to change the camera zoom when this checkpoint is reached")]
    public bool overrideLens = false;

    [Tooltip("Target orthographic size (higher = more zoomed out, lower = more zoomed in). Default is usually 5")]
    public float targetOrthographicSize = 7f;

    [Tooltip("How smoothly the camera zooms to the new size (higher = faster)")]
    public float lensTransitionSpeed = 2f;

    [Header("Wall Blocker")]
    [Tooltip("Assign a GameObject with a Collider2D that acts as a wall — starts inactive, activates when checkpoint is reached")]
    public GameObject wallBlocker;

    [Header("Optional Visuals")]
    [Tooltip("Optional visual that activates when checkpoint is reached (e.g. lit flag)")]
    public GameObject activatedVisual;

    // -- Private state --------------------------------------------------------

    private InsanityBar insanityBar;
    private CameraConfinerSwitcher confinerSwitcher;
    private CinemachineCamera virtualCamera;
    private bool isActivated = false;
    private bool isTransitioningLens = false;

    // -- Unity lifecycle ------------------------------------------------------

    private void Start()
    {
        insanityBar = FindFirstObjectByType<InsanityBar>();
        confinerSwitcher = FindFirstObjectByType<CameraConfinerSwitcher>();
        virtualCamera = FindFirstObjectByType<CinemachineCamera>();

        if (insanityBar == null)
            Debug.LogWarning(checkpointName + ": InsanityBar not found in scene!");

        if (confinerSwitcher == null)
            Debug.LogWarning(checkpointName + ": CameraConfinerSwitcher not found in scene!");

        if (overrideLens && virtualCamera == null)
            Debug.LogWarning(checkpointName + ": Lens override is on but no CinemachineCamera found!");

        if (wallBlocker != null)
            wallBlocker.SetActive(false);

        if (activatedVisual != null)
            activatedVisual.SetActive(false);
    }

    private void Update()
    {
        // Smoothly transition lens if activated
        if (isTransitioningLens && virtualCamera != null)
        {
            float current = virtualCamera.Lens.OrthographicSize;
            float next = Mathf.MoveTowards(current, targetOrthographicSize, lensTransitionSpeed * Time.deltaTime);

            LensSettings lens = virtualCamera.Lens;
            lens.OrthographicSize = next;
            virtualCamera.Lens = lens;

            if (Mathf.Approximately(next, targetOrthographicSize))
                isTransitioningLens = false;
        }
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

        // Start lens transition if enabled
        if (overrideLens && virtualCamera != null)
        {
            isTransitioningLens = true;
            Debug.Log(checkpointName + ": Starting lens transition to size " + targetOrthographicSize);
        }

        // Activate wall blocker
        if (wallBlocker != null)
            wallBlocker.SetActive(true);

        // Show activated visual
        if (activatedVisual != null)
            activatedVisual.SetActive(true);

        Debug.Log(checkpointName + ": Activated!");
    }

    // -- Editor helper --------------------------------------------------------

    private void OnDrawGizmos()
    {
        Gizmos.color = isActivated
            ? new Color(0f, 1f, 0.3f, 0.3f)
            : new Color(1f, 1f, 0f, 0.3f);

        Gizmos.DrawWireCube(transform.position, transform.localScale);

        Gizmos.color = Color.cyan;
        Vector3 respawn = transform.position + (Vector3)respawnOffset;
        Gizmos.DrawWireSphere(respawn, 0.2f);
    }
}