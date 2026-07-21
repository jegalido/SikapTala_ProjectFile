using UnityEngine;

/// <summary>
/// A localized "reality glitch" that appears at an object reacting to the Insanity
/// Vision (Shift). It flickers in at RANDOM intervals (roughly 1-2s on, a few seconds
/// off) and, while on, distorts with a tight chromatic (red/blue) split, digital
/// horizontal tearing and jitter - a reality-distortion cue telling players to press
/// Shift here. Fades out while the player is actually shifting. Purely visual.
/// </summary>
public class RealityGlitchHint : MonoBehaviour
{
    [Tooltip("Two tinted copies of the target sprite that get split apart for the chromatic-glitch look.")]
    [SerializeField] private SpriteRenderer[] channels;

    [Header("Glitch look")]
    [Range(0f, 1f)][SerializeField] private float baseAlpha = 0.6f;
    [Tooltip("How far the red/blue copies split. Keep small for a tight glitch.")]
    [SerializeField] private float chromaticOffset = 0.07f;
    [SerializeField] private float positionJitter = 0.11f;
    [Tooltip("Strength of the digital horizontal tearing.")]
    [SerializeField] private float sliceAmount = 0.35f;
    [Range(0f, 1f)][SerializeField] private float sliceChance = 0.5f;
    [SerializeField] private float flickerSpeed = 28f;
    [Range(0f, 1f)][SerializeField] private float minFlicker = 0.25f;

    [Header("Random appearance (seconds)")]
    [Tooltip("How long the glitch stays on (random between x and y).")]
    [SerializeField] private Vector2 visibleDuration = new Vector2(1f, 2f);
    [Tooltip("How long the glitch stays off between appearances (random between x and y).")]
    [SerializeField] private Vector2 hiddenDuration = new Vector2(2.5f, 4f);
    [Tooltip("Quick pop in / out time.")]
    [SerializeField] private float fadeTime = 0.14f;

    [Tooltip("Fade out while the player is shifting reality (blend 0 -> 1).")]
    [SerializeField] private bool fadeOutWhenShifted = true;

    private bool visiblePhase;
    private float phaseTimer;
    private float envelope;
    private Vector3[] baseLocalPos;
    private float seed;

    private void Awake()
    {
        seed = Random.value * 100f;
        CacheBase();
        visiblePhase = false;
        phaseTimer = Random.Range(0f, Mathf.Max(0.01f, hiddenDuration.y));
        envelope = 0f;
        Apply(0f, 0f);
    }

    private void OnEnable()
    {
        CacheBase();
    }

    private void CacheBase()
    {
        if (channels == null) return;
        baseLocalPos = new Vector3[channels.Length];
        for (int i = 0; i < channels.Length; i++)
            if (channels[i] != null) baseLocalPos[i] = channels[i].transform.localPosition;
    }

    private void Update()
    {
        if (channels == null || channels.Length == 0) return;

        // Random on/off cycle.
        phaseTimer -= Time.deltaTime;
        if (phaseTimer <= 0f)
        {
            visiblePhase = !visiblePhase;
            phaseTimer = visiblePhase
                ? Random.Range(visibleDuration.x, visibleDuration.y)
                : Random.Range(hiddenDuration.x, hiddenDuration.y);
        }
        float target = visiblePhase ? 1f : 0f;
        envelope = Mathf.MoveTowards(envelope, target, (1f / Mathf.Max(0.01f, fadeTime)) * Time.deltaTime);

        float t = Time.realtimeSinceStartup;
        float flicker = Mathf.Lerp(minFlicker, 1f, 0.5f + 0.5f * Mathf.Sin((t + seed) * flickerSpeed));
        float shiftFade = fadeOutWhenShifted ? (1f - Mathf.Clamp01(InsanityVisionEffect.ShiftBlend)) : 1f;
        float alpha = baseAlpha * flicker * envelope * shiftFade;

        Apply(alpha, envelope);
    }

    private void Apply(float alpha, float env)
    {
        float t = Time.realtimeSinceStartup;

        // Quantized (digital) horizontal tearing.
        float step = Mathf.Floor(t * 16f);
        float slice = (Mathf.PerlinNoise(step * 0.37f, seed) > 1f - sliceChance)
            ? (Mathf.PerlinNoise(step * 1.7f, seed + 3f) - 0.5f) * 2f * sliceAmount
            : 0f;

        Vector2 jit = new Vector2(
            (Mathf.PerlinNoise(t * 40f, seed) - 0.5f) * 2f,
            (Mathf.PerlinNoise(seed, t * 40f) - 0.5f) * 2f) * positionJitter * env;

        for (int i = 0; i < channels.Length; i++)
        {
            SpriteRenderer sr = channels[i];
            if (sr == null) continue;
            float dir = (i % 2 == 0) ? 1f : -1f;
            Vector3 baseOff = (baseLocalPos != null && i < baseLocalPos.Length) ? baseLocalPos[i] : Vector3.zero;
            sr.transform.localPosition = baseOff + new Vector3(jit.x + (dir * chromaticOffset + slice) * env, jit.y, 0f);
            Color c = sr.color;
            c.a = alpha;
            sr.color = c;
        }
    }
}
