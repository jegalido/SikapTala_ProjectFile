using UnityEngine;

/// <summary>
/// Warps the existing BGM as the player shifts into the dark (real) world:
/// pitch drops and a low-pass filter muffles it, so it turns creepy / distorted,
/// then snaps back to normal in the warm world. Reuses the current BGM AudioSource -
/// no extra audio clip required.
/// </summary>
public class DarkWorldAudio : MonoBehaviour
{
    [Header("References (auto-found if empty)")]
    [Tooltip("The BGM AudioSource to warp. Auto-finds a 'BGM' object, else the first AudioSource.")]
    public AudioSource bgm;
    public InsanityVisionEffect vision;

    [Header("Dark-world warp")]
    [Tooltip("Playback pitch at full dark (lower = deeper / slower / creepier).")]
    public float darkPitch = 0.78f;
    [Tooltip("Add & drive a low-pass filter for a muffled 'underwater' feel.")]
    public bool useLowPass = true;
    [Tooltip("Low-pass cutoff (Hz) at full dark. ~800-1200 = muffled.")]
    public float darkCutoffHz = 950f;
    [Tooltip("How quickly the warp follows the shift.")]
    public float responsiveness = 6f;

    private float basePitch = 1f;
    private float baseCutoff = 22000f;
    private AudioLowPassFilter lowPass;

    private void Start()
    {
        if (vision == null) vision = FindFirstObjectByType<InsanityVisionEffect>();
        if (bgm == null)
        {
            GameObject go = GameObject.Find("BGM");
            if (go != null) bgm = go.GetComponent<AudioSource>();
            if (bgm == null) bgm = FindFirstObjectByType<AudioSource>();
        }
        if (bgm != null)
        {
            basePitch = bgm.pitch;
            if (useLowPass)
            {
                lowPass = bgm.GetComponent<AudioLowPassFilter>();
                if (lowPass == null) lowPass = bgm.gameObject.AddComponent<AudioLowPassFilter>();
                baseCutoff = lowPass.cutoffFrequency;
                if (baseCutoff < 5000f) baseCutoff = 22000f; // treat an existing low value as "off"
                lowPass.cutoffFrequency = baseCutoff;
            }
        }
    }

    private void Update()
    {
        if (bgm == null || vision == null) return;
        float b = Mathf.Clamp01(vision.ShiftBlend);
        float t = 1f - Mathf.Exp(-responsiveness * Time.unscaledDeltaTime);

        float targetPitch = Mathf.Lerp(basePitch, darkPitch, b);
        bgm.pitch = Mathf.Lerp(bgm.pitch, targetPitch, t);

        if (lowPass != null)
        {
            float targetCut = Mathf.Lerp(baseCutoff, darkCutoffHz, b);
            lowPass.cutoffFrequency = Mathf.Lerp(lowPass.cutoffFrequency, targetCut, t);
        }
    }
}
