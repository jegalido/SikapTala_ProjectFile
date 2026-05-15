using System;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [SerializeField] private DialogueObject dialogueObject;
    public static Action OnDialogueEnded;
    private bool hasTriggered = false;
    private PlayerController playerMovement;

    private void OnEnable()
    {
        OnDialogueEnded += HandleDialogueEnded;
    }

    private void OnDisable()
    {
        OnDialogueEnded -= HandleDialogueEnded;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasTriggered) return;
        if (!other.CompareTag("Player")) return;

        hasTriggered = true;

        playerMovement = other.GetComponent<PlayerController>();

        if (playerMovement == null)
        {
            Debug.LogWarning("DialogueTrigger: Could not find PlayerController on Player.");
            return;
        }

        playerMovement.inDialogue = true;
        DialogueManager.dialogueManagerInstance.StartDialogue(dialogueObject);
    }

    private void HandleDialogueEnded()
    {
        if (!hasTriggered) return;

        if (playerMovement != null)
            playerMovement.inDialogue = false;

        Destroy(gameObject);
    }
}