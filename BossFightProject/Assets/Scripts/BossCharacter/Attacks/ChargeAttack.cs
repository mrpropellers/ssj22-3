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
            if (arena.DistanceFromFrontWall < m_FrontClearanceNeeded)
            {
                return false;
            }

            var bounds = sensors.CurrentBounds;
            var player = sensors.CurrentPlayerObservation;
            foreach (var position in player.PlayerPositionsLocal)
            {
                var positionGlobal = sensors.transform.TransformPoint(position);
                if (positionGlobal.y >= bounds.min.y && positionGlobal.y <= bounds.max.y
                    && position.x > m_PlayerMinimumDistance)
                {
                    return true;
                }
            }

            return false;
        }

        public override void Begin(BossSensors sensors, BossStats stats, HitboxManager hitboxes)
        {
            var totalChargingDistance = sensors.CurrentArenaObservation.DistanceFromFrontWall;
            var movementStats = stats.GroundMovementStats;
            var chargeTime = totalChargingDistance / (m_Speed * movementStats.SpeedScalar);
            var target = sensors.Forward2D * totalChargingDistance;
            var windup = m_WindupTime / movementStats.SpeedScalar;
            var numTweeensActive = sensors.transform.DOComplete();
            if (numTweeensActive > 0)
            {
                Debug.LogWarning($"Killed {numTweeensActive} tweens when starting Charge Attack...");
            }
            sensors.transform.DOBlendableMoveBy(target, chargeTime)
                .SetEase(Ease.InOutCubic)
                .SetDelay(windup);
            m_TotalAttackTime = windup + chargeTime;
            m_StartTime = Time.time;
            hitboxes.ActivateAfter(windup);
            //Debug.Log($"Charging for {m_TotalAttackTime} to get to {targetX}");
        }
    }
}
