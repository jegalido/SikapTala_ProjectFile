using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager dialogueManagerInstance;

    [Header("Dialogue Settings")]
    [SerializeField] private GameObject dialogueUI;
    [SerializeField] private Image SpeakerImage;
    [SerializeField] private TextMeshProUGUI SpeakerNameText;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private float typingSpeed = 0.03f;

    private DialogueObject currentDialogue;
    private int currentLineIndex;
    private bool isTyping;
    private  Coroutine typingCoroutine;



    private void Awake()
    {
        if (dialogueManagerInstance == null)
            dialogueManagerInstance = this;
           
        
        else
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);

    }

     
    public void StartDialogue(DialogueObject dialogue)
    {
        currentDialogue = dialogue;
        currentLineIndex = 0;
        dialogueUI.SetActive(true);
        ShowLine();
    }

    private  void ShowLine() { 
        
        DialogueLine line = currentDialogue.lines[currentLineIndex];
        SpeakerNameText.text = line.speakerName;
        SpeakerImage.sprite = line.speakerIcon;
        typingCoroutine = StartCoroutine(TypeLine(line.dialogueText));
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeLine(line.dialogueText));


    }

    private void NextLine()
    {
        currentLineIndex++;
        if(currentLineIndex> currentDialogue.lines.Length)
        {
            EndDialogue();
        }
        else
        {
            ShowLine();
        }
    }


    private void EndDialogue()
    {
        dialogueUI.SetActive(false);
        currentDialogue = null;
    }

    private IEnumerator TypeLine(string line)
    {
        isTyping = true;
        dialogueText.text = "";
        foreach (char c in line.ToCharArray())
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
        isTyping = false;
    }

}
