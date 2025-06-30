using System;
using System.Collections.Generic;
using UnityEngine;

public static class EquipUtils
{
    public static void CalculateStats(CharacterSheet character)
    {
        if (character.EquipList.AllEquippedItems.Count == 0) return;

        var itemList = character.EquipList.AllEquippedItems;

        //first, apply the highest overwrite value currently equipped
        var finalDefend = HighestOverride(itemList, StatsToModify.DefendDice);
        var finalAttack = HighestOverride(itemList, StatsToModify.AttackDice);
        foreach (var item in itemList)
        {
            foreach (var modifier in item.StatModifiers)
            {
                if (modifier.modificationType == StatModificationType.Add)
                {
                    switch (modifier.stat)
                    {
                        case StatsToModify.AttackDice:
                            finalAttack = ApplyModifier(finalAttack, modifier);
                            break;
                        case StatsToModify.DefendDice:
                            finalDefend = ApplyModifier(finalDefend, modifier);
                            break;
                    }
                }
            }
        }

        character.CurrentAttackDice = finalAttack;
        character.CurrentDefendDice = finalDefend;
    }

    /// <summary>
    /// return the highest override value in the equipment list
    /// </summary>
    /// <param name="equipList"></param>
    /// <param name="stat"></param>
    /// <returns></returns>
    private static int HighestOverride(List<Equipment> equipList, StatsToModify stat)
    {
        var highest = 0;

        foreach (var item in equipList)
        {
            foreach (var modifier in item.StatModifiers)
            {
                if (modifier.stat == stat)
                {
                    if (modifier.value >= highest)
                    {
                        highest = modifier.value;
                    }
                }
                else
                {
                    continue;
                }
            }
        }

        return highest;
    }

    /// <summary>
    /// applies an additive modifier
    /// </summary>
    /// <param name="current"></param>
    /// <param name="modifier"></param>
    /// <returns></returns>
    private static int ApplyModifier(int current, statModifier modifier)
    {
        return current + modifier.value;
    }
}
