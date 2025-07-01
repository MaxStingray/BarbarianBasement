using UnityEngine;

public class QuickMenu : MonoBehaviour
{
    [SerializeField] private QuickMenuItemButton _button1;
    [SerializeField] private QuickMenuItemButton _button2;
    [SerializeField] private QuickMenuItemButton _button3;

    public void AssignButton1(Item item, PlayerInventory inventory)
    {
        _button1.Assign(item, inventory);
    }

    public void AssignButton2(Item item, PlayerInventory inventory)
    {
        _button2.Assign(item, inventory);
    }

    public void AssignButton3(Item item, PlayerInventory inventory)
    {
        _button3.Assign(item, inventory);
    }
}
