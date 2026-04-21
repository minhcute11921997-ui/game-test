using UnityEngine;
using UnityEngine.UI;

public class EntityHpBar : MonoBehaviour
{
    [SerializeField] Image fillImage;
    Camera mainCam;
    Transform target;

    public void Init(Transform entityTransform, int maxHp)
    {
        target = entityTransform;
        mainCam = Camera.main;
        SetHp(maxHp, maxHp);
    }

    public void SetHp(int current, int max)
    {
        float ratio = (float)current / max;
        fillImage.fillAmount = ratio;
        fillImage.color = ratio > 0.5f ? Color.green
                        : ratio > 0.25f ? Color.yellow
                        : Color.red;
    }

    void LateUpdate()
    {
        if (target == null) return;
        // Luôn đứng trên đầu entity, nhìn về camera
        transform.position = target.position + Vector3.up * 0.7f;
        transform.forward = mainCam.transform.forward;
    }
}