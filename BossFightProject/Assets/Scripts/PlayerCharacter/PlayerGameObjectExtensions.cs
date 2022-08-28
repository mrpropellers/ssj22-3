using UnityEngine;

namespace BossFight
{
    static class PlayerGameObjectExtensions
    {
        /// <summary>
        /// Checks up the GameObject hierarchy to see if there is any objects in the tree holding a
        /// PlayerCharacterState component. If there is none - we assume this object doesn't belong to a player
        /// </summary>
        public static bool BelongsToPlayer(this GameObject self, out PlayerCharacter playerCharacter)
        {
            var parent = self.transform.parent;

            return self.TryGetComponent(out playerCharacter)
                || parent != null && BelongsToPlayer(parent.gameObject, out playerCharacter);
        }

        public static bool BelongsToPlayer(this GameObject self) => BelongsToPlayer(self, out _);

        public static bool BelongsToPlayer(this Component self) => self.gameObject.BelongsToPlayer(out _);

        public static bool BelongsToPlayer(this Component self, out PlayerCharacter playerCharacter) =>
            BelongsToPlayer(self.gameObject, out playerCharacter);
    }
}
