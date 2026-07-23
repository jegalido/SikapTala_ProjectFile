using UnityEngine;

[CreateAssetMenu(menuName = "Game/Item")]
public class ItemData : ScriptableObject
{
    public string itemID;
    public string itemName;
    public Sprite icon;

    public bool stackable = true;
    public int maxStack = 99;

    [Header("Use")]
    [Tooltip("Can this item be used from the hotbar?")]
    public bool consumable = false;
    [Tooltip("Sanity restored when used (for the inhaler). 0 = none.")]
    public float sanityRestore = 0f;
    [TextArea] public string description;
}