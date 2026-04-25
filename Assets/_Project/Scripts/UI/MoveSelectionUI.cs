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
    void Start()
    {
        Hide(); // Gọi ở Start thay vì Awake — lúc này script đã init xong
    }
    public void Show(List<MoveData> moves, System.Action<MoveData> onChosen)
    {
        // Debug.Log($"[MoveUI] Show() số chiêu: {moves?.Count ?? 0}, panel: {(panel == null ? "NULL" : panel.name)}");
        _onMoveChosen = onChosen;

        for (int i = panel.transform.childCount - 1; i >= 0; i--)
            Destroy(panel.transform.GetChild(i).gameObject);

        foreach (var move in moves)
        {
            if (move == null) { Debug.LogWarning("[MoveUI] move là NULL, bỏ qua"); continue; }
            try
            {
                // Debug.Log($"[MoveUI] Tạo nút: {move.moveName}");
                var btn = Instantiate(moveButtonPrefab, panel.transform);
                // Debug.Log($"[MoveUI] Instantiate xong, lấy MoveButtonUI...");
                var mbui = btn.GetComponent<MoveButtonUI>();
                if (mbui == null) { Debug.LogError("[MoveUI] MoveButtonPrefab KHÔNG có component MoveButtonUI!"); continue; }
                mbui.Setup(move, OnButtonClicked);
                // Debug.Log($"[MoveUI] Setup xong: {move.moveName}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[MoveUI] Exception khi tạo nút {move.moveName}: {e.Message}");
            }
        }

        panel.SetActive(true);
        // Debug.Log($"[MoveUI] panel.activeSelf = {panel.activeSelf}, childCount = {panel.transform.childCount}");
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
        // Gọi thẳng vào logic lùi bước của Controller
        if (CommandPhaseController.Instance != null)
        {
            CommandPhaseController.Instance.StepBack();
        }
    }
}