// Assets/_Project/Scripts/UI/MoveButtonUI.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class MoveButtonUI : MonoBehaviour
{
    [Header("Header (PanelTop)")]
    [SerializeField] private Image panelTop;           // nền màu category
    [SerializeField] private Image iconBG;             // khung viền icon
    [SerializeField] private Image iconElement;        // sprite icon hệ
    [SerializeField] private TextMeshProUGUI textCategoryTag;  // "HỆ LỬA"
    [SerializeField] private TextMeshProUGUI textMoveName;     // "RỒNG LỬA PHUN LỬA"

    [Header("Bottom Stats")]
    [SerializeField] private TextMeshProUGUI textPower;  // "120"
    [SerializeField] private TextMeshProUGUI textPPValue; // "5"
    [SerializeField] private TextMeshProUGUI textPPMax;   // "/ 5"
    [SerializeField] private Image cellPowerBorder;       // viền ô Power
    [SerializeField] private Image cellPPBorder;          // viền ô PP

    [Header("Button")]
    [SerializeField] private Button button;

    [Header("Icon Sprites — kéo vào theo thứ tự ElementType")]
    [SerializeField] private Sprite[] elementIcons; // index = (int)ElementType

    private MoveData _moveData;
    private Action<MoveData> _callback;

    public void Setup(MoveData move, Action<MoveData> onClick)
    {
        _moveData = move;
        _callback = onClick;

        Color catColor = GetCategoryColor(move);
        Color accentColor = GetAccentColor(move); // màu viền & text số

        // ── PanelTop ──────────────────────────────────────────────
        panelTop.color = catColor;

        // Viền icon và bottom border cùng accent color
        iconBG.color = accentColor;

        // Icon hệ nguyên tố
        int idx = (int)move.elementType;
        if (elementIcons != null && idx < elementIcons.Length && elementIcons[idx] != null)
            iconElement.sprite = elementIcons[idx];

        // Tag: "HỆ LỬA", "VẬT LÝ", v.v.
        textCategoryTag.text = GetCategoryTag(move).ToUpper();
        textCategoryTag.color = new Color(1f, 1f, 1f, 0.65f);

        // Tên chiêu in hoa
        textMoveName.text = move.moveName.ToUpper();

        // ── PanelBottom ───────────────────────────────────────────
        bool hasPower = move.category == MoveCategory.Physical
                     || move.category == MoveCategory.Special;

        textPower.text = hasPower ? move.basePower.ToString() : "—";
        textPower.color = Color.white;

        textPPValue.text = move.pp.ToString();
        textPPMax.text = $"/ {move.pp}";  // max = pp (runtime sẽ update nếu cần)
        textPPValue.color = Color.white;
        textPPMax.color = new Color(1f, 1f, 1f, 0.55f);

        // Viền ô bottom


        // ── Button ────────────────────────────────────────────────
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
            StatusSubType.Buff => new Color(0.10f, 0.50f, 0.18f),   // Xanh lá
            StatusSubType.Debuff => new Color(0.38f, 0.38f, 0.42f),   // Trắng xám
            StatusSubType.Heal => new Color(0.72f, 0.08f, 0.08f),   // Đỏ
            _ => new Color(0.35f, 0.35f, 0.38f),
        },
        MoveCategory.Environment => new Color(0.05f, 0.32f, 0.58f),   // Xanh nước biển
        _ => new Color(0.25f, 0.25f, 0.28f),
    };

    // ── Màu ACCENT (viền, text số) — sáng hơn màu nền ──────────
    static Color GetAccentColor(MoveData move) => move.category switch
    {
        MoveCategory.Physical => new Color(1.00f, 0.58f, 0.15f),   // Cam sáng
        MoveCategory.Special => new Color(0.72f, 0.45f, 1.00f),   // Tím sáng
        MoveCategory.Status => move.statusSubType switch
        {
            StatusSubType.Buff => new Color(0.30f, 0.95f, 0.45f),   // Xanh sáng
            StatusSubType.Debuff => new Color(0.85f, 0.85f, 0.90f),   // Trắng
            StatusSubType.Heal => new Color(1.00f, 0.35f, 0.35f),   // Đỏ sáng
            _ => new Color(0.80f, 0.80f, 0.85f),
        },
        MoveCategory.Environment => new Color(0.20f, 0.72f, 1.00f),   // Xanh sáng
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
        MoveCategory.Environment => "môi trường",
        _ => "—",
    };

}