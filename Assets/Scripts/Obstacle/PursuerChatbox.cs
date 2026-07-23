using UnityEngine;
using TMPro;

/// <summary>
/// A distorted, looping "typing" chatbox above the pursuer. It types out a line,
/// holds, clears, and moves to the next (random or in order). Only appears in the
/// dark (real) world. Edit / add lines freely in the inspector.
/// </summary>
public class PursuerChatbox : MonoBehaviour
{
    [Header("Lines (edit / add freely)")]
    [TextArea]
    public string[] phrases = new string[]
    {
        "you don't have to go back",
        "nobody's waiting for you there",
        "you've always been tired",
        "stay a little longer",
        "it's warm here",
        "rest now... just rest",
        "why do you keep running",
        "there's nothing out there for you",
    };
    [Tooltip("Pick lines randomly; otherwise cycle in order.")]
    public bool randomOrder = true;

    [Header("Typing")]
    public float charDelay = 0.06f;
    public float holdAfterComplete = 1.4f;
    public float clearPause = 0.5f;

    [Header("Distortion")]
    public float positionJitter = 0.02f;
    [Range(0f, 1f)] public float glitchAmount = 0.10f;
    public string glitchChars = "#%&@?/\\*<>_";

    [Header("References (auto-found if empty)")]
    public TMP_Text label;
    public CanvasGroup group;
    [Tooltip("Only show / type while shifted into the dark world.")]
    public InsanityVisionEffect vision;

    private enum Phase { Typing, Holding, Clearing }
    private Phase phase = Phase.Typing;
    private int index = -1;
    private string current = "";
    private int typed;
    private float timer;
    private Vector3 baseLocalPos;
    private float seed;

    private void Awake()
    {
        if (vision == null) vision = FindFirstObjectByType<InsanityVisionEffect>();
        if (label == null) label = GetComponentInChildren<TMP_Text>(true);
        if (group == null) group = GetComponent<CanvasGroup>();
        if (label != null) baseLocalPos = label.transform.localPosition;
        seed = Random.value * 100f;
        NextPhrase();
    }

    private void Update()
    {
        bool dark = vision == null || vision.ShiftBlend > 0.5f;

        if (group != null)
            group.alpha = Mathf.MoveTowards(group.alpha, dark ? 1f : 0f, Time.deltaTime * 6f);

        if (!dark) return; // pause typing while in the warm world

        timer += Time.deltaTime;
        switch (phase)
        {
            case Phase.Typing:
                if (timer >= charDelay)
                {
                    timer = 0f;
                    typed++;
                    if (typed >= current.Length) { typed = current.Length; phase = Phase.Holding; }
                }
                break;
            case Phase.Holding:
                if (timer >= holdAfterComplete) { phase = Phase.Clearing; timer = 0f; }
                break;
            case Phase.Clearing:
                if (timer >= clearPause) NextPhrase();
                break;
        }

        if (label != null)
        {
            Vector3 j = new Vector3(
                Mathf.PerlinNoise(Time.time * 17f, seed) - 0.5f,
                Mathf.PerlinNoise(seed, Time.time * 17f) - 0.5f, 0f) * positionJitter;
            label.transform.localPosition = baseLocalPos + j;
        }

        RenderText();
    }

    private void NextPhrase()
    {
        if (phrases == null || phrases.Length == 0) { current = ""; }
        else if (randomOrder) { index = Random.Range(0, phrases.Length); current = phrases[index]; }
        else { index = (index + 1) % phrases.Length; current = phrases[index]; }
        typed = 0;
        timer = 0f;
        phase = Phase.Typing;
        RenderText();
    }

    private void RenderText()
    {
        if (label == null) return;
        int len = Mathf.Clamp(typed, 0, current.Length);
        char[] arr = current.Substring(0, len).ToCharArray();
        if (glitchAmount > 0f && glitchChars.Length > 0)
        {
            for (int i = 0; i < arr.Length; i++)
                if (arr[i] != ' ' && Random.value < glitchAmount * 0.15f)
                    arr[i] = glitchChars[Random.Range(0, glitchChars.Length)];
        }
        label.text = new string(arr);
    }
}
