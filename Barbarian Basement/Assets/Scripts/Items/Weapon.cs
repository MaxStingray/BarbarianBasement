using UnityEngine;

public class Weapon : Item
{
    public int attackDice;
    protected override void OnUse(CharacterSheet characterSheet)
    {
        Debug.Log($"used {itemName}");

        var targetTile = MoveUtils.GetTargetTile(characterSheet.CurrentTile, characterSheet.FacingDirection, GameManager.Instance.FinalGrid);

        if (targetTile.IsOccupied && targetTile.OccupiedByCharacter)
        {
            
        }
    }
}
