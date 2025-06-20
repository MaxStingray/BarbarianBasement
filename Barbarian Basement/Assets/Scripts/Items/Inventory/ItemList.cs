using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// represents a collection of items we can pull from to populate containers
/// use different item lists for different dungeon floors
/// </summary>
[CreateAssetMenu(menuName = "Inventory/Item List")]
public class ItemList : ScriptableObject
{
    public List<Item> availableItems;

    public List<Item> GetRandomItems(int min, int max)
    {
        int itemCount = Random.Range(min, max + 1);
        List<Item> result = new List<Item>();
        List<Item> poolCopy = new List<Item>(availableItems);

        for (int i = 0; i < itemCount && poolCopy.Count > 0; i++)
        {
            int index = Random.Range(0, poolCopy.Count);

            result.Add(poolCopy[index]);
            poolCopy.RemoveAt(index);
        }

        return result;
    }
}
