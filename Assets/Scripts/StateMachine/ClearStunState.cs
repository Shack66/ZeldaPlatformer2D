using UnityEngine;

public class ClearStunState : StateMachineBehaviour
{
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // isStunned is turned off when entering to break the loop of Any State
        animator.SetBool(AnimationStrings.isStunned, false); 
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Physic block is removed
        animator.SetBool(AnimationStrings.lockVelocity, false);

        // canMove is true again for free movement
        animator.SetBool(AnimationStrings.canMove, true);
    }
}