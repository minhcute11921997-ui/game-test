using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class MoveButtonUI : MonoBehaviour
{
    [Header("Header (PanelTop)")]
    [SerializeField] private Image panelTop;
    [SerializeField] private Image iconBG;
    [SerializeField] private Image iconElement;
    [SerializeField] private TextMeshProUGUI textCategoryTag;
    [SerializeField] private TextMeshProUGUI textMoveName;

    [Header("Bottom Stats")]
    [SerializeField] private TextMeshProUGUI textPower;
    [SerializeField] private TextMeshProUGUI textPPValue;
    [SerializeField] private TextMeshProUGUI textPPMax;
    [SerializeField] private Image cellPowerBorder;
    [SerializeField] private Image cellPPBorder;

    [Header("Button")]
    [SerializeField] private Button button;

    [Header("Icon Sprites — kéo vào theo thứ tự ElementType")]
    [SerializeField] private Sprite[] elementIcons;

    private MoveData _moveData;
    private Action<MoveData> _callback;

    /// <summary>
    /// Setup button. currentPP = PP hiện tại (sau khi dùng), maxPP = PP tối đa của move.
    /// Nếu không truyền currentPP thì mặc định dùng move.pp (full).
    /// </summary>
    public void Setup(MoveData move, Action<MoveData> onClick, int currentPP = -1)
    {
        _moveData = move;
        _callback = onClick;

        int maxPP = move.pp;
        int dispPP = currentPP < 0 ? maxPP : currentPP;

        Color catColor = GetCategoryColor(move);
        Color accentColor = GetAccentColor(move);

        panelTop.color = catColor;
        iconBG.color = accentColor;

        // Icon hệ
        int idx = (int)move.elementType;
        if (elementIcons != null && idx >= 0 && idx < elementIcons.Length && elementIcons[idx] != null)
            iconElement.sprite = elementIcons[idx];

        textCategoryTag.text = GetCategoryTag(move).ToUpper();
        textCategoryTag.color = new Color(1f, 1f, 1f, 0.65f);
        textMoveName.text = move.moveName.ToUpper();

        bool hasPower = move.category == MoveCategory.Physical
                     || move.category == MoveCategory.Special;

        textPower.text = hasPower ? move.basePower.ToString() : "—";
        textPower.color = Color.white;

        // PP: đổi màu đỏ nếu còn ≤ 25%
        textPPValue.text = dispPP.ToString();
        textPPMax.text = $"/ {maxPP}";
        bool ppLow = maxPP > 0 && (float)dispPP / maxPP <= 0.25f;
        textPPValue.color = ppLow ? new Color(1f, 0.3f, 0.3f) : Color.white;
        textPPMax.color = new Color(1f, 1f, 1f, 0.55f);

        button.interactable = dispPP > 0;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => _callback?.Invoke(_moveData));
    }

    // ── Màu NỀN PanelTop theo category ───────────────────────────
    public static Color GetCategoryColor(MoveData move) => move.category switch
    {
        MoveCategory.Physical => new Color(0.78f, 0.38f, 0.05f),
        MoveCategory.Special => new Color(0.40f, 0.10f, 0.60f),
        MoveCategory.Status => move.statusSubType switch
        {
            StatusSubType.Buff => new Color(0.10f, 0.50f, 0.18f),
            StatusSubType.Debuff => new Color(0.38f, 0.38f, 0.42f),
            StatusSubType.Heal => new Color(0.72f, 0.08f, 0.08f),
            _ => new Color(0.35f, 0.35f, 0.38f),
        },
        MoveCategory.Weather => new Color(0.05f, 0.32f, 0.58f),
        MoveCategory.Terrain => new Color(0.22f, 0.42f, 0.10f),
        _ => new Color(0.25f, 0.25f, 0.28f),
    };

    // ── Màu ACCENT ────────────────────────────────────────────────
    static Color GetAccentColor(MoveData move) => move.category switch
    {
        MoveCategory.Physical => new Color(1.00f, 0.58f, 0.15f),
        MoveCategory.Special => new Color(0.72f, 0.45f, 1.00f),
        MoveCategory.Status => move.statusSubType switch
        {
            StatusSubType.Buff => new Color(0.30f, 0.95f, 0.45f),
            StatusSubType.Debuff => new Color(0.85f, 0.85f, 0.90f),
            StatusSubType.Heal => new Color(1.00f, 0.35f, 0.35f),
            _ => new Color(0.80f, 0.80f, 0.85f),
        },
        MoveCategory.Weather => new Color(0.20f, 0.72f, 1.00f),
        MoveCategory.Terrain => new Color(0.50f, 0.90f, 0.20f),
        _ => new Color(0.75f, 0.75f, 0.80f),
    };

    // ── Tag ngắn hiển thị dưới icon ──────────────────────────────
    static string GetCategoryTag(MoveData move) => move.category switch
    {
        MoveCategory.Physical => $"hệ {move.elementType.ToString().ToLower()}",
        MoveCategory.Special => $"hệ {move.elementType.ToString().ToLower()}",
        MoveCategory.Status => move.statusSubType switch
        {
            StatusSubType.Buff => "buff",
            StatusSubType.Debuff => "debuff",
            StatusSubType.Heal => "hồi phục",
            _ => "trạng thái",
        },
        MoveCategory.Weather => "thời tiết",
        MoveCategory.Terrain => "địa hình",
        _ => "—",
    };
}