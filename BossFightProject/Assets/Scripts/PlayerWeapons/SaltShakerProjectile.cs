using System;
using System.Collections;
using DG.Tweening;
using LeftOut;
using UnityAtoms.BaseAtoms;
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
        const int k_DoubleBounceBuffer = 30;

        Rigidbody2D m_Rigidbody;
        ParticleSystem m_Particles;
        Hurtbox m_Hurtbox;
        SpriteRenderer m_Renderer;
        bool m_IsCleaningUp;
        int m_NumBounces;
        int m_HurtboxReactivateFrame;

        float m_InitialLifespan;
        internal float Lifespan;
        internal ObjectPool<SaltShakerProjectile> Pool;

        [SerializeField]
        AnimationCurve m_DamageCurve;

        [SerializeField]
        [Range(0f, 10f)]
        float m_CleanupDelay = 3f;

        [SerializeField]
        [Range(1f, 3f)]
        float m_BounceDamageMultiplier = 1.75f;

        [SerializeField]
        [Range(0f, 5f)]
        float m_LifespanReductionOnBounce = 1.5f;

        [SerializeField]
        [Range(1, 5)]
        int m_NumSpins = 3;

        [SerializeField]
        float m_WallUpForce = 10f;

        [SerializeField]
        [Range(0.5f, 2f)]
        float m_SpinPeriodSeconds = 1f;

        [field: SerializeField]
        public VoidEvent OnBounce { get; private set;  }

        [field: SerializeField]
        public VoidEvent OnBreak { get; private set;  }

        float RatioLifespanLeft => Mathf.Clamp(Lifespan, 0f, m_InitialLifespan) / m_InitialLifespan;

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
            m_InitialLifespan = Lifespan;
            m_NumBounces = 0;
            m_Particles.Play();
            m_Hurtbox.Activate();
            m_Hurtbox.DamageAmount = m_DamageCurve.Evaluate(0);
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
        }

        void Update()
        {
            if (m_IsCleaningUp) return;
            if (Time.frameCount >= m_HurtboxReactivateFrame && !m_Hurtbox.IsOn)
            {
                m_Hurtbox.Activate();
            }
            Lifespan -= Time.deltaTime;
            m_Hurtbox.DamageAmount =
                m_DamageCurve.Evaluate(1.0f - RatioLifespanLeft) + m_NumBounces * m_BounceDamageMultiplier;
            // Ensure floating shakers don't stick around forever
            if (Lifespan < -m_InitialLifespan)
            {
                StartCleanUp();
            }
        }

        void HandleDamageResult(object _, DamageResult result)
        {
            if (m_IsCleaningUp) return;
            Debug.Assert(result.AttemptWasProcessed,
                "This event should only be invoked if the damage receiver did not ignore the damage attempt");
            Lifespan -= m_LifespanReductionOnBounce * (float)(result.AmountApplied + 1);
            m_Hurtbox.Deactivate();
            m_HurtboxReactivateFrame = Time.frameCount + k_DoubleBounceBuffer;
            m_NumBounces++;
            if (Lifespan <= 0f)
            {
                StartCleanUp();
            }
            // If we hit a wall, bounce upwards a little
            else if (result.AmountApplied == 0)
            {
                m_Rigidbody.AddForce(Vector2.up * m_WallUpForce);
                OnBounce.Raise();
            }
            else
            {
                OnBounce.Raise();
            }
        }

        void StartCleanUp()
        {
            OnBreak.Raise();
            m_Hurtbox.Deactivate();
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

