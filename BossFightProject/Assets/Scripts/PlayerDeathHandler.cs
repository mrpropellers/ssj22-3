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

        void Start()
        {
            PlayerKillStartEvent.Event.RegisterListener(this);
        }

        public void OnEventRaised(GameObject player)
        {
            var playerStateFound = player.BelongsToPlayer(out var playerState);
            Debug.Assert(playerStateFound, $"Raised an event for {player.name} which didn't have a " +
                $"{nameof(PlayerCharacter)} attached");
            // TODO: We should do some kind of async await for player death animation and check lives before respawn
            playerState.Respawn(m_RespawnPoint);
        }
    }
}
