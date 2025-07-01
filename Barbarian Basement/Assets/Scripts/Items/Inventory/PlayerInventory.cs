using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : Inventory
{
    private List<Item> quickSlotItems = new List<Item>();

    
    public void AddItemToQuickSlot(Item item)
    {
        RemoveItem(item);
        quickSlotItems.Add(item);
    }

    public void RemoveFromQuickSlot(Item item, bool used)
    {
        if (!quickSlotItems.Contains(item)) return;

        quickSlotItems.Remove(item);

        if (!used)
        {
            AddItem(item);
        }
    }

    public void ClearQuickSlots()
    {
        if (quickSlotItems.Count == 0) return;

        foreach (var item in quickSlotItems)
        {
            AddItem(item);
        }

        quickSlotItems.Clear();
    }

    public override void AddItem(Item item)
    {
        // don't add to inventory if it's armour and we already have it
        if (item is Equipment && ValidateItemInInventory(item))
        {
            OnInventoryChangeRefused?.Invoke();
            return;
        }

        Items.Add(item);
        OnInventoryChanged?.Invoke();
    }

    public override void RemoveItem(Item item)
    {
        if (!ValidateItemInInventory(item)) return;

        Items.Remove(item);
        OnInventoryChanged?.Invoke();
    }
}
