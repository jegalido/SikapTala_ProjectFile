using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    [Header("Settings")]
    public string promptText =
        "Press F to inspect item";

    public bool pickupable;

    [Header("UI")]
    public GameObject inspectPanel;

    [Header("Pickup")]
    public ItemData inventoryItem;

    bool isOpen;
    bool hasBeenPickedUp;

    public void Interact()
    {
        if (!isOpen)
        {
            OpenPanel();
        }
        else
        {
            ClosePanel();
        }
    }

    void OpenPanel()
    {
        isOpen = true;

        if (inspectPanel != null)
            inspectPanel.SetActive(true);

        InteractableSystem.Instance
            .SetOpenedInteractable(this);
    }

    void ClosePanel()
    {
        isOpen = false;

        if (inspectPanel != null)
            inspectPanel.SetActive(false);

        if (pickupable && !hasBeenPickedUp)
        {
            Pickup();
        }

        InteractableSystem.Instance
            .ClearOpenedInteractable();
    }

    void Pickup()
    {
        hasBeenPickedUp = true;

        if (inventoryItem != null)
        {
            InventorySystem.Instance
                .AddItem(inventoryItem);
        }

        gameObject.SetActive(false);
    }

    public void OpenFromInventory()
    {
        OpenPanel();
    }

    public bool MatchesItem(
        ItemData item)
    {
        return inventoryItem == item;
    }

    //-----------------------------------
    // Trigger Detection
    //-----------------------------------

    void OnTriggerEnter2D(
        Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        Debug.Log(
            "Player entered " +
            name
        );

        InteractableSystem.Instance
            .SetNearbyInteractable(this);
    }

    void OnTriggerExit2D(
        Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        Debug.Log(
            "Player exited " +
            name
        );

        InteractableSystem.Instance
            .ClearNearbyInteractable(this);
    }
}