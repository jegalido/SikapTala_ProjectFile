using UnityEngine;
using UnityEngine.UI;

public class InsanityBar : MonoBehaviour
{
    // -- Public / Inspector fields --------------------------------------------

    [Header("Insanity Drain")]
    [Tooltip("How many minutes until the bar fully drains from 100% to 0%")]
    public float drainDurationInMinutes = 2f;

    [Header("Restore On Collect")]
    [Tooltip("Percentage (0-100) of the bar restored when picking up a collectible")]
    public float restorePercent = 25f;

    [Header("UI References")]
    [Tooltip("The UI Slider that visually represents the insanity bar")]
    public Slider insanitySlider;

    [Tooltip("The GameObject shown when insanity reaches 0 (checkpoint prompt sprite)")]
    public GameObject checkpointPrompt;

    [Header("Player Reference")]
    [Tooltip("Drag your Player GameObject here")]
    public Transform player;

    // -- Private state --------------------------------------------------------

    private float currentInsanity;
    private bool isDepleted;
    private Vector3 lastCheckpointPosition;
    private bool hasCheckpoint;

    // -- Unity lifecycle ------------------------------------------------------

    private void Start()
    {
        currentInsanity = 100f;
        isDepleted = false;
        hasCheckpoint = false;

        if (insanitySlider != null)
        {
            insanitySlider.minValue = 0f;
            insanitySlider.maxValue = 100f;
            insanitySlider.value = currentInsanity;
        }

        if (checkpointPrompt != null)
            checkpointPrompt.SetActive(false);
    }

    private void Update()
    {
        if (isDepleted) return;

        DrainInsanity();
        UpdateSliderUI();
        CheckDepletion();
    }

    // -- Drain logic ----------------------------------------------------------

    private void DrainInsanity()
    {
        float drainPerSecond = 100f / (drainDurationInMinutes * 60f);
        currentInsanity -= drainPerSecond * Time.deltaTime;
        currentInsanity = Mathf.Clamp(currentInsanity, 0f, 100f);
    }

    // -- Checkpoint registration ----------------------------------------------

    public void RegisterCheckpoint(Vector3 position)
    {
        lastCheckpointPosition = position;
        hasCheckpoint = true;
        Debug.Log("InsanityBar: Checkpoint registered at " + position);
    }

    // -- Restore logic --------------------------------------------------------

    public void RestoreInsanity(float percent)
    {
        if (isDepleted) return;

        currentInsanity += Mathf.Clamp(percent, 0f, 100f);
        currentInsanity = Mathf.Clamp(currentInsanity, 0f, 100f);
        UpdateSliderUI();
    }

    // -- UI helpers -----------------------------------------------------------

    private void UpdateSliderUI()
    {
        if (insanitySlider != null)
            insanitySlider.value = currentInsanity;
    }

    private void CheckDepletion()
    {
        if (currentInsanity <= 0f && !isDepleted)
        {
            isDepleted = true;
            OnInsanityDepleted();
        }
    }

    private void OnInsanityDepleted()
    {
        Debug.Log("InsanityBar: Depleted!");

        if (checkpointPrompt != null)
            checkpointPrompt.SetActive(true);
        else
            Debug.LogWarning("InsanityBar: checkpointPrompt is NOT assigned in the Inspector!");

        // Teleport player to last checkpoint
        if (hasCheckpoint && player != null)
        {
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = Vector2.zero;
            player.position = lastCheckpointPosition;
            Debug.Log("InsanityBar: Player respawned at " + lastCheckpointPosition);
        }
        else
        {
            Debug.LogWarning("InsanityBar: No checkpoint registered yet or player not assigned!");
        }

        ResetInsanity();
    }

    // -- Public utility -------------------------------------------------------

    /// <summary>
    /// Returns the last registered checkpoint position.
    /// Used by DeathZone to teleport the player on fall.
    /// </summary>
    public Vector3 GetLastCheckpointPosition()
    {
        if (!hasCheckpoint)
        {
            Debug.LogWarning("InsanityBar: No checkpoint registered yet! Returning current player position.");
            return player != null ? player.position : Vector3.zero;
        }

        return lastCheckpointPosition;
    }

    /// <summary>
    /// Returns true if at least one checkpoint has been registered.
    /// </summary>
    public bool HasCheckpoint() => hasCheckpoint;

    public void ResetInsanity()
    {
        currentInsanity = 100f;
        isDepleted = false;

        UpdateSliderUI();

        if (checkpointPrompt != null)
            checkpointPrompt.SetActive(false);

        // Make all collectibles reappear
        CollectibleManager.ResetAll();
    }

    public float GetInsanity() => currentInsanity;
}