using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// The ending "choice" screen. Fades to black and stays black, then a creepy
/// glitching plea (the mother asking you to stay) and two buttons fade in:
/// Stay -> StayCutscene, Leave -> LeaveCutscene. The game is frozen while choosing.
/// </summary>
public class EndingChoice : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private CanvasGroup blackGroup;
    [SerializeField] private CanvasGroup choiceGroup;
    [SerializeField] private TMP_Text glitchText;
    [SerializeField] private Button stayButton;
    [SerializeField] private Button leaveButton;

    [Header("Scenes to load")]
    [SerializeField] private string stayScene = "StayCutscene";
    [SerializeField] private string leaveScene = "LeaveCutscene";

    [Header("Creepy plea (glitches, chosen at random)")]
    [TextArea]
    [SerializeField] private string[] phrases =
    {
        "please... don't leave me again",
        "stay. you're finally home.",
        "there's nothing left for you out there",
        "you'll only suffer if you go",
        "i've waited so long for you",
        "don't make me be alone again",
    };
    [SerializeField] private float phraseSwapTime = 3.5f;

    [Header("Timing")]
    [SerializeField] private float blackFadeTime = 1.6f;
    [SerializeField] private float choiceFadeTime = 1.3f;

    [Header("Glitch")]
    [SerializeField] private float jitter = 6f;
    [Range(0f, 1f)][SerializeField] private float glitchAmount = 0.12f;
    [SerializeField] private string glitchChars = "#%&@?/\\*<>_";

    private bool shown;
    private string current = "";
    private Vector2 textBasePos;
    private float phraseTimer;

    private void Awake()
    {
        if (blackGroup != null) blackGroup.alpha = 0f;
        if (choiceGroup != null)
        {
            choiceGroup.alpha = 0f;
            choiceGroup.interactable = false;
            choiceGroup.blocksRaycasts = false;
        }
        if (stayButton != null) stayButton.onClick.AddListener(OnStay);
        if (leaveButton != null) leaveButton.onClick.AddListener(OnLeave);
        if (glitchText != null) textBasePos = glitchText.rectTransform.anchoredPosition;
    }

    public void Show()
    {
        if (shown) return;
        shown = true;
        NextPhrase();
        StartCoroutine(ShowRoutine());
    }

    private IEnumerator ShowRoutine()
    {
        Time.timeScale = 0f;
        yield return Fade(blackGroup, 0f, 1f, blackFadeTime);
        yield return Fade(choiceGroup, 0f, 1f, choiceFadeTime);
        if (choiceGroup != null)
        {
            choiceGroup.interactable = true;
            choiceGroup.blocksRaycasts = true;
        }
        if (EventSystem.current != null && stayButton != null)
            EventSystem.current.SetSelectedGameObject(stayButton.gameObject);
    }

    private IEnumerator Fade(CanvasGroup g, float from, float to, float dur)
    {
        if (g == null) yield break;
        float t = 0f;
        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            g.alpha = Mathf.Lerp(from, to, t / Mathf.Max(0.01f, dur));
            yield return null;
        }
        g.alpha = to;
    }

    private void Update()
    {
        if (!shown || glitchText == null) return;

        phraseTimer += Time.unscaledDeltaTime;
        if (phraseTimer >= phraseSwapTime) NextPhrase();

        float t = Time.unscaledTime;
        Vector2 j = new Vector2(Mathf.PerlinNoise(t * 18f, 3f) - 0.5f, Mathf.PerlinNoise(7f, t * 18f) - 0.5f) * jitter;
        glitchText.rectTransform.anchoredPosition = textBasePos + j;

        if (!string.IsNullOrEmpty(current) && glitchChars.Length > 0)
        {
            char[] arr = current.ToCharArray();
            for (int i = 0; i < arr.Length; i++)
                if (arr[i] != ' ' && Random.value < glitchAmount * 0.12f)
                    arr[i] = glitchChars[Random.Range(0, glitchChars.Length)];
            glitchText.text = new string(arr);
        }
    }

    private void NextPhrase()
    {
        phraseTimer = 0f;
        if (phrases == null || phrases.Length == 0) { current = ""; return; }
        current = phrases[Random.Range(0, phrases.Length)];
        if (glitchText != null) glitchText.text = current;
    }

    private void OnStay() => Load(stayScene);
    private void OnLeave() => Load(leaveScene);

    private void Load(string scene)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(scene);
    }
}
