using System.Collections;
using UnityEngine;

namespace BossFight.BossCharacter
{
    public abstract class BossMove : ScriptableObject
    {
        public abstract string AnimatorTrigger { get; }
        public abstract bool IsFinished { get; }
        public virtual void DoUpdate() { }

        public virtual void Begin(BossSensors sensors, BossStats stats, HurtboxManager hurtboxes) {}

        public abstract bool CanStart(BossSensors sensors);

        protected static bool IsPlayerOnGround(Vector2 playerLocal, float distanceToGround) =>
            (Mathf.Abs(playerLocal.y - distanceToGround) < 1f);

    }
}
