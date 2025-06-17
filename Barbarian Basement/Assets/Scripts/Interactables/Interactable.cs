using UnityEngine;

public abstract class Interactable : MonoBehaviour, Iinteractable
{
    [SerializeField] protected AudioSource audioSource;
    [SerializeField] protected AudioClip interactClip;
    public GameTile TileData;
    public virtual void StartInteraction()
    {
        OnInteract();
        if (audioSource && interactClip)
        {
            audioSource.PlayOneShot(interactClip);
        }
    }
    public abstract void OnInteract();
}

