using UnityEngine;

public abstract class Interactable : MonoBehaviour, Iinteractable
{
    public virtual void StartInteraction()
    {
        Debug.Log("interacting");
        OnInteract();
    }
    public abstract void OnInteract();
}

