﻿using BossFight.Constants;
using DG.Tweening;
using UnityAtoms;
using UnityAtoms.BaseAtoms;
using UnityEngine;

namespace BossFight.BossCharacter
{
    [CreateAssetMenu(fileName = "TurnAround", menuName = "BossFight/TurnAround", order = 0)]
    class TurnAround : BossMove, IAtomListener<GameObject>
    {
        bool m_IsWaitingForAnimator;
        bool m_IsFinished;
        float m_TimeReverseStarted = float.NegativeInfinity;
        Animator m_Animator;

        [SerializeField]
        GameObjectEvent m_AnimatorFinishedEvent;

        [SerializeField]
        [Range(0.01f, 1f)]
        float m_WallDistanceThreshold = 0.25f;

        [SerializeField]
        [Range(0.1f, 2f)]
        float m_ReverseTime = 1f;

        bool IsBackingUp => Time.time - m_TimeReverseStarted < m_ReverseTime;

        public override string AnimatorTrigger => AnimatorParameters.TriggerTurnAround;
        public override bool IsFinished => m_IsFinished;
        public override bool CanStart(BossSensors sensors) =>
            sensors.CurrentArenaObservation.DistanceFromFrontWall < m_WallDistanceThreshold;

        // All the logic for the TurnAround move lives in the Animator state - so we just wait for it to finish
        public override void Begin(BossSensors sensors, BossStats stats, HurtboxManager _)
        {
            m_IsWaitingForAnimator = false;
            m_IsFinished = false;
            var arenaObservation = sensors.CurrentArenaObservation;
            var backUpDistance = Mathf.Abs(Mathf.Clamp(
                sensors.DistanceFromBackBounds(
                    sensors.FrontBoundsToPosition(arenaObservation.DistanceFromFrontWall)), int.MinValue, 0f));
            Debug.Log($"Backing up {backUpDistance}");

            var targetX = sensors.transform.position.x - (sensors.Forward2D.x * backUpDistance);
            //sensors.transform.DOMoveX(targetX, m_ReverseTime).SetEase(Ease.Linear);
            m_TimeReverseStarted = Time.time;
            Debug.Log($"Starting to backup at {m_TimeReverseStarted}");
            // We'll need to access the Animator for this maneuver, so store a reference now
            m_Animator = sensors.GetComponent<Animator>();
        }

        public override void DoUpdate()
        {
            base.DoUpdate();
            if (m_IsWaitingForAnimator || m_IsFinished)
            {
                return;
            }

            if (!IsBackingUp)
            {
                Debug.Log($"No longer backing up at {Time.time}");
                m_IsWaitingForAnimator = true;
                m_AnimatorFinishedEvent.RegisterListener(this);
                // This should trigger the TurnAround state which is handled in the Animator's state machine
                m_Animator.SetTrigger(AnimatorParameters.TriggerMoveFinished);
            }
        }

        public void OnEventRaised(GameObject item)
        {
            Debug.Log("Turn around finished");
            Debug.Assert(m_IsWaitingForAnimator);
            m_AnimatorFinishedEvent.UnregisterListener(this);
            m_IsWaitingForAnimator = false;
            m_IsFinished = true;
        }
    }
}
