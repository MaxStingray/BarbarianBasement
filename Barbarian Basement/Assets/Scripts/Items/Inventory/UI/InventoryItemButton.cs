using UnityEngine;

public class InventoryItemButton : ItemButtonUI
{
    public override void OnClick()
    {
        if (item is Gold)
        {
            item.UseItem(GameManager.Instance.Player);
            inventoryUI.Inventory.RemoveItem(item);
            inventoryUI.UpdateUI();
            return;
        }

        inventoryUI.ShowItemDetails(item);
    }
}
