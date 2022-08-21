using System;
using System.Collections.Generic;
using BossFight.Constants;
using LeftOut;
using UnityEngine;

namespace BossFight.BossCharacter
{
    /// <summary>
    /// Observes scene and translates observations into parameter settings inside the Boss's animator
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class BossAnimatorBrain : MonoBehaviour
    {
        Collider2D[] m_Colliders;
        List<PlayerCharacterStateManager> m_Players;
        Animator m_Animator;
        Bounds m_CurrentBounds;

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
            m_Animator = GetComponent<Animator>();
            m_Colliders = GetComponentsInChildren<Collider2D>();
        }


        void Update()
        {
            m_CurrentBounds = m_Colliders[0].bounds;
            for (var i = 1; i < m_Colliders.Length; ++i)
            {
                m_CurrentBounds.Encapsulate(m_Colliders[i].bounds);
            }
            CheckAndUpdatePlayerLocations();
        }

        void OnTriggerEnter2D(Collider2D otherCollider2D)
        {
            if (otherCollider2D.CompareTag(Tags.Walls))
            {

            }
        }

        void CheckAndUpdatePlayerLocations()
        {
            // Create some floats for computing means later
            var numPlayers = (float)m_Players.Count;
            var numBehind = 0f;
            var meanPosition = Vector2.zero;

            foreach (var player in m_Players)
            {
                var playerPosition = player.transform.position;
                var playerLocal = transform.InverseTransformPoint(playerPosition);
                // If player is inside bounds, but not touching, we assume above
                if (Mathf.Sign(playerLocal.x) < 0)
                {
                    numBehind++;
                }

                // Players in front and behind shouldn't cancel each other out, so we take the absolute value along x
                // but, players below shouldn't cause the boss to jump, so no abs on y
                meanPosition +=  new Vector2(Mathf.Abs(playerLocal.x), playerLocal.y);
            }

            meanPosition /= numPlayers;
            m_Animator.SetFloat(AnimatorParameters.PlayersBehindRatio, numBehind / numPlayers);
            m_Animator.SetFloat(AnimatorParameters.MeanPlayerXDistance, meanPosition.x);
            m_Animator.SetFloat(AnimatorParameters.MeanPlayerYDistance, meanPosition.y);
        }
    }
}
