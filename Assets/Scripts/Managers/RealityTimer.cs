using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

/// <summary>
/// Drives the "reality" bar and the forced reality-shift cycle.
///
/// Warm phase: the bar drains; the LOWER the player's sanity, the FASTER it drains.
/// When it empties, a forced shift into the dark world happens (player loses manual
/// control). Dark phase: the bar (recolored) drains again; the LOWER the sanity, the
/// LONGER it lasts. When it empties, the player snaps back to the warm world. Repeat.
///
/// The bar can never be paused or refilled directly - the only lever is keeping the
/// sanity (red) bar high, which slows the drain and shortens forced dark phases.
/// </summary>
public class RealityTimer : MonoBehaviour
{
    [Header("References (auto-found if empty)")]
    [SerializeField] private InsanityBar sanity;
    [SerializeField] private InsanityVisionEffect vision;

    [Header("Reality Bar UI")]
    [SerializeField] private Slider realitySlider;
    [Tooltip("The fill Image that gets recolored per phase. Auto-found from the slider if empty.")]
    [SerializeField] private Image fillImage;

    [Header("Warm phase - time until a forced shift (seconds)")]
    [Tooltip("Time to fill->empty when sanity is FULL (rare forced shifts).")]
    [SerializeField] private float warmTimeAtFullSanity = 30f;
    [Tooltip("Time to fill->empty when sanity is EMPTY (frequent forced shifts).")]
    [SerializeField] private float warmTimeAtZeroSanity = 6f;

    [Header("Dark phase - how long a forced shift lasts (seconds)")]
    [Tooltip("Forced-dark duration when sanity is FULL (short).")]
    [SerializeField] private float darkTimeAtFullSanity = 5f;
    [Tooltip("Forced-dark duration when sanity is EMPTY (long).")]
    [SerializeField] private float darkTimeAtZeroSanity = 20f;

    [Header("Colors")]
    [SerializeField] private Color warmBarColor = new Color(0.62f, 0.62f, 0.66f, 1f);
    [SerializeField] private Color darkBarColor = new Color(0.45f, 0.30f, 0.60f, 1f);
    [SerializeField] private Color warnColor = new Color(0.9f, 0.15f, 0.15f, 1f);

    [Header("Telegraph")]
    [Tooltip("Seconds before a forced shift that the bar starts flashing as a warning.")]
    [SerializeField] private float warnTime = 1.5f;
    [SerializeField] private float warnPulseSpeed = 20f;

    [Header("Events (hook SFX / effects here)")]
    public UnityEvent onForcedShiftStart;
    public UnityEvent onForcedShiftEnd;
    public UnityEvent onWarn;

    private enum Phase { Warm, Dark }
    private Phase phase = Phase.Warm;
    private float t = 1f;      // 1 = full, 0 = empty
    private bool warned;

    public bool IsForcedDark => phase == Phase.Dark;

    private void Start()
    {
        if (sanity == null) sanity = FindFirstObjectByType<InsanityBar>();
        if (vision == null) vision = FindFirstObjectByType<InsanityVisionEffect>();

        if (realitySlider != null)
        {
            realitySlider.minValue = 0f;
            realitySlider.maxValue = 1f;
            if (fillImage == null && realitySlider.fillRect != null)
                fillImage = realitySlider.fillRect.GetComponent<Image>();
        }

        phase = Phase.Warm;
        t = 1f;
        warned = false;
        if (vision != null) vision.forcedShift = false;
        UpdateBar(warmTimeAtFullSanity);
    }

    private void Update()
    {
        float s01 = sanity != null ? Mathf.Clamp01(sanity.GetInsanity() / 100f) : 1f;

        float duration = (phase == Phase.Warm)
            ? Mathf.Lerp(warmTimeAtZeroSanity, warmTimeAtFullSanity, s01) // full sanity -> long warm
            : Mathf.Lerp(darkTimeAtFullSanity, darkTimeAtZeroSanity, 1f - s01); // low sanity -> long dark
        duration = Mathf.Max(0.1f, duration);

        t -= Time.deltaTime / duration;

        if (t <= 0f)
        {
            t = 1f;
            warned = false;
            if (phase == Phase.Warm) EnterDark();
            else EnterWarm();
        }

        UpdateBar(duration);
    }

    private void EnterDark()
    {
        phase = Phase.Dark;
        if (vision != null) vision.forcedShift = true;
        onForcedShiftStart?.Invoke();
    }

    private void EnterWarm()
    {
        phase = Phase.Warm;
        if (vision != null) vision.forcedShift = false;
        onForcedShiftEnd?.Invoke();
    }

    private void UpdateBar(float duration)
    {
        if (realitySlider != null)
            realitySlider.SetValueWithoutNotify(Mathf.Clamp01(t));

        Color c = (phase == Phase.Warm) ? warmBarColor : darkBarColor;

        // Warm-phase telegraph: flash as a forced shift approaches.
        if (phase == Phase.Warm)
        {
            float remaining = t * duration;
            if (remaining <= warnTime)
            {
                if (!warned)
                {
                    warned = true;
                    onWarn?.Invoke();
                }
                float pulse = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * warnPulseSpeed);
                c = Color.Lerp(c, warnColor, pulse);
            }
        }

        if (fillImage != null) fillImage.color = c;
    }

    private void OnDisable()
    {
        // Never leave the world stuck in a forced shift if this is disabled.
        if (vision != null) vision.forcedShift = false;
    }
}
