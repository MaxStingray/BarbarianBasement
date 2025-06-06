using UnityEngine;

public class Enemy : CharacterSheet
{
    [SerializeField] protected string[] names;

    protected override void Awake()
    {
        base.Awake();

        characterName = GetName();
    }

    // Randomly select a name from the collection if any have been provided
    private string GetName()
    {
        if (names.Length == 0)
        {
            return "defaultName";
        }

        var index = Random.Range(0, names.Length);

        return names[index];
    }
}
