/*
    ------------------- Code Monkey -------------------

    Thank you for downloading this package
    I hope you find it useful in your projects
    If you have any questions let me know
    Cheers!

               unitycodemonkey.com
    --------------------------------------------------
 */

using System;
using UnityEngine;
using TMPro;
using UnityEngine.TerrainUtils;

public class DamagePopup : MonoBehaviour
{
    const float k_DisappearTimerMax = 1f;
    const float k_DefaultStartSpeed = 20f;
    static int s_SortingOrder = 100;
    static readonly Vector2 s_DefaultDirection =new Vector2(.7f, 1).normalized;

    float m_InitialFontSize;
    TextMeshPro textMesh;
    float disappearTimer;
    Color m_TextColor;
    Vector3 moveVector;

    internal Action<DamagePopup> Release;

    static Color StringToColor(string color)
    {
        float HexToDec(string hex) => Convert.ToInt32(hex, 16);

        return new Color(
            HexToDec(color[..2]),
            HexToDec(color[2..4]),
            HexToDec(color[4..6]),
            color.Length >= 8
                ? HexToDec(color[6..8])
                : 1f
            );
    }

    void Awake() {
        textMesh = transform.GetComponent<TextMeshPro>();
        m_InitialFontSize = textMesh.fontSize;
    }

    public void Initialize(
        Vector3 position,
        int damageAmount,
        float intensity,
        bool isCriticalHit,
        Vector2? direction = null)
    {
        var moveDirection = direction ?? s_DefaultDirection;
        var tf = transform;
        tf.position = position;
        tf.localScale = Vector3.one;
        tf.rotation = Quaternion.identity;

        textMesh.SetText(damageAmount.ToString());
        if (isCriticalHit)
        {
            textMesh.fontSize = m_InitialFontSize * 1.25f;
            m_TextColor = Color.red;
        }
        else
        {
            textMesh.fontSize = m_InitialFontSize;
            if (intensity < 0.5f)
            {
                transform.localScale = Vector3.one * 0.66f;
            }
            m_TextColor = intensity > 0.5f ? Color.white : Color.grey;
        }

        // Ensure popup for non-trivial hits always go up
        if (moveDirection.y < 0 && (isCriticalHit || intensity > 0.5f))
        {
            moveDirection.y = -moveDirection.y;
        }

        textMesh.color = m_TextColor;
        disappearTimer = k_DisappearTimerMax;

        s_SortingOrder++;
        textMesh.sortingOrder = s_SortingOrder;

        moveVector = moveDirection * k_DefaultStartSpeed;
    }

    void Update() {
        transform.position += moveVector * Time.deltaTime;
        moveVector -= moveVector * 8f * Time.deltaTime;

        if (disappearTimer > k_DisappearTimerMax * .5f) {
            // First half of the popup lifetime
            float increaseScaleAmount = .05f;
            transform.localScale += Vector3.one * increaseScaleAmount * Time.deltaTime;
        } else {
            // Second half of the popup lifetime
            float decreaseScaleAmount = 1f;
            transform.localScale -= Vector3.one * decreaseScaleAmount * Time.deltaTime;
        }

        disappearTimer -= Time.deltaTime;
        if (disappearTimer < 0) {
            // Start disappearing
            float disappearSpeed = 3f;
            m_TextColor.a -= disappearSpeed * Time.deltaTime;
            textMesh.color = m_TextColor;
            if (m_TextColor.a < 0) {
                Release(this);
            }
        }
    }

}
