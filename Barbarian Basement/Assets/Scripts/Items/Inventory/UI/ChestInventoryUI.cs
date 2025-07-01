using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class ChestInventoryUI : InventoryUI
{
    [SerializeField] private Button _takeButton;

    void Awake()
    {
        _takeButton.onClick.AddListener(OnTakeItem);
    }

    public void Initialise(Inventory chestInventory)
    {
        this.Inventory = chestInventory;
        UpdateUI();
    }

    public override void ShowItemDetails(Item item)
    {
        base.ShowItemDetails(item);
        _takeButton.gameObject.SetActive(true);
    }

    private void OnTakeItem()
    {
        if (selectedItem == null) return;

        if (selectedItem is Gold)
        {
            selectedItem.UseItem(GameManager.Instance.Player);
        }
        else
        {
            GameManager.Instance.Player.Inventory.AddItem(selectedItem);
        }

        Inventory.RemoveItem(selectedItem);
        selectedItem = null;
        UpdateUI();
    }
}
