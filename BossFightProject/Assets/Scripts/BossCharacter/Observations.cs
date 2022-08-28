using System;
using System.Collections.Generic;
using UnityEngine;

namespace BossFight.BossCharacter
{
    public struct PlayerObservation
    {
        public int FrameMeasured { get; }
        public int NumInsideBounds { get; }
        // Location in local space coordinates of the players
        public Vector2[] PlayerPositionsLocal { get; }

        internal PlayerObservation(int frame, Vector2[] positionsLocal, int numInsideBounds)
        {
            FrameMeasured = frame;
            NumInsideBounds = numInsideBounds;
            PlayerPositionsLocal = positionsLocal;
        }

        // Need an "empty constructor" for initializing null measurements on game start
        internal PlayerObservation(int frame)
        {
            FrameMeasured = frame;
            NumInsideBounds = int.MinValue;
            PlayerPositionsLocal = Array.Empty<Vector2>();
        }
    }

    public struct ArenaObservation
    {
        public int FrameMeasured { get; }
        public float DistanceFromFrontWall { get; }
        public float DistanceFromBehindWall { get; }
        public float DistanceFromGround { get; }

        internal ArenaObservation(int frame, float frontDistance, float backDistance, float groundDistance)
        {
            FrameMeasured = frame;
            DistanceFromFrontWall = frontDistance;
            DistanceFromBehindWall = backDistance;
            DistanceFromGround = groundDistance;
        }

        // Need an "empty constructor" for initializing null measurements on game start
        internal ArenaObservation(int frame)
        {
            FrameMeasured = frame;
            DistanceFromFrontWall = float.PositiveInfinity;
            DistanceFromBehindWall = float.PositiveInfinity;
            DistanceFromGround = float.PositiveInfinity;
        }
    }
}
