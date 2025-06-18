using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class Inventory : MonoBehaviour
{
    public List<Item> Items = new List<Item>();

    public UnityEvent OnInventoryChanged = new UnityEvent();

    public UnityEvent OnInventoryChangeRefused = new UnityEvent();

    /// <summary>
    /// How to handle an item being added
    /// </summary>
    /// <param name="item"></param>
    public abstract void AddItem(Item item);

    /// <summary>
    /// How to handle an item being removed
    /// </summary>
    /// <param name="item"></param>
    public abstract void RemoveItem(Item item);

    /// <summary>
    /// check if this inventory already contains this item
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public virtual bool ValidateItemInInventory(Item item)
    {
        return Items.Contains(item);
    }
}
