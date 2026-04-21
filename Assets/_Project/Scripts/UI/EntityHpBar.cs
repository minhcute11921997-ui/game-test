// Assets/_Project/Scripts/UI/EntityHpBar.cs
using UnityEngine;
using UnityEngine.UI;

public class EntityHpBar : MonoBehaviour
{
    [SerializeField] Image fillImage;

    /// <summary>Sprint 8: Crystal Aura — khung bao quanh HP bar. Kéo Image vào đây.</summary>
    [SerializeField] Image crystalAuraImage;

    private Transform _target;

    public void Init(Transform entityTransform)
    {
        _target = entityTransform;
    }

    public void SetHp(int current, int max)
    {
        if (fillImage == null) return;

        float ratio = (float)current / Mathf.Max(1, max);
        fillImage.fillAmount = ratio;

        // Fill color theo % máu
        fillImage.color = ratio > 0.5f ? Color.green
                        : ratio > 0.25f ? Color.yellow
                        : Color.red;

        // Sprint 8: Crystal Aura — màu cyan khi HP đầy, mờ dần khi HP giảm
        if (crystalAuraImage != null)
        {
            Color aura = new Color(0.3f, 0.9f, 1f, Mathf.Lerp(0.1f, 0.85f, ratio));
            crystalAuraImage.color = aura;
        }
    }

    void LateUpdate()
    {
        if (_target != null)
            transform.position = _target.position + Vector3.up * 0.8f;
    }
}