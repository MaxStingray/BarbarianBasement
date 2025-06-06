using UnityEngine;

/// <summary>
/// Utilities class for handling basic combat
/// </summary>
public static class CombatUtils
{
    public static void Attack(CharacterSheet attacker, CharacterSheet target)
    {
        int hits = 0;
        for (int i = 0; i < attacker.AttackDice; i++)
        {
            if (RollAttackDie())
            {
                hits++;
            }
        }

        int blocks = 0;
        for (int i = 0; i < target.DefendDice; i++)
        {
            if (RollDefenceDie(target is Enemy))
            {
                blocks++;
            }
        }

        int actualHits = hits - blocks;
        Debug.Log($"{attacker.CharacterName}: {hits} hits");
        Debug.Log($"{target.CharacterName}: {blocks} blocks");
        Debug.Log($"{target.CharacterName} takes {Mathf.Max(0, actualHits)} damage!");

        if (actualHits > 0)
        {
            target.TakeHits(actualHits);
        }
    }

    /// <summary>
    /// roll one attack die (3 sides of the attack die are skulls, therefore we always have a 50% chance of hitting)
    /// </summary>
    /// <returns> true if hit</returns>
    public static bool RollAttackDie()
    {
        var hitNumber = Random.Range(0, 100);

        if (hitNumber < 50)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// roll one Defence die
    /// enemies have a one in 6 chance to block
    /// heroes have 2 in 6
    /// </summary>
    /// <param name="isEnemy"></param>
    /// <returns>true if blocked</returns>
    public static bool RollDefenceDie(bool isEnemy)
    {
        var blockNumber = Random.Range(0, 100);

        if (isEnemy)
        {
            //round it to 17
            if (blockNumber < 17)
            {
                return true;
            }
        }
        else
        {
            //rounded down
            if (blockNumber < 33)
            {
                return true;
            }
        }

        return false;
    }
}
