using UnityEngine;

[System.Serializable]
public enum StatsToModify
{
    AttackDice,
    DefendDice,
}

[CreateAssetMenu(fileName = "Equipment", menuName = "Inventory/Equipment")]
public class Equipment : Item
{
    [SerializeField] private StatsToModify[] statsToModify;

    [SerializeField] private int statModifier;

    [SerializeField] private Item[] _conflictingItems;

    private int baseAttack;
    private int baseDefend;

    private bool _attackWasModified;
    private bool _defendWasModified;

    protected override void OnUse(CharacterSheet characterSheet)
    {
        //TODO: Check character inventory for conflicting items
        baseAttack = characterSheet.AttackDice;
        baseDefend = characterSheet.DefendDice;
        foreach (var stat in statsToModify)
        {
            switch (stat)
            {
                case StatsToModify.AttackDice:
                    characterSheet.CurrentAttackDice = statModifier;
                    _attackWasModified = true;
                    break;
                case StatsToModify.DefendDice:
                    characterSheet.CurrentDefendDice = statModifier;
                    _defendWasModified = true;
                    break;
            }
        }
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
