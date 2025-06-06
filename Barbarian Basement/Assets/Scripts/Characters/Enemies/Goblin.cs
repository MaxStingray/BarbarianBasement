using UnityEngine;

public class Goblin : Enemy
{
    protected override void Die()
    {
        base.Die();
        animator.Play("Die");
    }
}
