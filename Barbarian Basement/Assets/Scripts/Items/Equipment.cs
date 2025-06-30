using UnityEngine;

/// <summary>
/// Does this equipment override the player's stat or add to it?
/// </summary>
[System.Serializable]
public enum StatModificationType
{
    Add,
    Override,
}
[System.Serializable]
public enum StatsToModify
{
    AttackDice,
    DefendDice,
}

[System.Serializable]
public struct statModifier
{
    //the stat to modify
    public StatsToModify stat;
    //are we adding to the stat or overriding it?
    public StatModificationType modificationType;
    //how much to modify by
    public int value;
}

[CreateAssetMenu(fileName = "Equipment", menuName = "Inventory/Equipment")]
public class Equipment : Item
{
    [SerializeField] private EquipSlot _equipSlot;
    public EquipSlot EquipSlot => _equipSlot;
    [SerializeField] private statModifier[] _statModifiers;
    public statModifier[] StatModifiers => _statModifiers;

    [SerializeField] private Item[] _conflictingItems;

    protected override void OnUse(CharacterSheet characterSheet)
    {
        Debug.Log($"Equipped {itemName}");
    }

    public int ApplyModifier(int baseValue, statModifier modifier)
    {
        return modifier.modificationType switch
        {
            StatModificationType.Add => baseValue + modifier.value,
            StatModificationType.Override => modifier.value,
            _ => baseValue
        };
    }

    public void OnUnequip(CharacterSheet characterSheet)
    {
        Debug.Log($"Unequipped {itemName}");
    }
}
