using UnityEngine;

public class Stairs : Interactable
{
    public override void StartInteraction()
    {
        base.StartInteraction();
        //effects, UI, etc
    }


    public override void OnInteract()
    {
        GameManager.Instance.ResetAndStartNewDungeon(false);
    }

}
