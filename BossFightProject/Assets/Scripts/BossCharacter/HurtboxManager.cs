using System;
using System.Collections;
using System.Collections.Generic;
using LeftOut;
using LeftOut.GlobalConsts;
using UnityEngine;

namespace BossFight
{
    public class HurtboxManager : MonoBehaviour
    {
        enum State
        {
            InActive,
            Windup,
            Active
        }

        static Color s_WindupBaseColor = Color.red;
        State m_State;
        Hurtbox[] m_Hurtboxes;
        Color m_OriginalBaseColor;
        Color m_OriginalEmissiveColor;
        float m_TimeWindupStarted;
        float m_WindupDuration;

        // Color hurtboxes wind up to
        [SerializeField]
        [ColorUsage(true,true,0f,8f,0.125f,3f)]
        Color m_WindupColor;
        // Color that pulses when hurtboxes are done winding up
        [SerializeField]
        [ColorUsage(true,true,0f,8f,0.125f,3f)]
        Color m_PulseColor;

        [SerializeField]
        float m_PulsePeriod;
        //[SerializeField]
        //float m_ColorSpindownTime;

        [SerializeField]
        List<MeshRenderer> m_Indicators;

        float TimeElapsedFullyActive => Time.time - (m_TimeWindupStarted + m_WindupDuration);

        void Awake()
        {
            m_Hurtboxes = GetComponentsInChildren<Hurtbox>();
            m_State = State.InActive;
            if (m_Indicators.Count > 0)
            {
                m_OriginalBaseColor = m_Indicators[0].material.GetColor(ShaderProperty.MainColorId);
                m_OriginalEmissiveColor = m_Indicators[0].material.GetColor(ShaderProperty.EmissiveColorId);
            }
        }

        void Update()
        {
            switch (m_State)
            {
                case State.InActive:
                    return;
                case State.Windup:
                    DoWindup();
                    break;
                case State.Active:
                    DoActivePulse();
                    break;
            }
        }

        public void ActivateAfter(float seconds)
        {
            if (m_State != State.InActive)
            {
                Debug.LogWarning("Activating hurtboxes when they were already active.");
            }

            m_TimeWindupStarted = Time.time;
            m_WindupDuration = seconds;
            if (Mathf.Approximately(0f, seconds))
            {
                ActivateAll();
            }
            else
            {
                m_State = State.Windup;
            }
        }

        void ActivateAll()
        {
            m_State = State.Active;
            SetHurtboxes(true);
            SetIndicatorColors(s_WindupBaseColor, m_WindupColor);
        }

        void DoWindup()
        {
            if (Time.time - m_TimeWindupStarted >= m_WindupDuration)
            {
                ActivateAll();
                return;
            }
            var t = (Time.time - m_TimeWindupStarted) / m_WindupDuration;
            var baseColor = Color.Lerp(m_OriginalBaseColor, s_WindupBaseColor, t);
            var emissiveColor = Color.Lerp(m_OriginalEmissiveColor, m_WindupColor, t);
            SetIndicatorColors(baseColor, emissiveColor);
        }

        void DoActivePulse()
        {
            var elapsed = TimeElapsedFullyActive;
            Debug.Assert(elapsed >= 0f,
                "Trying to handle Active hurtboxes but TimeElapsed in active status is negative");

            var t = (elapsed % m_PulsePeriod) / m_PulsePeriod;
            // Double-check that the math is sound
            Debug.Assert(t >= 0);
            Debug.Assert(t <= 1f);

            if (t < 0.5f)
            {
                t *= 2f;
                var emissiveColor = Color.Lerp(m_WindupColor, m_PulseColor, t);
                SetIndicatorColors(s_WindupBaseColor, emissiveColor);
            }
            else
            {
                t = (1f - t) * 2f;
                var emissiveColor = Color.Lerp(m_PulseColor, m_WindupColor, t);
                SetIndicatorColors(s_WindupBaseColor, emissiveColor);
            }
        }

        void SetIndicatorColors(Color baseColor, Color emissiveColor)
        {
            var mpb = new MaterialPropertyBlock();
            mpb.SetColor(ShaderProperty.MainColorId, baseColor);
            mpb.SetColor(ShaderProperty.EmissiveColorId, emissiveColor);
            foreach (var indicator in m_Indicators)
            {
                indicator.SetPropertyBlock(mpb);
            }
        }

        public void DeactivateAll()
        {
            SetIndicatorColors(m_OriginalBaseColor, m_OriginalEmissiveColor);
            SetHurtboxes(false);
            m_State = State.InActive;
        }

        void SetHurtboxes(bool active)
        {
            foreach (var hurtbox in m_Hurtboxes)
            {
                if (!hurtbox.isActiveAndEnabled) continue;
                if (active)
                {
                    hurtbox.Activate();
                }
                else
                {
                    hurtbox.Deactivate();
                }
            }
        }
    }
}
