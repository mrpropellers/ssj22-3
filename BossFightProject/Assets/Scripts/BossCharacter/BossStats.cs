using System;
using System.Collections.Generic;
using UnityEngine;

namespace BossFight.BossCharacter
{
    [CreateAssetMenu(fileName = "BossStats", menuName = "BossFight/BossStats", order = 0)]
    public class BossStats : ScriptableObject
    {
        [System.Serializable]
        public struct GroundMovement
        {
            [Tooltip("How much ground movement speed is scaled for this boss")]
            [Range(0.1f, 10f)]
            public float SpeedScalar;
        }

        public GroundMovement GroundMovementStats;

        [Min(0f)]
        public float DecisionCooldown = 2f;

        public AnimationCurve JumpTrajectoryCurve;
        [Min(1f)]
        public float JumpAirTime;

        [SerializeField]
        internal List<BossMove> Attacks;

        void Reset()
        {
            GroundMovementStats = new GroundMovement()
            {
                SpeedScalar = 1f
            };

            DecisionCooldown = 2f;
            JumpAirTime = 5f;
        }
    }
}
