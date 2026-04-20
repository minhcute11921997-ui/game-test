// Assets/_Project/Scripts/Combat/BattlePhaseManager.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BattlePhase
{
    Idle,
    CommandPhase,   // Người chơi chọn lệnh
    ExecutionPhase, // Thực thi đồng thời
    JudgePhase,     // Phán xét Speed, áp dụng sát thương
    ResultPhase     // Kết thúc lượt, kiểm tra thắng thua
}

public class BattlePhaseManager : MonoBehaviour
{
    public static BattlePhaseManager Instance { get; private set; }

    public BattlePhase CurrentPhase { get; private set; } = BattlePhase.Idle;

    // Lệnh đã thu thập từ CommandPhase
    private readonly Dictionary<BattleEntity, BattleCommand> _commands = new();

    void Awake() => Instance = this;

    void Start()
    {
        // Tự động bắt đầu sau khi BattleManager spawn xong
        StartCoroutine(StartAfterSpawn());
    }

    IEnumerator StartAfterSpawn()
    {
        yield return null; // chờ 1 frame cho BattleManager.Start() chạy xong
        BeginCommandPhase();
    }

    // ── Bắt đầu Command Phase ─────────────────────────────────────
    public void BeginCommandPhase()
    {
        CurrentPhase = BattlePhase.CommandPhase;
        _commands.Clear();
        Debug.Log("[BattlePhase] === COMMAND PHASE ===");
        CommandPhaseController.Instance.BeginInput();
    }

    // ── Nhận lệnh từ CommandPhaseController ──────────────────────
    public void SubmitCommand(BattleEntity entity, BattleCommand cmd)
    {
        _commands[entity] = cmd;
        Debug.Log($"[Command] {entity.name} → Move:{cmd.moveTarget} Attack:{cmd.attackTarget}");

        // TODO Sprint 2: Khi có nhiều entity, check tất cả đã submit chưa
        // Hiện tại: 1vs1 → đủ 2 lệnh thì proceed
        if (_commands.Count >= GetActiveEntityCount())
            BeginExecutionPhase();
    }

    int GetActiveEntityCount()
    {
        // Tạm thời: 2 (1 player + 1 enemy)
        return 2;
    }

    // ── Execution Phase (Sprint 2) ────────────────────────────────
    void BeginExecutionPhase()
    {
        CurrentPhase = BattlePhase.ExecutionPhase;
        Debug.Log("[BattlePhase] === EXECUTION PHASE ===");
        // TODO Sprint 2: animate di chuyển đồng thời
        // Tạm thời: apply ngay
        foreach (var kvp in _commands)
        {
            var grid = BattleGridManager.Instance;
            if (!kvp.Value.moveTarget.Equals(kvp.Key.GridPos))
                grid.MoveEntity(kvp.Key, kvp.Key.GridPos, kvp.Value.moveTarget);
        }
        BeginJudgePhase();
    }

    // ── Judge Phase (Sprint 2) ─────────────────────────────────────
    void BeginJudgePhase()
    {
        CurrentPhase = BattlePhase.JudgePhase;
        Debug.Log("[BattlePhase] === JUDGE PHASE ===");
        // TODO Sprint 2: tính sát thương theo Speed, type matchup
        // Tạm thời: loopback về Command
        BeginCommandPhase();
    }
}