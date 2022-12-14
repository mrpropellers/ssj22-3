using System;
using LeftOut;
using UnityAtoms.BaseAtoms;
using UnityEngine;

namespace BossFight.BossCharacter.BossCharacter
{
    [RequireComponent(typeof(DamagePasserOncePerFrame))]
    public class BossDamageReceiver : MonoBehaviour
    {
        DamagePasserOncePerFrame m_Damageable;

        [SerializeField]
        IntConstant m_InitialHealth;

        [SerializeField]
        IntReference m_CurrentHealth;

        [SerializeField]
        [Range(1, 100)]
        int m_DamageClamp = 30;

        void Awake()
        {
            m_Damageable = GetComponent<DamagePasserOncePerFrame>();
            m_Damageable.OnDamageAttempt = HandleDamage;
        }

        void Start()
        {
            m_CurrentHealth.Value = m_InitialHealth.Value;
        }

        DamageResult HandleDamage(DamageAttempt attempt)
        {
            var amount = Math.Clamp((int)attempt.FinalDamageAmount, 0, m_DamageClamp);
            m_CurrentHealth.Value -= amount;
            return new DamageResult(amount);
        }

    }
}
