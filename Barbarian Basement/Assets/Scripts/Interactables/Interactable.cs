using UnityEngine;

public abstract class Interactable : MonoBehaviour, Iinteractable
{
    public GameTile TileData;
    public virtual void StartInteraction()
    {
        OnInteract();
    }
    public abstract void OnInteract();
}

