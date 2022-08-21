using Unity.VisualScripting;
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
    }

    [IncludeInSettings(true)]
    public static class AnimatorParameters
    {
        public static int HurtboxActive = Animator.StringToHash("HurtboxActive");
        public static int FacingDirection = Animator.StringToHash("FacingDirection");
        public static int ClosestWallXOffset = Animator.StringToHash("ClosestWallXOffset");
        public static int MeanPlayerYDistance = Animator.StringToHash("PlayerYDistance");
        public static int MeanPlayerXDistance = Animator.StringToHash("PlayerXDistance");
        public static int PlayersBehindRatio = Animator.StringToHash("PlayersBehindRatio");
    }
}
