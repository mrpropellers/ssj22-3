using System;
using LeftOut.Extensions;
using UnityEngine;

namespace BossFight.BossCharacter
{
    [RequireComponent(typeof(BossSensors))]
    [RequireComponent(typeof(Animator))]
    public class BossBrain : MonoBehaviour
    {
        BossSensors m_Sensors;
        Animator m_Animator;
        HurtboxManager m_Hurtboxes;

        BossAttack m_CurrentAttack;

        float m_RemainingDecisionCooldown;

        [SerializeField]
        BossStats m_BossStats;

        void Start()
        {
            m_Sensors = GetComponent<BossSensors>();
            m_Animator = GetComponent<Animator>();
            m_Hurtboxes = GetComponent<HurtboxManager>();
            if (m_Hurtboxes == null)
            {
                Debug.LogWarning($"No {nameof(HurtboxManager)} attached - may not activate hurtboxes correctly.");
            }

            m_RemainingDecisionCooldown = m_BossStats.DecisionCooldown * 2;
        }

        void Update()
        {
            if (m_CurrentAttack != null)
            {
                m_CurrentAttack.DoUpdate(gameObject);
                if (m_CurrentAttack.IsFinished)
                {
                    Debug.Log($"Attack finished: {m_CurrentAttack}");
                    m_CurrentAttack = null;
                    m_RemainingDecisionCooldown = m_BossStats.DecisionCooldown;
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
            PickNewAttack();
        }

        void PickNewAttack()
        {
            var attacksShuffled = m_BossStats.Attacks.GetShuffledCopy();
            var arenaObservation = m_Sensors.CurrentArenaObservation;
            var playerObservations = m_Sensors.CurrentPlayerObservation;
            foreach (var attack in attacksShuffled)
            {
                if (attack.CanStart(gameObject, arenaObservation, playerObservations))
                {
                    Debug.Log($"Starting attack: {attack}");
                    m_Animator.SetTrigger(attack.AnimatorTrigger);
                    m_Hurtboxes?.ActivateAll();

                    attack.Begin(gameObject, arenaObservation, playerObservations);
                    m_CurrentAttack = attack;
                    return;
                }
            }

            Debug.Log("Failed to find valid attack.");
            m_RemainingDecisionCooldown = m_BossStats.DecisionCooldown;
        }
    }
}
