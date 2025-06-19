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
    [SerializeField] private StatsToModify[] _statsToModify;
    [SerializeField] private statModifier[] _statModifiers;

    [SerializeField] private Item[] _conflictingItems;

    private int baseAttack;
    private int baseDefend;

    private bool _attackWasModified;
    private bool _defendWasModified;

    protected override void OnUse(CharacterSheet characterSheet)
    {
        baseAttack = characterSheet.AttackDice;
        baseDefend = characterSheet.DefendDice;

        foreach (var modifier in _statModifiers)
        {
            switch (modifier.stat)
            {
                case StatsToModify.AttackDice:
                    characterSheet.CurrentAttackDice = ApplyModifier(baseAttack, modifier);
                    _attackWasModified = true;
                    break;
                case StatsToModify.DefendDice:
                    characterSheet.CurrentDefendDice = ApplyModifier(baseDefend, modifier);
                    _defendWasModified = true;
                    break;
            }
        }

        GameManager.Instance.StatsPanel.UpdateStatsPanel(GameManager.Instance.Player);
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
        if (_attackWasModified)
        {
            characterSheet.CurrentAttackDice = baseAttack;
        }

        if (_defendWasModified)
        {
            characterSheet.CurrentDefendDice = baseDefend;
        }
    }
}
