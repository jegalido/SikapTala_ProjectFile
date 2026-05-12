using TMPro;
using UnityEngine;

public class InteractableSystem : MonoBehaviour
{
    public static InteractableSystem Instance;

    [Header("UI")]
    [SerializeField] GameObject promptUI;
    [SerializeField] TMP_Text promptText;

    InteractableObject currentTarget;
    InteractableObject openedInteractable;

    void Awake()
    {
        Instance = this;
        Debug.Log("InteractableSystem Awake");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            Debug.Log("Pressed F");
            HandleF();
        }

        if (Input.GetMouseButtonDown(0))
        {
            HandleHotbarUse();
        }
    }

    void HandleF()
    {
        if (openedInteractable != null)
        {
            openedInteractable.Interact();
            return;
        }

        if (currentTarget != null)
        {
            currentTarget.Interact();
        }
    }

    void HandleHotbarUse()
    {
        if (openedInteractable != null)
            return;

        ItemData equippedItem =
            InventorySystem.Instance.GetEquippedItem();

        if (equippedItem == null)
            return;

        InteractableObject[] all =
            FindObjectsByType<InteractableObject>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

        foreach (var interactable in all)
        {
            if (interactable.MatchesItem(equippedItem))
            {
                interactable.OpenFromInventory();
                break;
            }
        }
    }

    public void SetNearbyInteractable(
        InteractableObject interactable)
    {
        currentTarget = interactable;

        promptUI.SetActive(true);

        promptText.text =
            interactable.promptText;
    }

    public void ClearNearbyInteractable(
        InteractableObject interactable)
    {
        if (currentTarget != interactable)
            return;

        currentTarget = null;

        if (openedInteractable == null)
        {
            promptUI.SetActive(false);
        }

        Debug.Log(
            "Exited interactable: " +
            interactable.name
        );
    }

    public void SetOpenedInteractable(
        InteractableObject interactable)
    {
        openedInteractable = interactable;

        promptUI.SetActive(true);

        promptText.text =
            "Press F to close";
    }

    public void ClearOpenedInteractable()
    {
        openedInteractable = null;

        promptUI.SetActive(false);
    }
}