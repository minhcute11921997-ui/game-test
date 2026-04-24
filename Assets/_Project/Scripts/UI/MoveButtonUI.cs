// Assets/_Project/Scripts/UI/MoveButtonUI.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class MoveButtonUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI moveName;
    [SerializeField] private TextMeshProUGUI movePower;
    [SerializeField] private TextMeshProUGUI moveType;
    [SerializeField] private Image background;
    [SerializeField] private Button button;

    private MoveData _moveData;
    private Action<MoveData> _callback;

    public void Setup(MoveData move, Action<MoveData> onClick)
    {
        _moveData = move;
        _callback = onClick;

        moveName.text = move.moveName;
        movePower.text = move.category == MoveCategory.Status ? "—" : $"Power: {move.basePower}";
        moveType.text = move.elementType.ToString();

        // Màu nền theo hệ
        background.color = GetElementColor(move.elementType);

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => _callback?.Invoke(_moveData));
    }

    // Màu đặc trưng từng hệ
    static Color GetElementColor(ElementType type) => type switch
    {
        ElementType.Fire => new Color(1f, 0.35f, 0.2f),
        ElementType.Water => new Color(0.2f, 0.55f, 1f),
        ElementType.Wood => new Color(0.3f, 0.8f, 0.3f),
        ElementType.Thunder => new Color(1f, 0.9f, 0.1f),
        ElementType.Ice => new Color(0.5f, 0.85f, 1f),
        ElementType.Earth => new Color(0.7f, 0.55f, 0.3f),
        ElementType.Wind => new Color(0.6f, 1f, 0.8f),
        ElementType.Light => new Color(1f, 1f, 0.6f),
        ElementType.Dark => new Color(0.4f, 0.2f, 0.6f),
        _ => new Color(0.8f, 0.8f, 0.8f),
    };
}