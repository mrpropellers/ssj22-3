using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityAtoms.BaseAtoms;

namespace BossFight
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class ToggleDebugMode : MonoBehaviour
    {
        const string k_DebugText = "debug mode";

        TextMeshProUGUI m_TextMesh;
        string m_OriginalText;

        [SerializeField]
        BoolVariable m_DebugFlag;

        // Start is called before the first frame update
        void Start()
        {
            m_TextMesh = GetComponent<TextMeshProUGUI>();
            m_OriginalText = m_TextMesh.text;
        }

        public void ToggleDebug()
        {
            m_DebugFlag.Value = !m_DebugFlag.Value;
            m_TextMesh.text = m_DebugFlag.Value ? k_DebugText : m_OriginalText;
        }
    }
}
