using UnityEngine;

public class MerchantInventory : Inventory
{
    public int minItems { get; set; } = 3;
    public int maxItems { get; set; } = 5;

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

        Items = GameManager.Instance.EquipmentListForFloor.GetRandomItems(minItems, maxItems);
    }
}
