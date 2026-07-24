using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Hold Space (keyboard) or the gamepad North face button (Triangle / Y) to skip a cutscene.
/// On skip it jumps the PlayableDirector to its end and Stops it, which fires the director's
/// existing 'stopped' handler (TimelineSceneLoader / LoadMenuOnTimelineEnd) so the correct
/// next scene loads. If the scene has no director (or you set overrideScene), it loads
/// overrideScene directly. Auto-builds a small 'hold to skip' hint with a progress bar.
/// </summary>
public class HoldToSkip : MonoBehaviour
{
    [Tooltip("Director to skip. Auto-found if left empty.")]
    [SerializeField] private PlayableDirector director;

    [Tooltip("Seconds to hold before the cutscene skips.")]
    [SerializeField] private float holdDuration = 1f;

    [Tooltip("Optional. If set, load this scene directly instead of stopping the director.")]
    [SerializeField] private string overrideScene = "";

    [Tooltip("Show the on-screen hold-to-skip hint + progress bar.")]
    [SerializeField] private bool showHint = true;

    [Tooltip("Hint label text.")]
    [SerializeField] private string hintText = "hold  SPACE / Y  to skip";

    private float held;
    private bool done;
    private CanvasGroup hintGroup;
    private RectTransform fill;

    private void Awake()
    {
        if (director == null) director = FindFirstObjectByType<PlayableDirector>(FindObjectsInactive.Include);
        if (showHint) BuildHint();
    }

    private void Update()
    {
        if (done) return;

        bool holding = false;
        Keyboard kb = Keyboard.current;
        if (kb != null && kb.spaceKey.isPressed) holding = true;
        Gamepad gp = Gamepad.current;
        if (gp != null && gp.buttonNorth.isPressed) holding = true; // Triangle / Y

        held = holding ? held + Time.unscaledDeltaTime : Mathf.Max(0f, held - Time.unscaledDeltaTime * 2f);
        float p = Mathf.Clamp01(held / Mathf.Max(0.01f, holdDuration));

        if (hintGroup != null)
        {
            hintGroup.alpha = Mathf.MoveTowards(hintGroup.alpha, holding ? 1f : 0.4f, Time.unscaledDeltaTime * 5f);
            if (fill != null) fill.anchorMax = new Vector2(p, 0f);
        }

        if (p >= 1f) Skip();
    }

    private void Skip()
    {
        if (done) return;
        done = true;
        Time.timeScale = 1f;

        if (string.IsNullOrEmpty(overrideScene) && director != null && director.playableAsset != null)
        {
            director.time = director.duration;
            director.Evaluate();
            director.Stop();
            return;
        }

        SceneManager.LoadScene(string.IsNullOrEmpty(overrideScene) ? SceneManager.GetActiveScene().name : overrideScene);
    }

    private void BuildHint()
    {
        try
        {
            GameObject canvasGO = new GameObject("SkipHintCanvas");
            canvasGO.transform.SetParent(transform, false);
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 6000;
            CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            GameObject panel = new GameObject("Hint", typeof(RectTransform));
            panel.transform.SetParent(canvasGO.transform, false);
            RectTransform prt = panel.GetComponent<RectTransform>();
            prt.anchorMin = new Vector2(1f, 0f);
            prt.anchorMax = new Vector2(1f, 0f);
            prt.pivot = new Vector2(1f, 0f);
            prt.sizeDelta = new Vector2(360f, 64f);
            prt.anchoredPosition = new Vector2(-48f, 48f);
            hintGroup = panel.AddComponent<CanvasGroup>();
            hintGroup.alpha = 0.4f;

            GameObject txtGO = new GameObject("Label", typeof(RectTransform));
            txtGO.transform.SetParent(panel.transform, false);
            TextMeshProUGUI tmp = txtGO.AddComponent<TextMeshProUGUI>();
            tmp.text = hintText;
            tmp.fontSize = 28f;
            tmp.alignment = TextAlignmentOptions.BottomRight;
            tmp.color = new Color(1f, 1f, 1f, 0.9f);
            RectTransform trt = txtGO.GetComponent<RectTransform>();
            trt.anchorMin = new Vector2(0f, 0.35f);
            trt.anchorMax = new Vector2(1f, 1f);
            trt.offsetMin = Vector2.zero;
            trt.offsetMax = Vector2.zero;

            GameObject bg = new GameObject("BarBG", typeof(RectTransform));
            bg.transform.SetParent(panel.transform, false);
            Image bgImg = bg.AddComponent<Image>();
            bgImg.color = new Color(1f, 1f, 1f, 0.18f);
            RectTransform bgrt = bg.GetComponent<RectTransform>();
            bgrt.anchorMin = new Vector2(0f, 0f);
            bgrt.anchorMax = new Vector2(1f, 0f);
            bgrt.pivot = new Vector2(0f, 0f);
            bgrt.sizeDelta = new Vector2(0f, 6f);
            bgrt.anchoredPosition = Vector2.zero;

            GameObject fl = new GameObject("BarFill", typeof(RectTransform));
            fl.transform.SetParent(panel.transform, false);
            Image flImg = fl.AddComponent<Image>();
            flImg.color = new Color(0.85f, 0.15f, 0.15f, 0.95f);
            fill = fl.GetComponent<RectTransform>();
            fill.anchorMin = new Vector2(0f, 0f);
            fill.anchorMax = new Vector2(0f, 0f);
            fill.pivot = new Vector2(0f, 0f);
            fill.sizeDelta = new Vector2(0f, 6f);
            fill.anchoredPosition = Vector2.zero;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("HoldToSkip hint UI failed: " + e.Message);
            hintGroup = null;
            fill = null;
        }
    }
}
