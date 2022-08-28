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
        public const string GroundName = "Ground";
    }

    public static class LayerMasks
    {
        public static int Walls = LayerMask.GetMask(Layers.WallsName);
        public static int Ground = LayerMask.GetMask(Layers.GroundName);
    }

    public static class AnimatorParameters
    {
        public const string TriggerMoveFinished = "FinishMove";
        public const string TriggerChargeAttack = "Attack_BasicCharge";
        public const string TriggerTurnAround = "TurnAround";
    }
}
