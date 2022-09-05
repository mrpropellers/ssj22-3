using System;
using LeftOut;
using UnityEngine;

namespace BossFight
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Hurtbox))]
    [RequireComponent(typeof(SpriteRenderer))]
    public class SaltShakerProjectile : MonoBehaviour
    {
        Rigidbody2D m_Rigidbody;
        SpriteRenderer m_Renderer;

        public Action<SaltShakerProjectile> ReturnToPool;

        public Rigidbody2D Rigidbody => m_Rigidbody;

        static bool HasDamageable(Collider2D col, out IDamageable damageable)
            => col.TryGetComponent(out damageable);

        void Awake()
        {
            m_Rigidbody = GetComponent<Rigidbody2D>();
            m_Renderer = GetComponent<SpriteRenderer>();
            var hurtbox = GetComponent<Hurtbox>();
            hurtbox.OnDamageProcessed += HandleDamageResult;
        }

        // We'll be pooling these, so ensure anything we set on "destroy" is unset here
        void OnEnable()
        {
            m_Renderer.enabled = true;
            m_Rigidbody.isKinematic = false;
            m_Rigidbody.gravityScale = 0;
        }

        void HandleDamageResult(object _, DamageResult result)
        {
            Debug.Assert(result.AttemptWasProcessed,
                "This event should only be invoked if the damage receiver did not ignore the damage attempt");

            if (ReturnToPool != null)
            {
                ReturnToPool(this);
            }
            else
            {
                Debug.LogWarning("No ReturnToPool function assigned - forced to destroy");
                Destroy(gameObject);
            }
        }
    }
}

