using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Small HUD helper above the hotbar: shows the selected item's name/count and a
/// Use button. Keeps the button enabled only when the selected item is usable, and
/// mirrors the keyboard (Q) / gamepad (Triangle/Y) use action for mouse users.
/// </summary>
public class InventoryHud : MonoBehaviour
{
    [SerializeField] private TMP_Text selectedNameLabel;
    [SerializeField] private Button useButton;
    [SerializeField] private TMP_Text useButtonLabel;
    [SerializeField] private string useHint = "USE   [ Q / Y ]";

    private void OnEnable()
    {
        if (InventorySystem.Instance != null)
        {
            InventorySystem.Instance.OnInventoryChanged += Refresh;
            InventorySystem.Instance.OnHotbarSelectionChanged += OnSelChanged;
            InventorySystem.Instance.OnItemUsed += OnUsed;
        }
        if (useButton != null) useButton.onClick.AddListener(OnUseClicked);
        if (useButtonLabel != null) useButtonLabel.text = useHint;
        Refresh();
    }

    private void OnDisable()
    {
        if (InventorySystem.Instance != null)
        {
            InventorySystem.Instance.OnInventoryChanged -= Refresh;
            InventorySystem.Instance.OnHotbarSelectionChanged -= OnSelChanged;
            InventorySystem.Instance.OnItemUsed -= OnUsed;
        }
        if (useButton != null) useButton.onClick.RemoveListener(OnUseClicked);
    }

    private void OnSelChanged(int i) => Refresh();
    private void OnUsed(ItemData i) => Refresh();
    private void OnUseClicked()
    {
        if (InventorySystem.Instance != null) InventorySystem.Instance.UseSelectedItem();
    }

    private void Refresh()
    {
        InventorySystem inv = InventorySystem.Instance;
        if (inv == null || inv.hotbarSlots == null || inv.hotbarSlots.Count == 0) return;

        InventorySlot slot = inv.hotbarSlots[inv.selectedHotbarIndex];
        bool hasItem = slot != null && !slot.IsEmpty;
        bool usable = hasItem && slot.item.consumable && slot.amount > 0;

        if (selectedNameLabel != null)
            selectedNameLabel.text = hasItem
                ? slot.item.itemName + (slot.amount > 1 ? ("  x" + slot.amount) : "")
                : "";

        if (useButton != null) useButton.interactable = usable;
    }
}
