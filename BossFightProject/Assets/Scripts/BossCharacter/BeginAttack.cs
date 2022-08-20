using UnityEngine;

namespace BossFight.BossCharacter
{
    public class BeginAttack : StateMachineBehaviour
    {
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            animator.SetBool(Constants.AnimatorParameters.IsAttacking, true);
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            animator.SetBool(Constants.AnimatorParameters.HurtboxActive, true);
        }
    }
}
