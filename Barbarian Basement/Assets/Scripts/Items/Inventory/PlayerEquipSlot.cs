
[System.Serializable]
public class PlayerEquipSlot
{
    public Equipment EquippedItem { get; set; }
    public bool IsOccuped => EquippedItem != null;

    public void EquipIntoSlot(Equipment item, CharacterSheet characterSheet)
    {
        if (IsOccuped) ClearSlot(characterSheet);
        characterSheet.Inventory.RemoveItem(item);
        item.UseItem(characterSheet);
        //add to global list
        characterSheet.EquipList.AllEquippedItems.Add(item);
        EquippedItem = item;
    }

    public void ClearSlot(CharacterSheet characterSheet)
    {
        if (!IsOccuped) return;
        // remove from global list
        if (characterSheet.EquipList.AllEquippedItems.Contains(EquippedItem)) characterSheet.EquipList.AllEquippedItems.Remove(EquippedItem);
        characterSheet.Inventory.AddItem(EquippedItem);
        EquippedItem.OnUnequip(characterSheet);
        EquippedItem = null;
    }
}
