using UnityEngine;
using TMPro;
using System.Collections;

public class DamagePopup : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI label;

    public static DamagePopup Create(Vector3 worldPos, int damage, bool isCrit)
    {
        var prefab = Resources.Load<GameObject>("DamagePopup");
        var go = Instantiate(prefab, worldPos + Vector3.up * 0.5f, Quaternion.identity);
        var popup = go.GetComponent<DamagePopup>();
        popup.label.text = isCrit ? $"<b>{damage}!</b>" : damage.ToString();
        popup.label.color = isCrit ? Color.yellow : Color.white;
        popup.StartCoroutine(popup.Animate());
        return popup;
    }

    IEnumerator Animate()
    {
        float t = 0f;
        Vector3 startPos = transform.position;
        while (t < 0.8f)
        {
            t += Time.deltaTime;
            transform.position = startPos + Vector3.up * (t * 1.5f);
            float alpha = 1f - (t / 0.8f);
            label.alpha = alpha;
            yield return null;
        }
        Destroy(gameObject);
    }
}
