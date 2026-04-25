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

    public void Setup(MoveData move, Action<MoveData> onClick)
    {
        _moveData = move;
        _callback = onClick;

        Color catColor = GetCategoryColor(move);
        Color accentColor = GetAccentColor(move);

        panelTop.color = catColor;
        iconBG.color = accentColor;

        int idx = (int)move.elementType;
        if (elementIcons != null && idx < elementIcons.Length && elementIcons[idx] != null)
            iconElement.sprite = elementIcons[idx];

        textCategoryTag.text = GetCategoryTag(move).ToUpper();
        textCategoryTag.color = new Color(1f, 1f, 1f, 0.65f);
        textMoveName.text = move.moveName.ToUpper();

        bool hasPower = move.category == MoveCategory.Physical
                     || move.category == MoveCategory.Special;

        textPower.text = hasPower ? move.basePower.ToString() : "—";
        textPower.color = Color.white;
        textPPValue.text = move.pp.ToString();
        textPPMax.text = $"/ {move.pp}";
        textPPValue.color = Color.white;
        textPPMax.color = new Color(1f, 1f, 1f, 0.55f);

        button.interactable = true;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => _callback?.Invoke(_moveData));
    }

    // ── Màu NỀN PanelTop theo category ───────────────────────────
    public static Color GetCategoryColor(MoveData move) => move.category switch
    {
        MoveCategory.Physical => new Color(0.78f, 0.38f, 0.05f),   // Cam đậm
        MoveCategory.Special => new Color(0.40f, 0.10f, 0.60f),   // Tím
        MoveCategory.Status => move.statusSubType switch
        {
            StatusSubType.Buff => new Color(0.10f, 0.50f, 0.18f), // Xanh lá
            StatusSubType.Debuff => new Color(0.38f, 0.38f, 0.42f), // Xám
            StatusSubType.Heal => new Color(0.72f, 0.08f, 0.08f), // Đỏ
            _ => new Color(0.35f, 0.35f, 0.38f),
        },
        MoveCategory.Weather => new Color(0.05f, 0.32f, 0.58f),   // Xanh nước biển
        MoveCategory.Terrain => new Color(0.22f, 0.42f, 0.10f),   // Xanh lá đậm
        _ => new Color(0.25f, 0.25f, 0.28f),
    };

    // ── Màu ACCENT (viền, text số) ────────────────────────────────
    static Color GetAccentColor(MoveData move) => move.category switch
    {
        MoveCategory.Physical => new Color(1.00f, 0.58f, 0.15f),   // Cam sáng
        MoveCategory.Special => new Color(0.72f, 0.45f, 1.00f),   // Tím sáng
        MoveCategory.Status => move.statusSubType switch
        {
            StatusSubType.Buff => new Color(0.30f, 0.95f, 0.45f), // Xanh sáng
            StatusSubType.Debuff => new Color(0.85f, 0.85f, 0.90f), // Trắng
            StatusSubType.Heal => new Color(1.00f, 0.35f, 0.35f), // Đỏ sáng
            _ => new Color(0.80f, 0.80f, 0.85f),
        },
        MoveCategory.Weather => new Color(0.20f, 0.72f, 1.00f),   // Xanh trời sáng
        MoveCategory.Terrain => new Color(0.50f, 0.90f, 0.20f),   // Xanh lá sáng
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