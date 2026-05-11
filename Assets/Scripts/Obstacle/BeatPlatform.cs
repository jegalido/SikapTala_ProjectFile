using UnityEngine;

public class BeatPlatform : MonoBehaviour
{
    [Header("Beat / Pulse Animation")]
    [SerializeField] private float beatFrequency = 1.2f;       // Beats per second
    [SerializeField] private float beatScaleAmount = 0.15f;    // How much it scales up on beat
    [SerializeField] private float squishAmount = 0.08f;       // Counter-squish on opposite axis
    [SerializeField] private AnimationCurve beatCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Bounce")]
    [SerializeField] private float bounceForce = 20f;          // Upward force applied to player
    [SerializeField] private float bounceScaleMultiplier = 1.4f; // Extra squish on player landing
    [SerializeField] private float bounceRecoverySpeed = 8f;   // How fast platform recovers after bounce

    [Header("Visual Feedback")]
    [SerializeField] private Color restColor = Color.white;
    [SerializeField] private Color beatColor = new Color(1f, 0.85f, 0.3f); // Warm yellow pulse
    [SerializeField] private Color bounceColor = new Color(0.4f, 1f, 0.6f); // Green flash on bounce

    private Vector3 baseScale;
    private SpriteRenderer sr;
    private Rigidbody2D playerRb;           // cached on first contact

    private float beatTimer;
    private bool isBouncing;
    private float bounceTimer;
    private Vector3 bounceTargetScale;
    private const float bounceDuration = 0.25f;

    
    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        baseScale = transform.localScale;
    }

    private void Update()
    {
        if (isBouncing)
        {
            HandleBounceRecovery();
        }
        else
        {
            HandleBeatAnimation();
        }
    }

    
    private void HandleBeatAnimation()
    {
        beatTimer += Time.deltaTime * beatFrequency;

        // Two-phase beat: quick expand → slow fall back (like a heartbeat)
        float t = Mathf.PingPong(beatTimer, 1f);
        float pulse = beatCurve.Evaluate(t);

        // Scale: grow X & Y slightly, counter-squish the other axis
        float xScale = baseScale.x * (1f + beatScaleAmount * pulse);
        float yScale = baseScale.y * (1f - squishAmount * pulse);
        transform.localScale = new Vector3(xScale, yScale, baseScale.z);

        // Color lerp: subtle warm tint on the beat peak
        if (sr != null)
            sr.color = Color.Lerp(restColor, beatColor, pulse);
    }


    private void HandleBounceRecovery()
    {
        bounceTimer += Time.deltaTime * bounceRecoverySpeed;
        float t = Mathf.Clamp01(bounceTimer);

        // Ease back from squished bounce scale to base scale
        transform.localScale = Vector3.Lerp(bounceTargetScale, baseScale, t);

        if (sr != null)
            sr.color = Color.Lerp(bounceColor, restColor, t);

        if (t >= 1f)
        {
            isBouncing = false;
            bounceTimer = 0f;
            beatTimer = 0f; // restart beat cycle cleanly
        }
    }


    private void OnCollisionEnter2D(Collision2D col)
    {
        // Only bounce when something lands on TOP of the platform
        // Check that the contact normal points upward (player is above)
        foreach (ContactPoint2D contact in col.contacts)
        {
            if (contact.normal.y < -0.5f) // normal points down → player is on top
            {
                TryBounceObject(col.rigidbody);
                return;
            }
        }
    }

    private void TryBounceObject(Rigidbody2D targetRb)
    {
        if (targetRb == null) return;

        // Apply upward bounce force (override vertical velocity for consistent feel)
        targetRb.linearVelocity = new Vector2(targetRb.linearVelocity.x, bounceForce);

        // Trigger squish animation on this platform
        TriggerBounceSquish();
    }

    private void TriggerBounceSquish()
    {
        // Squish: wide & flat on impact
        bounceTargetScale = new Vector3(
            baseScale.x * (1f + bounceScaleMultiplier * beatScaleAmount * 2f),
            baseScale.y * (1f - bounceScaleMultiplier * squishAmount * 2f),
            baseScale.z
        );
        transform.localScale = bounceTargetScale;

        if (sr != null)
            sr.color = bounceColor;

        isBouncing = true;
        bounceTimer = 0f;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.2f, 1f, 0.4f, 0.35f);
        Gizmos.DrawWireCube(transform.position, transform.localScale * 1.05f);
    }
}