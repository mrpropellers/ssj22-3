using UnityEngine;

namespace BossFight.BossCharacter
{
    public class TurnAroundState : StateMachineBehaviour
    {
        Collider2D[] m_AllColliders;
        bool[] m_OriginalColliderStates;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            m_AllColliders = animator.gameObject.GetComponentsInChildren<Collider2D>();
            m_OriginalColliderStates = new bool[m_AllColliders.Length];
            for (var i = 0; i < m_AllColliders.Length; ++i)
            {
                m_OriginalColliderStates[i] = m_AllColliders[i].enabled;
                m_AllColliders[i].enabled = false;
            }
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            var tf = animator.transform;
            tf.localScale = new Vector3(tf.localScale.x * -1, tf.localScale.y, tf.localScale.z);
            for (var i = 0; i < m_AllColliders.Length; ++i)
            {
                m_AllColliders[i].enabled = m_OriginalColliderStates[i];
            }
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) { }

        public override void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) { }

        public override void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) { }
    }
}
