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

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    void Start() => StartCoroutine(StartAfterSpawn());

    IEnumerator StartAfterSpawn()
    {
        yield return null;
        BeginCommandPhase();
    }

    // ── Command Phase ─────────────────────────────────────────────
    public void BeginCommandPhase()
    {
        CurrentPhase = BattlePhase.CommandPhase;
        _commands.Clear();
        Debug.Log("[BattlePhase] === COMMAND PHASE ===");

        if (WeatherManager.Instance != null)
        {
            // Team 0 là Sân Trái, Team 1 là Sân Phải
            var leftWeather = WeatherManager.Instance.GetWeatherForTeam(0);
            var rightWeather = WeatherManager.Instance.GetWeatherForTeam(1);

            Debug.Log($"<color=cyan>[THỜI TIẾT TRÊN SÂN]</color> Sân Trái: <b>{leftWeather}</b> | Sân Phải: <b>{rightWeather}</b>");
        }

        foreach (var entity in BattleGridManager.Instance.GetAllEntities())
        {
            entity.OnTurnStart();
            entity.IncrementTurnCount();
        }

        TerrainManager.Instance.OnTurnStart();
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

    // ── Execution Phase ───────────────────────────────────────────
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

            if (entity.IsForcedStillByMagnet())
            {
                Debug.Log($"[Weather] {entity.name} bị Từ Trường khoá");
                targetPos = entity.GridPos;
            }

            if (!entity.CanMove)
            {
                Debug.Log($"[Terrain] {entity.name} bị Bẫy Gai khoá");
                targetPos = entity.GridPos;
            }

            if (!targetPos.Equals(entity.GridPos))
            {
                entity.TrackMovement(targetPos);
                var co = StartCoroutine(grid.MoveEntitySmooth(entity, targetPos, null, 0.3f));
                moveCoroutines.Add(co);
            }
        }

        foreach (var co in moveCoroutines)
            yield return co;

        foreach (var entity in entities)
        {
            entity.OnMoved();
            TerrainManager.Instance.OnEntityEnterCell(entity, entity.GridPos);
        }

        BeginJudgePhase();
    }

    // ── Judge Phase ───────────────────────────────────────────────
    void BeginJudgePhase()
    {
        CurrentPhase = BattlePhase.JudgePhase;
        Debug.Log("[BattlePhase] === JUDGE PHASE ===");

        var ordered = new List<KeyValuePair<BattleEntity, BattleCommand>>(_commands);
        ordered.Sort((a, b) =>
        {
            // Ưu tiên 1: So sánh Speed (giảm dần, b so với a)
            int cmp = b.Key.Speed.CompareTo(a.Key.Speed);

            // Nếu Speed khác nhau, trả về kết quả luôn
            if (cmp != 0) return cmp;

            // Ưu tiên 2: Cùng Speed -> So sánh điểm Roll (giảm dần, đứa nào roll cao hơn đi trước)
            return b.Key.TieBreakRoll.CompareTo(a.Key.TieBreakRoll);
        });

        foreach (var kvp in ordered)
        {
            MoveData dbgMove = kvp.Key.GetMove();
            Debug.Log($"<color=cyan>[JudgePhase DEBUG]</color> {kvp.Key.name} | " +
                      $"HasAttack={kvp.Value.HasAttack} | " +
                      $"Move={(dbgMove != null ? dbgMove.moveName : "NULL")} | " +
                      $"Category={(dbgMove != null ? dbgMove.category.ToString() : "?")}");

            BattleEntity attacker = kvp.Key;
            BattleCommand cmd = kvp.Value;
            if (attacker == null) continue;

            MoveData move = attacker.GetMove();
            if (move == null) continue;

            // ── Thời Tiết ──────────────────────────────────────────
            if (move.category == MoveCategory.Weather)
            {
                HandleWeatherMove(attacker, cmd, move);
                continue;
            }

            if (attacker == null) continue;
            // ── Địa Hình ───────────────────────────────────────────
            if (move.category == MoveCategory.Terrain)
            {
                HandleTerrainMove(attacker, cmd, move);
                continue;
            }

            if (!cmd.HasAttack) continue;
            if (move.category == MoveCategory.Status && attacker.TeamId == 1)
                attacker.IncrementBuffCount();

            WeatherManager.Instance.GetEffectiveAoE(
                attacker.TeamId, move.shape, move.aoeRadius,
                out AttackShape effectiveShape, out int effectiveRadius);

            var cells = BattleGridManager.Instance.GetAoECells(
                cmd.attackTarget, effectiveShape, attacker.GridPos, effectiveRadius);

            foreach (var cell in cells)
            {
                BattleEntity target = BattleGridManager.Instance.GetEntityAt(cell);
                if (target == null || target.TeamId == attacker.TeamId) continue;

                int distType = CalcDistType(effectiveShape, cell, cmd.attackTarget);

                var r = CombatCalculator.Calculate(
                    attacker, target, move,
                    attackerLevel: attacker.Level,
                    attackerLuck: attacker.Data.luck,
                    aoeShape: effectiveShape,
                    cellDistanceType: distType);

                string eff = r.typeMultiplier > 1f ? " HIỆU QUẢ!" :
                              r.typeMultiplier < 1f ? " Không hiệu quả..." : "";
                string crit = r.isCritical ? " CHÍ MẠNG!" : "";
                string stab = r.isStab ? " [STAB]" : "";

                Debug.Log($"[Combat] {attacker.Data.thingName}{stab} → " +
                          $"{target.Data.thingName}: {r.damage} dmg (x{r.typeMultiplier}){eff}{crit}");

                target.TakeDamage(r.damage, r.isCritical);
            }
        }

        StartCoroutine(EndJudgePhase());
    }

    // ── Xử lý chiêu Thời Tiết ────────────────────────────────────
    void HandleWeatherMove(BattleEntity attacker, BattleCommand cmd, MoveData move)
    {
        WeatherTarget target = ResolveWeatherTarget(cmd, move);
        WeatherManager.Instance.ApplyWeather(move, target);
        Debug.Log($"[Weather] {attacker.name} tung thời tiết: {move.weatherType} → {target}");
    }

    // ── Xử lý chiêu Địa Hình ─────────────────────────────────────
    void HandleTerrainMove(BattleEntity attacker, BattleCommand cmd, MoveData move)
    {
        var cells = BattleGridManager.Instance.GetAoECells(
            cmd.attackTarget, move.terrainShape, attacker.GridPos, move.aoeRadius);

        foreach (var cell in cells)
            TerrainManager.Instance.PlaceTerrain(cell, move);

        Debug.Log($"<color=magenta>[Terrain]</color> {attacker.name} đặt {move.terrainEffect} | " +
                  $"target={cmd.attackTarget} | {cells.Count} ô");
    }

    WeatherTarget ResolveWeatherTarget(BattleCommand cmd, MoveData move)
    {
        if (move.weatherTarget == WeatherTarget.Both)
            return WeatherTarget.Both;

        if (!cmd.HasAttack || cmd.attackTarget.Equals(cmd.moveTarget))
            return move.weatherTarget;

        var cfg = BattleGridManager.Instance.config;
        if (cmd.attackTarget.col <= cfg.LeftMaxCol) return WeatherTarget.TeamLeft;
        if (cmd.attackTarget.col >= cfg.RightMinCol) return WeatherTarget.TeamRight;

        return move.weatherTarget;
    }

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

        var deadEntities = new List<BattleEntity>();
        foreach (var entity in BattleGridManager.Instance.GetAllEntities())
            if (entity.IsDead) deadEntities.Add(entity);

        foreach (var dead in deadEntities)
            Destroy(dead.gameObject);

        yield return null;

        var allAlive = BattleGridManager.Instance.GetAllEntities();
        TerrainManager.Instance.OnTurnEnd(allAlive);
        WeatherManager.Instance.OnTurnEnd();

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