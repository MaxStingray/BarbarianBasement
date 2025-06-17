using UnityEngine;
using UnityEngine.Events;

//[CreateAssetMenu(fileName = "Item", menuName = "Inventory/Item")]
public abstract class Item : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public string description;

    public virtual void UseItem(CharacterSheet characterSheet)
    {
        OnUse(characterSheet);
    }

    protected abstract void OnUse(CharacterSheet characterSheet);
}
