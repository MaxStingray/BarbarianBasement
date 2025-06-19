using UnityEngine.UI;
using TMPro;
using UnityEngine;

/// <summary>
/// a UI that displays an inventory (player, container, merchant etc)
/// </summary>
public abstract class InventoryUI : MonoBehaviour
{
    public Inventory Inventory;
    public Transform ItemButtonParent;
    public GameObject ItemButtonPrefab;

    [SerializeField] protected GameObject itemPreview;

    [SerializeField] protected TextMeshProUGUI nameText;
    [SerializeField] protected TextMeshProUGUI descriptionText;
    [SerializeField] protected Image iconImage;

    protected Item selectedItem;

    protected Item previousItem;

    public virtual void OnEnable()
    {
        Inventory.OnInventoryChanged.AddListener(UpdateUI);
        UpdateUI();
    }

    public virtual void UpdateUI()
    {
        // Clear old buttons
        foreach (Transform child in ItemButtonParent)
            Destroy(child.gameObject);

        foreach (Item item in Inventory.Items)
        {
            GameObject buttonObj = Instantiate(ItemButtonPrefab, ItemButtonParent);
            ItemButtonUI button = buttonObj.GetComponent<ItemButtonUI>();
            button.Setup(item, this);
        }
    }

    public virtual void ShowItemDetails(Item item)
    {
        if (selectedItem != null)
        {
            previousItem = selectedItem;
        }
        selectedItem = item;
        nameText.text = item.itemName;
        descriptionText.text = item.description;
        iconImage.sprite = item.icon;
    }

    protected virtual void SetPreviewWindowState(bool show)
    {
        itemPreview.SetActive(show);
    }
}
