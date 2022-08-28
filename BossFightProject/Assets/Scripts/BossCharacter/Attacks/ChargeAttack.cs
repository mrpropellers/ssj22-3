using System.Linq;
using DG.Tweening;
using UnityEngine;

namespace BossFight.BossCharacter
{
    [CreateAssetMenu(fileName = "ChargeAttack", menuName = "BossFight/ChargeAttack", order = 0)]
    class ChargeAttack : BossMove
    {
        bool m_IsFinished;
        float m_StartingX;
        float m_TotalAttackTime;
        float m_StartTime;

        [SerializeField]
        float m_Speed = 10f;

        [SerializeField]
        [Range(0f, 5f)]
        float m_WindupTime = 1.5f;

        [SerializeField]
        float m_FrontClearanceNeeded = 5f;

        [SerializeField]
        float m_PlayerMinimumDistance = 3f;

        public override string AnimatorTrigger => Constants.AnimatorParameters.TriggerChargeAttack;
        public override bool IsFinished => Time.time - m_StartTime > m_TotalAttackTime;

        public override bool CanStart(BossSensors sensors)
        {
            var arena = sensors.CurrentArenaObservation;
            var player = sensors.CurrentPlayerObservation;
            return arena.DistanceFromFrontWall > m_FrontClearanceNeeded &&
                player.PlayerPositionsLocal.Any(v =>
                    IsPlayerOnGround(v, arena.DistanceFromGround)
                    && v.x > m_PlayerMinimumDistance);
        }

        public override void Begin(BossSensors sensors, BossStats stats, HurtboxManager hurtboxes)
        {
            var totalChargingDistance = sensors.CurrentArenaObservation.DistanceFromFrontWall;
            var movementStats = stats.GroundMovementStats;
            var chargeTime = totalChargingDistance / (m_Speed * movementStats.SpeedScalar);
            var targetX = sensors.transform.position.x + (sensors.Forward2D * totalChargingDistance).x;
            var windup = m_WindupTime / movementStats.SpeedScalar;
            sensors.transform.DOMoveX(targetX, chargeTime).SetEase(Ease.InOutCubic)
                .SetDelay(windup);
            m_TotalAttackTime = windup + chargeTime;
            m_StartTime = Time.time;
            hurtboxes.ActivateAfter(windup);
            Debug.Log($"Charging for {m_TotalAttackTime} to get to {targetX}");
        }
    }
}
