using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Insanity Vision Effect
/// - Hold Shift: screen goes dull gray + chromatic aberration on edges
/// - Shift also reveals hidden GameObjects (e.g. hidden clues, paths, secrets)
/// - Release Shift: everything returns to normal smoothly
///
/// Setup:
///   1. Install URP (Universal Render Pipeline) in Package Manager
///   2. Add a Volume component to your camera or a new GameObject
///   3. Add Vignette, Color Adjustments, and Chromatic Aberration overrides to the Volume
///   4. Assign the Volume to this script
///   5. Add hidden GameObjects to the hiddenObjects array
///   6. Attach this script to your Camera or a Manager GameObject
/// </summary>
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

    [Header("Hidden Objects")]
    [Tooltip("GameObjects that become visible only when Shift is held")]
    public GameObject[] hiddenObjects;

    // -- Private state --------------------------------------------------------

    private ColorAdjustments colorAdjustments;
    private ChromaticAberration chromaticAberration;
    private Vignette vignette;

    private float currentBlend = 0f;      // 0 = normal, 1 = full effect
    private bool effectActive = false;

    // Default values (captured on Start)
    private float defaultSaturation;
    private float defaultExposure;
    private float defaultChromaticIntensity;
    private float defaultVignetteIntensity;

    // -- Unity lifecycle ------------------------------------------------------

    private void Start()
    {
        if (postProcessVolume == null)
        {
            Debug.LogWarning("InsanityVisionEffect: No Post Process Volume assigned!");
            return;
        }

        // Grab override references from the volume profile
        postProcessVolume.profile.TryGet(out colorAdjustments);
        postProcessVolume.profile.TryGet(out chromaticAberration);
        postProcessVolume.profile.TryGet(out vignette);

        // Store defaults so we can smoothly return to them
        if (colorAdjustments != null)
        {
            defaultSaturation = colorAdjustments.saturation.value;
            defaultExposure = colorAdjustments.postExposure.value;
        }

        if (chromaticAberration != null)
            defaultChromaticIntensity = chromaticAberration.intensity.value;

        if (vignette != null)
            defaultVignetteIntensity = vignette.intensity.value;

        // Make sure hidden objects start hidden
        SetHiddenObjects(false);
    }

    private void Update()
    {
        effectActive = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        // Smoothly blend effect in or out
        float targetBlend = effectActive ? 1f : 0f;
        currentBlend = Mathf.MoveTowards(currentBlend, targetBlend, Time.deltaTime * transitionSpeed);

        ApplyEffects(currentBlend);

        // Show/hide objects — only toggle at the threshold to avoid constant SetActive calls
        SetHiddenObjects(currentBlend > 0.5f);
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
            // Subtle vignette darkening on edges adds to the dull feeling
            vignette.intensity.value =
                Mathf.Lerp(defaultVignetteIntensity, defaultVignetteIntensity + 0.25f, blend);
        }
    }

    // -- Hidden objects -------------------------------------------------------

    private bool lastHiddenState = false;

    private void SetHiddenObjects(bool visible)
    {
        if (visible == lastHiddenState) return; // no change, skip
        lastHiddenState = visible;

        if (hiddenObjects == null) return;

        foreach (GameObject obj in hiddenObjects)
        {
            if (obj != null)
                obj.SetActive(visible);
        }
    }

    // -- Editor helper --------------------------------------------------------

    private void OnValidate()
    {
        // Clamp inspector values to safe ranges
        targetSaturation = Mathf.Clamp(targetSaturation, -100f, 100f);
        targetChromaticIntensity = Mathf.Clamp01(targetChromaticIntensity);
        transitionSpeed = Mathf.Max(0.1f, transitionSpeed);
    }
}