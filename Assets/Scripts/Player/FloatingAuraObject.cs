using UnityEngine;

/// <summary>
/// Attach this script to any existing GameObject that already has a SpriteRenderer.
/// It will add floating, rotation, aura rings, sparkles, and a shadow on top of whatever
/// sprite your GameObject already uses — no extra setup needed.
///
/// Setup:
///   1. Create a Sprite GameObject (or any GameObject with a SpriteRenderer)
///   2. Attach this script to it
///   3. Customize values in the Inspector
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class FloatingAuraObject : MonoBehaviour
{
    // -- Sprite Overrides -----------------------------------------------------

    [Header("Sprite Overrides (optional)")]
    [Tooltip("Override the sprite color. Leave white to keep the original.")]
    public Color spriteColor = Color.white;

    [Tooltip("Sorting Layer name — must match the layer your map/tilemap uses (e.g. 'Default', 'Foreground'). All aura/shadow children will use this same layer so they are never hidden behind the map.")]
    public string sortingLayerName = "Default";

    [Tooltip("Sorting order of the main sprite. Aura layers sit just below this on the same layer.")]
    public int sortingOrder = 5;

    // -- Floating Animation ---------------------------------------------------

    [Header("Floating Animation")]
    [Tooltip("Enable floating up and down animation")]
    public bool enableFloating = true;

    [Tooltip("How high the object floats up and down")]
    public float floatHeight = 0.3f;

    [Tooltip("Speed of the floating animation")]
    public float floatSpeed = 2f;

    [Tooltip("Phase offset (0-6.28) so multiple objects don't sync up")]
    public float floatPhaseOffset = 0f;

    // -- Rotation Animation ---------------------------------------------------

    [Header("Rotation Animation")]
    [Tooltip("Enable slow rotation of the sprite")]
    public bool enableRotation = false;

    [Tooltip("Degrees per second to rotate")]
    public float rotationSpeed = 30f;

    // -- Aura / Glow Ring -----------------------------------------------------

    [Header("Aura Settings")]
    [Tooltip("Enable the inner aura ring around the object")]
    public bool enableAura = true;

    [Tooltip("Color of the aura — alpha controls transparency")]
    public Color auraColor = new Color(0.4f, 0.8f, 1f, 0.5f);

    [Tooltip("Size of the aura ring relative to the sprite")]
    public float auraScale = 1.8f;

    [Tooltip("How fast the aura pulses in and out")]
    public float auraPulseSpeed = 1.5f;

    [Tooltip("How much the aura size pulses (0 = no pulse)")]
    public float auraPulseAmount = 0.15f;

    [Tooltip("Spin the aura ring")]
    public bool spinAura = true;

    [Tooltip("Speed the inner aura spins (degrees/sec)")]
    public float auraSpinSpeed = 20f;

    // -- Outer Aura Ring ------------------------------------------------------

    [Header("Outer Aura Ring")]
    [Tooltip("Add a second outer aura ring for extra glow")]
    public bool enableOuterAura = true;

    [Tooltip("Color of the outer aura ring")]
    public Color outerAuraColor = new Color(0.2f, 0.5f, 1f, 0.25f);

    [Tooltip("Size of the outer aura ring")]
    public float outerAuraScale = 2.5f;

    [Tooltip("Pulse speed of outer ring")]
    public float outerPulseSpeed = 1f;

    [Tooltip("Spin speed of outer ring (negative = opposite direction)")]
    public float outerAuraSpinSpeed = -12f;

    // -- Sparkle Particles ----------------------------------------------------

    [Header("Sparkle Particles")]
    [Tooltip("Enable sparkle particle effect around the object")]
    public bool enableSparkles = true;

    [Tooltip("Color of sparkle particles")]
    public Color sparkleColor = new Color(1f, 1f, 0.6f, 1f);

    [Tooltip("How many sparkles spawn per second")]
    public float sparkleRate = 8f;

    [Tooltip("How far sparkles spread from the object")]
    public float sparkleSpread = 0.8f;

    [Tooltip("How long each sparkle lives (seconds)")]
    public float sparkleLifetime = 0.8f;

    [Tooltip("Size of each sparkle")]
    public float sparkleSize = 0.1f;

    // -- Shadow ---------------------------------------------------------------

    [Header("Shadow")]
    [Tooltip("Show a faint elliptical shadow below the object")]
    public bool enableShadow = true;

    [Tooltip("Color of the shadow")]
    public Color shadowColor = new Color(0f, 0f, 0f, 0.3f);

    [Tooltip("Vertical offset from the object's base position")]
    public float shadowOffset = -0.5f;

    [Tooltip("Shadow shrinks/fades as the object floats up")]
    public bool shadowScalesWithFloat = true;

    // -- Private state --------------------------------------------------------

    private SpriteRenderer spriteRenderer;

    private GameObject auraObj;
    private SpriteRenderer auraRenderer;

    private GameObject outerAuraObj;
    private SpriteRenderer outerAuraRenderer;

    private GameObject shadowObj;
    private SpriteRenderer shadowRenderer;

    private ParticleSystem sparkleSystem;

    private Vector3 startPosition;
    private float floatTimer;

    // -- Unity lifecycle ------------------------------------------------------

    private void Awake()
    {
        startPosition = transform.position;

        // Grab the SpriteRenderer that already lives on this GameObject
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.color = spriteColor;
        spriteRenderer.sortingLayerName = sortingLayerName;
        spriteRenderer.sortingOrder = sortingOrder;

        BuildEffects();
    }

    private void Update()
    {
        HandleFloating();
        HandleRotation();
        HandleAura();
        HandleShadow();
    }

    // -- Build effects --------------------------------------------------------

    private void BuildEffects()
    {
        if (enableAura)
            BuildAuraRing();

        if (enableOuterAura)
            BuildOuterAuraRing();

        if (enableShadow)
            BuildShadow();

        if (enableSparkles)
            BuildSparkles();
    }

    private void BuildAuraRing()
    {
        auraObj = new GameObject("Aura_Inner");
        auraObj.transform.SetParent(transform);
        auraObj.transform.localPosition = Vector3.zero;
        auraObj.transform.localScale = Vector3.one * auraScale;

        auraRenderer = auraObj.AddComponent<SpriteRenderer>();
        auraRenderer.sprite = CreateCircleSprite(64);
        auraRenderer.color = auraColor;
        auraRenderer.sortingLayerName = sortingLayerName;
        auraRenderer.sortingOrder = sortingOrder - 1;
    }

    private void BuildOuterAuraRing()
    {
        outerAuraObj = new GameObject("Aura_Outer");
        outerAuraObj.transform.SetParent(transform);
        outerAuraObj.transform.localPosition = Vector3.zero;
        outerAuraObj.transform.localScale = Vector3.one * outerAuraScale;

        outerAuraRenderer = outerAuraObj.AddComponent<SpriteRenderer>();
        outerAuraRenderer.sprite = CreateCircleSprite(64);
        outerAuraRenderer.color = outerAuraColor;
        outerAuraRenderer.sortingLayerName = sortingLayerName;
        outerAuraRenderer.sortingOrder = sortingOrder - 2;
    }

    private void BuildShadow()
    {
        // Shadow is a sibling (child of the same parent) so it doesn't move with the float
        Transform shadowParent = transform.parent != null ? transform.parent : transform;

        shadowObj = new GameObject("Shadow");
        shadowObj.transform.SetParent(shadowParent);
        shadowObj.transform.position = new Vector3(
            transform.position.x,
            transform.position.y + shadowOffset,
            transform.position.z
        );
        shadowObj.transform.localScale = new Vector3(transform.localScale.x * 0.8f, transform.localScale.y * 0.2f, 1f);

        shadowRenderer = shadowObj.AddComponent<SpriteRenderer>();
        shadowRenderer.sprite = CreateCircleSprite(32);
        shadowRenderer.color = shadowColor;
        shadowRenderer.sortingLayerName = sortingLayerName;
        shadowRenderer.sortingOrder = sortingOrder - 3;
    }

    private void BuildSparkles()
    {
        GameObject sparkleObj = new GameObject("Sparkles");
        sparkleObj.transform.SetParent(transform);
        sparkleObj.transform.localPosition = Vector3.zero;

        sparkleSystem = sparkleObj.AddComponent<ParticleSystem>();

        var main = sparkleSystem.main;
        main.loop = true;
        main.startLifetime = sparkleLifetime;
        main.startSpeed = 0.3f;
        main.startSize = sparkleSize;
        main.startColor = sparkleColor;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = sparkleSystem.emission;
        emission.rateOverTime = sparkleRate;

        var shape = sparkleSystem.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = sparkleSpread;

        var colorOverLifetime = sparkleSystem.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(sparkleColor, 0f),
                new GradientColorKey(sparkleColor, 1f)
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLifetime.color = grad;

        var sizeOverLifetime = sparkleSystem.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f, 1f);
        sizeCurve.AddKey(1f, 0f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        sparkleSystem.Play();
    }

    // -- Animation handlers ---------------------------------------------------

    private void HandleFloating()
    {
        if (!enableFloating) return;

        floatTimer += Time.deltaTime * floatSpeed;
        float yOffset = Mathf.Sin(floatTimer + floatPhaseOffset) * floatHeight;
        transform.position = startPosition + new Vector3(0f, yOffset, 0f);
    }

    private void HandleRotation()
    {
        if (!enableRotation) return;
        transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
    }

    private void HandleAura()
    {
        float pulse = (Mathf.Sin(Time.time * auraPulseSpeed) + 1f) * 0.5f;

        if (auraObj != null)
        {
            auraObj.transform.localScale = Vector3.one * (auraScale + pulse * auraPulseAmount);

            if (spinAura)
                auraObj.transform.Rotate(0f, 0f, auraSpinSpeed * Time.deltaTime);

            Color c = auraColor;
            c.a = auraColor.a * (0.7f + pulse * 0.3f);
            auraRenderer.color = c;
        }

        if (outerAuraObj != null)
        {
            float outerPulse = (Mathf.Sin(Time.time * outerPulseSpeed + 1f) + 1f) * 0.5f;
            outerAuraObj.transform.localScale = Vector3.one * (outerAuraScale + outerPulse * auraPulseAmount);

            if (spinAura)
                outerAuraObj.transform.Rotate(0f, 0f, outerAuraSpinSpeed * Time.deltaTime);

            Color oc = outerAuraColor;
            oc.a = outerAuraColor.a * (0.6f + outerPulse * 0.4f);
            outerAuraRenderer.color = oc;
        }
    }

    private void HandleShadow()
    {
        if (shadowObj == null) return;

        // Keep shadow anchored to world-space base position
        shadowObj.transform.position = new Vector3(
            transform.position.x,
            startPosition.y + shadowOffset,
            transform.position.z
        );

        if (shadowScalesWithFloat && enableFloating)
        {
            float floatProgress = (transform.position.y - startPosition.y + floatHeight) / (floatHeight * 2f);
            float scaleX = Mathf.Lerp(transform.localScale.x * 0.9f, transform.localScale.x * 0.5f, floatProgress);
            shadowObj.transform.localScale = new Vector3(scaleX, transform.localScale.y * 0.2f, 1f);

            Color sc = shadowColor;
            sc.a = Mathf.Lerp(shadowColor.a, shadowColor.a * 0.3f, floatProgress);
            shadowRenderer.color = sc;
        }
    }

    // -- Circle sprite generator (used for aura rings and shadow) -------------

    private Sprite CreateCircleSprite(int resolution)
    {
        Texture2D tex = new Texture2D(resolution, resolution);
        Vector2 center = new Vector2(resolution * 0.5f, resolution * 0.5f);
        float radius = resolution * 0.5f;

        for (int x = 0; x < resolution; x++)
        {
            for (int y = 0; y < resolution; y++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                float alpha = Mathf.Clamp01(1f - dist / radius);
                alpha = Mathf.Pow(alpha, 0.5f); // soft edge falloff
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, resolution, resolution), new Vector2(0.5f, 0.5f));
    }

    // -- Editor helper --------------------------------------------------------

    private void OnValidate()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = spriteColor;
            spriteRenderer.sortingLayerName = sortingLayerName;
            spriteRenderer.sortingOrder = sortingOrder;
        }

        if (auraRenderer != null)
        {
            auraRenderer.sortingLayerName = sortingLayerName;
            auraRenderer.sortingOrder = sortingOrder - 1;
        }

        if (outerAuraRenderer != null)
        {
            outerAuraRenderer.sortingLayerName = sortingLayerName;
            outerAuraRenderer.sortingOrder = sortingOrder - 2;
        }

        if (shadowRenderer != null)
        {
            shadowRenderer.sortingLayerName = sortingLayerName;
            shadowRenderer.sortingOrder = sortingOrder - 3;
        }

        floatHeight = Mathf.Max(0f, floatHeight);
        floatSpeed = Mathf.Max(0f, floatSpeed);
        auraScale = Mathf.Max(0.1f, auraScale);
        outerAuraScale = Mathf.Max(0.1f, outerAuraScale);
        sparkleRate = Mathf.Max(0f, sparkleRate);
        sparkleSize = Mathf.Max(0.01f, sparkleSize);
    }
}