using UnityEngine;

namespace BossFight.BossCharacter
{
    public class DeactivateHurtbox : StateMachineBehaviour
    {
        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            animator.SetBool(Constants.AnimatorParameters.HurtboxActive, false);
        }
    }
}
