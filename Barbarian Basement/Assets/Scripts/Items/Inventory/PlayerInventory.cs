using UnityEngine;

public class PlayerInventory : Inventory
{
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
