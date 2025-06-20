using UnityEngine;

public class Chest : Interactable
{
    //the chest's inventory
    [SerializeField] private ChestInventory _inventory;

    void Awake()
    {
        _inventory.GetRandomContents();
    }

    public override void OnInteract()
    {
        Debug.Log("You opened a chest");
        GameManager.Instance.ChestInventoryUI.gameObject.SetActive(true);
        GameManager.Instance.ChestInventoryUI.Initialise(_inventory);
    }
}
