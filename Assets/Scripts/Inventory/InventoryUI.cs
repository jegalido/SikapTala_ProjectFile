using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] Transform hotbarPanel;
    [SerializeField] Transform backpackPanel;

    InventorySlotUI[] hotbarUI;
    InventorySlotUI[] backpackUI;

    void Start()
    {
        CacheSlots();

        InventorySystem.Instance.OnInventoryChanged += RefreshUI;
        InventorySystem.Instance.OnHotbarSelectionChanged += RefreshHotbarSelection;

        RefreshUI();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            backpackPanel.gameObject.SetActive(
                !backpackPanel.gameObject.activeSelf
            );
        }
    }

    void CacheSlots()
    {
        hotbarUI =
            hotbarPanel.GetComponentsInChildren<InventorySlotUI>();

        backpackUI =
            backpackPanel.GetComponentsInChildren<InventorySlotUI>();

        for (int i = 0; i < hotbarUI.Length; i++)
            hotbarUI[i].Initialize(i);

        for (int i = 0; i < backpackUI.Length; i++)
            backpackUI[i].Initialize(i);
    }

    public void RefreshUI()
    {
        var inventory = InventorySystem.Instance;

        for (int i = 0; i < hotbarUI.Length; i++)
        {
            bool selected =
                i == inventory.selectedHotbarIndex;

            hotbarUI[i].Refresh(
                inventory.hotbarSlots[i],
                selected
            );
        }

        for (int i = 0; i < backpackUI.Length; i++)
        {
            backpackUI[i].Refresh(
                inventory.backpackSlots[i]
            );
        }
    }

    void RefreshHotbarSelection(int index)
    {
        RefreshUI();
    }
}