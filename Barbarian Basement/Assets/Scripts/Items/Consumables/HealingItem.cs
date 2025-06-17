using UnityEngine;

[CreateAssetMenu(fileName = "HealingItem", menuName = "Inventory/Treasure/HealingItem")]
public class HealingItem : Consumable
{
    [SerializeField] private int numberOfHealingDice;
    protected override void OnUse(CharacterSheet characterSheet)
    {
        base.OnUse(characterSheet);
        int healAmount = 0;

        for (int i = 1; i < numberOfHealingDice; i++)
        {
            healAmount += CombatUtils.RollMovementDie();
        }

        characterSheet.CurrentBodyPoints = Mathf.Max(characterSheet.CurrentBodyPoints + healAmount, characterSheet.BodyPoints);
    }
}
