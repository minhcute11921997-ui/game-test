// Assets/_Project/Scripts/Combat/BattlePhaseManager.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BattlePhase
{
    Idle,
    CommandPhase,
    ExecutionPhase,
    JudgePhase,
    ResultPhase
}

public class BattlePhaseManager : MonoBehaviour
{
    public static BattlePhaseManager Instance { get; private set; }
    public BattlePhase CurrentPhase { get; private set; } = BattlePhase.Idle;

    private readonly Dictionary<BattleEntity, BattleCommand> _commands = new();

    // ── Sprint 3: kéo BattleResultManager vào đây trong Inspector ──
    [SerializeField] BattleResultManager resultManager;

    void Awake() => Instance = this;

    void Start() => StartCoroutine(StartAfterSpawn());

    IEnumerator StartAfterSpawn()
    {
        yield return null;
        BeginCommandPhase();
    }

    // ── Command Phase ──────────────────────────────────────────────
    public void BeginCommandPhase()
    {
        CurrentPhase = BattlePhase.CommandPhase;
        _commands.Clear();
        Debug.Log("[BattlePhase] === COMMAND PHASE ===");
        CommandPhaseController.Instance.BeginInput();
    }

    public void SubmitCommand(BattleEntity entity, BattleCommand cmd)
    {
        _commands[entity] = cmd;
        Debug.Log($"[Command] {entity.name} → Move:{cmd.moveTarget} Attack:{cmd.attackTarget}");

        if (_commands.Count >= GetActiveEntityCount())
            StartCoroutine(BeginExecutionPhase()); // ← Sprint 3: coroutine
    }

    int GetActiveEntityCount() => 2;

    // ── Execution Phase: chờ smooth move xong rồi mới Judge ───────
    IEnumerator BeginExecutionPhase()
    {
        CurrentPhase = BattlePhase.ExecutionPhase;
        Debug.Log("[BattlePhase] === EXECUTION PHASE ===");

        var grid = BattleGridManager.Instance;
        var moveCoroutines = new List<Coroutine>();

        foreach (var kvp in _commands)
        {
            BattleEntity entity = kvp.Key;
            GridPos target = kvp.Value.moveTarget;

            if (!target.Equals(entity.GridPos))
            {
                // Sprint 3: MoveEntitySmooth thay vì MoveEntity (snap)
                var co = StartCoroutine(grid.MoveEntitySmooth(entity, target, null, 0.3f));
                moveCoroutines.Add(co);
            }
        }

        foreach (var co in moveCoroutines)
            yield return co; // chờ tất cả entity đến đích

        BeginJudgePhase();
    }

    // ── Judge Phase: tính damage + hiện popup + cập nhật HP bar ───
    void BeginJudgePhase()
    {
        CurrentPhase = BattlePhase.JudgePhase;
        Debug.Log("[BattlePhase] === JUDGE PHASE ===");

        var ordered = new List<KeyValuePair<BattleEntity, BattleCommand>>(_commands);
        ordered.Sort((a, b) => b.Key.Speed.CompareTo(a.Key.Speed));

        foreach (var kvp in ordered)
        {
            BattleEntity attacker = kvp.Key;
            BattleCommand cmd = kvp.Value;
            if (attacker == null || !cmd.HasAttack) continue;

            MoveData move = attacker.GetMove();
            if (move == null) continue;

            var cells = BattleGridManager.Instance.GetAoECells(
                cmd.attackTarget, move.shape, attacker.GridPos);

            foreach (var cell in cells)
            {
                BattleEntity target = BattleGridManager.Instance.GetEntityAt(cell);
                if (target == null || target.TeamId == attacker.TeamId) continue;

                // Tính cellDistanceType cho AoE 3x3
                int distType = 0;
                if (move.shape == AttackShape.Square3x3)
                {
                    int dc = Mathf.Abs(cell.col - cmd.attackTarget.col);
                    int dr = Mathf.Abs(cell.row - cmd.attackTarget.row);
                    if (dc == 0 && dr == 0) distType = 0;       // tâm
                    else if (dc + dr == 1) distType = 1;       // cận tâm
                    else distType = 2;       // rìm góc
                }

                var r = CombatCalculator.Calculate(
                    attacker.Data,
                    target.Data,
                    move,
                    attackerLevel: attacker.Level,
                    attackerLuck: attacker.Data.luck,
                    aoeShape: move.shape,
                    cellDistanceType: distType
                );

                string eff = r.typeMultiplier > 1f ? " HIỆU QUẢ!" :
                              r.typeMultiplier < 1f ? " Không hiệu quả..." : "";
                string crit = r.isCritical ? " CHÍ MẠNG!" : "";
                string stab = r.isStab ? " [STAB]" : "";

                Debug.Log($"[Combat] {attacker.Data.thingName}{stab} → " +
                          $"{target.Data.thingName}: {r.damage} dmg " +
                          $"(x{r.typeMultiplier}){eff}{crit}");

                // Sprint 3: truyền isCritical để popup hiện màu vàng khi chí mạng
                target.TakeDamage(r.damage, r.isCritical);
            }
        }

        // Sprint 3: dừng 0.5s cho popup kịp bay lên rồi mới check kết thúc
        StartCoroutine(CheckBattleEndThenLoop());
    }

    IEnumerator CheckBattleEndThenLoop()
    {
        yield return new WaitForSeconds(0.5f);

        if (resultManager != null && resultManager.CheckBattleEnd())
        {
            CurrentPhase = BattlePhase.ResultPhase;
            Debug.Log("[BattlePhase] === RESULT PHASE ===");
            // BattleResultManager tự hiện Win/Lose panel và load Overworld
        }
        else
        {
            BeginCommandPhase();
        }
    }
}