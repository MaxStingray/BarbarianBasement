using UnityEngine;

public abstract class Interactable : MonoBehaviour, Iinteractable
{
    public virtual void StartInteraction()
    {
        OnInteract();
    }
    public abstract void OnInteract();
}

