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

    [Header("Shift Drain")]
    [Tooltip("Extra sanity drain multiplier while the player voluntarily holds Shift.")]
    public float manualShiftDrainMultiplier = 2.5f;
    [Tooltip("Leave empty to auto-find. Used to detect manual shifting for faster drain.")]
    public InsanityVisionEffect visionEffect;

    [Header("Shift Glow (drains faster feedback)")]
    [Tooltip("Glow/outline color pulsed on the sanity bar while the player holds Shift.")]
    public Color shiftGlowColor = new Color(1f, 0.5f, 0.15f, 1f);
    public float glowPulseSpeed = 10f;
    [Tooltip("How much the bar breathes (scales) while shifting.")]
    public float glowScalePulse = 0.05f;
    private Outline fillOutline;
    private RectTransform barRect;
    private Vector3 barBaseScale = Vector3.one;

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

        if (visionEffect == null)
            visionEffect = FindFirstObjectByType<InsanityVisionEffect>();

        if (insanitySlider != null && insanitySlider.fillRect != null)
        {
            Image fillImg = insanitySlider.fillRect.GetComponent<Image>();
            if (fillImg != null)
            {
                fillOutline = fillImg.GetComponent<Outline>();
                if (fillOutline == null) fillOutline = fillImg.gameObject.AddComponent<Outline>();
                fillOutline.effectDistance = new Vector2(5f, 5f);
                Color c = shiftGlowColor; c.a = 0f; fillOutline.effectColor = c;
            }
            barRect = insanitySlider.fillRect; // scale/glow only the fill, not the whole bar (text)
            if (barRect != null) barBaseScale = barRect.localScale;
        }
    }

    private void Update()
    {
        // Sanity is now persistent pressure - it drains but never triggers a respawn.
        DrainInsanity();
        UpdateSliderUI();
        UpdateShiftGlow();
    }

    private void UpdateShiftGlow()
    {
        bool shifting = visionEffect != null && visionEffect.ManualShiftActive;
        float pulse = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * glowPulseSpeed);

        if (fillOutline != null)
        {
            Color c = shiftGlowColor;
            c.a = shifting
                ? Mathf.Lerp(0.35f, 1f, pulse)
                : Mathf.MoveTowards(fillOutline.effectColor.a, 0f, Time.unscaledDeltaTime * 8f);
            fillOutline.effectColor = c;
        }

        if (barRect != null)
        {
            float s = shifting ? (1f + glowScalePulse * pulse) : 1f;
            barRect.localScale = barBaseScale * s;
        }
    }

    // -- Drain logic ----------------------------------------------------------

    private void DrainInsanity()
    {
        float drainPerSecond = 100f / (drainDurationInMinutes * 60f);
        float mult = (visionEffect != null && visionEffect.ManualShiftActive) ? manualShiftDrainMultiplier : 1f;
        currentInsanity -= drainPerSecond * mult * Time.deltaTime;
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

    /// <summary>
    /// Adds the given percentage to the current sanity (clamped at 100).
    /// Use this for a PARTIAL top-up.
    /// </summary>
    public void RestoreInsanity(float percent)
    {
        currentInsanity += Mathf.Clamp(percent, 0f, 100f);
        currentInsanity = Mathf.Clamp(currentInsanity, 0f, 100f);
        UpdateSliderUI();
    }

    /// <summary>
    /// Instantly fills the sanity bar to 100%, regardless of current value.
    /// Use this when a collectible should fully restore sanity.
    /// </summary>
    public void RestoreInsanityFull()
    {
        currentInsanity = 100f;
        UpdateSliderUI();
    }

    // -- UI helpers -----------------------------------------------------------

    private void UpdateSliderUI()
    {
        if (insanitySlider != null)
            insanitySlider.value = currentInsanity;
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