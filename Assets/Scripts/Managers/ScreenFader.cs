using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Full-screen death / respawn transition. On death it "passes out": a red gasp,
/// a tunnel-vision vignette closing in, then black. It holds in the dark, then fades
/// back in as consciousness returns. The whole time the game is frozen (timeScale 0)
/// on unscaled time, so the sanity / reality bars only resume AFTER the fade finishes.
///
/// Any respawn should route through ScreenFader.Respawn(player, position).
/// </summary>
public class ScreenFader : MonoBehaviour
{
    public static ScreenFader Instance { get; private set; }

    [Header("Layers (full-screen, each with a CanvasGroup)")]
    [SerializeField] private CanvasGroup black;
    [SerializeField] private CanvasGroup vignette;
    [SerializeField] private CanvasGroup flash;

    [Header("Timing (seconds, unscaled)")]
    public float fadeOutTime = 1.15f;
    public float holdTime = 0.7f;
    public float fadeInTime = 1.0f;

    [Header("Feel")]
    [Range(0f, 1f)] public float flashStrength = 0.5f;

    [Header("Events (hook SFX: gasp / flatline / heartbeat)")]
    public UnityEvent onBlackout;
    public UnityEvent onRecover;

    public bool IsTransitioning { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        Set(black, 0f); Set(vignette, 0f); Set(flash, 0f);
    }

    /// <summary>Fade out, teleport the player to <paramref name="to"/> at full black, fade in.</summary>
    public static void Respawn(Transform player, Vector3 to)
    {
        Action teleport = () =>
        {
            if (player == null) return;
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = Vector2.zero;
            player.position = to;
        };
        if (Instance != null) Instance.PlayDeathRespawn(teleport);
        else teleport();
    }

    public void PlayDeathRespawn(Action atBlackout)
    {
        if (IsTransitioning) return; // ignore re-entry while a transition plays
        StartCoroutine(Routine(atBlackout));
    }

    private IEnumerator Routine(Action atBlackout)
    {
        IsTransitioning = true;
        float prev = Time.timeScale;
        Time.timeScale = 0f;

        // Gasp flash.
        if (flash != null)
        {
            float ft = 0f, fd = 0.12f;
            while (ft < fd) { ft += Time.unscaledDeltaTime; Set(flash, Mathf.Lerp(0f, flashStrength, ft / fd)); yield return null; }
        }

        // Collapse: tunnel-vision vignette closes, black fills.
        float t = 0f;
        while (t < fadeOutTime)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / fadeOutTime);
            Set(vignette, Mathf.Clamp01(k * 1.5f));
            Set(black, k * k);
            if (flash != null) Set(flash, Mathf.Lerp(flashStrength, 0f, k));
            yield return null;
        }
        Set(black, 1f);

        onBlackout?.Invoke();
        atBlackout?.Invoke();

        float h = 0f;
        while (h < holdTime) { h += Time.unscaledDeltaTime; yield return null; }

        onRecover?.Invoke();

        // Fade back in - consciousness returns.
        t = 0f;
        while (t < fadeInTime)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / fadeInTime);
            Set(black, 1f - k);
            Set(vignette, (1f - k) * 0.7f);
            yield return null;
        }
        Set(black, 0f); Set(vignette, 0f); if (flash != null) Set(flash, 0f);

        Time.timeScale = (prev <= 0f) ? 1f : prev;
        IsTransitioning = false;
    }

    private static void Set(CanvasGroup g, float a)
    {
        if (g != null) g.alpha = a;
    }
}
