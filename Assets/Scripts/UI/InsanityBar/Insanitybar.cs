using UnityEngine;
using UnityEngine.UI;


public class InsanityBar : MonoBehaviour
{
 

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

    private float currentInsanity;   // 0.0 - 100.0
    private bool isDepleted;



    private void Start()
    {
        currentInsanity = 100f;
        isDepleted = false;

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


    private void DrainInsanity()
    {
        // Convert minutes to seconds, then to per-frame drain
        float drainPerSecond = 100f / (drainDurationInMinutes * 60f);
        currentInsanity -= drainPerSecond * Time.deltaTime;
        currentInsanity = Mathf.Clamp(currentInsanity, 0f, 100f);
    }


    public void RestoreInsanity(float percent)
    {
        if (isDepleted) return;

        currentInsanity += Mathf.Clamp(percent, 0f, 100f);
        currentInsanity = Mathf.Clamp(currentInsanity, 0f, 100f);
        UpdateSliderUI();
    }

    
    // UI helpers
 

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
        if (checkpointPrompt != null)
            checkpointPrompt.SetActive(true);

        // Optionally pause game time while prompt is shown
        // Time.timeScale = 0f;
    }

    // -----------------------------------------------------------------------
    // Public utility
    // -----------------------------------------------------------------------

   
    /// Call this when the player returns to checkpoint to reset the bar.
  
    public void ResetInsanity()
    {
        currentInsanity = 100f;
        isDepleted = false;

        UpdateSliderUI();

        if (checkpointPrompt != null)
            checkpointPrompt.SetActive(false);

        // Time.timeScale = 1f; // uncomment if you paused above
    }

    /// <summary>
    /// Returns current insanity value (0-100) for external reads.
    /// </summary>
    public float GetInsanity() => currentInsanity;
}