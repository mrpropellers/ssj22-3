using System;
using System.Collections;
using UnityAtoms.BaseAtoms;
using UnityEngine;
using Cinemachine;

using BossFight.Constants;

namespace BossFight
{
    public class PlayerDamageFeedback : MonoBehaviour
    {
        const float k_MinCutoffFrequency = 10f;
        const float k_MaxCutoffFrequency = 22000f;

        bool m_IsPlayingFeedback;

        [SerializeField]
        float m_MinFilterCutoff;
        [SerializeField]
        float m_MaxFilterResonance;
        [SerializeField]
        AnimationCurve m_FilterCutoffOverTime;
        [SerializeField]
        AnimationCurve m_TimeScaleOverTime;

        [SerializeField]
        [Range(0f, 4f)]
        float m_EffectDuration;

        AudioLowPassFilter m_AudioFilter;
        [SerializeField]
        CinemachineImpulseSource m_OnHitImpulse;

        [field: SerializeField]
        public VoidEvent OnPlayerDamage;

        void Start()
        {
            OnPlayerDamage.Register(PlayFeedback);
            m_AudioFilter = GameObject.FindWithTag(Tags.MusicPlayer).GetComponent<AudioLowPassFilter>();
        }

        void PlayFeedback()
        {
            Debug.Assert(!m_IsPlayingFeedback,
                "Feedback needs to be over before the next event is raised");
            m_IsPlayingFeedback = true;
            StartCoroutine(DoFeedback());
        }

        IEnumerator DoFeedback()
        {
            var timeElapsed = 0f;
            m_AudioFilter.enabled = true;

            while (timeElapsed < m_EffectDuration)
            {
                var t = Mathf.Clamp01(timeElapsed / m_EffectDuration);
                Time.timeScale = Mathf.Clamp01(m_TimeScaleOverTime.Evaluate(t));
                var filterScalar = Mathf.Clamp01(m_FilterCutoffOverTime.Evaluate(t));
                m_AudioFilter.cutoffFrequency = Mathf.Clamp(
                    m_MinFilterCutoff + k_MaxCutoffFrequency * filterScalar,
                    k_MinCutoffFrequency, k_MaxCutoffFrequency);
                m_AudioFilter.lowpassResonanceQ = Mathf.Clamp(
                    m_MaxFilterResonance * (1f - filterScalar), 1f, m_MaxFilterResonance);
                yield return null;
                timeElapsed += Time.unscaledDeltaTime;
            }

            m_AudioFilter.enabled = false;
            Time.timeScale = 1f;
            m_IsPlayingFeedback = false;
        }
    }
}
