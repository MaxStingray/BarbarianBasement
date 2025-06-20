using UnityEngine;

public class QuickMenu : MonoBehaviour
{
    [SerializeField] private QuickMenuItemButton _button1;
    [SerializeField] private QuickMenuItemButton _button2;
    [SerializeField] private QuickMenuItemButton _button3;

    public void AssignButton1(Item item)
    {
        _button1.Assign(item);
    }

    public void AssignButton2(Item item)
    {
        _button2.Assign(item);
    }

    public void AssignButton3(Item item)
    {
        _button3.Assign(item);
    }
}
