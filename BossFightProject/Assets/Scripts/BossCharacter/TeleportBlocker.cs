using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BossFight.BossCharacter
{
    /// <summary>
    /// This was an attempt to clobber a nasty bug stemming from DOTween and the Animation system fighting over
    /// who gets to decide where the Slug should be, but this should have been fixed by removing DOTween from
    /// the TurnAroundState class. Leaving this here in case it crops back up
    /// </summary>
    public class TeleportBlocker : MonoBehaviour
    {
        Vector3 m_LastPosition;

        [SerializeField]
        [Range(1,15)]
        float TeleportThreshold = 3f;

        public bool WasTeleportDetected => Mathf.Abs(transform.position.x - m_LastPosition.x) > TeleportThreshold;

        void Start()
        {
            m_LastPosition = transform.position;
        }

        void LateUpdate()
        {
            if (WasTeleportDetected)
            {
                Debug.LogError("Teleport detected! This bug should be fixed!!");
                //transform.position = m_LastPosition;
            }
            m_LastPosition = transform.position;
        }
    }
}
