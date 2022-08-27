using System.Linq;
using UnityEngine;

namespace BossFight.BossCharacter
{
    [CreateAssetMenu(fileName = "ChargeAttack", menuName = "BossFight/ChargeAttack", order = 0)]
    class ChargeAttack : BossAttack
    {
        bool m_IsFinished;

        [SerializeField]
        float m_FrontClearanceNeeded = 5f;

        [SerializeField]
        float m_PlayerMinimumDistance = 3f;

        public override string AnimatorTrigger => Constants.AnimatorTriggers.ChargeAttack;
        public override bool IsFinished => m_IsFinished;

        public override bool CanStart(GameObject self,
            ArenaObservation arena, PlayerObservation player)
        {
            return arena.DistanceFromFrontWall > m_FrontClearanceNeeded &&
                player.PlayerLocations.Any(v => IsPlayerOnGround(v, arena.DistanceFromGround));
        }

        public override void Begin(GameObject self,
            ArenaObservation arenaObservation, PlayerObservation playerObservation)
        {
        }

        public override void DoUpdate(GameObject self)
        {

        }
    }
}
