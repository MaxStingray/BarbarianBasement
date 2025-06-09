using UnityEngine;
using UnityEngine.Animations;

public static class AnimationUtils
{
    public static void ValidateAnimationAndPlay(Animator animator, string state)
    {
        if (animator == null) return;

        int stateID = Animator.StringToHash(state);

        if (!animator.HasState(0, stateID)) return;

        animator.Play(state);
    }
}
