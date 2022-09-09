using System;
using System.Collections;
using DG.Tweening;
using LeftOut;
using UnityAtoms.BaseAtoms;
using UnityEngine;
using TMPro;
using UnityEngine.Pool;
using Random = UnityEngine.Random;

namespace BossFight
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Hitbox))]
    //[RequireComponent(typeof(SpriteRenderer))]
    public class SaltShakerProjectile : MonoBehaviour
    {
        [SerializeField]
        BoolVariable m_IsDebugMode;

        const int k_DoubleBounceBuffer = 10;
        const int k_CritThreshold = 15;

        Rigidbody2D m_Rigidbody;
        ParticleSystem m_Particles;
        Hitbox m_Hitbox;
        SpriteRenderer m_Renderer;
        TextMeshPro m_TextMesh;
        bool m_IsCleaningUp;
        int m_NumBounces;
        int m_HurtboxReactivateFrame;
        int m_MaxHit = int.MinValue;
        Vector2 m_LastTravelDirectionEven;
        Vector2 m_LastTravelDirectionOdd;

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

        [field: SerializeField]
        public BossDamageEventChannel OnDamageChannel { get; private set; }

        float RatioLifespanLeft =>
            Mathf.Clamp(Lifespan, 0f, m_InitialLifespan) / m_InitialLifespan;

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
            m_Hitbox = GetComponent<Hitbox>();
            m_Hitbox.OnDamageProcessed += HandleDamageResult;
            m_TextMesh = GetComponent<TextMeshPro>();
            m_IsDebugMode.Changed.Register(SetDebugMode);

            // We can define this in the prefab
            //m_Rigidbody.gravityScale = 0;
        }

        void OnEnable()
        {
            m_IsCleaningUp = false;
            m_InitialLifespan = Lifespan;
            m_NumBounces = 0;
            m_Particles.Play();
            m_Hitbox.Activate();
            m_Hitbox.DamageMultiplier = m_DamageCurve.Evaluate(0);
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
            SetDebugMode(m_IsDebugMode.Value);
        }

        void OnDisable()
        {
        }

        void SetDebugMode(bool value)
        {
            m_TextMesh.enabled = value;
            m_Renderer.enabled = !value;
            switch (value)
            {
                case true when m_IsCleaningUp:
                    m_TextMesh.color = Color.grey;
                    break;
                case true when !m_IsCleaningUp:
                    m_TextMesh.color = Color.white;
                    break;
                case false when m_IsCleaningUp:
                    m_Particles.Stop();
                    break;
                case false when !m_IsCleaningUp:
                    m_Particles.Play();
                    break;
            }

        }

        void Update()
        {
            if (m_IsCleaningUp) return;
            // Keep rolling history of last two travel directions -- we want to get the n-1th for the damage
            // number direction to account for physics updating before we have a chance to resolve collision
            if (Time.frameCount % 2 == 0)
                m_LastTravelDirectionEven = m_Rigidbody.velocity.normalized;
            else
                m_LastTravelDirectionOdd = m_Rigidbody.velocity.normalized;
            if (Time.frameCount >= m_HurtboxReactivateFrame && !m_Hitbox.IsOn)
            {
                m_Hitbox.Activate();
            }
            Lifespan -= Time.deltaTime;
            m_Hitbox.DamageMultiplier =
                m_DamageCurve.Evaluate(1.0f - RatioLifespanLeft) + m_NumBounces * m_BounceDamageMultiplier;
            if (m_IsDebugMode)
                m_TextMesh.text = m_Hitbox.CurrentDamage.ToString();
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
            Lifespan -= m_LifespanReductionOnBounce * (float)(result.AmountApplied);
            m_Hitbox.Deactivate();
            m_HurtboxReactivateFrame = Time.frameCount + k_DoubleBounceBuffer;
            m_NumBounces++;
            m_MaxHit = Math.Max(m_MaxHit, result.AmountApplied);
            if (result.AmountApplied > 0)
            {
                var isBounce = Lifespan > 0f;
                Debug.Assert(result.AmountApplied < k_CritThreshold || !isBounce,
                    "Bounces should never go above crit threshold");
                var isCrit = !isBounce && result.AmountApplied > k_CritThreshold;
                // Treat all bounces as intensity 0 events
                var intensity = isBounce ? 0f : 1f;
                // If we're currently in an even frame, get the last odd travel direction, and vice versa
                var direction = Time.frameCount % 2 == 0
                    ? m_LastTravelDirectionOdd :
                    m_LastTravelDirectionEven;
                OnDamageChannel.OnDamage?.Invoke(new BossDamageEventChannel.DamageInfo(
                    transform.position, result.AmountApplied, direction, intensity, isCrit));

            }
            if (Lifespan <= 0f)
            {
                StartCleanUp();
            }
            // If we hit a wall, bounce upwards a little
            else if (result.AmountApplied == 0)
            {
                m_Rigidbody.AddForce(Vector2.down * m_WallUpForce);
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
            m_Hitbox.Deactivate();
            m_Particles.Stop(false, ParticleSystemStopBehavior.StopEmitting);
            m_IsCleaningUp = true;
            m_Renderer.enabled = false;
            if (m_IsDebugMode.Value)
            {
                m_TextMesh.color = Color.grey;
            }
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

