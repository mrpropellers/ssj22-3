using UnityEngine;

namespace BossFight.BossCharacter
{
    public class SetPositionOnExit : StateMachineBehaviour
    {
        public Vector3 PositionToSet;

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            animator.gameObject.transform.SetPositionAndRotation(PositionToSet, Quaternion.identity);
        }
    }
}
