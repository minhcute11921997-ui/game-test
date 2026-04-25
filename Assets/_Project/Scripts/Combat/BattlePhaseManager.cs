// Assets/_Project/Scripts/Combat/BattlePhaseManager.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BattlePhase { Idle, CommandPhase, ExecutionPhase, JudgePhase, ResultPhase }

public class BattlePhaseManager : MonoBehaviour
{
    public static BattlePhaseManager Instance { get; private set; }
    public BattlePhase CurrentPhase { get; private set; } = BattlePhase.Idle;

    private readonly Dictionary<BattleEntity, BattleCommand> _commands = new();

    [SerializeField] BattleResultManager resultManager;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }
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

        foreach (var entity in BattleGridManager.Instance.GetAllEntities())
        {
            entity.OnTurnStart();
            entity.IncrementTurnCount(); // ← THÊM: Setup archetype đếm lượt
        }

        CommandPhaseController.Instance.BeginInput();
    }

    public void SubmitCommand(BattleEntity entity, BattleCommand cmd)
    {
        _commands[entity] = cmd;
        Debug.Log($"[Command] {entity.name} → Move:{cmd.moveTarget} Attack:{cmd.attackTarget}");

        if (_commands.Count >= GetActiveEntityCount())
            StartCoroutine(BeginExecutionPhase());
    }

    int GetActiveEntityCount()
    => BattleGridManager.Instance.GetAllEntities().Count;

    // ── Execution Phase ────────────────────────────────────────────
    IEnumerator BeginExecutionPhase()
    {
        CurrentPhase = BattlePhase.ExecutionPhase;
        Debug.Log("[BattlePhase] === EXECUTION PHASE ===");

        var grid = BattleGridManager.Instance;
        var moveCoroutines = new List<Coroutine>();
        var entities = new List<BattleEntity>(_commands.Keys);

        foreach (var entity in entities)
        {
            var cmd = _commands[entity];
            GridPos targetPos = cmd.moveTarget;

            // Từ Trường
            if (entity.IsForcedStillByMagnet())
            {
                Debug.Log($"[Weather] {entity.name} bị Từ Trường khoá");
                targetPos = entity.GridPos;
            }

            // Bẫy Gai
            if (!entity.CanMove)
            {
                Debug.Log($"[Terrain] {entity.name} bị Bẫy Gai khoá");
                targetPos = entity.GridPos;
            }

            if (!targetPos.Equals(entity.GridPos))
            {
                entity.TrackMovement(targetPos); // ← THÊM: cập nhật LastMoveDir trước khi move
                var co = StartCoroutine(grid.MoveEntitySmooth(entity, targetPos, null, 0.3f));
                moveCoroutines.Add(co);
            }
        }

        foreach (var co in moveCoroutines)
            yield return co;

        // Trigger terrain + OnMoved
        foreach (var entity in entities)
        {
            entity.OnMoved();
            TerrainManager.Instance.OnEntityEnterCell(entity, entity.GridPos);
        }

        BeginJudgePhase();
    }

    // ── Judge Phase ────────────────────────────────────────────────
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

            // ── Chiêu Môi Trường
            if (move.category == MoveCategory.Environment)
            {
                HandleEnvironmentMove(attacker, cmd, move);
                continue;
            }

            // ── Chiêu Status (buff/debuff): đếm buff cho AI enemy
            if (move.category == MoveCategory.Status && attacker.TeamId == 1)
                attacker.IncrementBuffCount(); // ← THÊM: ULTRA archetype dùng

            // ── Bão Tuyết: thu nhỏ AoE
            WeatherManager.Instance.GetEffectiveAoE(attacker.TeamId, move.shape, move.aoeRadius, out AttackShape effectiveShape, out int effectiveRadius);

            var cells = BattleGridManager.Instance.GetAoECells(
    cmd.attackTarget, effectiveShape, attacker.GridPos, effectiveRadius);

            foreach (var cell in cells)
            {
                BattleEntity target = BattleGridManager.Instance.GetEntityAt(cell);
                if (target == null || target.TeamId == attacker.TeamId) continue;

                int distType = CalcDistType(effectiveShape, cell, cmd.attackTarget);

                var r = CombatCalculator.Calculate(
                    attacker.Data,
                    target.Data,
                    move,
                    attackerLevel: attacker.Level,
                    attackerLuck: attacker.Data.luck,
                    aoeShape: effectiveShape,
                    cellDistanceType: distType
                );

                string eff = r.typeMultiplier > 1f ? " HIỆU QUẢ!" :
                              r.typeMultiplier < 1f ? " Không hiệu quả..." : "";
                string crit = r.isCritical ? " CHÍ MẠNG!" : "";
                string stab = r.isStab ? " [STAB]" : "";

                Debug.Log($"[Combat] {attacker.Data.thingName}{stab} → " +
                          $"{target.Data.thingName}: {r.damage} dmg " +
                          $"(x{r.typeMultiplier}){eff}{crit}");

                target.TakeDamage(r.damage, r.isCritical);
            }
        }

        StartCoroutine(EndJudgePhase());
    }

    // ── Xử lý chiêu Môi Trường ────────────────────────────────────
    void HandleEnvironmentMove(BattleEntity attacker, BattleCommand cmd, MoveData move)
    {
        if (move.envCategory == EnvironmentCategory.Weather)
        {
            WeatherManager.Instance.ApplyWeather(move);
            Debug.Log($"[Env] {attacker.name} tung thời tiết: {move.weatherType}");
        }
        else if (move.envCategory == EnvironmentCategory.Terrain)
        {
            var cells = BattleGridManager.Instance.GetAoECells(
                cmd.attackTarget, move.terrainShape, attacker.GridPos, 1);

            foreach (var cell in cells)
                TerrainManager.Instance.PlaceTerrain(cell, move);

            Debug.Log($"[Env] {attacker.name} đặt terrain: {move.terrainEffect} " +
                      $"tại {cmd.attackTarget}, {cells.Count} ô");
        }
    }


    // ── Helper: tính cellDistanceType ─────────────────────────────
    int CalcDistType(AttackShape shape, GridPos cell, GridPos center)
    {
        if (shape != AttackShape.Square3x3) return 0;
        int dc = Mathf.Abs(cell.col - center.col);
        int dr = Mathf.Abs(cell.row - center.row);
        if (dc == 0 && dr == 0) return 0;
        if (dc + dr == 1) return 1;
        return 2;
    }

    // ── Cuối JudgePhase ───────────────────────────────────────────
    IEnumerator EndJudgePhase()
    {
        yield return new WaitForSeconds(0.5f);

        // ── Dọn dẹp entity đã chết trong JudgePhase ──────────────────
        var deadEntities = new List<BattleEntity>();
        foreach (var entity in BattleGridManager.Instance.GetAllEntities())
        {
            if (entity.IsDead) deadEntities.Add(entity);
        }
        foreach (var dead in deadEntities)
            Destroy(dead.gameObject);

        // Chờ 1 frame để Destroy thực sự có hiệu lực trước khi check result
        yield return null;

        // ── Hiệu ứng cuối lượt ───────────────────────────────────────
        var allAlive = BattleGridManager.Instance.GetAllEntities();
        TerrainManager.Instance.OnTurnEnd(allAlive);
        WeatherManager.Instance.OnTurnEnd();

        // ── Kiểm tra kết thúc trận ───────────────────────────────────
        if (resultManager != null && resultManager.CheckBattleEnd())
        {
            CurrentPhase = BattlePhase.ResultPhase;
            Debug.Log("[BattlePhase] === RESULT PHASE ===");
        }
        else
        {
            BeginCommandPhase();
        }
    }
}