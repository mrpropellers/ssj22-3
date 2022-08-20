using UnityEditor.UIElements;
using UnityEngine;

namespace BossFight.Constants
{
    public static class Tags
    {
        public const string RigTarget = "BossRig";
    }

    public static class Layers
    {
        public static int Player = LayerMask.NameToLayer("Player");
    }

    public static class AnimatorParameters
    {
        public static int IsAttacking = Animator.StringToHash("IsAttacking");
        public static int HurtboxActive = Animator.StringToHash("HurtboxActive");
    }
}
