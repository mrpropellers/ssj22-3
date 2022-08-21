using UnityEngine;

namespace BossFight.BossCharacter
{
    public class ActivateHurtboxOnExit : StateMachineBehaviour
    {
        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            // We wait until attack windup is done before activating hurtbox to give the player
            // a chance to react to the attack starting
            animator.SetBool(Constants.AnimatorParameters.HurtboxActive, true);
        }
    }
}
