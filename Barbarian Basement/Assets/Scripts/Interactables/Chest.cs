using UnityEngine;

public class Chest : Interactable
{
    public override void OnInteract()
    {
        Debug.Log("You opened a chest");
    }
}
