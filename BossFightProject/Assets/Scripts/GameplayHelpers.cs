using UnityEngine;
using LeftOut.GlobalConsts;

namespace BossFight
{
    public static class GameplayHelpers
    {
        // TODO: This could traverse all child objects, if that's useful...
        public static bool BelongsToPlayer(this GameObject self) => self.CompareTag(Tags.Player);

        public static bool BelongsToPlayer(this Component self) => self.gameObject.BelongsToPlayer();
    }
}
