using UnityEngine;
using Color = UnityEngine.Color;

public class TriggerLongInvincibility : StateMachineBehaviour
{
    public float duration = 2f; // How much the invencibility will last
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Damageable damageable = animator.GetComponent<Damageable>();
        if (damageable != null)
        {
            damageable.StartLongInvincibility(duration);
        }
    }
}
