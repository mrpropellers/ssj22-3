using System;
using System.Collections.Generic;
using LeftOut;
using LeftOut.Extensions;
using LeftOut.JamAids;
using UnityEngine;

namespace BossFight.BossCharacter
{
    /// <summary>
    /// Observes scene and translates observations into parameter settings inside the Boss's animator
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class BossAttackChecks : MonoBehaviour, ICollision2DReceiver, ITrigger2DReceiver
    {
        // Tracks how many total player contact points there are for debugging
        int DEBUG_PlayerContacts = 0;
        List<PlayerCharacterStateManager> m_Players;
        Dictionary<PlayerCharacterStateManager, int> m_PlayerContactPoints;

        [SerializeField]
        [Min(0f)]
        float m_CloseAttackThreshold = 1f;
        [SerializeField]
        [Min(0f)]
        float m_MidAttackThreshold = 2f;
        [SerializeField]
        [Min(0f)]
        float m_FarAttackThreshold = 3f;


        Vector2 Forward2D
        {
            get
            {
                var tf = transform;
                var forward = tf.right;
                forward.Scale(tf.lossyScale);
                return forward;
            }
        }

        void Start()
        {
            m_Players = InstanceTrackingList<PlayerCharacterStateManager>.GetReference();
            m_PlayerContactPoints = new Dictionary<PlayerCharacterStateManager, int>();
        }


        void Update()
        {


        }


        void OnValidate()
        {
            if (m_MidAttackThreshold <= m_CloseAttackThreshold)
            {
                Debug.LogWarning(
                    $"{nameof(m_MidAttackThreshold)} must be larger than {nameof(m_CloseAttackThreshold)}");
                m_MidAttackThreshold = m_CloseAttackThreshold + float.Epsilon * 2f;
            }
            if (m_FarAttackThreshold <= m_MidAttackThreshold)
            {
                Debug.LogWarning(
                    $"{nameof(m_FarAttackThreshold)} must be larger than {nameof(m_MidAttackThreshold)}");
                m_FarAttackThreshold = m_MidAttackThreshold + float.Epsilon * 2f;
            }
        }

        void OnDrawGizmos()
        {
            var position = (Vector2)transform.position;
            var up = 0.5f * Vector2.up;
            var forward = Forward2D;
            Gizmos.color = Color.red;
            var close = position + m_CloseAttackThreshold * forward;
            Gizmos.DrawLine(close, close + up);
            Gizmos.color = Color.yellow;
            var mid = position + m_MidAttackThreshold * forward;
            Gizmos.DrawLine(mid, mid + up);
            Gizmos.color = Color.green;
            var far = position + m_FarAttackThreshold * forward;
            Gizmos.DrawLine(far, far + up);
        }

        void CheckAndTrackPlayerCollisionEnter(Collider2D other)
        {
            if (other.gameObject.TryGetComponentInParent(out PlayerCharacterStateManager playerState))
            {
                if (!m_PlayerContactPoints.ContainsKey(playerState))
                {
                    m_PlayerContactPoints.Add(playerState, 0);
                }

                m_PlayerContactPoints[playerState]++;
                DEBUG_PlayerContacts++;
            }
        }

        void CheckAndTrackPlayerCollisionExit(Collider2D other)
        {
            if (other.gameObject.TryGetComponentInParent(out PlayerCharacterStateManager playerState))
            {
                m_PlayerContactPoints[playerState]--;
                DEBUG_PlayerContacts--;
            }
        }

        void CheckAndUpdatePlayerLocations()
        {

        }

        void OnCollisionEnter2D(Collision2D collision)
        {
            CheckAndTrackPlayerCollisionEnter(collision.collider);
        }

        void OnTriggerEnter2D(Collider2D otherCollider)
        {
            CheckAndTrackPlayerCollisionEnter(otherCollider);
        }

        public void OnChildCollision2DEnter(Collision2D collision2D)
        {
            CheckAndTrackPlayerCollisionEnter(collision2D.collider);
        }

        public void OnChildCollision2DStay(Collision2D collision2D)
        {
        }

        public void OnChildCollision2DExit(Collision2D collision2D)
        {
            CheckAndTrackPlayerCollisionExit(collision2D.collider);
        }

        public void OnChildTrigger2DEnter(Collider2D otherCollider)
        {
            CheckAndTrackPlayerCollisionEnter(otherCollider);
        }

        public void OnChildTrigger2DStay(Collider2D otherCollider)
        {
        }

        public void OnChildTrigger2DExit(Collider2D otherCollider)
        {
            CheckAndTrackPlayerCollisionExit(otherCollider);
        }
    }
}
