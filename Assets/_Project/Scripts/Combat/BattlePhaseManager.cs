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

        // Reset trạng thái đầu lượt cho mọi entity
        foreach (var entity in BattleGridManager.Instance.GetAllEntities())
            entity.OnTurnStart();

        CommandPhaseController.Instance.BeginInput();
    }

    public void SubmitCommand(BattleEntity entity, BattleCommand cmd)
    {
        _commands[entity] = cmd;
        Debug.Log($"[Command] {entity.name} → Move:{cmd.moveTarget} Attack:{cmd.attackTarget}");

        if (_commands.Count >= GetActiveEntityCount())
            StartCoroutine(BeginExecutionPhase());
    }

    int GetActiveEntityCount() => 2;

    // ── Execution Phase ────────────────────────────────────────────
    IEnumerator BeginExecutionPhase()
{
    CurrentPhase = BattlePhase.ExecutionPhase;
    Debug.Log("[BattlePhase] === EXECUTION PHASE ===");

    var grid = BattleGridManager.Instance;
    var moveCoroutines = new List<Coroutine>();

    // Dùng list entity để tránh lỗi CS1612 khi sửa moveTarget
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
            BattleEntity  attacker = kvp.Key;
            BattleCommand cmd      = kvp.Value;
            if (attacker == null || !cmd.HasAttack) continue;

            MoveData move = attacker.GetMove();
            if (move == null) continue;

            // ── Chiêu Môi Trường: xử lý riêng, không tính damage thường
            if (move.category == MoveCategory.Environment)
            {
                HandleEnvironmentMove(attacker, cmd, move);
                continue;
            }

            // ── Bão Tuyết: thu nhỏ AoE trước khi tính
            AttackShape effectiveShape  = move.shape;
            int         effectiveRadius = move.aoeRadius;
            ApplyBlizzardEffect(attacker, ref effectiveShape, ref effectiveRadius);

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
                    attackerLevel:    attacker.Level,
                    attackerLuck:     attacker.Data.luck,
                    aoeShape:         effectiveShape,
                    cellDistanceType: distType
                );

                string eff  = r.typeMultiplier > 1f ? " HIỆU QUẢ!" :
                              r.typeMultiplier < 1f ? " Không hiệu quả..." : "";
                string crit = r.isCritical ? " CHÍ MẠNG!" : "";
                string stab = r.isStab     ? " [STAB]"    : "";

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
            // Lấy các ô theo terrainShape từ attackTarget
            var cells = BattleGridManager.Instance.GetAoECells(
    cmd.attackTarget, move.terrainShape, attacker.GridPos, 1);

            foreach (var cell in cells)
                TerrainManager.Instance.PlaceTerrain(cell, move);

            Debug.Log($"[Env] {attacker.name} đặt terrain: {move.terrainEffect} " +
                      $"tại {cmd.attackTarget}, {cells.Count} ô");
        }
    }

    // ── Bão Tuyết: thu nhỏ shape ──────────────────────────────────
    void ApplyBlizzardEffect(BattleEntity attacker, ref AttackShape shape, ref int radius)
    {
        if (!WeatherManager.Instance.IsBlizzardActive(attacker.TeamId)) return;

        switch (shape)
        {
            case AttackShape.Square3x3:
                shape  = AttackShape.Square2x2;
                radius = Mathf.Max(1, radius - 1);
                Debug.Log("[Weather] Bão Tuyết: Square3x3 → Square2x2");
                break;

            case AttackShape.Square2x2:
                shape  = AttackShape.Single;
                radius = 1;
                Debug.Log("[Weather] Bão Tuyết: Square2x2 → Single");
                break;

            case AttackShape.Cross:
    radius = Mathf.Max(0, radius - 1);
    if (radius == 0) shape = AttackShape.Single;
    Debug.Log($"[Weather] Bão Tuyết: Cross radius → {radius}");
    break;

            // Single và Line: giữ nguyên
        }
    }

    // ── Helper: tính cellDistanceType ─────────────────────────────
    int CalcDistType(AttackShape shape, GridPos cell, GridPos center)
    {
        if (shape != AttackShape.Square3x3) return 0;

        int dc = Mathf.Abs(cell.col - center.col);
        int dr = Mathf.Abs(cell.row - center.row);
        if (dc == 0 && dr == 0) return 0;   // tâm
        if (dc + dr == 1)       return 1;   // cận tâm
        return 2;                            // rìm góc
    }

    // ── Cuối JudgePhase: terrain + weather giảm lượt ──────────────
    IEnumerator EndJudgePhase()
    {
        yield return new WaitForSeconds(0.5f);

        // Terrain: hiệu ứng cuối lượt (Bẫy Gai) + giảm lượt tồn tại
        var allEntities = BattleGridManager.Instance.GetAllEntities();
        TerrainManager.Instance.OnTurnEnd(allEntities);

        // Weather: giảm lượt tồn tại
        WeatherManager.Instance.OnTurnEnd();

        // Kiểm tra kết thúc battle
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