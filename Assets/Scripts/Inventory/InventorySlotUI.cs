using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] Image icon;
    [SerializeField] TMP_Text amountText;

    [Header("Selection")]
    [SerializeField] Outline selectionOutline;

    int slotIndex;

    public int SlotIndex => slotIndex;

    public void Initialize(int index)
    {
        slotIndex = index;
    }

    public void Refresh(InventorySlot slot, bool selected = false)
    {
        bool hasItem = !slot.IsEmpty;

        //--------------------------------
        // ICON
        //--------------------------------

        if (hasItem)
        {
            icon.enabled = true;
            icon.sprite = slot.item.icon;

            amountText.text =
                slot.amount > 1
                ? slot.amount.ToString()
                : "";
        }
        else
        {
            icon.enabled = false;
            amountText.text = "";
        }

        //--------------------------------
        // SELECTION
        //--------------------------------

        if (selectionOutline != null)
        {
            selectionOutline.enabled = selected;
        }
    }
}