using System;
using LeftOut;
using Unity.VisualScripting;
using UnityEngine;

namespace BossFight
{
    [IncludeInSettings(true)]
    public class HurtboxManager : MonoBehaviour
    {
        Hurtbox[] m_Hurtboxes;

        [SerializeField]
        float WindupTime;

        void Awake()
        {
            m_Hurtboxes = GetComponentsInChildren<Hurtbox>();
        }

        public void ActivateAll()
        {
            SetAll(true);
        }

        public void DeactivateAll()
        {
            SetAll(false);
        }

        void SetAll(bool active)
        {
            foreach (var hurtbox in m_Hurtboxes)
            {
                if (active)
                {
                    hurtbox.Activate(WindupTime);
                }
                else
                {
                    hurtbox.Deactivate();
                }
            }
        }
    }
}
