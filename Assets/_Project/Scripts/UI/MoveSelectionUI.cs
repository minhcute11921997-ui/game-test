using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MoveSelectionUI : MonoBehaviour
{
    public static MoveSelectionUI Instance { get; private set; }

    [Header("References")]
    [SerializeField] private GameObject panel;
    [SerializeField] private GameObject moveButtonPrefab;

    private System.Action<MoveData> _onMoveChosen;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        Hide();
    }

    public void Show(List<MoveData> moves, System.Action<MoveData> onChosen)
    {
        _onMoveChosen = onChosen;

        // Xóa nút cũ
        for (int i = panel.transform.childCount - 1; i >= 0; i--)
            Destroy(panel.transform.GetChild(i).gameObject);

        // Tạo nút mới cho từng chiêu
        foreach (var move in moves)
        {
            if (move == null) continue;
            var btn = Instantiate(moveButtonPrefab, panel.transform);
            btn.GetComponent<MoveButtonUI>().Setup(move, OnButtonClicked);
        }

        panel.SetActive(true);
    }

    public void Hide()
    {
        if (panel != null) panel.SetActive(false);
    }

    void OnButtonClicked(MoveData move)
    {
        Hide();
        _onMoveChosen?.Invoke(move);
    }
}