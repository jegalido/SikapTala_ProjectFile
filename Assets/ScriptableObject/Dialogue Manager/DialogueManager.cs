using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager dialogueManagerInstance;

    [Header("Dialogue UI")]
    [SerializeField] private GameObject dialogueUI;
    [SerializeField] private Image speakerImage;
    [SerializeField] private TextMeshProUGUI speakerNameText;
    [SerializeField] private TextMeshProUGUI dialogueText;

    [Header("Animation Data")]
    [SerializeField] private SpeakerAnimationData[] animationDataList;

    [Header("Typing")]
    [SerializeField] private float typingSpeed = 0.03f;

    private DialogueObject currentDialogue;
    private int currentLineIndex;
    private bool isTyping;
    private Coroutine typingCoroutine;
    private Coroutine animCoroutine;

    // 

    private void Awake()
    {
        if (dialogueManagerInstance == null)
            dialogueManagerInstance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
    }

    // 

    public void StartDialogue(DialogueObject dialogue)
    {
        currentDialogue = dialogue;
        currentLineIndex = 0;
        dialogueUI.SetActive(true);
        ShowLine();
    }

    public void OnAdvanceInput()
    {
        // Do nothing if no dialogue is active
        if (currentDialogue == null) return;

        if (isTyping)
            FinishLineImmediately();
        else
            NextLine();
    }

    // 

    private void ShowLine()
    {
        DialogueLine line = currentDialogue.lines[currentLineIndex];

        speakerNameText.text = line.speakerName;

        SpeakerAnimationData data = FindAnimationData(line.speakerName);

        if (animCoroutine != null) StopCoroutine(animCoroutine);

        if (data != null)
        {
            SpeakerAnimationData.EmotionEntry entry = data.GetEntry(line.emotion);
            if (entry != null && entry.frames.Length > 0)
                animCoroutine = StartCoroutine(AnimateIcon(entry));
        }

        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeLine(line.dialogueText));
    }

    private SpeakerAnimationData FindAnimationData(string speakerName)
    {
        foreach (var data in animationDataList)
            if (data != null && data.speakerName == speakerName)
                return data;
        return null;
    }

    private void NextLine()
    {
        currentLineIndex++;

        if (currentLineIndex >= currentDialogue.lines.Length)
            EndDialogue();
        else
            ShowLine();
    }

    private void FinishLineImmediately()
    {
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        dialogueText.text = currentDialogue.lines[currentLineIndex].dialogueText;
        isTyping = false;
    }

    private void EndDialogue()
    {
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        if (animCoroutine != null) StopCoroutine(animCoroutine);

        dialogueUI.SetActive(false);
        currentDialogue = null;

        DialogueTrigger.OnDialogueEnded?.Invoke();
    }

    //

    private IEnumerator AnimateIcon(SpeakerAnimationData.EmotionEntry entry)
    {
        float delay = 1f / entry.frameRate;
        int frameIndex = 0;

        while (true)
        {
            speakerImage.sprite = entry.frames[frameIndex];
            frameIndex = (frameIndex + 1) % entry.frames.Length;
            yield return new WaitForSeconds(delay);
        }
    }

    private IEnumerator TypeLine(string line)
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char c in line)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }
}