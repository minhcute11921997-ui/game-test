// Assets/_Project/Scripts/Combat/GachaManager.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Sprint 7: Hiện hộp thoại Gacha kỹ năng khi Thing lên cấp.
///
/// Cần prefab "GachaPanel" trong Resources/ với cấu trúc:
///   - TextMeshProUGUI  "GachaTitle"
///   - Button           "GachaOption0" (+ TMP label con)
///   - Button           "GachaOption1" (+ TMP label con)
///   - Button           "GachaOption2" (+ TMP label con)
/// </summary>
public class GachaManager : MonoBehaviour
{
    public static GachaManager Instance { get; private set; }

    [SerializeField] GameObject gachaPanelPrefab;

    private bool       _chosen;
    private GameObject _panel;

    void Awake() => Instance = this;

    // ── Public API ────────────────────────────────────────────────
    /// <summary>Hiện Gacha panel và chờ player chọn 1 move.</summary>
    public IEnumerator ShowGacha(ThingData thing, List<MoveData> options)
    {
        if (options == null || options.Count == 0) yield break;

        var prefab = gachaPanelPrefab != null
            ? gachaPanelPrefab
            : Resources.Load<GameObject>("GachaPanel");

        if (prefab == null)
        {
            Debug.LogWarning("[GachaManager] GachaPanel prefab không tìm thấy trong Resources/ — bỏ qua gacha");
            yield break;
        }

        _chosen = false;
        _panel  = Instantiate(prefab);

        // Title
        var title = _panel.transform.Find("GachaTitle")?.GetComponent<TextMeshProUGUI>();
        if (title != null) title.text = $"{thing.thingName} lên Lv.{thing.level}! Chọn kỹ năng:";

        // Option buttons
        for (int i = 0; i < 3; i++)
        {
            int      idx   = i;
            var      btnGo = _panel.transform.Find($"GachaOption{i}");
            if (btnGo == null) continue;

            var btn = btnGo.GetComponent<Button>();
            var lbl = btnGo.GetComponentInChildren<TextMeshProUGUI>();

            if (i < options.Count)
            {
                MoveData move = options[i];
                if (lbl != null)
                    lbl.text = $"{move.moveName}\n[{move.elementType}]  Pwr:{move.basePower}";

                btnGo.gameObject.SetActive(true);
                btn?.onClick.AddListener(() =>
                {
                    LearnMove(thing, move);
                    _chosen = true;
                });
            }
            else
            {
                btnGo.gameObject.SetActive(false);
            }
        }

        yield return new WaitUntil(() => _chosen);
        Destroy(_panel);
    }

    // ── Internal ──────────────────────────────────────────────────
    void LearnMove(ThingData thing, MoveData move)
    {
        if (thing.learnedMoves == null)
            thing.learnedMoves = new List<MoveData>();

        // First 4 slots = equipped; anything beyond 4 = vault (stored but not equipped)
        if (thing.learnedMoves.Count < 4)
            Debug.Log($"[Gacha] {thing.thingName} trang bị {move.moveName} (slot {thing.learnedMoves.Count + 1}/4)");
        else
            Debug.Log($"[Gacha] {thing.thingName} kho đầy — {move.moveName} vào kho chờ (slot {thing.learnedMoves.Count + 1})");

        thing.learnedMoves.Add(move);
    }
}
