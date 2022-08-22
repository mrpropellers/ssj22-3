using System;
using System.Collections.Generic;
using LeftOut;
using Unity.VisualScripting;
using UnityEngine;

namespace BossFight.BossCharacter
{
    /// <summary>
    /// Observes scene and translates observations into parameter settings inside the Boss's animator
    /// </summary>
    [RequireComponent(typeof(Animator))]
    [IncludeInSettings(true)]
    public class BossSensors : MonoBehaviour
    {
        [IncludeInSettings(true)]
        public struct PlayerObservation
        {
            public int FrameMeasured { get; }
            public Vector2 MeanPlayerVector { get; }
            public int NumInsideBounds { get; }

            internal PlayerObservation(int frame, Vector2 playerVector, int numInsideBounds)
            {
                FrameMeasured = frame;
                MeanPlayerVector = playerVector;
                NumInsideBounds = numInsideBounds;
            }
        }

        [IncludeInSettings(true)]
        public struct ArenaObservation
        {
            public int FrameMeasured { get; }
            public float DistanceFromFrontWall { get; }
            public float DistanceFromBehindWall { get; }

            internal ArenaObservation(int frame, float frontDistance, float backDistance)
            {
                FrameMeasured = frame;
                DistanceFromFrontWall = frontDistance;
                DistanceFromBehindWall = backDistance;
            }
        }

        const float k_MaxRaycastDistance = 1000f;

        Collider2D[] m_Colliders;
        List<PlayerCharacter> m_Players;
        Animator m_Animator;
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

        void Start()
        {
            m_Players = InstanceTrackingList<PlayerCharacter>.GetReference();
            m_Animator = GetComponent<Animator>();
            m_Colliders = GetComponentsInChildren<Collider2D>();
            m_LastKnownPlayerState = new PlayerObservation(int.MinValue, Vector2.positiveInfinity, 0);
            m_LastKnownArenaState = new ArenaObservation(
                int.MinValue, float.PositiveInfinity, float.PositiveInfinity);
        }

        void Update()
        {
            m_CurrentBounds = m_Colliders[0].bounds;
            for (var i = 1; i < m_Colliders.Length; ++i)
            {
                m_CurrentBounds.Encapsulate(m_Colliders[i].bounds);
            }
        }

        ArenaObservation GenerateArenaObservation()
        {
            Debug.Assert(Time.frameCount != m_LastKnownArenaState.FrameMeasured,
            "Should only generate at most one observation per frame.");
            var origin = (Vector2)m_CurrentBounds.center;
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
                Debug.LogWarning("Missing a forward raycast - must assume we're on top of a wall.");
                distanceBack = 0f;
            }
            else
            {
                distanceBack = hitBack.distance;
            }

            return new ArenaObservation(Time.frameCount, distanceFront, distanceBack);
        }

        PlayerObservation GeneratePlayerObservation()
        {
            Debug.Assert(Time.frameCount != m_LastKnownPlayerState.FrameMeasured,
            "Should only generate at most one observation per frame.");
            // Create some floats for computing means later
            var numPlayers = (float)m_Players.Count;
            var meanPosition = Vector2.zero;
            var numInBounds = 0;

            foreach (var player in m_Players)
            {
                var playerPosition = player.transform.position;
                if (m_CurrentBounds.Contains(playerPosition))
                {
                    numInBounds++;
                }
                var playerLocal = transform.InverseTransformPoint(playerPosition);

                // Players in front and behind shouldn't cancel each other out, so we take the absolute value along x
                // but, players below shouldn't cause the boss to jump, so no abs on y
                meanPosition +=  new Vector2(Mathf.Abs(playerLocal.x), playerLocal.y);
            }

            meanPosition /= numPlayers;
            return new PlayerObservation(Time.frameCount, meanPosition, numInBounds);
        }
    }
}
