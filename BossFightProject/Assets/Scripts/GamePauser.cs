using System;
using System.Collections;
using System.Collections.Generic;
using UnityAtoms.BaseAtoms;
using UnityEngine;

namespace BossFight
{
    public class GamePauser : MonoBehaviour
    {
        bool m_IsPaused;
        float m_OriginalTimeScale;

        [SerializeField]
        GameObject m_PauseMenu;

        [SerializeField]
        VoidEvent m_PauseEvent;

        void Start()
        {
            // This happens when restart is clicked in Editor -- not sure if it would also happen in Player
            if (!m_IsPaused && Mathf.Approximately(Time.timeScale, 0f))
            {
                Debug.LogWarning("Assuming we restarted -- setting timeScale to 1");
                Time.timeScale = 1f;
            }

            m_PauseEvent.Register(HandlePause);
        }

        void OnDestroy()
        {
            m_PauseEvent.Unregister(HandlePause);
        }

        void HandlePause()
        {
            m_IsPaused = !m_IsPaused;

            if (m_IsPaused)
            {
                m_OriginalTimeScale = Time.timeScale;
            }
            m_PauseMenu.SetActive(m_IsPaused);
            Time.timeScale = m_IsPaused ? 0f : m_OriginalTimeScale;
        }
    }
}
