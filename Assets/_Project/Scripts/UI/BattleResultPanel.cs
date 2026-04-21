// Assets/_Project/Scripts/UI/BattleResultPanel.cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Sprint 8/9: Panel kết quả trận đấu.
///
/// Gắn lên một GameObject trong BattleScene (mặc định inactive).
/// Cần các child TMP và Button (wired qua Inspector hoặc FindChild):
///   - TextMeshProUGUI  "ResultTitle"     — CHIẾN THẮNG! / THẤT BẠI...
///   - TextMeshProUGUI  "ExpText"         — EXP: +xxx
///   - TextMeshProUGUI  "StarsText"       — ★★★
///   - TextMeshProUGUI  "MaterialsText"   — danh sách nguyên liệu
///   - Button           "ContinueButton"  — về Overworld
/// </summary>
public class BattleResultPanel : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI resultTitle;
    [SerializeField] TextMeshProUGUI expText;
    [SerializeField] TextMeshProUGUI starsText;
    [SerializeField] TextMeshProUGUI materialsText;
    [SerializeField] Button          continueButton;

    public bool IsDone { get; private set; }

    void Awake()
    {
        gameObject.SetActive(false);
        continueButton?.onClick.AddListener(() => IsDone = true);
    }

    /// <summary>Hiện panel với thông tin kết quả trận.</summary>
    public void Show(bool isWin, int expGained, int stars, Dictionary<string, int> materials)
    {
        IsDone              = false;
        gameObject.SetActive(true);

        if (resultTitle  != null)
            resultTitle.text  = isWin ? "CHIẾN THẮNG!" : "THẤT BẠI...";

        if (expText != null)
            expText.text = expGained > 0 ? $"EXP: +{expGained}" : "";

        if (starsText != null)
            starsText.text = stars > 0
                ? new string('★', stars) + new string('☆', 3 - Mathf.Min(stars, 3))
                : "";

        if (materialsText != null && materials != null && materials.Count > 0)
        {
            var sb = new System.Text.StringBuilder("Nguyên liệu:\n");
            foreach (var kvp in materials)
                sb.AppendLine($"  {kvp.Key}  ×{kvp.Value}");
            materialsText.text = sb.ToString();
        }
        else if (materialsText != null)
        {
            materialsText.text = "";
        }
    }
}
