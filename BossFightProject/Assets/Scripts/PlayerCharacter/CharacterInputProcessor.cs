using TarodevController;
using UnityEngine;

namespace BossFight
{
    public class CharacterInputProcessor : PlayerController
    {
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
