using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

using LeftOut.JamAids;

namespace BossFight
{
    public class SaltShakerWeapon : MonoBehaviour
    {
        ObjectPool<SaltShakerProjectile> m_ProjectilePool;
        float m_LastFiredTime;

        [SerializeField]
        GameObject m_ProjectilePrefab;

        [SerializeField]
        [Range(0.1f, 10f)]
        float m_ProjectileSpeed;

        [SerializeField]
        [Range(0f, 0.5f)]
        float m_Cooldown;

        public bool CanFire => Time.time - m_LastFiredTime >= m_Cooldown;

        void Start()
        {
            m_ProjectilePool = new ObjectPool<SaltShakerProjectile>(
                CreateProjectile,
                actionOnGet: GenericPoolActions.ActivateAndEnable,
                actionOnRelease: GenericPoolActions.DeactivateAndDisable
            );
            m_LastFiredTime = float.MinValue;
        }

        static SaltShakerProjectile CreateProjectile()
        {
            var projectile = new GameObject("Salt Projectile");
            return projectile.AddComponent<SaltShakerProjectile>();
        }

        void OnValidate()
        {
            if (m_ProjectilePrefab != null &&
                !m_ProjectilePrefab.TryGetComponent(out SaltShakerProjectile _))
            {
                Debug.LogError($"{m_ProjectilePrefab.name} is not a valid prefab because it has no " +
                    $"{nameof(SaltShakerProjectile)} component. Removing.");
                m_ProjectilePrefab = null;
            }
        }

        public void Fire()
        {
            Debug.Assert(CanFire);
            m_LastFiredTime = Time.time;
            var projectile = m_ProjectilePool.Get();
            projectile.Rigidbody.velocity = transform.forward * m_ProjectileSpeed;
        }
    }
}
