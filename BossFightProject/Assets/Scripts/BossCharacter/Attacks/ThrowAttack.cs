using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using BossFight.Constants;
using UnityAtoms.BaseAtoms;
using UnityEngine;

namespace BossFight.BossCharacter
{
    [CreateAssetMenu(fileName = "ThrowAttack", menuName = "BossFight/ThrowAttack", order = 0)]
    class ThrowAttack : BossMove
    {
        float m_StartTime = float.MinValue;
        float m_FinishTime = float.MaxValue;

        bool m_WaitingForCollision = true;
        Animator m_BossAnimator;

        [SerializeField]
        [Range(-2f, 4f)]
        float m_VerticalAimCompensation = 3f;

        [SerializeField]
        VoidEvent RockBreakEvent;

        [SerializeField]
        [Range(0f, 5f)]
        float WindupDuration = 1.5f;

        [SerializeField]
        [Range(1f, 5f)]
        float m_CooldownTime = 5f;

        float AttackDuration => WindupDuration;

        public override string AnimatorTrigger => Constants.AnimatorParameters.TriggerRockFling;
        public override bool IsFinished => Time.time - m_StartTime > AttackDuration && !m_WaitingForCollision;

        public override bool CanStart(BossSensors sensors)
        {
            // In-editor problems... m_StartTime doesn't re-initialize between PlayMode starts
            if (m_StartTime > Time.time)
            {
                m_StartTime = Time.time - AttackDuration - m_CooldownTime;
            }
            if (m_StartTime + AttackDuration + m_CooldownTime > Time.time)
                return false;
            var player = sensors.CurrentPlayerObservation;
            foreach (var position in player.PlayerPositionsLocal)
            {
                if (position.x > 0)
                {
                    return true;
                }
            }

            return false;
        }

        public override void Begin(BossSensors sensors, BossStats stats, HitboxManager hitboxes)
        {
            Debug.Assert(hitboxes.Weapon != null);
            RockBreakEvent.Register(HandleRockBroke);
            m_WaitingForCollision = true;
            m_StartTime = Time.time;
            m_BossAnimator = sensors.GetComponent<Animator>();
            sensors.StartCoroutine(FireAfterWindup(sensors, hitboxes.Weapon));
        }

        IEnumerator FireAfterWindup(BossSensors sensors, ProjectileWeapon rockFlinger)
        {
            // TODO: We should be waiting for some kind of Windup complete event
            yield return new WaitForSeconds(WindupDuration);
            m_BossAnimator.SetTrigger(AnimatorParameters.TriggerMoveFinished);
            // TODO: Remove assumption that there's only one player
            var target = sensors.CurrentPlayerObservation.PlayerPositionsLocal[0] + m_VerticalAimCompensation * Vector2.up;
            yield return new WaitForSeconds(0.15f);
            rockFlinger.FireInDirection(target);
        }

        void HandleRockBroke()
        {
            m_WaitingForCollision = false;
        }
    }
}
