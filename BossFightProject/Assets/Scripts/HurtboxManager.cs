using System;
using LeftOut;
using UnityEngine;

namespace BossFight
{
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
