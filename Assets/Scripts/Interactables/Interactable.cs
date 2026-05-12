using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    [Header("Interaction")]
    public string promptText = "Press F to interact";

    public bool isTakeable = false;

    public abstract void Interact();
}