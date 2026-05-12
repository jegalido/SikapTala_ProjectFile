using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager dialogueManagerInstance;

    [Header("Dialogue UI")]
    [SerializeField] private GameObject dialogueUI;
    [SerializeField] private TextMeshProUGUI speakerNameText;
    [SerializeField] private TextMeshProUGUI dialogueText;

    [Header("Speaker Icons")]
    [SerializeField] private GameObject motherIcon;
    [SerializeField] private GameObject playerIcon;

    [Header("Typing")]
    [SerializeField] private float typingSpeed = 0.03f;

    private Animator motherAnimator;
    private Animator playerAnimator;
    private DialogueObject currentDialogue;
    private int currentLineIndex;
    private bool isTyping;
    private Coroutine typingCoroutine;

  

    private void Awake()
    {
        if (dialogueManagerInstance == null)
            dialogueManagerInstance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        // Auto-grab Animators from the icon GameObjects
        motherAnimator = motherIcon.GetComponent<Animator>();
        playerAnimator = playerIcon.GetComponent<Animator>();

        if (motherAnimator == null)
            Debug.LogWarning("DialogueManager: No Animator found on MotherIcon.");
        if (playerAnimator == null)
            Debug.LogWarning("DialogueManager: No Animator found on PlayerIcon.");
    }



    public void StartDialogue(DialogueObject dialogue)
    {
        currentDialogue = dialogue;
        currentLineIndex = 0;
        dialogueUI.SetActive(true);
        ShowLine();
    }

    public void OnAdvanceInput()
    {
        if (currentDialogue == null) return;

        if (isTyping)
            FinishLineImmediately();
        else
            NextLine();
    }


    private void ShowLine()
    {
        DialogueLine line = currentDialogue.lines[currentLineIndex];

        speakerNameText.text = line.speakerName;

        // Hide all first
        motherIcon.SetActive(false);
        playerIcon.SetActive(false);

        // Show and animate the correct one
        switch (line.speakerName)
        {
            case "Mother":
                motherIcon.SetActive(true);
                motherAnimator.SetInteger("emotion", (int)line.emotion);
                break;
            case "Player":
                playerIcon.SetActive(true);
                playerAnimator.SetInteger("emotion", (int)line.emotion);
                break;
        }

        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeLine(line.dialogueText));
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

        motherIcon.SetActive(false);
        playerIcon.SetActive(false);

        dialogueUI.SetActive(false);
        currentDialogue = null;

        DialogueTrigger.OnDialogueEnded?.Invoke();
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