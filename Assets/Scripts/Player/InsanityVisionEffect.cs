using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class InsanityVisionEffect : MonoBehaviour
{
    // -- Public / Inspector fields --------------------------------------------

    [Header("Post Processing Volume")]
    [Tooltip("Assign the Global Volume that has your post processing overrides")]
    public Volume postProcessVolume;

    [Header("Effect Transition Speed")]
    [Tooltip("How fast the effect fades in and out")]
    public float transitionSpeed = 4f;

    [Header("Grayscale Settings")]
    [Tooltip("Saturation when effect is fully active (-100 = fully gray)")]
    public float targetSaturation = -100f;

    [Tooltip("How dark/dull the screen gets (0 = no change, -0.3 = noticeably darker)")]
    public float targetExposure = -0.3f;

    [Header("Chromatic Aberration Settings")]
    [Tooltip("Intensity of chromatic aberration (0-1). 0.5 is strong but not nauseating")]
    public float targetChromaticIntensity = 0.5f;

    [Header("Object Visibility On Shift")]
    [Tooltip("GameObjects that are HIDDEN normally and become VISIBLE when Shift is held")]
    public GameObject[] revealOnShift;

    [Tooltip("GameObjects that are VISIBLE normally and become HIDDEN when Shift is held")]
    public GameObject[] hideOnShift;

    // -- Private state --------------------------------------------------------

    private ColorAdjustments colorAdjustments;
    private ChromaticAberration chromaticAberration;
    private Vignette vignette;
    private bool postProcessReady = false;

    private float currentBlend = 0f;
    private bool lastShiftState = false;

    private float defaultSaturation;
    private float defaultExposure;
    private float defaultChromaticIntensity;
    private float defaultVignetteIntensity;

    // -- Unity lifecycle ------------------------------------------------------

    private void Start()
    {
        // Try to set up post processing — but don't return if it fails
        // Hidden object logic works independently
        if (postProcessVolume != null)
        {
            postProcessVolume.profile.TryGet(out colorAdjustments);
            postProcessVolume.profile.TryGet(out chromaticAberration);
            postProcessVolume.profile.TryGet(out vignette);

            if (colorAdjustments != null)
            {
                defaultSaturation = colorAdjustments.saturation.value;
                defaultExposure = colorAdjustments.postExposure.value;
            }

            if (chromaticAberration != null)
                defaultChromaticIntensity = chromaticAberration.intensity.value;

            if (vignette != null)
                defaultVignetteIntensity = vignette.intensity.value;

            postProcessReady = true;
        }
        else
        {
            Debug.LogWarning("InsanityVisionEffect: No Post Process Volume assigned — screen effects disabled but object visibility still works.");
        }

        // Initialize object states regardless of post processing
        SetObjectArray(revealOnShift, false);  // start hidden
        SetObjectArray(hideOnShift, true);     // start visible
    }

    private void Update()
    {
        bool shiftHeld = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        // Smoothly blend post processing effect
        float targetBlend = shiftHeld ? 1f : 0f;
        currentBlend = Mathf.MoveTowards(currentBlend, targetBlend, Time.deltaTime * transitionSpeed);

        // Apply screen effects only if post processing is ready
        if (postProcessReady)
            ApplyEffects(currentBlend);

        // Toggle objects at halfway threshold
        bool shiftActive = currentBlend > 0.5f;
        if (shiftActive != lastShiftState)
        {
            lastShiftState = shiftActive;
            SetObjectArray(revealOnShift, shiftActive);
            SetObjectArray(hideOnShift, !shiftActive);

            Debug.Log("InsanityVisionEffect: Shift active = " + shiftActive
                + " | Revealing " + (revealOnShift?.Length ?? 0) + " objects"
                + " | Hiding " + (hideOnShift?.Length ?? 0) + " objects");
        }
    }

    // -- Effect application ---------------------------------------------------

    private void ApplyEffects(float blend)
    {
        if (colorAdjustments != null)
        {
            colorAdjustments.saturation.value =
                Mathf.Lerp(defaultSaturation, targetSaturation, blend);

            colorAdjustments.postExposure.value =
                Mathf.Lerp(defaultExposure, targetExposure, blend);
        }

        if (chromaticAberration != null)
        {
            chromaticAberration.intensity.value =
                Mathf.Lerp(defaultChromaticIntensity, targetChromaticIntensity, blend);
        }

        if (vignette != null)
        {
            vignette.intensity.value =
                Mathf.Lerp(defaultVignetteIntensity, defaultVignetteIntensity + 0.25f, blend);
        }
    }

    // -- Object visibility helper ---------------------------------------------

    private void SetObjectArray(GameObject[] objects, bool visible)
    {
        if (objects == null) return;

        foreach (GameObject obj in objects)
        {
            if (obj != null)
                obj.SetActive(visible);
        }
    }

    // -- Editor helper --------------------------------------------------------

    private void OnValidate()
    {
        targetSaturation = Mathf.Clamp(targetSaturation, -100f, 100f);
        targetChromaticIntensity = Mathf.Clamp01(targetChromaticIntensity);
        transitionSpeed = Mathf.Max(0.1f, transitionSpeed);
    }
}