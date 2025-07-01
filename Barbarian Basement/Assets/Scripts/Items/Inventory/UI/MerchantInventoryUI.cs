using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class ItemWithSource
{
    public Item Item;
    public bool IsPlayerItem;

    public ItemWithSource(Item item, bool isPlayerItem)
    {
        Item = item;
        IsPlayerItem = isPlayerItem;
    }
}
public class MerchantInventoryUI : InventoryUI
{
    private PlayerInventory _playerInventory => GameManager.Instance.Player.PlayerInventory;

    [SerializeField] private Button _buyButton;
    [SerializeField] private Button _sellButton;

    [SerializeField] private Transform _playerWindowItemButtonParent;

    [SerializeField] private TextMeshProUGUI _goldText;
    [SerializeField] private TextMeshProUGUI _priceText;

    //list of all items, player and merchant with their sources
    public List<ItemWithSource> ItemList = new List<ItemWithSource>();

    void Awake()
    {
        _buyButton.onClick.AddListener(OnBuyItem);
        _sellButton.onClick.AddListener(OnSellItem);
    }

    public void Initialise(Inventory merchantInventory)
    {
        this.Inventory = merchantInventory;
        UpdateUI();
    }

    /// <summary>
    /// returns a combined list of all items with their sources
    /// </summary>
    /// <param name="player"></param>
    /// <param name="merchant"></param>
    /// <returns></returns>
    private List<ItemWithSource> CombinedList(Inventory player, Inventory merchant)
    {
        List<ItemWithSource> builtList = new List<ItemWithSource>();
        foreach (var item in player.Items)
        {
            builtList.Add(new ItemWithSource(item, true));
        }

        foreach (var item in merchant.Items)
        {
            builtList.Add(new ItemWithSource(item, false));
        }

        return builtList;
    }

    /// <summary>
    /// dont use base. Instead populate the menu based on the newly created list
    /// </summary>
    public override void UpdateUI()
    {
        //just in case
        ItemList.Clear();
        //create a new combined list of both inventories
        ItemList = CombinedList(_playerInventory, Inventory);
        // Clear old buttons
        foreach (Transform child in ItemButtonParent)
            Destroy(child.gameObject);

        foreach (Transform child in _playerWindowItemButtonParent)
            Destroy(child.gameObject);

        foreach (ItemWithSource item in ItemList)
        {
            Transform parent = item.IsPlayerItem ? _playerWindowItemButtonParent : ItemButtonParent;
            GameObject buttonObj = Instantiate(ItemButtonPrefab, parent);
            ItemButtonUI button = buttonObj.GetComponent<ItemButtonUI>();
            button.Setup(item.Item, this);
        }

        if (selectedItem == null || (previousItem != null && previousItem != selectedItem))
        {
            SetPreviewWindowState(false);
        }

        _goldText.text = $"Gold: {_playerInventory.Gold}";

    }

    private void OnBuyItem()
    {
        if (_playerInventory.Gold >= selectedItem.Value)
        {
            //buy it
            _playerInventory.Gold -= selectedItem.Value;
            Inventory.RemoveItem(selectedItem);
            _playerInventory.AddItem(selectedItem);
            UpdateUI();
        }
        else
        {
            Debug.Log($"not enough gold to buy {selectedItem.itemName}");
        }
    }

    private void OnSellItem()
    {
        _playerInventory.AddGold(selectedItem.SaleValue);
        _playerInventory.RemoveItem(selectedItem);
        Inventory.AddItem(selectedItem);
        UpdateUI();
    }

    public override void ShowItemDetails(Item item)
    {
        _priceText.text = "";
        _sellButton.gameObject.SetActive(false);
        _buyButton.gameObject.SetActive(false);
        base.ShowItemDetails(item);
        var itemSource = FindSource(item);

        if (itemSource.IsPlayerItem)
        {
            _sellButton.gameObject.SetActive(true);
            _priceText.text = $"Sell price: {selectedItem.SaleValue}";
        }
        else
        {
            _buyButton.gameObject.SetActive(true);
            _priceText.text = $"Price: {selectedItem.Value}";
        }
    }

    private ItemWithSource FindSource(Item item)
    {
        return ItemList.Find(i => ReferenceEquals(i.Item, item));
    }
}
