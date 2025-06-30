using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public enum EquipSlot
{
    Head,
    Body,
    Accessory
}

public class EquipList : MonoBehaviour
{
    public PlayerEquipSlot HeadSlot = new PlayerEquipSlot();
    public PlayerEquipSlot BodySlot = new PlayerEquipSlot();
    public List<PlayerEquipSlot> AccessorySlots = new List<PlayerEquipSlot>();

    public List<Equipment> AllEquippedItems = new List<Equipment>();

    public UnityEvent OnEquipmentChanged = new UnityEvent();

    public void EquipItem(Equipment item, CharacterSheet character)
    {
        if (AllEquippedItems.Contains(item))
        {
            Debug.Log($"{item.itemName} is already equipped");
            return;
        }
        
        switch (item.EquipSlot)
        {
            case EquipSlot.Head:
                HeadSlot.EquipIntoSlot(item, character);
                break;
            case EquipSlot.Body:
                BodySlot.EquipIntoSlot(item, character);
                break;
            case EquipSlot.Accessory:
                var slot = CreateAccessorySlot(item, character);
                AccessorySlots.Add(slot);
                break;
        }
        Debug.Log("Total equipped: " + character.EquipList.AllEquippedItems.Count);
        OnEquipmentChanged.Invoke();
    }

    private PlayerEquipSlot CreateAccessorySlot(Equipment item, CharacterSheet character)
    {
        var slot = new PlayerEquipSlot();
        slot.EquipIntoSlot(item, character);
        return slot;
    }

    public void UnequipItem(Equipment item, CharacterSheet character)
    {
        if (HeadSlot.EquippedItem == item)
            HeadSlot.ClearSlot(character);
        else if (BodySlot.EquippedItem == item)
            BodySlot.ClearSlot(character);
        else
        {
            var slot = AccessorySlots.Find(s => s.EquippedItem == item);
            if (slot != null)
            {
                slot.ClearSlot(character);
                AccessorySlots.Remove(slot);
            }
        }

        OnEquipmentChanged.Invoke();
    }
}
