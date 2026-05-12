using UnityEngine;
using UnityEngine.Rendering.Universal;

public class FlickeringLightEffectScript : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private Light2D targetLight;

    [Header("Base Intensity")]
    [SerializeField] private float minIntensity = 0.6f;
    [SerializeField] private float maxIntensity = 1.2f;

    [Header("Flicker Movement")]
    [SerializeField] private float flickerSpeed = 4f;
    [SerializeField] private float smoothing = 8f;

    [Header("Random Flicker Spikes")]
    [SerializeField] private float spikeChance = 0.08f;
    [SerializeField] private float spikeStrength = 0.35f;
    [SerializeField] private float spikeRecoverySpeed = 12f;

    private float noiseSeed;
    private float currentIntensity;
    private float spikeOffset;

    void Awake()
    {
        if (targetLight == null)
            targetLight = GetComponent<Light2D>();

        noiseSeed = Random.Range(0f, 999f);
        currentIntensity = targetLight.intensity;
    }

    void Update()
    {
        float time = Time.time * flickerSpeed;

        // Smooth natural flicker using Perlin noise
        float noise = Mathf.PerlinNoise(noiseSeed, time);

        float targetIntensity = Mathf.Lerp(
            minIntensity,
            maxIntensity,
            noise
        );

        // Occasional sharper flickers (power instability vibe)
        if (Random.value < spikeChance * Time.deltaTime * 60f)
        {
            float direction = Random.value > 0.5f ? 1f : -1f;
            spikeOffset = direction * spikeStrength;
        }

        // Recover spike smoothly
        spikeOffset = Mathf.Lerp(
            spikeOffset,
            0f,
            spikeRecoverySpeed * Time.deltaTime
        );

        targetIntensity += spikeOffset;

        // Final smoothing
        currentIntensity = Mathf.Lerp(
            currentIntensity,
            targetIntensity,
            smoothing * Time.deltaTime
        );

        targetLight.intensity = currentIntensity;
    }
}