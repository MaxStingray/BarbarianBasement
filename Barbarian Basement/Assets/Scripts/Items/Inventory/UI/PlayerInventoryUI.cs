using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInventoryUI : InventoryUI
{
    public Button AddToQuickMenuButton;
    public Button EquipButton;

    [SerializeField] private GameObject _quickMenuWindow;
    [SerializeField] private GameObject _equipMenuWindow;

    [SerializeField] private Button _qmSlot1Button;
    [SerializeField] private Button _qmSlot2Button;
    [SerializeField] private Button _qmSlot3Button;

    [SerializeField] private QuickMenu _quickMenu;

    void Awake()
    {
        _qmSlot1Button.onClick.AddListener(AssignToSlot1);
        _qmSlot2Button.onClick.AddListener(AssignToSlot2);
        _qmSlot3Button.onClick.AddListener(AssignToSlot3);
        AddToQuickMenuButton.onClick.AddListener(OnAddToQuickMenuClicked);
        EquipButton.onClick.AddListener(OnEquipButtonClicked);
    }

    public override void ShowItemDetails(Item item)
    {
        base.ShowItemDetails(item);

        AddToQuickMenuButton.gameObject.SetActive(item is Consumable || item is Weapon);
        EquipButton.gameObject.SetActive(item is Equipment);
    }

    private void OnAddToQuickMenuClicked()
    {
        if (selectedItem == null) return;
        if (selectedItem is Consumable || selectedItem is Weapon)
        {
            SetPreviewWindowState(false);
        }
    }

    protected override void SetPreviewWindowState(bool show)
    {
        base.SetPreviewWindowState(show);

        if (selectedItem is Consumable || selectedItem is Weapon)
        {
            _quickMenuWindow.SetActive(!show);
        }
    }

    private void OnEquipButtonClicked()
    {
        selectedItem.UseItem(GameManager.Instance.Player);
    }

    private void AssignToSlot1()
    {
        _quickMenu.AssignButton1(selectedItem);
        GameManager.Instance.Player.Inventory.RemoveItem(selectedItem);
        SetPreviewWindowState(true);
    }
    private void AssignToSlot2()
    {
        _quickMenu.AssignButton2(selectedItem);
        GameManager.Instance.Player.Inventory.RemoveItem(selectedItem);
        SetPreviewWindowState(true);
    }
    private void AssignToSlot3()
    {
        _quickMenu.AssignButton3(selectedItem);
        GameManager.Instance.Player.Inventory.RemoveItem(selectedItem);
        SetPreviewWindowState(true);
    }
}
