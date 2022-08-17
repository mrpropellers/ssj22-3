using System;
using LeftOut.GlobalConsts;
using UnityAtoms.BaseAtoms;
using UnityEngine;

namespace BossFight
{
    public class PlayerCharacterStateManager : MonoBehaviour
    {
        [SerializeField]
        IntConstant m_StartingHealth;

        [field: SerializeField]
        IntVariableInstancer PlayerHealth { get; set; }

        public GameObjectEventReference DeathStartedEvent;

        void Awake()
        {
            if (!gameObject.CompareTag(Tags.Player))
            {
                Debug.LogWarning($"{name} is not tagged as {Tags.Player} -- correcting now...");
                gameObject.tag = Tags.Player;
            }
        }

        public void ResetCharacterState(Transform teleportTo = null)
        {
            PlayerHealth.Value = m_StartingHealth.Value;
            if (teleportTo != null)
            {
                transform.SetPositionAndRotation(teleportTo.position, teleportTo.rotation);
            }
        }

        public void InstantKill(bool forceDeath = false)
        {
            Die();
        }

        void Die()
        {
            // TODO: We may want to decide not to die based on the situation
            DeathStartedEvent.Event.Raise(gameObject);
        }
    }
}
