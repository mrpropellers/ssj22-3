using UnityEngine;

namespace BossFight.Constants
{
    public static class Tags
    {
        public const string RigTarget = "BossRig";
        public const string Walls = "Level";
    }

    public static class Layers
    {
        public static int Player = LayerMask.NameToLayer("Player");
        public const string WallsName = "Ground";
        public static int Walls = LayerMask.NameToLayer(WallsName);
    }

    public static class LayerMasks
    {
        public static int Walls = LayerMask.GetMask(Layers.WallsName);
    }

    public static class AnimatorParameters
    {
    }
}
