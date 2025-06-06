using UnityEngine;

public class Barbarian : CharacterSheet
{
    public bool IsDead { get; private set; }
    protected override void Die()
    {
        base.Die();
        IsDead = true;
    }
}
