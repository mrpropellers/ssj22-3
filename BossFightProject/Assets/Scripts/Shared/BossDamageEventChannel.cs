using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu]
public class BossDamageEventChannel : ScriptableObject
{
    public struct DamageInfo
    {
        public int DamageDone { get; private set; }
        public Vector3 FeedbackPosition { get; private set; }
        public Vector2 FeedbackDirection { get; private set; }
        public float Intensity { get; private set; }
        public bool IsCrit { get; private set; }

        public DamageInfo(Vector3 position, int damage, Vector2 direction, float intensity, bool isCrit)
        {
            FeedbackPosition = position;
            DamageDone = damage;
            FeedbackDirection = direction;
            Intensity = intensity;
            IsCrit = isCrit;
        }
    }

    [field: SerializeField]
    // DamageDone, Direction, RatioToMax, IsACrit
    public UnityEvent<DamageInfo> OnDamage { get; private set; }
}
