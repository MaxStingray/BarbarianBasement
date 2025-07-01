using UnityEngine;
public abstract class Consumable : Item
{
    protected override void OnUse(CharacterSheet characterSheet)
    {
        Debug.Log($"used {itemName}");

        if (characterSheet is Player player)
        {
            GameManager.Instance.StatsPanel.UpdateStatsPanel(player);
        }
    }
}
