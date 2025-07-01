using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInventoryUI : InventoryUI
{
    [SerializeField] private EquipmentUI _equipUI;
    public Button AddToQuickMenuButton;
    public Button EquipButton;

    [SerializeField] private GameObject _quickMenuWindow;
    [SerializeField] private GameObject _equipMenuWindow;

    [SerializeField] private Button _qmSlot1Button;
    [SerializeField] private Button _qmSlot2Button;
    [SerializeField] private Button _qmSlot3Button;

    [SerializeField] private QuickMenu _quickMenu;

    PlayerInventory PlayerInventory => (PlayerInventory)Inventory;

    [SerializeField] private TextMeshProUGUI _goldText;

    void Awake()
    {
        _qmSlot1Button.onClick.AddListener(AssignToSlot1);
        _qmSlot2Button.onClick.AddListener(AssignToSlot2);
        _qmSlot3Button.onClick.AddListener(AssignToSlot3);
        AddToQuickMenuButton.onClick.AddListener(OnAddToQuickMenuClicked);
        EquipButton.onClick.AddListener(OnEquipButtonClicked);
    }

    public override void UpdateUI()
    {
        base.UpdateUI();
        _goldText.text = $"Gold: {PlayerInventory.Gold}";
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

        if (!selectedItem) return;

        if (selectedItem is Consumable || selectedItem is Weapon)
        {
            _quickMenuWindow.SetActive(!show);
        }
    }

    private void OnEquipButtonClicked()
    {
        if (selectedItem is Equipment equipment)
        {
            _equipUI.OnClickEquip(equipment);
            SetPreviewWindowState(false);
        }
    }

    private void AssignToSlot1()
    {
        PlayerInventory.AddItemToQuickSlot(selectedItem);
        _quickMenu.AssignButton1(selectedItem, PlayerInventory);
        GameManager.Instance.Player.Inventory.RemoveItem(selectedItem);
        ClearItemDetails();
        _quickMenuWindow.SetActive(false);
        SetPreviewWindowState(false);
    }
    private void AssignToSlot2()
    {
        PlayerInventory.AddItemToQuickSlot(selectedItem);
        _quickMenu.AssignButton2(selectedItem, PlayerInventory);
        GameManager.Instance.Player.Inventory.RemoveItem(selectedItem);
        ClearItemDetails();
        _quickMenuWindow.SetActive(false);
        SetPreviewWindowState(false);
    }
    private void AssignToSlot3()
    {
        PlayerInventory.AddItemToQuickSlot(selectedItem);
        _quickMenu.AssignButton3(selectedItem, PlayerInventory);
        GameManager.Instance.Player.Inventory.RemoveItem(selectedItem);
        ClearItemDetails();
        _quickMenuWindow.SetActive(false);
        SetPreviewWindowState(false);
    }
}
