using System.Collections;
using UnityEngine;

[System.Serializable]
public enum WeaponType
{
    Melee,
    Ranged,
    Throwable,
}

[CreateAssetMenu(fileName = "Equipment", menuName = "Inventory/Weapon")]
public class Weapon : Item
{
    public int AttackDice;
    public int Range;

    public WeaponType WeaponType;

    public Item[] ConflictingItems;
    protected override void OnUse(CharacterSheet characterSheet)
    {
        if (characterSheet is not Player player) return;

        Debug.Log($"attacked with {itemName}!");

        switch (WeaponType)
        {
            case WeaponType.Melee:
                if (ValidTargetForMelee(characterSheet, player, out GameTile targetTile))
                {
                    GameManager.Instance.StartCoroutine(AttackRoutine(player, targetTile.OccupiedByCharacter));
                }
                break;
            case WeaponType.Ranged:
                GameManager.Instance.StartCoroutine(TryRangedAttack(player));
                break;
            case WeaponType.Throwable:
                //if the target is close, just attack as normal
                if (ValidTargetForMelee(characterSheet, player, out GameTile throwableTargetTile))
                {
                    GameManager.Instance.StartCoroutine(AttackRoutine(player, throwableTargetTile.OccupiedByCharacter));
                }
                else
                {
                    GameManager.Instance.StartCoroutine(TryRangedAttack(player));
                }
                break;
        }

    }

    private bool ValidTargetForMelee(CharacterSheet characterSheet, Player player, out GameTile targetTile)
    {
        targetTile = MoveUtils.GetTargetTile(characterSheet.CurrentTile, characterSheet.FacingDirection, GameManager.Instance.FinalGrid);

        return targetTile != null && targetTile.IsOccupied && targetTile.OccupiedByCharacter;
    }

    private IEnumerator TryRangedAttack(Player player)
    {
        if (CombatUtils.ValidTargetForRanged(player, Range ,GameManager.Instance.FinalGrid, out GameTile target))
        {
            CombatUtils.Attack(AttackDice, player, target.OccupiedByCharacter, out bool hit);
            if (hit)
            {
                //sfx, sounds etc here
            }
            yield return null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private IEnumerator AttackRoutine(Player player, CharacterSheet target)
    {
        CombatUtils.Attack(AttackDice, player, target, out bool hit);
        yield return GameManager.Instance.StartCoroutine(CombatUtils.PlayCharacterAttackEffects(player, hit));
        GameManager.Instance.PlayerManager.SetUsedAction();
    }
}
