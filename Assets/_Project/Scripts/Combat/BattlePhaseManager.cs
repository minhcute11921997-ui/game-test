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

        // Di chuyển tất cả entity đến vị trí đã chọn
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

        // Sắp xếp theo Speed giảm dần — thing nhanh hơn đánh trước
        var orderedAttackers = new System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<BattleEntity, BattleCommand>>(_commands);
        orderedAttackers.Sort((a, b) => b.Key.Speed.CompareTo(a.Key.Speed));

        foreach (var kvp in orderedAttackers)
        {
            BattleEntity attacker = kvp.Key;
            BattleCommand cmd = kvp.Value;

            // Bỏ qua nếu attacker đã bị hạ
            if (attacker == null) continue;
            if (!cmd.HasAttack) continue;

            // Tìm entity trên ô attackTarget
            MoveData move = attacker.GetMove();
            if (move == null) continue;

            var aoeCells = BattleGridManager.Instance.GetAoECells(
                cmd.attackTarget, move.shape, attacker.GridPos);

            foreach (var cell in aoeCells)
            {
                BattleEntity target = BattleGridManager.Instance.GetEntityAt(cell);
                if (target == null || target.TeamId == attacker.TeamId) continue; // bỏ qua đồng đội

                var result = CombatCalculator.Calculate(attacker.Data, target.Data, move);
                string eff = result.typeMultiplier > 1f ? " HIỆU QUẢ!" :
                              result.typeMultiplier < 1f ? " Không hiệu quả..." : "";
                string crit = result.isCritical ? " CHÍ MẠNG!" : "";
                string stab = result.isStab ? " [STAB]" : "";
                Debug.Log($"[Combat] {attacker.Data.thingName}{stab} → {target.Data.thingName}: " +
                          $"{result.damage} dmg (x{result.typeMultiplier}){eff}{crit}");
                target.TakeDamage(result.damage);
            }
            if (target == null) continue;

            // Lấy move mặc định (Sprint 3 sẽ có chọn move)
            if (move == null)
            {
                Debug.LogWarning($"[Judge] {attacker.name} không có move! Bỏ qua.");
                continue;
            }

            // === COMBAT CALCULATION ===
            var result = CombatCalculator.Calculate(attacker.Data, target.Data, move);

            // Log kết quả cho debug
            string effectiveness = result.typeMultiplier > 1f ? "HIỆU QUẢ!" :
                                   result.typeMultiplier < 1f ? "Không hiệu quả..." : "";
            string crit = result.isCritical ? " CHÍ MẠNG!" : "";
            string stab = result.isStab ? " [STAB]" : "";

            Debug.Log($"[Combat] {attacker.name}{stab} → {target.name}: {result.damage} dmg " +
                      $"(x{result.typeMultiplier} type){effectiveness}{crit}");

            // Áp sát thương
            target.TakeDamage(result.damage);
        }

        // Kết thúc lượt, quay về Command Phase
        BeginCommandPhase();
    }
}