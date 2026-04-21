// Assets/_Project/Scripts/UI/EntityHpBar.cs
using UnityEngine;
using UnityEngine.UI;

public class EntityHpBar : MonoBehaviour
{
    [SerializeField] Image fillImage;

    private Transform _target;

    public void Init(Transform entityTransform)
    {
        _target = entityTransform;
    }

    public void SetHp(int current, int max)
    {
        if (fillImage == null) return;
        float ratio = (float)current / max;
        fillImage.fillAmount = ratio;

        // Đổi màu theo % máu
        fillImage.color = ratio > 0.5f ? Color.green
                        : ratio > 0.25f ? Color.yellow
                        : Color.red;
    }

    void LateUpdate()
    {
        // Luôn đứng trên đầu entity
        if (_target != null)
            transform.position = _target.position + Vector3.up * 0.8f;
    }
}