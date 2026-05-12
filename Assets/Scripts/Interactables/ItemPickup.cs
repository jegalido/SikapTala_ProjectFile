using UnityEngine;

public class ItemPickup : Interactable
{
    public ItemData itemData;
    public int amount = 1;

    private void Reset()
    {
        isTakeable = true;
        promptText = "Press F to take item";
    }

    public override void Interact()
    {
        bool added = InventorySystem.Instance.AddItem(itemData, amount);

        if (added)
        {
            Destroy(gameObject);
        }
    }
}