using System.Collections.Generic;
using UnityEngine;

namespace BossFight.BossCharacter
{
    public class RootMovementState : StateMachineBehaviour
    {
        Transform m_StartingPose;
        Transform m_RigTarget;
        List<Transform> m_ChildTransforms;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            animator.gameObject.GetRigTargetFromChildren(
                out m_RigTarget, out m_ChildTransforms);
            var placeholder = GameplayHelpers.GameObjectPool.Get();
            m_StartingPose = placeholder.transform;
            m_StartingPose.SetPositionAndRotation(m_RigTarget.position, m_RigTarget.rotation);
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            var translation = m_RigTarget.position - m_StartingPose.position;
            var rotation = m_RigTarget.rotation * Quaternion.Inverse(m_StartingPose.rotation);
            var tf = animator.gameObject.transform;
            tf.SetPositionAndRotation(tf.position + translation, rotation * tf.rotation);
            var inverseRotation = Quaternion.Inverse(rotation);
            foreach (var child in m_ChildTransforms)
            {
                child.SetPositionAndRotation(
                    child.position - translation, inverseRotation * child.rotation);
            }
            GameplayHelpers.GameObjectPool.Release(m_StartingPose.gameObject);
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) { }

        public override void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) { }

        public override void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) { }
    }
}
