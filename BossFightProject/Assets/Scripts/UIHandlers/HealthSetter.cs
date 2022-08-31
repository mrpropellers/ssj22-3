using System;
using TMPro;
using UnityAtoms;
using UnityAtoms.BaseAtoms;
using UnityEngine;

namespace BossFight
{
    /// <summary>
    /// Receive events from the IntEventAtom, which invokes WriteHealthToGUI every time health changes
    /// </summary>
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class HealthSetter : MonoBehaviour, IAtomListener<int>
    {
        int m_InitialHealth;
        TextMeshProUGUI m_GUI;

        [SerializeField]
        IntReference m_HealthAtom;
        protected virtual void Awake()
        {
            m_GUI = GetComponent<TextMeshProUGUI>();
            m_HealthAtom.GetEvent<IntEvent>().RegisterListener(this);
        }

        void Start()
        {
            // We'll store the value of initial health just in case someone wants to extend this script
            m_InitialHealth = m_HealthAtom.Value;
            WriteHealthToGUI(m_HealthAtom.Value);
        }

        // This is called any time the HealthAtom's value changes
        public void OnEventRaised(int value)
        {
            WriteHealthToGUI(value);
        }

        void WriteHealthToGUI(int value)
        {
            m_GUI.text = value.ToString();
        }

    }
}
