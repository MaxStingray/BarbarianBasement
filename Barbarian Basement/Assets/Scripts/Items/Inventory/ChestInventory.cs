using UnityEngine;

public class ChestInventory : Inventory
{
    public int minItems { get; set; } = 1;
    public int maxItems { get; set; } = 3;

    private bool _isPopulated = false;

    public override void AddItem(Item item)
    {
        Items.Add(item);
        OnInventoryChanged.Invoke();
    }

    public override void RemoveItem(Item item)
    {
        if (!ValidateItemInInventory(item)) return;
        Items.Remove(item);
        OnInventoryChanged.Invoke();
    }

    public void GetRandomContents()
    {
        if (_isPopulated) return;

        Items = GameManager.Instance.ItemListForFloor.GetRandomItems(minItems, maxItems);
    }
}
