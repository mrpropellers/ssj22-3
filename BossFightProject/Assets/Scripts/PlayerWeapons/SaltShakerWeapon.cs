using System;
using System.Security.Cryptography;
using LeftOut.Extensions;
using UnityEngine;
using UnityEngine.Pool;

using LeftOut.JamAids;

namespace BossFight
{
    public class SaltShakerWeapon : MonoBehaviour
    {
        ObjectPool<SaltShakerProjectile> m_ProjectilePool;
        float m_LastFiredTime;
        ForwardProviderSideView m_ForwardProvider;
        Vector2 m_InitialLocalPosition;

        [SerializeField]
        GameObject m_ProjectilePrefab;

        [field: SerializeField]
        public float Kickback { get; private set; } = 1f;

        [SerializeField]
        [Range(0f, 90f)]
        float m_ProjectileArc = 35f;

        [SerializeField]
        [Range(0.1f, 50f)]
        float m_ProjectileSpeed;

        [SerializeField]
        [Range(2f, 20f)]
        float m_ProjectileLifespan = 5f;

        [SerializeField]
        [Range(0f, 0.5f)]
        float m_Cooldown;

        Vector2 Forward => m_ForwardProvider.Forward;

        public bool CanFire => Time.time - m_LastFiredTime >= m_Cooldown;

        void Start()
        {
            m_ProjectilePool = new ObjectPool<SaltShakerProjectile>(
                CreateProjectile,
                actionOnGet: InitializeProjectile,
                actionOnRelease: GenericPoolActions.DeactivateAndDisable
            );
            m_LastFiredTime = float.MinValue;
            m_ForwardProvider = GetComponentInParent<ForwardProviderSideView>();
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
            else if (!m_ProjectilePrefab.IsAssetOnDisk())
            {
                Debug.LogWarning($"The currently assigned {nameof(m_ProjectilePrefab)} ({m_ProjectilePrefab.name}) " +
                    $"is not a prefab on disk - you may get unexpected behavior.");
                // We must disable it to keep it from destroying itself on Start
                m_ProjectilePrefab.SetActive(false);
            }
        }
        SaltShakerProjectile CreateProjectile()
        {
            var projectile = Instantiate(m_ProjectilePrefab).GetComponent<SaltShakerProjectile>();
            projectile.Pool = m_ProjectilePool;
            return projectile;
        }

        void InitializeProjectile(SaltShakerProjectile projectile)
        {
            projectile.transform.position = transform.position;
            projectile.transform.rotation = transform.rotation;
            projectile.Lifespan = m_ProjectileLifespan;
            projectile.gameObject.SetActive(true);
            projectile.enabled = true;
        }

        public void Fire()
        {
            Debug.Assert(CanFire, "Don't call Fire if we can't fire.");

            // NOTE: We need to adjust the transform before calling m_ProjectilePool.Get() to ensure the
            //       projectile initializes on the side we're facing
            // We do it here instead of in Initialize because we don't want to call Forward twice every time
            var tf = transform;
            var spriteForward = Forward;
            var forwardSign = Mathf.Sign(spriteForward.x);
            var currentLocalPosition = tf.localPosition;
            if (!Mathf.Approximately(Mathf.Sign(currentLocalPosition.x), forwardSign))
            {
                currentLocalPosition.x *= -1f;
                tf.localPosition = currentLocalPosition;
            }

            m_LastFiredTime = Time.time;
            var projectile = m_ProjectilePool.Get();
            var trajectory =
                Quaternion.Euler(forwardSign * m_ProjectileArc * Vector3.forward) * spriteForward;
            projectile.Rigidbody.velocity = trajectory * m_ProjectileSpeed;
        }
    }
}
