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
    }
    void OnDestroy() { if (Instance == this) Instance = null; }
    void Start() { Hide(); }

    // ← entity là tham số thứ 3, mặc định null nếu không truyền
    public void Show(List<MoveData> moves, System.Action<MoveData> onChosen, BattleEntity entity = null)
    {
        _onMoveChosen = onChosen;

        for (int i = panel.transform.childCount - 1; i >= 0; i--)
            Destroy(panel.transform.GetChild(i).gameObject);

        foreach (var move in moves)
        {
            if (move == null) { Debug.LogWarning("[MoveUI] move là NULL, bỏ qua"); continue; }
            try
            {
                var btn = Instantiate(moveButtonPrefab, panel.transform);
                var mbui = btn.GetComponent<MoveButtonUI>();
                if (mbui == null) { Debug.LogError("[MoveUI] MoveButtonPrefab KHÔNG có MoveButtonUI!"); continue; }

                // Lấy PP hiện tại từ entity, nếu không có thì dùng max (-1)
                int currentPP = entity != null ? entity.GetCurrentPP(move) : -1;
                mbui.Setup(move, OnButtonClicked, currentPP);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[MoveUI] Exception khi tạo nút {move.moveName}: {e.Message}");
            }
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

    public void OnCancelButtonClicked()
    {
        if (CommandPhaseController.Instance != null)
            CommandPhaseController.Instance.StepBack();
    }
}