using UnityEngine;

namespace BossFight
{
    public static class GameplayHelpers
    {
        /// <summary>
        /// Checks up the GameObject hierarchy to see if there is any objects in the tree holding a
        /// PlayerCharacterState component. If there is none - we assume this object doesn't belong to a player
        /// </summary>
        public static bool BelongsToPlayer(this GameObject self, out PlayerCharacterStateManager playerCharacterStateManager)
        {
            var parent = self.transform.parent;

            return self.TryGetComponent(out playerCharacterStateManager)
                || parent != null && BelongsToPlayer(parent.gameObject, out playerCharacterStateManager);
        }

        public static bool BelongsToPlayer(this GameObject self) => BelongsToPlayer(self, out _);

        public static bool BelongsToPlayer(this Component self) => self.gameObject.BelongsToPlayer(out _);

        public static bool BelongsToPlayer(this Component self, out PlayerCharacterStateManager playerCharacterStateManager) =>
            BelongsToPlayer(self.gameObject, out playerCharacterStateManager);
    }
}
