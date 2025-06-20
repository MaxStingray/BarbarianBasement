using UnityEngine.UI;
using UnityEngine;

public class QuickMenuItemButton : MonoBehaviour
{
    private Item _assignedItem;
    [SerializeField] private Image _iconImage;

    [SerializeField] private Button _button;

    public void Assign(Item item)
    {
        _assignedItem = item;
        _iconImage.sprite = item.icon;
        _button.onClick.AddListener(OnClick);
    }

    public void Unassign()
    {
        _iconImage.sprite = null;
        _button.onClick.RemoveAllListeners();
        _assignedItem = null;
    }

    private void OnClick()
    {
        _assignedItem.UseItem(GameManager.Instance.Player);

        if (_assignedItem is Consumable)
        {
            Unassign();
        }
    }
}
