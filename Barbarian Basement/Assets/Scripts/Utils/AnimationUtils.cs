using System.Collections;
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
    
    public static AnimationClip FindAnimationClip(Animator animator, string clipName)
    {
        var clips = animator.runtimeAnimatorController.animationClips;
        foreach (var clip in clips)
        {
            if (clip.name == clipName)
            {
                return clip;
            }
        }
        return null;
    }

    public static IEnumerator AwaitAnimationComplete(Animator animator, string state)
    {
        if (animator == null) yield break;
        
        var clip = FindAnimationClip(animator, state);

        float clipLength = clip != null ? clip.length : 0.5f; // fallback length if no clip found

        yield return new WaitForSeconds(clipLength);
    }
}
