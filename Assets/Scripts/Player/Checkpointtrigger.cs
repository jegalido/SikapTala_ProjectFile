using UnityEngine;


public class CheckpointTrigger : MonoBehaviour
{
    [Header("Checkpoint Settings")]
  
    public string checkpointName = "Checkpoint";
    public Vector2 respawnOffset = new Vector2(0f, 1f);
    public GameObject activatedVisual;

    private InsanityBar insanityBar;
    private bool isActivated = false;

   

    private void Start()
    {
        insanityBar = FindFirstObjectByType<InsanityBar>();

        if (insanityBar == null)
            Debug.LogWarning(checkpointName + ": InsanityBar not found in scene!");

     
        if (activatedVisual != null)
            activatedVisual.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)

    {

        Debug.Log("Trigger hit by: " + other.gameObject.name + " | Tag: " + other.tag);
        if (!other.CompareTag("Player")) return;
        if (isActivated) return;   

        isActivated = true;

       
        Vector3 respawnPosition = transform.position + (Vector3)respawnOffset;
        insanityBar?.RegisterCheckpoint(respawnPosition);

       
        if (activatedVisual != null)
            activatedVisual.SetActive(true);

        Debug.Log(checkpointName + ": Activated!");
    }

    // -- Editor helper --------------------------------------------------------

    private void OnDrawGizmos()
    {
     
        Gizmos.color = isActivated
            ? new Color(0f, 1f, 0.3f, 0.3f)   // green when active
            : new Color(1f, 1f, 0f, 0.3f);     // yellow when inactive

        Gizmos.DrawWireCube(transform.position, transform.localScale);

        // Show respawn point
        Gizmos.color = Color.cyan;
        Vector3 respawn = transform.position + (Vector3)respawnOffset;
        Gizmos.DrawWireSphere(respawn, 0.2f);
    }
}