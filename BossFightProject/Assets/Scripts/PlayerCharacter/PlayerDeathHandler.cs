using System;
using UnityAtoms;
using UnityAtoms.BaseAtoms;
using UnityEngine;

namespace BossFight
{
    public class PlayerDeathHandler : MonoBehaviour, IAtomListener<GameObject>
    {
        [SerializeField]
        Transform m_RespawnPoint;

        public GameObjectEventReference PlayerKillStartEvent;
        // This could be changed to RespawnReady and respawn handling could be its own thing
        // - but for now it's fine to handle death and respawn in same class
        public GameObjectEventReference PlayerRespawnedEvent;

        void Start()
        {
            PlayerKillStartEvent.Event.RegisterListener(this);
        }

        public void OnEventRaised(GameObject player)
        {
            var playerStateFound = player.BelongsToPlayer(out var playerState);
            Debug.Assert(playerStateFound, $"Raised an event for {player.name} which didn't have a " +
                $"{nameof(PlayerCharacterStateManager)} attached");
            // TODO: We should do some kind of async await for player death animation and check lives before respawn
            playerState.ResetCharacterState(m_RespawnPoint);
            PlayerRespawnedEvent.Event.Raise(player);
        }
    }
}
