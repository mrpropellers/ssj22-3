using System;
using UnityEngine;

namespace BossFight.BossCharacter
{
    [CreateAssetMenu(fileName = "BossStats", menuName = "BossFight/BossStats", order = 0)]
    public class BossStats : ScriptableObject
    {
        [Min(5f)]
        public float GroundAcceleration;
        [Min(10f)]
        public float GroundDeceleration;
        [Min(1f)]
        public float GroundMaxSpeed;

        [Min(0f)]
        public float MinDistanceForCharge;

        public AnimationCurve JumpTrajectoryCurve;
        [Min(1f)]
        public float JumpAirTime;

        public float TimeToMaxSpeed => GroundMaxSpeed / GroundAcceleration;
        public float DistanceToMaxSpeed => 0.5f * GroundMaxSpeed * GroundMaxSpeed / GroundAcceleration;
        public float MaxStoppingTime => GroundMaxSpeed / GroundDeceleration;
        public float MaxStoppingDistance => 0.5f * GroundMaxSpeed * GroundMaxSpeed / GroundDeceleration;

        public float ComputeTimeToReach(float distance)
        {
            // >>> TODO Finish the calc here
            return 5f;
        }

        public float ComputeCurrentPosition(float timeLeft, float totalDistance)
        {
            var totalTime = ComputeTimeToReach(totalDistance);
            var t = Mathf.Clamp01((totalTime - timeLeft) / totalTime);
            return Mathf.Lerp(0, totalDistance, t / ComputeTimeToReach(totalDistance));
        }
    }
}
