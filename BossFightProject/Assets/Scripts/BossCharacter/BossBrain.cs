using System;
using BossFight.Constants;
using DG.Tweening;
using LeftOut.Extensions;
using UnityEngine;

namespace BossFight.BossCharacter
{
    [RequireComponent(typeof(BossSensors))]
    [RequireComponent(typeof(UnityEngine.Animator))]
    public class BossBrain : MonoBehaviour
    {
        BossSensors m_Sensors;
        UnityEngine.Animator m_Animator;
        HitboxManager m_Hitboxes;
        BossMove m_CurrentMove;

        float m_RemainingDecisionCooldown;

        [SerializeField]
        BossStats m_BossStats;

        // Explicitly check for null with == here so we can use null-coalescing ?.Function() calls later
        // NOTE: We'll still get a warning from Rider, but the warning is wrong.
        public HitboxManager hitboxes => m_Hitboxes == null ? null : m_Hitboxes;

        void Start()
        {
            m_Sensors = GetComponent<BossSensors>();
            m_Animator = GetComponent<UnityEngine.Animator>();
            m_Hitboxes = GetComponent<HitboxManager>();
            if (m_Hitboxes == null)
            {
                Debug.LogWarning($"No {nameof(HitboxManager)} attached - may not activate hurtboxes correctly.");
            }

            m_RemainingDecisionCooldown = m_BossStats.DecisionCooldown * 2;
        }

        void Update()
        {
            if (m_CurrentMove != null)
            {
                m_CurrentMove.DoUpdate();
                if (m_CurrentMove.IsFinished)
                {
                    CleanUpFinishedMove();
                }
                else
                {
                    return;
                }
            }

            m_RemainingDecisionCooldown -= Time.deltaTime;
            if (m_RemainingDecisionCooldown > 0f)
            {
                return;
            }

            Debug.Log("Attempting to pick attack...");
            PickNewMove();
        }

        void OnDrawGizmos()
        {
            Gizmos.color = m_CurrentMove == null ? Color.white : Color.red;
            m_Sensors ??= GetComponent<BossSensors>();
            var bounds = m_Sensors.CurrentBounds;
            Gizmos.DrawWireCube(bounds.center, bounds.size);
        }

        void CleanUpFinishedMove()
        {
            Debug.Log($"Attack finished: {m_CurrentMove}");
            m_Animator.SetTrigger(AnimatorParameters.TriggerMoveFinished);
            m_CurrentMove = null;
            m_RemainingDecisionCooldown = m_BossStats.DecisionCooldown;
            transform.DOComplete();
            hitboxes?.DeactivateAll();
        }

        void PickNewMove()
        {
            var attacksShuffled = m_BossStats.Attacks.GetShuffledCopy();
            foreach (var attack in attacksShuffled)
            {
                if (attack.CanStart(m_Sensors))
                {
                    Debug.Log($"Starting attack: {attack}");
                    m_Animator.SetTrigger(attack.AnimatorTrigger);

                    attack.Begin(m_Sensors, m_BossStats, hitboxes);
                    m_CurrentMove = attack;
                    return;
                }
            }

            Debug.Log("Failed to find valid attack.");
            m_RemainingDecisionCooldown = m_BossStats.DecisionCooldown;
        }
    }
}
