using UnityEngine;
using TMPro;
using System.Collections;

public class DamagePopup : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI label;

    // ── Public factories ──────────────────────────────────────────────
    public static DamagePopup Create(Vector3 worldPos, int damage, bool isCrit)
    {
        string text  = isCrit ? $"<b>{damage}!</b>" : damage.ToString();
        Color  color = isCrit ? Color.yellow : Color.white;
        return CreateText(worldPos, text, color);
    }

    /// <summary>Sprint 5: né đòn thành công.</summary>
    public static DamagePopup CreateMiss(Vector3 worldPos)
        => CreateText(worldPos, "MISS", new Color(0.75f, 0.75f, 0.75f));

    /// <summary>Sprint 6: thay đổi stage chỉ số.</summary>
    public static DamagePopup CreateStatus(Vector3 worldPos, string text)
        => CreateText(worldPos, text, new Color(0.5f, 1f, 0.5f));

    // ── Internal ──────────────────────────────────────────────────────
    static DamagePopup CreateText(Vector3 worldPos, string text, Color color)
    {
        var prefab = Resources.Load<GameObject>("DamagePopup");
        if (prefab == null) return null;

        var go    = Instantiate(prefab, worldPos + Vector3.up * 0.5f, Quaternion.identity);
        var popup = go.GetComponent<DamagePopup>();
        if (popup != null && popup.label != null)
        {
            popup.label.text  = text;
            popup.label.color = color;
        }
        popup?.StartCoroutine(popup.Animate());
        return popup;
    }

    IEnumerator Animate()
    {
        float  t        = 0f;
        Vector3 startPos = transform.position;
        while (t < 0.8f)
        {
            t                   += Time.deltaTime;
            transform.position   = startPos + Vector3.up * (t * 1.5f);
            float alpha          = 1f - (t / 0.8f);
            if (label != null) label.alpha = alpha;
            yield return null;
        }
        Destroy(gameObject);
    }
}
