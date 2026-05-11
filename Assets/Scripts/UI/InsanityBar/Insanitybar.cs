using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Insanity Bar System with Multi-Checkpoint Support
/// - Drains over time (rate set in minutes)
/// - Refills when collecting objects (percent-based)
/// - Shows a checkpoint prompt when bar hits 0
/// - Respawns player at the last registered checkpoint
/// </summary>
public class InsanityBar : MonoBehaviour
{
    // -- Public / Inspector fields --------------------------------------------

    [Header("Insanity Drain")]
    public float drainDurationInMinutes = 2f;

    [Header("Restore On Collect")]
    public float restorePercent = 25f;

    [Header("UI References")]
    public Slider insanitySlider;

    public GameObject checkpointPrompt;

    [Header("Player Reference")]
 
    public Transform player;


    private float currentInsanity;
    private bool isDepleted;
    private Vector3 lastCheckpointPosition;
    private bool hasCheckpoint;

 

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

    // -- Drain logic 

    private void DrainInsanity()
    {
        float drainPerSecond = 100f / (drainDurationInMinutes * 60f);
        currentInsanity -= drainPerSecond * Time.deltaTime;
        currentInsanity = Mathf.Clamp(currentInsanity, 0f, 100f);
    }

   
    public void RegisterCheckpoint(Vector3 position)
    {
        lastCheckpointPosition = position;
        hasCheckpoint = true;
        Debug.Log("InsanityBar: Checkpoint registered at " + position);
    }

    // -- Restore logic (called by collectibles) --------------------------------

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

        
        if (hasCheckpoint && player != null)
        {
            player.position = lastCheckpointPosition;
            Debug.Log("InsanityBar: Player respawned at " + lastCheckpointPosition);
        }
        else
        {
            Debug.LogWarning("InsanityBar: No checkpoint registered yet or player not assigned!");
        }


        if (checkpointPrompt != null)
            checkpointPrompt.SetActive(true);
        else
            Debug.LogWarning("InsanityBar: checkpointPrompt is NOT assigned in the Inspector!");

        // Time.timeScale = 0f; // uncomment to freeze game on depletion
    }

    

   
    /// Call this to reset insanity and hide the prompt after respawn.
   
    public void ResetInsanity()
    {
        currentInsanity = 100f;
        isDepleted = false;

        UpdateSliderUI();

        if (checkpointPrompt != null)
            checkpointPrompt.SetActive(false);

        // Time.timeScale = 1f;
    }

    public float GetInsanity() => currentInsanity;
}