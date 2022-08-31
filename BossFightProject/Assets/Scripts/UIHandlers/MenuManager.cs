using System;
using System.Collections;
using System.Collections.Generic;
using UnityAtoms.BaseAtoms;
using UnityEngine;

namespace BossFight
{
    public class MenuManager : MonoBehaviour
    {
        [SerializeField]
        GameObject m_HideableButtons;
        [SerializeField]
        GameObject m_OptionsObject;

        // Since BackButton is used multiple places, and the behaviour is different both places, we go through
        // an Event atom instead of having the button explicitly call DeactivateOptions here
        [SerializeField]
        VoidEvent m_BackButtonPressed;

        void Awake()
        {
            m_BackButtonPressed.Register(DeactivateOptions);
        }

        void OnDestroy()
        {
            m_BackButtonPressed.Unregister(DeactivateOptions);
        }

        public void ActivateOptions()
        {
            m_HideableButtons.SetActive(false);
            m_OptionsObject.SetActive(true);
        }
        public void DeactivateOptions()
        {
            m_HideableButtons.SetActive(true);
            m_OptionsObject.SetActive(false);
        }
    }
}
