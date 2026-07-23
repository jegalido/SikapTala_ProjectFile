using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Realistic failing-light flicker for a 2D URP Light2D. The light mostly holds
/// steady, then stutters in short random bursts (like a dying fluorescent tube).
/// The lower the player's sanity, the MORE OFTEN it flickers (and the harder it
/// dips), tying the environment to the character's state of mind.
/// </summary>
[RequireComponent(typeof(Light2D))]
public class SanityLightFlicker : MonoBehaviour
{
    [Header("References (auto-found if empty)")]
    public Light2D light2D;
    public InsanityBar sanity;

    [Tooltip("Base intensity. Leave < 0 to capture the light's current intensity at Start.")]
    public float baseIntensity = -1f;

    [Header("How often it flickers (seconds between bursts)")]
    [Tooltip("Interval between flicker bursts at FULL sanity (rare).")]
    public float calmInterval = 6f;
    [Tooltip("Interval between flicker bursts at EMPTY sanity (frequent).")]
    public float panicInterval = 0.6f;

    [Header("Flicker burst")]
    public Vector2 burstDuration = new Vector2(0.06f, 0.4f);
    public Vector2 stutterRate = new Vector2(0.02f, 0.09f);
    [Tooltip("Lowest the light dips to during a flicker, as a fraction of base intensity. 0.2 = down to 20% (80% dimmer). It never fully turns off.")]
    [Range(0.05f, 1f)] public float minFlickerScale = 0.2f;

    private float nextFlicker;
    private float burstEnd;
    private float nextStutter;
    private float target;
    private bool bursting;

    private void Start()
    {
        if (light2D == null) light2D = GetComponent<Light2D>();
        if (sanity == null) sanity = FindFirstObjectByType<InsanityBar>();
        if (baseIntensity < 0f && light2D != null) baseIntensity = light2D.intensity;
        target = baseIntensity;
        // random offset so all the lights don't flicker in unison
        nextFlicker = Time.time + Random.Range(0f, calmInterval);
    }

    private void Update()
    {
        if (light2D == null) return;

        float s = sanity != null ? Mathf.Clamp01(sanity.GetInsanity() / 100f) : 1f;
        float t = Time.time;

        if (bursting)
        {
            if (t >= nextStutter)
            {
                nextStutter = t + Random.Range(stutterRate.x, stutterRate.y);
                target = baseIntensity * Random.Range(minFlickerScale, 1f);
            }
            if (t >= burstEnd)
            {
                bursting = false;
                target = baseIntensity;
            }
        }
        else
        {
            target = baseIntensity;
            if (t >= nextFlicker)
            {
                bursting = true;
                burstEnd = t + Random.Range(burstDuration.x, burstDuration.y);
                nextStutter = t;
                float interval = Mathf.Lerp(panicInterval, calmInterval, s); // low sanity -> short interval
                nextFlicker = burstEnd + Random.Range(interval * 0.5f, interval * 1.5f);
            }
        }

        light2D.intensity = target;
    }

    private void OnDisable()
    {
        if (light2D != null && baseIntensity >= 0f) light2D.intensity = baseIntensity;
    }
}
