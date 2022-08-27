using System;
using System.Collections.Generic;
using LeftOut;
using UnityEngine;

namespace BossFight.BossCharacter
{
    public class BossSensors : MonoBehaviour
    {

        const float k_MaxRaycastDistance = 1000f;

        Collider2D[] m_Colliders;
        List<PlayerCharacter> m_Players;
        Bounds m_CurrentBounds;
        PlayerObservation m_LastKnownPlayerState;
        ArenaObservation m_LastKnownArenaState;

        public PlayerObservation CurrentPlayerObservation
        {
            get
            {
                if (m_LastKnownPlayerState.FrameMeasured != Time.frameCount)
                {
                    m_LastKnownPlayerState = GeneratePlayerObservation();
                }

                return m_LastKnownPlayerState;
            }
        }


        public ArenaObservation CurrentArenaObservation
        {
            get
            {
                if (m_LastKnownArenaState.FrameMeasured != Time.frameCount)
                {
                    m_LastKnownArenaState = GenerateArenaObservation();
                }

                return m_LastKnownArenaState;
            }
        }

        public Vector2 Forward2D
        {
            get
            {
                var tf = transform;
                var forward = tf.right;
                forward.Scale(tf.lossyScale);
                return forward;
            }
        }

        void Awake()
        {
            m_Players = InstanceTrackingList<PlayerCharacter>.GetReference();
            m_Colliders = GetComponentsInChildren<Collider2D>();
            m_LastKnownPlayerState = new PlayerObservation(int.MinValue);
            m_LastKnownArenaState = new ArenaObservation(int.MinValue);
        }

        void Update()
        {
            m_CurrentBounds = m_Colliders[0].bounds;
            for (var i = 1; i < m_Colliders.Length; ++i)
            {
                m_CurrentBounds.Encapsulate(m_Colliders[i].bounds);
            }
        }

        void OnDrawGizmosSelected()
        {
            if (!Application.isPlaying)
            {
                return;
            }
            var tf = transform;
            var pos = tf.position;
            var playerObservation = CurrentPlayerObservation;
            Gizmos.color = Color.red;
            foreach (var location in playerObservation.PlayerLocations)
            {
                var center = tf.TransformPoint(location);
                Gizmos.DrawWireSphere(new Vector3(center.x, center.y, pos.z), 1f);
            }

            var arenaObservation = CurrentArenaObservation;

            Gizmos.color = Color.white;
            Gizmos.DrawLine(pos, pos + (Vector3)(Forward2D * arenaObservation.DistanceFromFrontWall));
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(pos, pos - (Vector3)(Forward2D * arenaObservation.DistanceFromBehindWall));
            Gizmos.color = Color.gray;
            Gizmos.DrawLine(pos, pos + (Vector3)(Vector2.down * arenaObservation.DistanceFromGround));
        }

        ArenaObservation GenerateArenaObservation()
        {
            Debug.Assert(Time.frameCount != m_LastKnownArenaState.FrameMeasured,
            "Should only generate at most one observation per frame.");
            var origin = (Vector2)transform.position;
            var forward = Forward2D;
            var hitFront = Physics2D.Raycast(origin, forward, k_MaxRaycastDistance, Constants.LayerMasks.Walls);
            float distanceFront;
            if (hitFront.collider == null)
            {
                Debug.LogWarning("Missing a forward raycast - must assume we're on top of a wall.");
                distanceFront = 0f;
            }
            else
            {
                distanceFront = hitFront.distance;
            }

            var hitBack = Physics2D.Raycast(origin, -forward, k_MaxRaycastDistance, Constants.LayerMasks.Walls);
            float distanceBack;
            if (hitBack.collider == null)
            {
                Debug.LogWarning("Missing a backwards raycast - must assume we're on top of a wall.");
                distanceBack = 0f;
            }
            else
            {
                distanceBack = hitBack.distance;
            }

            var hitGround = Physics2D.Raycast(
                origin, Vector2.down, k_MaxRaycastDistance, Constants.LayerMasks.Ground);
            float distanceGround;
            if (hitGround.collider == null)
            {
                Debug.LogWarning("Missing a ground raycast - did we fall off the edge?");
                distanceGround = 0f;
            }
            else
            {
                distanceGround = hitGround.distance;
            }


            return new ArenaObservation(Time.frameCount, distanceFront, distanceBack, distanceGround);
        }

        PlayerObservation GeneratePlayerObservation()
        {
            Debug.Assert(Time.frameCount != m_LastKnownPlayerState.FrameMeasured,
            "Should only generate at most one observation per frame.");
            // Create some floats for computing means later
            var locations = new Vector2[m_Players.Count];
            var numInBounds = 0;

            for (var i = 0; i < m_Players.Count; ++i)
            {
                var player = m_Players[i];
                var playerPosition = player.transform.position;
                if (m_CurrentBounds.Contains(playerPosition))
                {
                    numInBounds++;
                }
                locations[i] = transform.InverseTransformPoint(playerPosition);
            }

            return new PlayerObservation(Time.frameCount, locations, numInBounds);
        }
    }
}
