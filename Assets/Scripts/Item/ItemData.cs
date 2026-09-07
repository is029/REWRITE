using UnityEngine;

public enum ItemType
{
    Heal,
    Damage
}

[CreateAssetMenu(fileName = "NewItem", menuName = "REWRITE/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("Šî–{î•ñ")]
    public string itemName;

    [TextArea(2, 4)]
    public string description;

    public Sprite icon;

    [Header("Œø‰Ê")]
    public ItemType itemType;

    public int value;
}