using System;
using BossFight.Constants;
using UnityEngine;

namespace BossFight
{
    public class MusicSetter : MonoBehaviour
    {
        public bool PlayAfterSet;
        [Min(0f)]
        public float PlayDelay = 0f;
        public AudioClip ClipToSet;

        void Start()
        {
            var player = GameObject.FindWithTag(Tags.MusicPlayer);
            if (player == null)
            {
                Debug.LogWarning($"No GameObject tagged {Tags.MusicPlayer} found - unable to set music", this);
                return;
            }

            var source = player.GetComponent<AudioSource>();
            source.clip = ClipToSet;
            if (PlayAfterSet)
            {
                source.PlayDelayed(PlayDelay);
            }
        }
    }
}
