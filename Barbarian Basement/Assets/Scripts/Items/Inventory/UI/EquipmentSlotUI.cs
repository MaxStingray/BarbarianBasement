using UnityEngine;
using UnityEngine.UI;

public class EquipmentSlotUI : MonoBehaviour
{
    [SerializeField] private Image _icon;
    [SerializeField] private Sprite _defaultSprite;
    [SerializeField] private Button _button;

    private PlayerEquipSlot _equipSlot;
    private CharacterSheet _character;

    void OnDestroy()
    {
        _button.onClick.RemoveAllListeners();
    }

    public void Init(PlayerEquipSlot slot, CharacterSheet character)
    {
        _equipSlot = slot;
        _character = character;
        _button.onClick.AddListener(OnClickUnequip);
    }

    public void Refresh()
    {
        if (_equipSlot == null) return;
        
        _icon.sprite = _equipSlot.IsOccuped ? _equipSlot.EquippedItem.icon : _defaultSprite;
    }

    public void OnClickUnequip()
    {
        _equipSlot?.ClearSlot(_character);
        Refresh();
    }
}
