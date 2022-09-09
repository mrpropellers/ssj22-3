using System;
using System.Collections;
using LeftOut;
using LeftOut.GameplayManagement;
using LeftOut.GlobalConsts;
using LeftOut.JamAids;
using UnityAtoms.BaseAtoms;
using UnityEngine;

namespace BossFight
{
    [RequireComponent(typeof(DamagePasserOncePerFrame))]
    [RequireComponent(typeof(ForwardProviderSideView))]
    public class PlayerCharacter : MonoBehaviour, ITrackableInstance
    {
        #region Player Stats
        [SerializeField]
        IntConstant StartingHealth;
        [SerializeField]
        FloatConstant PostRespawnFreezeTime;
        [SerializeField]
        FloatConstant PostRespawnInvulnerabilityTime;
        [SerializeField]
        FloatConstant PostDamageInvulnerabilityTime;
        #endregion


        #region Current State

        float m_InvulnerabilityTimeRemaining;
        [SerializeField]
        BoolVariableInstancer PlayerHasControl;
        [SerializeField]
        IntVariableInstancer PlayerHealth;
        #endregion


        #region Global Events

        [SerializeField]
        VoidEvent PlayerHit;
        [SerializeField]
        GameObjectEventReference DeathStartedEvent;
        [SerializeField]
        GameObjectEventReference RespawnStartedEvent;
        #endregion


        #region Properties

        public bool IsInvulnerable => m_InvulnerabilityTimeRemaining > 0f;
        #endregion
        public event OnTrackedInstanceDestroyed OnDestroyed;

        #region Unity Events
        void Awake()
        {
            if (!gameObject.CompareTag(Tags.Player))
            {
                Debug.LogWarning($"{name} is not tagged as {Tags.Player} -- correcting now...");
                gameObject.tag = Tags.Player;
            }

            GetComponent<DamagePasserOncePerFrame>().OnDamageAttempt += HandleIncomingDamage;
            Debug.Log("Registering self to InstanceTracker");
            InstanceTrackingList<PlayerCharacter>.Add(this);
        }

        void Start()
        {
            ResetStats();
        }

        void FixedUpdate()
        {
            if (IsInvulnerable)
            {
                m_InvulnerabilityTimeRemaining -= Time.deltaTime;
            }
        }

        void OnDestroy()
        {
            OnDestroyed?.Invoke(this);
        }
        #endregion

        #region Public Methods
        public void Respawn(Transform teleportTo = null)
        {
            Debug.Log($"{name} has respawned.");
            if (teleportTo != null)
            {
                transform.SetPositionAndRotation(teleportTo.position, teleportTo.rotation);
            }
            ResetStats();
            RespawnStartedEvent.Event.Raise(gameObject);
            m_InvulnerabilityTimeRemaining = PostRespawnInvulnerabilityTime.Value;
            StartCoroutine(ActivateControlAfterSeconds(PostRespawnFreezeTime.Value));
        }

        public void InstantKill(bool forceDeath = false)
        {
            if (IsInvulnerable && !forceDeath)
            {
                return;
            }
            Die();
        }
        #endregion

        #region Private Methods

        IEnumerator ActivateControlAfterSeconds(float time)
        {
            yield return new WaitForSeconds(time);
            PlayerHasControl.Value = true;
        }

        DamageResult HandleIncomingDamage(DamageAttempt damage)
        {
            var damageToApply = IsInvulnerable ? 0 : Mathf.RoundToInt(damage.FinalDamageAmount);
            if (damageToApply > 0)
            {
                PlayerHealth.Value -= damageToApply;
                PlayerHit.Raise();
                if (PlayerHealth.Value <= 0)
                {
                    Die();
                }
                else
                {
                    StartInvulnerabilityPeriod(PostDamageInvulnerabilityTime.Value);
                }
            }

            return new DamageResult(damageToApply);
        }

        void StartInvulnerabilityPeriod(float invulnerabilitySeconds)
        {
            m_InvulnerabilityTimeRemaining = invulnerabilitySeconds;
        }

        void ResetStats()
        {
            PlayerHealth.Value = StartingHealth.Value;

        }

        void Die()
        {
            Debug.Log($"{name} has died.");
            if (PlayerHealth.Value > 0)
            {
                PlayerHealth.Value = 0;
            }
            PlayerHasControl.Value = false;
            // TODO: We may want to decide not to die based on the situation
            DeathStartedEvent.Event.Raise(gameObject);
        }
        #endregion
    }
}
