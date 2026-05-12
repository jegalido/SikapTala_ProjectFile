using System;
using System.Collections.Generic;
using UnityEngine;

public class InventorySystem : MonoBehaviour
{
    public static InventorySystem Instance;

    public int hotbarSize = 6;
    public int backpackSize = 20;

    public List<InventorySlot> hotbarSlots = new();
    public List<InventorySlot> backpackSlots = new();

    public int selectedHotbarIndex = 0;

    public Action OnInventoryChanged;
    public Action<int> OnHotbarSelectionChanged;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        Initialize();
    }

    void Initialize()
    {
        for (int i = 0; i < hotbarSize; i++)
            hotbarSlots.Add(new InventorySlot());

        for (int i = 0; i < backpackSize; i++)
            backpackSlots.Add(new InventorySlot());
    }

    void Update()
    {
        HandleHotbarInput();
    }

    void HandleHotbarInput()
    {
        for (int i = 0; i < hotbarSlots.Count; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                SelectHotbarSlot(i);
            }
        }
    }

    public void SelectHotbarSlot(int index)
    {
        if (index < 0 || index >= hotbarSlots.Count)
            return;

        selectedHotbarIndex = index;

        OnHotbarSelectionChanged?.Invoke(index);
    }

    public bool AddItem(ItemData item, int amount = 1)
    {
        if (TryStackItem(hotbarSlots, item, amount)) return true;
        if (TryStackItem(backpackSlots, item, amount)) return true;

        if (TryPlaceInEmptySlot(hotbarSlots, item, amount)) return true;
        if (TryPlaceInEmptySlot(backpackSlots, item, amount)) return true;

        return false;
    }

    bool TryStackItem(List<InventorySlot> slots, ItemData item, int amount)
    {
        if (!item.stackable)
            return false;

        foreach (var slot in slots)
        {
            if (slot.item == item && slot.amount < item.maxStack)
            {
                slot.amount += amount;

                OnInventoryChanged?.Invoke();

                return true;
            }
        }

        return false;
    }

    bool TryPlaceInEmptySlot(List<InventorySlot> slots, ItemData item, int amount)
    {
        foreach (var slot in slots)
        {
            if (slot.IsEmpty)
            {
                slot.Set(item, amount);

                OnInventoryChanged?.Invoke();

                return true;
            }
        }

        return false;
    }

    public bool HasItem(string itemID)
    {
        return FindItem(itemID) != null;
    }

    public InventorySlot FindItem(string itemID)
    {
        foreach (var slot in hotbarSlots)
        {
            if (!slot.IsEmpty && slot.item.itemID == itemID)
                return slot;
        }

        foreach (var slot in backpackSlots)
        {
            if (!slot.IsEmpty && slot.item.itemID == itemID)
                return slot;
        }

        return null;
    }

    public bool RemoveItem(string itemID, int amount = 1)
    {
        var slot = FindItem(itemID);

        if (slot == null)
            return false;

        slot.amount -= amount;

        if (slot.amount <= 0)
            slot.Clear();

        OnInventoryChanged?.Invoke();

        return true;
    }

    public ItemData GetEquippedItem()
    {
        return hotbarSlots[selectedHotbarIndex].item;
    }
}