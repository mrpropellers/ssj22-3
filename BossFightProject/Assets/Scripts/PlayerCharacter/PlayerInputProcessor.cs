using System;
using System.Collections;
using DG.Tweening;
using LeftOut.JamAids;
using TarodevController;
using UnityAtoms;
using UnityAtoms.BaseAtoms;
using UnityEngine;

namespace BossFight
{
    public class PlayerInputProcessor : PlayerController, IAtomListener<bool>
    {
        float m_LastAttackPressedTime = float.NegativeInfinity;
        int m_LastAttackPressedFrame = int.MinValue;
        ForwardProviderSideView m_ForwardProvider;
        float m_TimeUntilNextFootstep;
        bool m_InHitstun;

        // NOTE: If this is smaller than attack cooldown could end up in an awkward
        //      situation where we get two attacks off one buffered button press
        [SerializeField]
        [Range(0f, 1f)]
        float m_AttackBufferTime = 0.5f;

        [SerializeField]
        [Range(0.01f, 1f)]
        float m_FootstepPeriod = 0.5f;

        [SerializeField]
        [Range(0f, 2f)]
        float m_OnHitControlLoss = 1f;

        [SerializeField]
        BoolVariableInstancer m_CanProcessInput;

        [SerializeField]
        SaltShakerWeapon m_Weapon;

        [field: SerializeField]
        public VoidEvent OnJumped { get; private set; }

        [field: SerializeField]
        public VoidEvent OnFootstep { get; private set; }
        [field: SerializeField]
        public VoidEvent OnPlayerHit { get; private set; }

        bool AttackIsInBuffer =>
            Time.time - m_LastAttackPressedTime < m_AttackBufferTime
            // Just in case the game is running very slow, always check whether attack was
            // pressed last frame
            || Time.frameCount - m_LastAttackPressedFrame < 2;

        public bool CanProcessInput => m_CanProcessInput.Value;

        protected override void Awake()
        {
            base.Awake();
            m_CanProcessInput.Variable.Changed.RegisterListener(this);
            // Ensure no leftover data from previous editor runs
            m_LastAttackPressedFrame = Time.frameCount - 10;
            m_LastAttackPressedTime = Time.time - m_AttackBufferTime * 2f;
            m_ForwardProvider = GetComponent<ForwardProviderSideView>();
            Jumped += OnJumped.Raise;
            m_TimeUntilNextFootstep = m_FootstepPeriod;
            OnPlayerHit.Register(HandleHit);
        }

        void OnDestroy()
        {
            Jumped -= OnJumped.Raise;
        }

        protected override void HandleHorizontal()
        {
            base.HandleHorizontal();
            if (!_grounded) return;
            var speedFactor = Mathf.Clamp01(Mathf.Abs(_speed.x) / _stats.MaxSpeed);
            m_TimeUntilNextFootstep -= Time.deltaTime * speedFactor;
            if (m_TimeUntilNextFootstep <= 0f)
            {
                OnFootstep.Raise();
                // Correct for footstep sounds getting too far behind due to stutter
                if (m_TimeUntilNextFootstep <= -m_FootstepPeriod * .5f)
                {
                    m_TimeUntilNextFootstep = 0;
                }

                m_TimeUntilNextFootstep += m_FootstepPeriod;
            }

        }

        void HandleHit()
        {
            _rb.velocity = (-m_ForwardProvider.Forward + Vector2.up * 0.5f) * _stats.MaxSpeed;
            StartCoroutine(OnHitControlLoss());
        }

        protected override void HandleAttacking()
        {
            if (!_attackToConsume && !AttackIsInBuffer) return;
            if (m_Weapon.CanFire)
            {
                Debug.Log("Attacking.");
                m_Weapon.Fire();
                // Give weapon kickback if on ground - if in air this messes with controls too much
                if (_grounded)
                {
                    var forward = m_ForwardProvider.Forward;
                    transform.DOBlendableLocalMoveBy(-forward * m_Weapon.Kickback, 0.05f)
                        .SetEase(Ease.OutCirc)
                        .SetRelative();
                    transform.DOBlendablePunchRotation(forward.x * Vector3.forward * 10f, 0.075f)
                        .SetEase(Ease.OutCirc)
                        .SetRelative();
                }
                //_rb.AddForce(-forward * m_Weapon.Kickback, ForceMode2D.Impulse);
                //_rb.AddTorque(-forward.x * m_Weapon.Kickback, ForceMode2D.Impulse);
            }
            else if (_attackToConsume)
            {
                m_LastAttackPressedFrame = Time.frameCount;
                m_LastAttackPressedTime = Time.time;
            }
            _attackToConsume = false;
        }

        public void OnEventRaised(bool playerHasControl)
        {
            // Player input handler gets fully disabled when the player doesn't have control of the character
            if (!playerHasControl)
            {
                Debug.Log("Disabling player control");
            }

            _input.enabled = playerHasControl;
        }

        protected override void FixedUpdate()
        {
            if (m_InHitstun) return;
            if (!m_CanProcessInput.Value)
            {
                // Clear any frameInput data that was gathered if the player doesn't have control
                _frameInput = new FrameInput();
            }
            base.FixedUpdate();
        }

        IEnumerator OnHitControlLoss()
        {
            m_CanProcessInput.Value = false;
            m_InHitstun = true;
            var timeElapsed = 0f;
            while (timeElapsed < m_OnHitControlLoss)
            {
                yield return null;
                timeElapsed += Time.deltaTime;
            }
            m_InHitstun = false;
            timeElapsed = 0f;
            while (timeElapsed < m_OnHitControlLoss)
            {
                yield return null;
                timeElapsed += Time.deltaTime;
            }
            m_CanProcessInput.Value = true;
        }

        bool HasHitCeiling(RaycastHit2D[] hits)
        {
            if (hits.Length == 0)
            {
                Debug.LogWarning("Don't call this function if you haven't hit any colliders");
                return false;
            }

            // If we hit anything that is NOT a one-way platform, we must assume it's solid ceiling...
            foreach (var hit in hits)
            {
                if (!hit.collider.TryGetComponent(out PlatformEffector2D effector) || !effector.useOneWay)
                {
                    return true;
                }
            }

            // ... otherwise it must have been one-way platforms (and we shouldn't kill upward velocity)
            return false;
        }

        protected override void CheckCollisions()
        {
            var position = (Vector2)transform.position;
            var offset = position + _col.offset;

            _groundHitCount = Physics2D.CapsuleCastNonAlloc(offset, _col.size, _col.direction, 0, Vector2.down, _groundHits, _stats.GrounderDistance);
            var ceilingHits = Physics2D.CapsuleCastNonAlloc(offset, _col.size, _col.direction, 0, Vector2.up, _ceilingHits, _stats.GrounderDistance);

            if (ceilingHits > 0 && _speed.y > 0 && HasHitCeiling(_ceilingHits)) _speed.y = 0;

            if (!_grounded && _groundHitCount > 0) {
                _grounded = true;
                _coyoteUsable = true;
                _doubleJumpUsable = true;
                _bufferedJumpUsable = true;
                _endedJumpEarly = false;
                _canDash = true;
                RaiseGroundChanged(true, Mathf.Abs(_speed.y));
            }
            else if (_grounded && _groundHitCount == 0) {
                _grounded = false;
                _frameLeftGrounded = _fixedFrame;
                RaiseGroundChanged(false, 0);
            }
        }

    }
}
