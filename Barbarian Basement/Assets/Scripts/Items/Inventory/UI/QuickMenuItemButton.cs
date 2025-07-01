using UnityEngine.UI;
using UnityEngine;

public class QuickMenuItemButton : MonoBehaviour
{
    private Item _assignedItem;
    [SerializeField] private Image _iconImage;
    [SerializeField] private Sprite _defaultSprite;

    [SerializeField] private Button _button;

    private PlayerInventory _inventory;

    public void Assign(Item item, PlayerInventory inventory)
    {
        _assignedItem = item;
        _iconImage.sprite = item.icon;
        _button.onClick.AddListener(OnClick);
        _inventory = inventory;
    }

    public void Unassign(bool used)
    {
        _iconImage.sprite = _defaultSprite;
        _button.onClick.RemoveAllListeners();
        _inventory.RemoveFromQuickSlot(_assignedItem, used);
        _assignedItem = null;
    }

    private void OnClick()
    {
        _assignedItem.UseItem(GameManager.Instance.Player);

        if (_assignedItem is Consumable)
        {
            Unassign(true);
        }

        if (_assignedItem is Weapon weapon && weapon.WeaponType == WeaponType.Throwable)
        {
            Unassign(true);
        }
    }
}
