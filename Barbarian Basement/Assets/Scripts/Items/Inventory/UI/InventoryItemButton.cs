using UnityEngine;

public class InventoryItemButton : ItemButtonUI
{
    public override void OnClick()
    {
        inventoryUI.ShowItemDetails(item);
    }
}
