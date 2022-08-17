using System;
using UnityEngine;

namespace BossFight
{
     public class KillBox : MonoBehaviour
     {
         void OnTriggerEnter2D(Collider2D otherCollider)
         {
             if (otherCollider.BelongsToPlayer(out var playerState))
             {
                 playerState.InstantKill(true);
             }
         }
     }
}
