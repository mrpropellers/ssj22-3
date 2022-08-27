using UnityEngine;

namespace BossFight.BossCharacter
{
    abstract class BossAttack : ScriptableObject
    {
        public abstract string AnimatorTrigger { get; }
        public abstract bool IsFinished { get; }
        public abstract void DoUpdate(GameObject self);

        public virtual void Begin(GameObject self,
            ArenaObservation arenaObservation, PlayerObservation playerObservation) {}

        public abstract bool CanStart(GameObject self,
            ArenaObservation arenaObservation, PlayerObservation playerObservation);

        protected static bool IsPlayerOnGround(Vector2 playerLocal, float distanceToGround) =>
            (Mathf.Abs(playerLocal.y - distanceToGround) < 1f);
    }
}
