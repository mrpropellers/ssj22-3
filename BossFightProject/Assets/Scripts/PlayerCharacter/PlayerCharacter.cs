using System;
using System.Collections;
using LeftOut;
using LeftOut.GameplayManagement;
using LeftOut.GlobalConsts;
using Unity.VisualScripting;
using UnityAtoms.BaseAtoms;
using UnityEngine;

namespace BossFight
{
    [IncludeInSettings(true)]
    [RequireComponent(typeof(Damageable))]
    public class PlayerCharacter : MonoBehaviour, ITrackableInstance
    {
        #region Player Stats

        [SerializeField]
        IntConstant StartingHealth;
        [SerializeField]
        FloatConstant PostRespawnFreezeTime;
        [SerializeField]
        FloatConstant PostRespawnInvulnerabilityTime;
        // TODO: Replace with a different value
        FloatConstant PostDamageInvulnerabilityTime => PostRespawnInvulnerabilityTime;
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

            GetComponent<Damageable>().OnDamageReceived += HandleIncomingDamage;
        }

        void Start()
        {
            InstanceTrackingList<PlayerCharacter>.Add(this);
            ResetStats();
        }

        void FixedUpdate()
        {
            if (IsInvulnerable)
            {
                m_InvulnerabilityTimeRemaining -= Time.deltaTime;
            }
        }

        void OnValidate()
        {
            if (PostRespawnInvulnerabilityTime.Value < PostRespawnFreezeTime.Value)
            {
                Debug.LogWarning("Invulnerability time must at least be as long as the freeze time");
                PostRespawnInvulnerabilityTime.Value = PostRespawnFreezeTime.Value;
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

        void HandleIncomingDamage(object _, Damageable.DamageEventArgs damageArgs)
        {
            if (!IsInvulnerable)
            {
                PlayerHealth.Value -= damageArgs.Amount;
                m_InvulnerabilityTimeRemaining = PostDamageInvulnerabilityTime.Value;
            }
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
