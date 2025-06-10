using UnityEngine;

public class Stairs : Interactable
{
    public override void StartInteraction()
    {
        base.StartInteraction();
        Debug.Log("Ascending to the next floor...");
        //effects, UI, etc
    }


    public override void OnInteract()
    {
        GameManager.Instance.MoveToNextFloor();
    }

}
