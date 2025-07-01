using UnityEngine.UI;
using TMPro;
using UnityEngine;
using Unity.VisualScripting;
using UnityEditor.Compilation;

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

    [SerializeField] protected Sprite defaultSprite;

    protected Item selectedItem;

    protected Item previousItem;

    public virtual void OnEnable()
    {
        //sometimes initialisation is handled elsewhere, so only do this if inventory is already assigned
        if (Inventory)
        {
            Inventory.OnInventoryChanged.AddListener(UpdateUI);
            UpdateUI();
            selectedItem = null;
            SetPreviewWindowState(false);
        }
    }

    public virtual void OnDisable()
    {
        Inventory.OnInventoryChanged.RemoveListener(UpdateUI);
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

        if (selectedItem == null || (previousItem != null && previousItem != selectedItem))
        {
            if (Inventory.Items.Count != 0)
            {
                selectedItem = Inventory.Items[Inventory.Items.Count - 1];
                ShowItemDetails(selectedItem);
            }
            else
            {
                SetPreviewWindowState(false);
            }
        }
    }

    public virtual void ShowItemDetails(Item item)
    {
        if (selectedItem != null)
        {
            previousItem = selectedItem;
        }
        SetPreviewWindowState(true);
        selectedItem = item;
        nameText.text = item.itemName;
        descriptionText.text = item.description;
        iconImage.sprite = item.icon;
    }

    public virtual void ClearItemDetails()
    {
        selectedItem = null;
        nameText.text = "";
        descriptionText.text = "";
        iconImage.sprite = defaultSprite;
    }

    protected virtual void SetPreviewWindowState(bool show)
    {
        itemPreview.SetActive(show);
    }
}
