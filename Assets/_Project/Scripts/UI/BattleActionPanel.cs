// Assets/_Project/Scripts/UI/BattleActionPanel.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleActionPanel : MonoBehaviour
{
    public static BattleActionPanel Instance { get; private set; }

    [SerializeField] private GameObject panel;
    [SerializeField] private Button btnFight;
    [SerializeField] private Button btnCapture;
    [SerializeField] private Button btnFlee;
    [SerializeField] private TextMeshProUGUI txtFleeChance; // tuỳ chọn: hiển thị %

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }
    void OnDestroy() { if (Instance == this) Instance = null; }

    void Start()
    {
        Hide();
        btnFight.onClick.AddListener(OnFight);
        btnCapture.onClick.AddListener(OnCapture);
        btnFlee.onClick.AddListener(OnFlee);
    }

    public void Show()
    {
        panel.SetActive(true);

        // Cập nhật % bỏ đi dựa trên level địch
        if (txtFleeChance != null && RuntimeGameState.CurrentEnemy != null)
        {
            int chance = CalcFleeChance(RuntimeGameState.CurrentEnemy.level);
            txtFleeChance.text = $"({chance}%)";
        }
    }

    public void Hide() => panel.SetActive(false);

    void OnFight()
    {
        Hide();
        CommandPhaseController.Instance.StartFightFlow();
    }

    void OnCapture()
    {
        Hide();
        BookSelectionUI.Instance.Show(OnBookChosen);
    }

    void OnFlee()
    {
        Hide();
        int chance = CalcFleeChance(RuntimeGameState.CurrentEnemy?.level ?? 1);
        bool success = UnityEngine.Random.Range(0, 100) < chance;

        if (success)
        {
            Debug.Log($"<color=cyan>[Flee] Thoát thành công! ({chance}%)</color>");
            // Load về Overworld ngay
            UnityEngine.SceneManagement.SceneManager.LoadScene("OverworldScene");
        }
        else
        {
            Debug.Log($"<color=orange>[Flee] Thoát thất bại ({chance}%) — Bỏ lượt player</color>");
            CommandPhaseController.Instance.SkipPlayerTurn();
        }
    }

    void OnBookChosen(BookData book)
    {
        if (book == null) { Show(); return; } // Cancel → quay lại panel

        // Trừ sách
        RuntimeGameState.UseBook(book);

        // Tính capture chance
        ThingData enemy = RuntimeGameState.CurrentEnemy;
        int enemyLevel = enemy != null ? enemy.level : 1;
        int chance = Mathf.Clamp(60 - enemyLevel * 3 + book.captureRateBonus, 5, 95);
        bool captured = UnityEngine.Random.Range(0, 100) < chance;

        Debug.Log($"[Capture] Dùng {book.bookName} | Chance: {chance}% | Kết quả: {(captured ? "THÀNH CÔNG" : "THẤT BẠI")}");

        if (captured)
        {
            // Thêm vào party
            if (enemy != null) RuntimeGameState.Party.Add(enemy);
            Debug.Log($"<color=green>[Capture] Bắt được {enemy?.thingName}!</color>");
            UnityEngine.SceneManagement.SceneManager.LoadScene("OverworldScene");
        }
        else
        {
            Debug.Log("<color=orange>[Capture] Thất bại — Bỏ lượt player</color>");
            CommandPhaseController.Instance.SkipPlayerTurn();
        }
    }

    int CalcFleeChance(int enemyLevel) =>
        Mathf.Clamp(70 - enemyLevel * 3, 10, 90);
}