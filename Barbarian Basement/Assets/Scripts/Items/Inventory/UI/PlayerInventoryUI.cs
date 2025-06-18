using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInventoryUI : InventoryUI
{
    public Button AddToQuickMenuButton;
    public Button EquipButton;

    void Awake()
    {
        AddToQuickMenuButton.onClick.AddListener(OnAddToQuickMenuClicked);
        EquipButton.onClick.AddListener(OnEquipButtonClicked);
    }

    public override void ShowItemDetails(Item item)
    {
        base.ShowItemDetails(item);

        AddToQuickMenuButton.gameObject.SetActive(item is Consumable);
        EquipButton.gameObject.SetActive(item is Equipment);
    }

    private void OnAddToQuickMenuClicked()
    {
        if (selectedItem == null) return;
        if (selectedItem is Consumable)
        {
            // add to quick menu here
        }
    }

    private void OnEquipButtonClicked()
    {
        selectedItem.UseItem(GameManager.Instance.Player);
    }
}
