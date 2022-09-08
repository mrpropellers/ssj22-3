using System.Collections;
using System.Collections.Generic;
using UnityAtoms.BaseAtoms;
using UnityEngine;

namespace BossFight.BossCharacter
{
    public class BossAnimationEventBroadcaster : MonoBehaviour
    {
        [field: SerializeField] public VoidEvent TurnAroundEvent { get; private set; }
        [field: SerializeField] public VoidEvent IdleEvent { get; private set; }
        [field: SerializeField] public VoidEvent BigStepEvent { get; private set; }
        [field: SerializeField] public VoidEvent LittleStepEvent { get; private set; }
        [field: SerializeField] public VoidEvent AlternatingStepEvent { get; private set; }


        public void PlayIdle() => IdleEvent.Raise();
        public void PlayTurnAround() => TurnAroundEvent.Raise();
        public void PlayBigStep() => BigStepEvent.Raise();
        public void PlayLittleStep() => LittleStepEvent.Raise();
        public void PlayAlternatingStep() => AlternatingStepEvent.Raise();
    }
}
