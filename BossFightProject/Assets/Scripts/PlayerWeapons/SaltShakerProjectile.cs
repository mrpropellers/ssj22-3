using System;
using System.Collections;
using DG.Tweening;
using LeftOut;
using UnityEngine;
using UnityEngine.Pool;
using Random = UnityEngine.Random;

namespace BossFight
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Hurtbox))]
    //[RequireComponent(typeof(SpriteRenderer))]
    public class SaltShakerProjectile : MonoBehaviour
    {
        Rigidbody2D m_Rigidbody;
        ParticleSystem m_Particles;
        Hurtbox m_Hurtbox;
        SpriteRenderer m_Renderer;
        bool m_IsCleaningUp;

        internal float Lifespan;
        internal ObjectPool<SaltShakerProjectile> Pool;

        [SerializeField]
        [Range(0f, 10f)]
        float m_CleanupDelay = 3f;

        [SerializeField]
        [Range(1, 5)]
        int m_NumSpins = 3;

        [SerializeField]
        [Range(0.5f, 2f)]
        float m_SpinPeriodSeconds = 1f;


        float SpinPeriodFudged => m_SpinPeriodSeconds +
            Random.Range(-0.5f, .5f) * m_SpinPeriodSeconds;
        public Rigidbody2D Rigidbody => m_Rigidbody;

        static bool HasDamageable(Collider2D col, out IDamageable damageable)
            => col.TryGetComponent(out damageable);

        void Awake()
        {
            m_Rigidbody = GetComponent<Rigidbody2D>();
            m_Renderer = GetComponentInChildren<SpriteRenderer>();
            m_Particles = GetComponentInChildren<ParticleSystem>();
            m_Hurtbox = GetComponent<Hurtbox>();
            m_Hurtbox.OnDamageProcessed += HandleDamageResult;
            // We can define this in the prefab
            //m_Rigidbody.gravityScale = 0;
        }

        void OnEnable()
        {
            m_IsCleaningUp = false;
            m_Particles.Play();
            m_Hurtbox.Activate();
            m_Renderer.enabled = true;
            //m_Renderer.gameObject.SetActive(true);
            m_Renderer.transform
                .DORotate(
                    Vector3.forward * 360f * m_NumSpins,
                    SpinPeriodFudged * m_NumSpins,
                    RotateMode.FastBeyond360)
                .SetRelative()
                // We just tell it how much to rotate, so no need to set loop
                //.SetLoops(m_NumSpins, LoopType.Restart)
                .SetEase(Ease.OutCirc);
        }

        void OnDisable()
        {
            m_Hurtbox.Deactivate();
        }

        void Update()
        {
            if (m_IsCleaningUp) return;
            Lifespan -= Time.deltaTime;
            if (Lifespan <= 0f)
            {
                StartCleanUp();
            }
        }

        void HandleDamageResult(object _, DamageResult result)
        {
            if (m_IsCleaningUp) return;
            Debug.Assert(result.AttemptWasProcessed,
                "This event should only be invoked if the damage receiver did not ignore the damage attempt");
            StartCleanUp();
        }

        void StartCleanUp()
        {
            m_Particles.Stop(false, ParticleSystemStopBehavior.StopEmitting);
            m_IsCleaningUp = true;
            m_Renderer.enabled = false;
            //m_Renderer.gameObject.SetActive(false);
            if (m_CleanupDelay > 0f)
            {
                StartCoroutine(CleanUpAfter(m_CleanupDelay));
            }
            else
            {
                CleanUp();
            }
        }

        void CleanUp()
        {
            if (Pool != null)
            {
                Pool.Release(this);
            }
            else
            {
                Debug.LogWarning("No pool assigned - forced to destroy");
                Destroy(gameObject);
            }
        }

        IEnumerator CleanUpAfter(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            CleanUp();
        }
    }
}

