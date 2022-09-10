using System;
using System.Security.Cryptography;
using LeftOut.Extensions;
using UnityEngine;
using UnityEngine.Pool;

using LeftOut.JamAids;
using UnityAtoms.BaseAtoms;

namespace BossFight
{
    public class ProjectileWeapon : MonoBehaviour
    {
        ObjectPool<Projectile> m_ProjectilePool;
        float m_LastFiredTime;
        IForwardProvider m_ForwardProvider;
        Vector2 m_InitialLocalPosition;

        [SerializeField]
        VoidEvent m_OnFire;

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
        [Range(1f, 20f)]
        float m_ProjectileLifespan = 5f;

        [SerializeField]
        [Range(0, 30)]
        int m_HitboxActivationDelay = 0;

        [SerializeField]
        [Range(0f, 1f)]
        float m_Cooldown = 0.25f;

        Vector2 Forward => m_ForwardProvider.Forward;
        float ForwardSign => Mathf.Sign(Forward.x);

        public bool CanFire => Time.time - m_LastFiredTime >= m_Cooldown;

        void Start()
        {
            m_ProjectilePool = new ObjectPool<Projectile>(
                CreateProjectile,
                actionOnGet: InitializeProjectile,
                actionOnRelease: GenericPoolActions.DeactivateAndDisable
            );
            m_LastFiredTime = float.MinValue;
            m_ForwardProvider = GetComponentInParent<IForwardProvider>();
        }

        void OnValidate()
        {

            if (m_ProjectilePrefab != null &&
                !m_ProjectilePrefab.TryGetComponent(out Projectile _))
            {
                Debug.LogError($"{m_ProjectilePrefab.name} is not a valid prefab because it has no " +
                    $"{nameof(Projectile)} component. Removing.");
                m_ProjectilePrefab = null;
            }
            else if (!m_ProjectilePrefab.IsAssetOnDisk() && m_ProjectilePrefab.activeInHierarchy)
            {
                Debug.LogWarning($"The currently assigned {nameof(m_ProjectilePrefab)} ({m_ProjectilePrefab.name}) " +
                    $"is not a prefab on disk - you may get unexpected behavior.");
                // We must disable it to keep it from destroying itself on Start
                m_ProjectilePrefab.SetActive(false);
            }
        }
        Projectile CreateProjectile()
        {
            var projectile = Instantiate(m_ProjectilePrefab).GetComponent<Projectile>();
            projectile.Pool = m_ProjectilePool;
            return projectile;
        }

        void InitializeProjectile(Projectile projectile)
        {
            projectile.transform.position = transform.position;
            projectile.transform.rotation = transform.rotation;
            projectile.Lifespan = m_ProjectileLifespan;
            projectile.gameObject.SetActive(true);
            projectile.enabled = true;
            if (m_HitboxActivationDelay > 0)
            {
                projectile.DelayActivation(m_HitboxActivationDelay);
            }

        }

        Projectile GetProjectile()
        {
            if (m_ForwardProvider is not SpriteForwardProvider)
            {
                return m_ProjectilePool.Get();
            }
            // NOTE: We need to adjust the transform before calling m_ProjectilePool.Get() to ensure the
            //       projectile initializes on the side we're facing
            var tf = transform;
            var currentLocalPosition = tf.localPosition;
            if (!Mathf.Approximately(Mathf.Sign(currentLocalPosition.x), ForwardSign))
            {
                currentLocalPosition.x *= -1f;
                tf.localPosition = currentLocalPosition;
            }

            return m_ProjectilePool.Get();
        }

        public void Fire()
        {
            var projectile = GetProjectile();
            var trajectory =
                Quaternion.Euler(ForwardSign * m_ProjectileArc * Vector3.forward) * Forward;
            Fire(projectile, trajectory);
        }

        public void FireInDirection(Vector2 direction) => Fire(GetProjectile(),
            transform.TransformDirection(direction));

        void Fire(Projectile projectile, Vector2 trajectory)
        {
            Debug.Assert(CanFire, "Don't call Fire if we can't fire.");

            m_LastFiredTime = Time.time;
            projectile.Rigidbody.velocity = trajectory.normalized * m_ProjectileSpeed;
            m_OnFire.Raise();
        }
    }
}
