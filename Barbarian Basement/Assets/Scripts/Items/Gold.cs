using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "Inventory/Gold")]
public class Gold : Item
{
    [SerializeField] private int amount;
    protected override void OnUse(CharacterSheet characterSheet)
    {
        characterSheet.Inventory.AddGold(amount);
    }
}
