using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.InputSystem;

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

    [Header("Object Visibility On Shift - GROUPS (toggles ALL children)")]
    [Tooltip("Drag a parent here; ALL its children are HIDDEN normally and REVEALED when shifted.")]
    public GameObject[] revealOnShiftGroups;
    [Tooltip("Drag a parent here; ALL its children are VISIBLE normally and HIDDEN when shifted.")]
    public GameObject[] hideOnShiftGroups;

    [Header("Gamepad")]
    [Tooltip("Analog threshold for L2 to count as 'held'")]
    [Range(0f, 1f)] public float triggerThreshold = 0.1f;

    [Header("Forced Shift")]
    [Tooltip("Driven by RealityTimer. When true, the dark vision is forced ON and manual toggling is locked out.")]
    public bool forcedShift = false;

    /// <summary>True while the player is voluntarily holding Shift and NOT being forced. Used to drain sanity faster.</summary>
    public bool ManualShiftActive { get; private set; }

    // -- Private state --------------------------------------------------------

    private ColorAdjustments colorAdjustments;
    private ChromaticAberration chromaticAberration;
    private Vignette vignette;
    private bool postProcessReady = false;

    
    public float ShiftBlend => currentBlend;
private float currentBlend = 0f;
    private bool lastShiftState = false;

    private float defaultSaturation;
    private float defaultExposure;
    private float defaultChromaticIntensity;
    private float defaultVignetteIntensity;

    // -- Unity lifecycle ------------------------------------------------------

    private void Start()
    {
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
            Debug.LogWarning("InsanityVisionEffect: No Post Process Volume assigned � screen effects disabled but object visibility still works.");
        }

        SetObjectArray(revealOnShift, false);
        SetObjectArray(hideOnShift, true);
        SetGroupArray(revealOnShiftGroups, false);
        SetGroupArray(hideOnShiftGroups, true);
    }

private void Update()
    {
        Gamepad pad = Gamepad.current;

        bool manualInput = Input.GetKey(KeyCode.LeftShift)
            || Input.GetKey(KeyCode.RightShift)
            || (pad != null && pad.rightTrigger.ReadValue() > triggerThreshold);

        // Manual shifting only counts (and is only allowed) when a forced shift is not active.
        ManualShiftActive = manualInput && !forcedShift;

        bool shiftHeld = forcedShift || ManualShiftActive;

        float targetBlend = shiftHeld ? 1f : 0f;
        currentBlend = Mathf.MoveTowards(currentBlend, targetBlend, Time.deltaTime * transitionSpeed);

        if (postProcessReady)
            ApplyEffects(currentBlend);

        bool shiftActive = currentBlend > 0.5f;
        if (shiftActive != lastShiftState)
        {
            lastShiftState = shiftActive;
            SetObjectArray(revealOnShift, shiftActive);
            SetObjectArray(hideOnShift, !shiftActive);
            SetGroupArray(revealOnShiftGroups, shiftActive);
            SetGroupArray(hideOnShiftGroups, !shiftActive);
        }
    }

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

    private void SetObjectArray(GameObject[] objects, bool visible)
    {
        if (objects == null) return;

        foreach (GameObject obj in objects)
        {
            if (obj != null)
                obj.SetActive(visible);
        }
    }

private void SetGroupArray(GameObject[] groups, bool visible)
    {
        if (groups == null) return;
        foreach (GameObject g in groups)
        {
            if (g == null) continue;
            Transform t = g.transform;
            for (int i = 0; i < t.childCount; i++)
                t.GetChild(i).gameObject.SetActive(visible);
        }
    }


    private void OnValidate()
    {
        targetSaturation = Mathf.Clamp(targetSaturation, -100f, 100f);
        targetChromaticIntensity = Mathf.Clamp01(targetChromaticIntensity);
        transitionSpeed = Mathf.Max(0.1f, transitionSpeed);
    }
}