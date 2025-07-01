using UnityEngine;

public class Merchant : Interactable
{
    [SerializeField] private MerchantInventory _inventory;

    void Awake()
    {
        _inventory.GetRandomContents();
    }

    public override void OnInteract()
    {
        Debug.Log("What are ya buyin?");
        GameManager.Instance.MerchantUI.gameObject.SetActive(true);
        GameManager.Instance.MerchantUI.Initialise(_inventory);
    }
}
