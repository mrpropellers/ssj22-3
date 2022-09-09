using System;
using LeftOut.JamAids;
using UnityEngine;
using UnityEngine.Pool;

namespace Shared
{
    public class DamagePopupFactory : MonoBehaviour
    {
        ObjectPool<DamagePopup> m_DamagePopups;

        [SerializeField]
        float m_ZOffset = -4f;

        [SerializeField]
        BossDamageEventChannel m_BossDamageChannel;

        [SerializeField]
        DamagePopup m_PopupPrefab;

        void Awake()
        {
            m_DamagePopups = new ObjectPool<DamagePopup>(
                Create,
                GenericPoolActions.ActivateAndEnable,
                GenericPoolActions.DeactivateAndDisable);
        }

        void OnEnable()
        {
            m_BossDamageChannel.OnDamage.AddListener(HandleDamageEvent);
        }

        void OnDisable()
        {
            m_BossDamageChannel.OnDamage.RemoveListener(HandleDamageEvent);
        }

        DamagePopup Create()
        {
            var popup = Instantiate(m_PopupPrefab).GetComponent<DamagePopup>();
            popup.Release = m_DamagePopups.Release;
            return popup;
        }

        void HandleDamageEvent(BossDamageEventChannel.DamageInfo info)
        {
            var popup = m_DamagePopups.Get();
            popup.Initialize(info.FeedbackPosition, info.DamageDone, info.Intensity, info.IsCrit, info.FeedbackDirection);
            var pup = popup.transform.position;
            popup.transform.position = new Vector3(pup.x, pup.y, m_ZOffset);
        }
    }
}
