using System.Collections.Generic;
using UnityEngine;

namespace BossFight.BossCharacter
{
    public class RootMovementState : StateMachineBehaviour
    {
        struct InitialPose
        {
            internal Vector3 Position;
            internal Quaternion Orientation;
        }

        Dictionary<Transform, InitialPose> m_PoseRecord;
        Transform m_RigTarget;
        Transform m_InitialRigTransform;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            animator.gameObject.GetRigTargetFromChildren(
                out m_RigTarget, out var childTransforms);
            m_PoseRecord = new Dictionary<Transform, InitialPose>();
            foreach (var child in childTransforms)
            {
                m_PoseRecord[child] = new InitialPose()
                {
                    Position = child.localPosition,
                    Orientation = child.rotation
                };
            }

            var placeholder = GameplayHelpers.GameObjectPool.Get();
            m_InitialRigTransform = placeholder.transform;
            m_InitialRigTransform.SetPositionAndRotation(m_RigTarget.position, m_RigTarget.rotation);
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            var translation = m_RigTarget.position - m_InitialRigTransform.position;
            var rotation = m_RigTarget.rotation * Quaternion.Inverse(m_InitialRigTransform.rotation);
            var tf = animator.gameObject.transform;
            tf.SetPositionAndRotation(tf.position + translation, rotation * tf.rotation);
            foreach (var child in m_PoseRecord.Keys)
            {
                var record = m_PoseRecord[child];
                child.localPosition = record.Position;
                child.localRotation = record.Orientation;
            }
            GameplayHelpers.GameObjectPool.Release(m_InitialRigTransform.gameObject);
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) { }

        public override void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) { }

        public override void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) { }
    }
}
