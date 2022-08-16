using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace BossFight
{
    public class KillBox : MonoBehaviour
    {
        [SerializeField]
        public Transform RespawnPoint;

        [SerializeField]
        public UnityEvent OnPlayerKilled;

        void Start()
        {
            if (RespawnPoint == null)
            {
                Debug.LogWarning($"No {nameof(RespawnPoint)} set -- using own transform");
                RespawnPoint = transform;
            }
        }

        void OnTriggerEnter2D(Collider2D otherCollider)
        {
            if (otherCollider.BelongsToPlayer())
            {
                OnPlayerKilled?.Invoke();
                otherCollider.transform.SetPositionAndRotation(
                    RespawnPoint.position, RespawnPoint.rotation);
            }
        }
    }
}
