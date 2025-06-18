using UnityEngine.UI;
using UnityEngine;

public abstract class ItemButtonUI : MonoBehaviour
{
    [SerializeField] protected Image iconImage;
    protected Item item;
    //the UI that is displaying this button (player, container, etc)
    protected InventoryUI inventoryUI;

    public virtual void Setup(Item newItem, InventoryUI ui)
    {
        item = newItem;
        iconImage.sprite = item.icon;
    }

    public abstract void OnClick();
}
